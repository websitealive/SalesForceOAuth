﻿using Newtonsoft.Json.Linq;
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
            string urlReferrer = Request.RequestUri.Authority.ToString();
            if (lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
                {
                    //Access token update
                    HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
                    if (msg.StatusCode != HttpStatusCode.OK)
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode,true); }

                    try
                    {
                        string InstanceUrl = "", ApiVersion = "", ItemId ="", ItemType= "";
                        MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
                        int chatId = 0;  
                        MyAppsDb.GetTaggedChatId(lData.ObjectRef, lData.GroupId, lData.SessionId,ref chatId, ref ItemId, ref ItemType,urlReferrer); 
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
                            MyAppsDb.ChatQueueItemAdded(chatId,urlReferrer, lData.ObjectRef);
                            PostedObjectDetail output = new PostedObjectDetail();
                            output.Id = sR.Id;
                            output.ObjectName = "Chat";
                            output.Message = "Chat added successfully!";
                            return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
                        } 
                        else
                        {
                            return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
                        }
                    }
                    catch (Exception ex)
                    {
                        return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError,true);
                    }
                }
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,true);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token,string ObjectRef, int GroupId, int SessionId, string ObjType, string ObjId, string callback)
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYChat-GetTagChat", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            #endregion JWT Token
            string urlReferrer = Request.RequestUri.Authority.ToString();
            List<Lead> myLeads = new List<Lead> { };
            try
            {
                MyAppsDb.TagChat(ObjectRef, GroupId, SessionId, ObjType, ObjId, urlReferrer);
                PostedObjectDetail output = new PostedObjectDetail();
                output.ObjectName = "TagChat";
                output.Message = "Chat Tagged successfully!";
                return MyAppsDb.ConvertJSONPOutput(callback , output, HttpStatusCode.OK,false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFChat-GetTagChat", "Unhandled exception", HttpStatusCode.InternalServerError);
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
