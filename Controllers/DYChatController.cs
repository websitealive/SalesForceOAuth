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
namespace SalesForceOAuth.Controllers
{
    public class DYChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(string token)
        {
            //string AccessToken = "";
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
                try 
                {
                    //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                    string outputPayload;
                    try
                    {
                        outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                    }
                    catch (Exception ex)
                    {
                        return MyAppsDb.ConvertJSONOutput(ex.InnerException, HttpStatusCode.InternalServerError);
                    }
                    JObject values = JObject.Parse(outputPayload); // parse as array  
                    MessageData lData = new MessageData();
                    lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                    lData.ObjectRef = values.GetValue("ObjectRef").ToString();
                    lData.Message = values.GetValue("Message").ToString();
                    lData.Subject = values.GetValue("Subject").ToString();
                    lData.SessionId = Convert.ToInt32(values.GetValue("SessionId").ToString());
                    #region dynamics api call
                    string ItemType = "Account";
                    //string ItemId = "/accounts(b123e935-92cc-e611-8104-c4346bac5238)"; 
                    //string ItemId = "/leads(b56264dc-7332-e611-80e5-5065f38b31c1)"; 
                    string ItemId = ""; 
                    int chatId = 0;
                    MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType);
                    ItemId = "/" + ItemType + "(" + ItemId + ")";
                    try
                    {
                        //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
                        HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(lData.ObjectRef, lData.GroupId.ToString());
                        if (msg.StatusCode == HttpStatusCode.OK)
                        { AccessToken = msg.Content.ReadAsStringAsync().Result; }
                        else
                        { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        StringBuilder requestURI = new StringBuilder();
                        requestURI.Append("/api/data/v8.0/tasks");

                        //DYChatPostValue aData = new DYChatPostValue();
                        //aData.subject = lData.Subject;
                        //aData.description = lData.Messsage;
                        //aData.regardingobjectid_account = ItemId; "primarycontactid@odata.bind":"/accounts("+ ItemId + ")"
                        JObject aData = new JObject();
                        aData.Add("subject", lData.Subject);
                        aData.Add("description", lData.Message);
                        if(ItemId.Contains("account"))
                            aData.Add("regardingobjectid_account_task@odata.bind", ItemId);
                        else
                            aData.Add("regardingobjectid_lead_task@odata.bind", ItemId);
                        StringContent content = new StringContent(aData.ToString(), Encoding.UTF8, "application/json");
                        HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var output = response.Headers.Location.OriginalString;
                            var id = output.Substring(output.IndexOf("(") + 1, 36);
                            PostedObjectDetail pObject = new PostedObjectDetail();
                            pObject.Id = id;
                            pObject.ObjectName = "Chat";
                            pObject.Message = "Chat added successfully!";
                            MyAppsDb.ChatQueueItemAddedDynamics(chatId); 
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
                    #endregion dynamics api call

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token, string callback)
        {
            //var re = Request;
            //var headers = re.Headers;
            string ObjectRef = "", ObjType = "", ObjId = "";
            int GroupId = 0, SessionId = 0; 
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
                JObject values = JObject.Parse(outputPayload); // parse as array  
                GroupId = Convert.ToInt32( values.GetValue("GroupId").ToString());
                SessionId = Convert.ToInt32(values.GetValue("SessionId").ToString());
                ObjectRef = values.GetValue("ObjectRef").ToString();
                ObjType = values.GetValue("ObjType").ToString();
                ObjId = values.GetValue("ObjId").ToString();
                try
                {
                    MyAppsDb.TagChatDynamics(ObjectRef, GroupId, SessionId, ObjType, ObjId);
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.ObjectName = "TagChat";
                    output.Message = "Chat Tagged successfully!";
                    return MyAppsDb.ConvertJSONPOutput(callback,output, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback ,"Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            //}
            //else
            //{
            //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
            //}
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
        public string regardingobjectid_account { get; set; }
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
        public string regardingobjectid_account{ get; set; }
    }

}
