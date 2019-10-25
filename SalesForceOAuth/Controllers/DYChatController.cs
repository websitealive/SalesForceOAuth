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
using Microsoft.Xrm.Sdk.Query;
using SalesForceOAuth.Models;
using SalesForceOAuth.Web_API_Helper_Code;
using CRM.Notification;

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
                string ItemId = "", ItemType = "", OwnerId = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int chatId = 0;
                bool IsChatPushed = false;
                try
                {
                    //Test system
                    //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                    //    password = "Getthat$$$5", authType = "Office365";
                    //Live system
                    string ApplicationURL = "", userName = "", password = "", authType = "";

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
                    
                    MyAppsDb.GetTaggedChatDynamicsId(lData.ObjectRef, lData.GroupId, lData.SessionId, ref chatId, ref ItemId, ref ItemType, ref OwnerId, urlReferrer);

                    // Wher to save chats
                    EntitySettings entitySettings = Repository.GetDyEntitySettings(lData.ObjectRef, lData.GroupId, urlReferrer);

                    // Get Back End Fields
                    var getBackEndFeields = Repository.GetDYBackEndFields(lData.ObjectRef, lData.GroupId, urlReferrer, ItemType);

                    if (chatId != 0)
                    {
                        Guid newChatId;
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                        {
                            #region set properties
                            string ownerInfo, postMessage, chatEntity, parentLookupField;
                            IOrganizationService objser = (IOrganizationService)proxyservice;
                            Entity registration;
                            Entity post = new Entity("post");
                            post["source"] = new OptionSetValue(2);
                            post["type"] = new OptionSetValue(4);

                            if (OwnerId != "")
                            {
                                ColumnSet entityColumn = new ColumnSet();
                                entityColumn.AddColumn("fullname");
                                Entity chk = objser.Retrieve("systemuser", new Guid(OwnerId), entityColumn);

                                postMessage = "Chat with AliveChat ID: " + lData.SessionId + " Created By " + chk.Attributes["fullname"];
                                ownerInfo = "Chat Created By " + chk.Attributes["fullname"];
                                lData.Message = lData.Message + "|" + ownerInfo;
                            }
                            else
                            {
                                postMessage = "AliveChat ID: " + lData.SessionId + " is Created.";
                                ownerInfo = null;
                            }
                            #endregion

                            if (entitySettings.SaveChatsTo == "alivechat_entity")
                            {
                                if (ItemType.Contains("account"))
                                {
                                    chatEntity = "ayu_alivechat";
                                    parentLookupField = "ayu_account";
                                }
                                else if (ItemType.Contains("lead"))
                                {
                                    chatEntity = "ayu_leadalivechat";
                                    parentLookupField = "ayu_leadid";
                                }
                                else if (ItemType.Contains("contact"))
                                {
                                    chatEntity = "ayu_contactalivechat";
                                    parentLookupField = "ayu_contactid";
                                }
                                else
                                {
                                    chatEntity = "ayu_chat";
                                    parentLookupField = "ayu_" + ItemType + "id";
                                }

                                ConditionExpression filterOwnRcd = new ConditionExpression();
                                filterOwnRcd.AttributeName = "uniquename";
                                filterOwnRcd.Operator = ConditionOperator.Equal;
                                filterOwnRcd.Values.Add(ConfigurationManager.AppSettings["DynamicsManagedSolName"].ToString());

                                FilterExpression filter1 = new FilterExpression();
                                filter1.Conditions.Add(filterOwnRcd);


                                QueryExpression query = new QueryExpression("solution");
                                query.ColumnSet.AddColumns("solutionid", "friendlyname", "version", "ismanaged", "uniquename");
                                query.Criteria.AddFilter(filter1);
                                EntityCollection result1 = objser.RetrieveMultiple(query);
                                if (result1.Entities.Count > 0)
                                {
                                    registration = new Entity(chatEntity);
                                    registration[parentLookupField] = new EntityReference(ItemType, new Guid(ItemId));
                                    registration["ayu_name"] = "AliveChat ID: " + lData.SessionId;
                                    if (OwnerId != "")
                                    {
                                        registration["ownerid"] = new EntityReference("systemuser", new Guid(OwnerId));
                                    }
                                    registration["ayu_chat"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                    newChatId = objser.Create(registration);
                                }
                                else
                                {
                                    newChatId = Guid.Empty;
                                }
                            }
                            else if (entitySettings.SaveChatsTo == "custom_activity_type")
                            {
                                //The code of block in the if condition is only for Sounders Client
                                //if ((lData.ObjectRef == "c1" && lData.GroupId == 8370) | (lData.ObjectRef == "dev0" && lData.GroupId == 7))
                                //if (lData.ObjectRef == "c1" && lData.GroupId == 8370)
                                if (lData.ObjectRef == "c1" && lData.GroupId == 8370)
                                {
                                    Entity task = new Entity("task");
                                    task["subject"] = "AliveChat ID: " + lData.SessionId;
                                    task["description"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                    task["kore_tasktype"] = "Web Chat";
                                    task["regardingobjectid"] = new EntityReference(ItemType, new Guid(ItemId));
                                    if (OwnerId != "")
                                    {
                                        task["ownerid"] = new EntityReference("systemuser", new Guid(OwnerId));
                                    }
                                    var tsakId = objser.Create(task);
                                }
                                Entity task2 = new Entity(entitySettings.CustomActivityName);
                                task2["subject"] = "AliveChat ID: " + lData.SessionId;
                                task2["description"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                task2["regardingobjectid"] = new EntityReference(ItemType, new Guid(ItemId));
                                if (OwnerId != "")
                                {
                                    task2["ownerid"] = new EntityReference("systemuser", new Guid(OwnerId));
                                }
                                newChatId = objser.Create(task2);
                            }
                            else
                            {
                                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                                
                                Entity note = new Entity("annotation");
                                note["subject"] = "AliveChat ID: " + lData.SessionId + " @ " + cstTime + " CDT";
                                note["notetext"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                note["objectid"] = new EntityReference(ItemType, new Guid(ItemId));
                                newChatId = objser.Create(note);
                            }

                            if (newChatId != Guid.Empty)
                            {
                                IsChatPushed = true;
                                if (getBackEndFeields.Count > 0)
                                {
                                    Entity parentEntity = objser.Retrieve(ItemType, new Guid(ItemId), new ColumnSet(true));
                                    foreach (var item in getBackEndFeields)
                                    {
                                        if(item.FieldType == "lookup")
                                        {
                                            parentEntity[item.FieldName] = new EntityReference(item.LookupEntityName, new Guid(item.LookupEntityRecordId));
                                        }
                                        else if (item.FieldType == "datetime")
                                        {
                                            if(item.IsUsingCurrentDate == 1)
                                            {
                                                parentEntity[item.FieldName] = DateTime.Now;
                                            }
                                            else
                                            {
                                                parentEntity[item.FieldName] = Convert.ToDateTime(item.ValueDetail);
                                            }
                                        }
                                        else if (item.FieldType == "currency")
                                        {
                                            parentEntity[item.FieldName] = new Money(Convert.ToDecimal(item.ValueDetail));
                                        }
                                        else
                                        {
                                            parentEntity[item.FieldName] = item.ValueDetail;
                                        }
                                    }
                                    objser.Update(parentEntity);
                                }

                                post["regardingobjectid"] = new EntityReference(ItemType, new Guid(ItemId));
                                post["text"] = postMessage;
                                Guid newPostID = objser.Create(post);

                                PostedObjectDetail pObject = new PostedObjectDetail();
                                pObject.Id = newChatId.ToString();
                                pObject.ObjectName = "Chat";
                                pObject.Message = "Chat added successfully!";
                                MyAppsDb.ChatQueueItemAddedDynamics(chatId, urlReferrer, lData.ObjectRef, 1, "Chat Added Successfully");
                                return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                            }
                            else
                            {
                                string errorMessage = "Could not add new Chat, Please Import Alive Chat Solution in to your orginization or turn off Use Alive Chat feature under Integration Settings in Admin Panel !";
                                Slack.SendMessage(lData.ObjectRef, lData.GroupId, lData.SessionId, ItemId, ItemType, errorMessage);
                                MyAppsDb.ChatQueueItemAddedDynamics(chatId, urlReferrer, lData.ObjectRef, 2, errorMessage);
                                return MyAppsDb.ConvertJSONOutput("Could not add new Chat, Please Import Alive Chat Solution in to your orginization or turn off Use Alive Chat feature under Integration Settings in Admin Panel ", HttpStatusCode.InternalServerError, true);
                            }
                        }
                    }
                    else
                    {
                        // MyAppsDb.ChatQueueItemAddedDynamics(chatId, urlReferrer, lData.ObjectRef, 2, "No Chat in queue to publish");
                        return MyAppsDb.ConvertJSONOutput("No Chat in queue to publish", HttpStatusCode.InternalServerError, true);
                    }
                    #endregion code for post add message
                }
                catch (Exception ex)
                {
                    string msg = string.Empty;
                    if (IsChatPushed)
                    {
                        msg = "Request Completed with some errors. Errors :" + ex.Message.Replace("'", " ");
                    }
                    else
                    {
                        msg = ex.Message;
                    }
                    Slack.SendMessage(lData.ObjectRef, lData.GroupId, lData.SessionId, ItemId, ItemType, msg);
                    MyAppsDb.ChatQueueItemAddedDynamics(chatId, urlReferrer, lData.ObjectRef, 2, msg);
                    return MyAppsDb.ConvertJSONOutput(ex, "DYChat-PostChat", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.InternalServerError, true);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token, string ObjectRef, int GroupId, int SessionId, int Alive5ContactId, string ObjType, string ObjId, string callback, string OwnerId)
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
                MyAppsDb.TagChatDynamics(ObjectRef, GroupId, SessionId, Alive5ContactId, ObjType, ObjId, OwnerId, urlReferrer);
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
