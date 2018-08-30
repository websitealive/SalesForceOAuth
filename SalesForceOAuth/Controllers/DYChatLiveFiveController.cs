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
            #region code for post add message
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
                string ChatId;
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);
                bool flag = Repository.IsChatExist(lData.EntitytId, lData.EntitytType, lData.ObjectRef, urlReferrer, out ChatId);

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

                string ChatEntityName = string.Empty;
                string RelationField = string.Empty;
                if (lData.EntitytType == "Account")
                {
                    RelationField = "ayu_accountid";
                    ChatEntityName = "ayu_alive5sms";
                }
                else if (lData.EntitytType == "Contact")
                {
                    RelationField = "ayu_contactid";
                    ChatEntityName = "ayu_contactalive5sms";
                }
                else
                {
                    RelationField = "ayu_leadid";
                    ChatEntityName = "ayu_leadalive5sms";
                }
                //In chats in CRM
                if (!flag)
                {
                    // Inserting the new chat
                    Guid newChatId;
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                    {
                        #region set properties
                        IOrganizationService objser = (IOrganizationService)proxyservice;
                        Entity registration = new Entity(ChatEntityName);
                        registration[RelationField] = new EntityReference(lData.EntitytType.ToLower(), new Guid(lData.EntitytId));
                        registration["ayu_name"] = "AliveChat ID: " + lData.SessionId;
                        registration["ayu_alive5sms"] = lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                        #endregion set properties
                        newChatId = objser.Create(registration);
                    }
                    if (newChatId != Guid.Empty)
                    {
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = newChatId.ToString();
                        pObject.ObjectName = "Chat";
                        pObject.Message = "Chat added successfully!";

                        Repository.AddChatInfo(lData.ObjectRef, urlReferrer, "Dynamic", lData.EntitytId, lData.EntitytType, newChatId.ToString());

                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Chat, check mandatory fields", HttpStatusCode.InternalServerError, true);
                    }

                }
                else
                {
                    // Update the Previous chat
                    ColumnSet cols = new ColumnSet(new String[] { "ayu_alive5sms" });
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                    {
                        Entity retrievedChats = proxyservice.Retrieve(ChatEntityName, new Guid(ChatId), cols);

                        var prChats = retrievedChats.Attributes["ayu_alive5sms"];
                        var newChats = retrievedChats.Attributes["ayu_alive5sms"] + "|" + lData.Message;
                        retrievedChats.Attributes["ayu_alive5sms"] = newChats.Replace("|", "\r\n").Replace("&#39;", "'");
                        proxyservice.Update(retrievedChats);
                    }

                    PostedObjectDetail pObject = new PostedObjectDetail();
                    pObject.Id = lData.ChatId.ToString();
                    pObject.ObjectName = "Chat";
                    pObject.Message = "Chat added successfully!";

                    return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);

                }

                #endregion code for post add message  

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFChat-PostChat", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}

