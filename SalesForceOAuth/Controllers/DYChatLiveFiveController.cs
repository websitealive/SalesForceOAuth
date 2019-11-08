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
using SalesForceOAuth.Models;

namespace SalesForceOAuth.Controllers
{
    public class DYChatLiveFiveController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageDataCopy lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyChats-PostChats", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            try
            {
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string ChatId, RowId;
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);
                bool flag = Repository.IsChatExist(lData.EntitytId, lData.EntitytType, lData.App, lData.ObjectRef, urlReferrer, out ChatId, out RowId);

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

                // Wher to save chats
                EntitySettings entitySettings = Repository.GetDyEntitySettings(lData.ObjectRef, lData.GroupId, urlReferrer);

                // Get Back End Fields
                var getBackEndFeields = Repository.GetDYBackEndFields(lData.ObjectRef, lData.GroupId, urlReferrer, lData.EntitytType);

                Guid newChatId = Guid.Empty;
                PostedObjectDetail pObject = new PostedObjectDetail();
                string HeadingSms = " App: " + lData.App + " |  Name: " + lData.OwnerName + " |  E-mail: " + lData.OwnerEmail + " |  Phone Number: " + lData.OwnerPhone + " | | Chat Content |";
                //In chats in CRM
                if (!flag)
                {
                    // Inserting the new chat
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                    {
                        IOrganizationService objser = (IOrganizationService)proxyservice;
                        if (entitySettings.SaveChatsTo == "alivechat_entity")
                        {
                            string ChatEntityName = string.Empty;
                            string RelationField = string.Empty;
                            if (lData.EntitytType.ToLower() == "account")
                            {
                                RelationField = "ayu_accountid";
                                ChatEntityName = "ayu_alive5sms";
                            }
                            else if (lData.EntitytType.ToLower() == "contact")
                            {
                                RelationField = "ayu_contactid";
                                ChatEntityName = "ayu_contactalive5sms";
                            }
                            else if (lData.EntitytType.ToLower() == "lead")
                            {
                                RelationField = "ayu_leadid";
                                ChatEntityName = "ayu_leadalive5sms";
                            }
                            else
                            {
                                ChatEntityName = "ayu_chat";
                                RelationField = "ayu_" + lData.EntitytType.ToLower() + "id";
                            }
                        }
                        else if (entitySettings.SaveChatsTo == "custom_activity_type")
                        {
                            Entity task2 = new Entity(entitySettings.CustomActivityName);
                            task2["subject"] = "AliveChat ID: " + lData.SessionId;
                            task2["description"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                            task2["regardingobjectid"] = new EntityReference(lData.EntitytType.ToLower(), new Guid(lData.EntitytId));

                            newChatId = objser.Create(task2);
                        }
                        else
                        {
                            Entity note = new Entity("annotation");
                            note["subject"] = lData.Subject;
                            note["notetext"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                            note["objectid"] = new EntityReference(lData.EntitytType.ToLower(), new Guid(lData.EntitytId));
                            newChatId = objser.Create(note);
                        }

                        if (newChatId != Guid.Empty)
                        {
                            if (getBackEndFeields.Count > 0)
                            {
                                Entity parentEntity = proxyservice.Retrieve(lData.EntitytType.ToLower(), new Guid(lData.EntitytId), new ColumnSet(true));
                                foreach (var item in getBackEndFeields)
                                {
                                    if (item.FieldType == "lookup")
                                    {
                                        parentEntity[item.FieldName] = new EntityReference(item.LookupEntityName, new Guid(item.LookupEntityRecordId));
                                    }
                                    else if (item.FieldType == "datetime")
                                    {
                                        if (item.IsUsingCurrentDate == 1)
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
                                proxyservice.Update(parentEntity);
                            }

                            pObject.Id = newChatId.ToString();
                            pObject.ObjectName = "Chat";
                            pObject.Message = "Chat added successfully!";

                            Repository.AddChatInfo(lData.ObjectRef, urlReferrer, "Dynamic", lData.EntitytId, lData.EntitytType, lData.App, newChatId.ToString());

                            return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                        }
                        else
                        {
                            return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError, true);
                        }
                    }
                }
                else
                {
                    // Update the Previous chat
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                    {
                        try
                        {
                            if (entitySettings.SaveChatsTo == "custom_activity_type")
                            {
                                ColumnSet cols = new ColumnSet(new String[] { "description" });
                                Entity retrievedtask = new Entity(entitySettings.CustomActivityName);
                                var newChats = lData.Message;
                                retrievedtask.Attributes["description"] = retrievedtask.Attributes["notetext"] + "\r\n" + newChats.Replace("|", "\r\n").Replace("&#39;", "'");
                                proxyservice.Update(retrievedtask);
                            }
                            else
                            {
                                ColumnSet cols = new ColumnSet(new String[] { "notetext" });
                                Entity retrievedNote = proxyservice.Retrieve("annotation", new Guid(ChatId), cols);
                                var newChats = lData.Message;
                                retrievedNote.Attributes["notetext"] = retrievedNote.Attributes["notetext"] + "\r\n" + newChats.Replace("|", "\r\n").Replace("&#39;", "'");
                                proxyservice.Update(retrievedNote);
                            }
                        }
                        catch (Exception)
                        {
                            if (entitySettings.SaveChatsTo == "custom_activity_type")
                            {
                                Entity task2 = new Entity(entitySettings.CustomActivityName);
                                task2["subject"] = "AliveChat ID: " + lData.SessionId;
                                task2["description"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                task2["regardingobjectid"] = new EntityReference(lData.EntitytType.ToLower(), new Guid(lData.EntitytId));

                                newChatId = proxyservice.Create(task2);
                            }
                            else
                            {
                                Entity note = new Entity("annotation");
                                note["subject"] = lData.Subject;
                                note["notetext"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                                note["objectid"] = new EntityReference(lData.EntitytType.ToLower(), new Guid(lData.EntitytId));
                                newChatId = proxyservice.Create(note);
                            }
                        }

                    }
                    if (newChatId == Guid.Empty)
                    {
                        pObject.Id = lData.ChatId.ToString();
                        pObject.ObjectName = "Chat";
                        pObject.Message = "Chat Updated successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError, true);
                    }
                }


            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFChat-PostChat", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}

