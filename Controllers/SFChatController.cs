using Newtonsoft.Json.Linq;
using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            string AccessToken = "";
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
               // string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                //string outputPayload;
                //try
                //{
                //    outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
                //}
                //catch (Exception ex)
                //{
                //    return MyAppsDb.ConvertJSONOutput(ex.InnerException, HttpStatusCode.InternalServerError);
                //}
                //JObject values = JObject.Parse(outputPayload); // parse as array  
                //MessageData lData = new MessageData();
                //lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                //lData.ObjectRef = values.GetValue("ObjectRef").ToString();
                //lData.Message = values.GetValue("Message").ToString();
                //lData.Subject = values.GetValue("Subject").ToString();
                //lData.SessionId = Convert.ToInt32(values.GetValue("SessionId").ToString());

                if(lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
                {
                    //Access token update
                    HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef));

                    if (msg.StatusCode != HttpStatusCode.OK)
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                    try
                    {
                        string InstanceUrl = "", ApiVersion = "", ItemId ="", ItemType= "";
                        MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                        int chatId = 0;  
                        MyAppsDb.GetTaggedChatId(lData.ObjectRef, lData.GroupId, lData.SessionId,ref chatId, ref ItemId, ref ItemType); 
                        ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        TaskLogACall lTemp = new TaskLogACall();
                        lTemp.Subject = lData.Subject; //"WebsiteAlive-Chat1";
                        lTemp.Description = lData.Message.Replace("|", "\r\n") ;
                        if (ItemType == "Lead" || ItemType == "Contact")
                            lTemp.WhoId = ItemId;
                        else
                            lTemp.WhatId = ItemId;
                        lTemp.Status = "Completed"; 
                        var lACall = lTemp ;
                        SuccessResponse sR = await client.CreateAsync("Task", lACall).ConfigureAwait(false);
                        if (sR.Success == true)
                        {
                            MyAppsDb.ChatQueueItemAdded(chatId);
                            PostedObjectDetail output = new PostedObjectDetail();
                            output.Id = sR.Id;
                            output.ObjectName = "Chat";
                            output.Message = "Chat added successfully!";
                            return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK);
                        } 
                        else
                        {
                            return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError);
                        }
                    }
                    catch (Exception ex)
                    {
                        return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                    }
                }
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token,string ObjectRef, int GroupId, int SessionId, string ObjType, string ObjId, string callback)
        {
            //var re = Request;
            //var headers = re.Headers;
            //string ObjectRef = "", ObjType = "", ObjId = "";
            //int GroupId = 0, SessionId = 0;
            //if (headers.Contains("Authorization"))
            //{
                #region JWT Token 
                //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback,ex.InnerException, HttpStatusCode.InternalServerError);
                }
                #endregion JWT Token
                //JObject values = JObject.Parse(outputPayload); // parse as array  
                //GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                //SessionId = Convert.ToInt32(values.GetValue("SessionId").ToString());
                //ObjectRef = values.GetValue("ObjectRef").ToString();
                //ObjType = values.GetValue("ObjType").ToString();
                //ObjId = values.GetValue("ObjId").ToString();
                List<Lead> myLeads = new List<Lead> { };
                try
                {
                    MyAppsDb.TagChat(ObjectRef, GroupId, SessionId, ObjType, ObjId);
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.ObjectName = "TagChat";
                    output.Message = "Chat Tagged successfully!";
                    return MyAppsDb.ConvertJSONPOutput(callback , output, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback,"Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            //}
            //else
            //{
            //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
            //}
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
    public class MessageData: MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public int SessionId { get; set; }
        //public string ItemId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

    }
  
}
