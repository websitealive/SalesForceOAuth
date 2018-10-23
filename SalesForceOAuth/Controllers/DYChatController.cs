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

namespace SalesForceOAuth.Controllers
{
    public class DYChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            if (lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
            {
                #region code for post add message    
                try
                {
                    //Test system
                    //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                    //    password = "Getthat$$$5", authType = "Office365";
                    //Live system
                    string ApplicationURL = "", userName = "", password = "", authType = "";
                    string urlReferrer = Request.RequestUri.Authority.ToString();
                    int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                    Uri organizationUri;
                    Uri homeRealmUri;
                    ClientCredentials credentials = new ClientCredentials();
                    ClientCredentials deviceCredentials = new ClientCredentials();
                    credentials.UserName.UserName = userName;
                    credentials.UserName.Password = password;
                    deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                    deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
                    organizationUri = new Uri(ApplicationURL + "/XRMServices/2011/Organization.svc");
                    homeRealmUri = null;
                    string ItemId = "", ItemType = "";
                    int chatId = 0;
                    MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType, urlReferrer);
                    if (chatId != 0)
                    {
                        Guid newChatId;
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                        {
                            #region set properties
                            IOrganizationService objser = (IOrganizationService)proxyservice;
                            Entity registration;
                            Entity post = new Entity("post");
                            post["source"] = new OptionSetValue(2);
                            post["type"] = new OptionSetValue(4);
                            if (ItemType.Contains("account"))
                            {
                                registration = new Entity("ayu_alivechat");
                                registration["ayu_account"] = new EntityReference("account", new Guid(ItemId));
                                registration["ayu_name"] = "AliveChat ID: " + lData.SessionId;
                                registration["ayu_chat"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");

                                post["regardingobjectid"] = new EntityReference("account", new Guid(ItemId)); ;
                                post["text"] = "AliveChat ID: " + lData.SessionId + " is Created.";
                            }
                            else if (ItemType.Contains("lead"))
                            {
                                registration = new Entity("ayu_leadalivechat");
                                registration["ayu_leadid"] = new EntityReference("lead", new Guid(ItemId));
                                registration["ayu_name"] = "AliveChat ID: " + lData.SessionId;
                                registration["ayu_chat"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");

                                post["regardingobjectid"] = new EntityReference("lead", new Guid(ItemId)); ;
                                post["text"] = "AliveChat ID: " + lData.SessionId + " is Created.";
                            }
                            else if (ItemType.Contains("contact"))
                            {
                                registration = new Entity("ayu_contactalivechat");
                                registration["ayu_contactid"] = new EntityReference("contact", new Guid(ItemId));
                                registration["ayu_name"] = "AliveChat ID: " + lData.SessionId;
                                registration["ayu_chat"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");

                                post["regardingobjectid"] = new EntityReference("contact", new Guid(ItemId)); ;
                                post["text"] = "AliveChat ID: " + lData.SessionId + " is Created.";

                            }
                            else
                            {
                                registration = new Entity();
                                newChatId = Guid.Empty;

                                return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError, true);
                            }

                            #endregion
                            newChatId = objser.Create(registration);
                            Guid newPostID = objser.Create(post);
                        }
                        if (newChatId != Guid.Empty)
                        {
                            PostedObjectDetail pObject = new PostedObjectDetail();
                            pObject.Id = newChatId.ToString();
                            pObject.ObjectName = "Chat";
                            pObject.Message = "Chat added successfully!";
                            MyAppsDb.ChatQueueItemAddedDynamics(chatId, urlReferrer, lData.ObjectRef);
                            return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                        }
                        else
                        {
                            return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError, true);
                        }
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("No Chat in queue to publish", HttpStatusCode.InternalServerError, true);
                    }

                    #endregion code for post add message  

                    #region old code 
                    //    string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                    //connectionString += "RequireNewInstance=true;";
                    //CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                    //if (crmSvc != null && crmSvc.IsReady)
                    //{
                    //    string ItemId = "", ItemType = ""; 
                    //    int chatId = 0;
                    //    MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType);
                    //    if (chatId != 0)
                    //    {
                    //        // create activity
                    //        Guid activityId = crmSvc.CreateNewActivityEntry("task", "account", new Guid(ItemId), lData.Subject, lData.Message, crmSvc.OAuthUserId);

                    //        if (activityId != Guid.Empty)
                    //        {
                    //            PostedObjectDetail pObject = new PostedObjectDetail();
                    //            pObject.Id = activityId.ToString();
                    //            pObject.ObjectName = "Chat";
                    //            pObject.Message = "Chat added successfully!";
                    //            MyAppsDb.ChatQueueItemAddedDynamics(chatId);
                    //            return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK,false);
                    //        }
                    //        else
                    //        {
                    //            return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError,true);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        return MyAppsDb.ConvertJSONOutput("No Chat in queue to publish", HttpStatusCode.InternalServerError,true);
                    //    }
                    //        #endregion old code 
                    //    }
                    //    else
                    //{
                    //    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError,true);
                    //}
                    //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                    #region dynamics api call
                    //string ItemType = "Account";
                    ////string ItemId = "/accounts(b123e935-92cc-e611-8104-c4346bac5238)"; 
                    ////string ItemId = "/leads(b56264dc-7332-e611-80e5-5065f38b31c1)"; 
                    //string ItemId = ""; 
                    //int chatId = 0;
                    //MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType);
                    //ItemId = "/" + ItemType + "(" + ItemId + ")";
                    //try
                    //{
                    //    //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
                    //    HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(lData.ObjectRef, lData.GroupId.ToString());
                    //    if (msg.StatusCode == HttpStatusCode.OK)
                    //    { AccessToken = msg.Content.ReadAsStringAsync().Result; }
                    //    else
                    //    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                    //    HttpClient client = new HttpClient();
                    //    client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
                    //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    //    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    //    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    //    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    //    StringBuilder requestURI = new StringBuilder();
                    //    requestURI.Append("/api/data/v8.0/tasks");

                    //    //DYChatPostValue aData = new DYChatPostValue();
                    //    //aData.subject = lData.Subject;
                    //    //aData.description = lData.Messsage;
                    //    //aData.regardingobjectid_account = ItemId; "primarycontactid@odata.bind":"/accounts("+ ItemId + ")"
                    //    JObject aData = new JObject();
                    //    aData.Add("subject", lData.Subject);
                    //    aData.Add("description", lData.Message);
                    //    if(ItemId.Contains("account"))
                    //        aData.Add("regardingobjectid_account_task@odata.bind", ItemId);
                    //    else
                    //        aData.Add("regardingobjectid_lead_task@odata.bind", ItemId);
                    //    StringContent content = new StringContent(aData.ToString(), Encoding.UTF8, "application/json");
                    //    HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result ;
                    //    if (response.IsSuccessStatusCode)
                    //    {
                    //        var output = response.Headers.Location.OriginalString;
                    //        var id = output.Substring(output.IndexOf("(") + 1, 36);
                    //        PostedObjectDetail pObject = new PostedObjectDetail();
                    //        pObject.Id = id;
                    //        pObject.ObjectName = "Chat";
                    //        pObject.Message = "Chat added successfully!";
                    //        MyAppsDb.ChatQueueItemAddedDynamics(chatId); 
                    //        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK);
                    //    }
                    //    else
                    //    {
                    //        return MyAppsDb.ConvertJSONOutput("Dynamics Error: " + response.StatusCode, HttpStatusCode.InternalServerError);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                    //}
                    #endregion dynamics api call

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "DYChat-PostChat", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
                #endregion code for post add message
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.InternalServerError, true);
            }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token, string ObjectRef, int GroupId, int SessionId, string ObjType, string ObjId, string callback)
        {
            #region JWT Token 
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DyChat-GetTagChat", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            #endregion JWT Token
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MyAppsDb.TagChatDynamics(ObjectRef, GroupId, SessionId, ObjType, ObjId, urlReferrer);
                PostedObjectDetail output = new PostedObjectDetail();
                output.ObjectName = "TagChat";
                output.Message = "Chat Tagged successfully!";
                return MyAppsDb.ConvertJSONPOutput(callback, output, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYChat-GetTagChat", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
        //pubic searchChat()
        //{
        //    //sample code
        //    string InstanceUrl = "", AccessToken = "", ApiVersion = "", Resource = "";
        //    MyAppsDb.GetAPICredentialsDynamics(objectRef, groupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref Resource);
        //    HttpClient client = new HttpClient();
        //    client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
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
        public string regardingobjectid_account { get; set; }
    }

}
