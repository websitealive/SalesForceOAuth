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

namespace SalesForceOAuth.Controllers
{
    public class DYChatLiveFiveController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage PostAddMessage(MessageDataCopy lData)
        {
            if (lData.token.Equals(ConfigurationManager.AppSettings["APISecureMessageKey"]))
            {
                #region code for post add message    
                try
                {
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


                    //In chats in CRM
                    if (lData.ChatId == null)
                    {
                        // Inserting the new chat
                        Guid newChatId;
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                        {
                            #region set properties
                            IOrganizationService objser = (IOrganizationService)proxyservice;
                            Entity registration = new Entity("new_alive5chat");


                            registration["ayu_lead"] = new EntityReference("lead", new Guid(lData.LeadId));

                            registration["new_name"] = "AliveChat ID: " + lData.SessionId;
                            registration["new_5chat"] = lData.Message;
                            #endregion set properties
                            newChatId = objser.Create(registration);
                        }
                        if (newChatId != Guid.Empty)
                        {
                            PostedObjectDetail pObject = new PostedObjectDetail();
                            pObject.Id = newChatId.ToString();
                            pObject.ObjectName = "Chat";
                            pObject.Message = "Chat added successfully!";

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


                        ColumnSet cols = new ColumnSet(
                            new String[] { "new_5chat", "address1_postalcode", "lastusedincampaign", "versionnumber" });
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                        {
                            Entity retrievedChats = proxyservice.Retrieve("new_alive5chat", lData.ChatId, cols);

                            Console.Write("retrieved ");
                        }

                        return MyAppsDb.ConvertJSONOutput("Record Updated Successfully", HttpStatusCode.OK, false);
                    }

                    #endregion code for post add message  

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SFChat-PostChat", "Unhandled exception", HttpStatusCode.InternalServerError);
                }

            }
            else
            {
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.InternalServerError, true);
            }
        }
    }

    public class MessageDataCopy : MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public int SessionId { get; set; }
        public Guid ChatId { get; set; }
        public byte[] LeadId { get; set; }
        //public string ItemId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<CustomObject> CustomFields { get; set; }
    }
}

