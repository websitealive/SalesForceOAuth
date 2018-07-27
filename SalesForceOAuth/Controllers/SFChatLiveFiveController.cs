using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using MySql.Data.MySqlClient;
using SalesForceOAuth.Web_API_Helper_Code;
using Microsoft.Xrm.Sdk.Query;
using System.Dynamic;

namespace SalesForceOAuth.Controllers
{
    public class SFChatLiveFiveController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            string AccessToken = "";
            string urlReferrer = Request.RequestUri.Authority.ToString();
            if (lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
            {
                //Access token update
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
                try
                {
                    string InstanceUrl = "", ApiVersion = "", ItemId = "", ItemType = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                    int chatId = 0; string OwnerEmail = "";
                    MyAppsDb.GetTaggedChatId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType, ref OwnerEmail, urlReferrer);
                    if (chatId == 0)
                    {
                        return MyAppsDb.ConvertJSONOutput("No chat in queue!", HttpStatusCode.OK, false);
                    }
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ////find lead owner user
                    OwnerEmail = (OwnerEmail == null ? "" : OwnerEmail);
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
                        "where Username like '%" + OwnerEmail + "%' " +
                        "OR Email like '%" + OwnerEmail + "%' ").ConfigureAwait(false);
                    string ownerId = "";
                    foreach (dynamic c in cont.Records)
                    {
                        ownerId = c.Id;
                    }
                    SuccessResponse sR;
                    dynamic lTemp = new ExpandoObject();
                    lTemp.Subject = lData.Subject;
                    lTemp.Description = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                    lTemp.Status = "Completed";
                    if (ItemType == "Lead" || ItemType == "Contact") lTemp.WhoId = ItemId; else lTemp.WhatId = ItemId;
                    if (ownerId != "" && OwnerEmail != "")
                    {
                        MyAppsDb.AddProperty(lTemp, "OwnerId", ownerId);
                    }
                    //if (lData.CustomFields != null)
                    //{
                    //    foreach (CustomObject c in lData.CustomFields)
                    //    {
                    //        if (c.field.ToLower().Equals("subject"))
                    //            lTemp.Subject = c.value;
                    //        else
                    //            MyAppsDb.AddProperty(lTemp, c.field, c.value);
                    //    }
                    //}

                    sR = await client.CreateAsync("Task", lTemp).ConfigureAwait(false);
                    if (sR.Success == true)
                    {
                        MyAppsDb.ChatQueueItemAdded(chatId, urlReferrer, lData.ObjectRef);
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Chat";
                        output.Message = "Chat added successfully!";
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.OK, true);
                    }
                    //return MyAppsDb.ConvertJSONOutput("SalesForce Error: ", HttpStatusCode.OK, true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.OK, true);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized, true);
        }

        public class MessageDataCopy : MyValidation
        {
            public string siteRef { get; set; }
            public string token { get; set; }
            public string ObjectRef { get; set; }
            public int GroupId { get; set; }
            public int SessionId { get; set; }
            public Guid ChatId { get; set; }
            public string LeadId { get; set; }
            //public string ItemId { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
            public List<CustomObject> CustomFields { get; set; }
        }
    }
}
