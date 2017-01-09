using Newtonsoft.Json;
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
namespace SalesForceOAuth.Controllers
{
    public class DYChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "", ItemId = "", ItemType = "", resource="";
                    int chatId = 0;
                    MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType);
                    MyAppsDb.GetAPICredentialsDynamics(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref resource);
                    try
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        StringBuilder requestURI = new StringBuilder();
                        requestURI.Append("/api/data/v8.0/tasks");
                        DYChatPostValue aData = new DYChatPostValue();
                        aData.subject = lData.Subject;
                        aData.description = lData.Messsage;
                        aData._regardingobjectid_value = ItemId;
                        StringContent content = new StringContent(JsonConvert.SerializeObject(aData), Encoding.UTF8, "application/json");
                        HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var output = response.Headers.Location.OriginalString;
                            var id = output.Substring(output.IndexOf("(") + 1, 36);
                            PostedObjectDetail pObject = new PostedObjectDetail();
                            pObject.Id = id;
                            pObject.ObjectName = "Chat";
                            pObject.Message = "Chat added successfully!";
                            return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK);
                        }
                        else
                        {
                            return MyAppsDb.ConvertJSONOutput("Dynamics Error: " + response.StatusCode, HttpStatusCode.InternalServerError);
                        }
                    }
                    catch (Exception ex)
                    {
                        return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                    }







                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    

                    //ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    //TaskLogACall lTemp = new TaskLogACall();
                    //lTemp.Subject = lData.Subject; //"WebsiteAlive-Chat1";
                    //lTemp.Description = lData.Messsage.Replace("|", "\r\n");
                    //if (ItemType == "Lead" || ItemType == "Contact")
                    //    lTemp.WhoId = ItemId;
                    //else
                    //    lTemp.WhatId = ItemId;
                    //lTemp.Status = "Completed";
                    //var lACall = lTemp;
                    //SuccessResponse sR = await client.CreateAsync("Task", lACall);



                    //if (sR.Success == true)
                    //{
                    //    MyAppsDb.ChatQueueItemAdded(chatId);
                    //    PostedObjectDetail output = new PostedObjectDetail();
                    //    output.Id = sR.Id;
                    //    output.ObjectName = "Chat";
                    //    output.Message = "Chat added successfully!";
                    //    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK);
                    //}
                    //else
                    //{
                    //    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError);
                    //}
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string objectRef, int groupId, int sessionId, string ValidationKey, string objType, string objId, string callback)
        {
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
               // List<Lead> myLeads = new List<Lead> { };
                try
                {
                    MyAppsDb.TagChatDynamics(objectRef, groupId, sessionId, objType, objId);
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.ObjectName = "TagChat";
                    output.Message = "Chat Tagged successfully!";
                    return MyAppsDb.ConvertJSONPOutput(callback, output, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return MyAppsDb.ConvertJSONPOutput(callback, "Your request isn't authorized!", HttpStatusCode.Unauthorized);
            }
        }
        //pubic searchChat()
        //{
        //    //sample code
        //    string InstanceUrl = "", AccessToken = "", ApiVersion = "", Resource = "";
        //    MyAppsDb.GetAPICredentialsDynamics(objectRef, groupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref Resource);
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
        //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
        //    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
        //    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
        //    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
        //    System.Text.StringBuilder requestURI = new StringBuilder();
        //    requestURI.Append("/api/data/v8.0/tasks?$select=activityid,subject,description,statecode,_regardingobjectid_value");
        //    requestURI.Append("&$top=100");
        //    requestURI.Append("&$filter=contains(description, 'naveed')");

        //    HttpResponseMessage response = client.GetAsync(requestURI.ToString()).Result;
        //    List<DYTask> myLeads = new List<DYTask> { };
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = response.Content.ReadAsStringAsync().Result;
        //        var odata = JsonConvert.DeserializeObject<DYTaskOutputContainer>(json);
        //        foreach (DYTaskOutput o in odata.value)
        //        {
        //            DYTask l = new DYTask();
        //            l.activityid = o.activityid;
        //            l.description = o.description;
        //            l.statecode = o.statecode;
        //            l.subject = o.subject;
        //            l._regardingobjectid_value = o._regardingobjectid_value;
        //            myLeads.Add(l);
        //        }

        //    }
            //sample code end
       // }

    }
    public class DYTask
    {
        public string activityid { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public int statecode { get; set; }
        public string _regardingobjectid_value { get; set; }
    }
    public class DYTaskOutput : DYTask
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string address1_composites { get; set; }
    }

    public class DYTaskOutputContainer
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYTaskOutput[] value { get; set; }
    }

    public class DYChatPostValue
    {
        public string subject { get; set; }
        public string description { get; set; }
        public string _regardingobjectid_value { get; set; }
    }

}
