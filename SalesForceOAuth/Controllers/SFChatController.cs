using CRM.Notification;
using Newtonsoft.Json.Linq;
using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            string AccessToken = "";
            string urlReferrer = Request.RequestUri.Authority.ToString();
            int chatId = 0;
            bool IsChatPushed = false;
            if (lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
            {
                try
                {
                    HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
                    if (msg.StatusCode != HttpStatusCode.OK)
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
                }
                catch (Exception eee)
                {
                    return MyAppsDb.ConvertJSONOutput("--Internal Exception: " + eee.Message, HttpStatusCode.OK, true);
                }
                string InstanceUrl = "", ApiVersion = "", ItemId = "", ItemType = "";
                try
                {
                    
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                    string OwnerEmail = "";
                    MyAppsDb.GetTaggedChatId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType, ref OwnerEmail, urlReferrer);
                    if (chatId == 0)
                    {
                        Slack.SendMessage(lData.ObjectRef, lData.GroupId, lData.SessionId, ItemId, ItemType, "No chat in queue!");
                        MyAppsDb.ChatQueueItemAdded(chatId, urlReferrer, lData.ObjectRef, 2, "No chat in queue!");
                        return MyAppsDb.ConvertJSONOutput("No chat in queue!", HttpStatusCode.OK, false);
                    }

                    // Get Back End Fields and create object for update
                    var getBackEndFeields = Repository.GetSFBackEndFields(lData.ObjectRef, lData.GroupId, urlReferrer, ItemType);
                    var getBackEndFeieldsForActivity = Repository.GetSFBackEndFields(lData.ObjectRef, lData.GroupId, urlReferrer, "Task");
                    dynamic UpdateRecord = new ExpandoObject();
                    foreach (var item in getBackEndFeields)
                    {
                        MyAppsDb.AddProperty(UpdateRecord, item.FieldName, item.ValueDetail, item.FieldType);
                    }

                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    //find lead owner user
                    OwnerEmail = (OwnerEmail == null ? "" : OwnerEmail);
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
                        "where Username like '%" + OwnerEmail + "%' " +
                        "OR Email like '%" + OwnerEmail + "%' ").ConfigureAwait(false);
                    string ownerId = "";
                    foreach (dynamic c in cont.Records)
                    {
                        ownerId = c.Id;
                    }
                    
                    DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                    SuccessResponse sR;
                    dynamic lTemp = new ExpandoObject();
                    //lTemp.Subject = lData.Subject;
                    lTemp.Subject = "AliveChat ID: " + lData.SessionId + " @ " + cstTime + " CDT";
                    lTemp.Description = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                    lTemp.Status = "Completed";
                    if (ItemType == "Lead" || ItemType == "Contact") lTemp.WhoId = ItemId; else lTemp.WhatId = ItemId;
                    if (ownerId != "" && OwnerEmail != "")
                    {
                        MyAppsDb.AddProperty(lTemp, "OwnerId", ownerId);
                    }
                    if (lData.CustomFields != null)
                    {
                        foreach (CustomObject c in lData.CustomFields)
                        {
                            if (c.field.ToLower().Equals("subject"))
                                lTemp.Subject = c.value;
                            else
                                MyAppsDb.AddProperty(lTemp, c.field, c.value);
                        }
                    }
                    // Only For Contact
                    if (lData.ObjectRef == "c1" && lData.GroupId == 23038)
                    {
                        if (ItemType == "Contact")
                        {
                            string accountId = null;
                            StringBuilder query = new StringBuilder();
                            StringBuilder columns = new StringBuilder();
                            columns.Append("Id, AccountId, Account.Name ");
                            query.Append("SELECT " + columns + " From " + ItemType);
                            query.Append(" where Id ='" + ItemId + "'");
                            QueryResult<dynamic> result = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                            foreach (dynamic c in result.Records)
                            {
                                accountId = (c.AccountId != null ? c.AccountId : null);
                            }
                            if (accountId != null)
                            {
                                MyAppsDb.AddProperty(lTemp, "WhatId", accountId);
                            }
                            MyAppsDb.AddProperty(lTemp, "WhoId", ItemId);
                        }
                    }
                    //if (ItemType == "Account")
                    //{
                    //    MyAppsDb.AddProperty(lTemp, "WhatId", ItemId);
                    //}
                    //
                    foreach (var item in getBackEndFeieldsForActivity)
                    {
                        if (!(item.FieldName == "WhatId" || item.FieldName == "WhoId"))
                        {
                            if (item.FieldType == "datetime" && item.ValueDetail == "")
                            {
                                item.ValueDetail = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
                            }
                            if (item.FieldType == "lookup")
                            {
                                item.ValueDetail = item.LookupEntityRecordId;
                            }
                            MyAppsDb.AddProperty(lTemp, item.FieldName, item.ValueDetail, item.FieldType);
                        }
                    }
                    sR = await client.CreateAsync("Task", lTemp).ConfigureAwait(false);
                    if (sR.Success == true)
                    {
                        IsChatPushed = true;

                        if (getBackEndFeields.Count > 0)
                        {
                            await client.UpdateAsync(ItemType, ItemId, UpdateRecord);
                        }

                        MyAppsDb.ChatQueueItemAdded(chatId, urlReferrer, lData.ObjectRef, 1, "Chat Added Successfully");
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Chat";
                        output.Message = "Chat added successfully!";

                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        Slack.SendMessage(lData.ObjectRef, lData.GroupId, lData.SessionId, ItemId, ItemType, "SalesForce Error: " + sR.Errors);
                        MyAppsDb.ChatQueueItemAdded(chatId, urlReferrer, lData.ObjectRef, 2, "SalesForce Error: " + sR.Errors);
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.OK, true);
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Empty;
                    if (IsChatPushed)
                    {
                        msg = "Request Completed with some errore. Errors :" + ex.Message;
                    }
                    else
                    {
                        msg = ex.Message;
                    }
                    Slack.SendMessage(lData.ObjectRef, lData.GroupId, lData.SessionId, ItemId, ItemType, msg);
                    MyAppsDb.ChatQueueItemAdded(chatId, urlReferrer, lData.ObjectRef, 2, msg);
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.OK, true);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized, true);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token, string ObjectRef, int GroupId, int SessionId, int Alive5ContactId, string ObjType, string ObjId, string OwnerEmail, string callback)
        {
            #region JWT Token 
            //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYChat-GetTagChat", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            #endregion JWT Token
            string urlReferrer = Request.RequestUri.Authority.ToString();
            List<Lead> myLeads = new List<Lead> { };
            try
            {
                MyAppsDb.TagChat(ObjectRef, GroupId, SessionId, Alive5ContactId, ObjType, ObjId, urlReferrer, OwnerEmail);
                PostedObjectDetail output = new PostedObjectDetail();
                output.ObjectName = "TagChat";
                output.Message = "Chat Tagged successfully!";
                return MyAppsDb.ConvertJSONPOutput(callback, output, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFChat-GetTagChat", "Unhandled exception", HttpStatusCode.OK);
            }
        }
    }
    public class TaskLogACall
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string WhoId { get; set; }
        public string WhatId { get; set; }
        public string Status { get; set; }
    }
    public class TaskLogACallOW
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string WhoId { get; set; }
        public string WhatId { get; set; }
        public string Status { get; set; }
        public string OwnerId { get; set; }
    }
    public class MessageData : MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public int SessionId { get; set; }
        //public string ItemId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<CustomObject> CustomFields { get; set; }
    }

}
