using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYContactController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact(DYContactPostData lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-PostContact", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alive5.crm11.dynamics.com", userName = "alive5@alive5.onmicrosoft.com",
                //password = "Passw0rd1", authType = "Office365";
                //string ApplicationURL = "https://websitealive.crmgate.pk/websitealive", userName = "naveed@crmgate.local",
                //    password = "@Abc.123", authType = "IFD";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);

                if (crmSvc != null && crmSvc.IsReady)
                {
                    //create Account object
                    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                    inData.Add("firstname", new CrmDataTypeWrapper(lData.FirstName, CrmFieldType.String));
                    inData.Add("lastname", new CrmDataTypeWrapper(lData.LastName, CrmFieldType.String));
                    if (lData.CustomFields != null)
                    {
                        foreach (DYCustomObject c in lData.CustomFields)
                        {
                            CrmFieldType type;
                            switch (c.type.ToLower())
                            {
                                case "string":
                                    { type = CrmFieldType.String; break; }
                                case "decimal":
                                    { type = CrmFieldType.CrmDecimal; break; }
                                case "lookup":
                                    { type = CrmFieldType.Lookup; break; }
                                case "bool":
                                    { type = CrmFieldType.CrmBoolean; break; }
                                default:
                                    { type = CrmFieldType.String; break; }
                            }
                            if(type ==  CrmFieldType.Lookup)
                            {
                                if (c.value.ToString().Length > 0)
                                {
                                    inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));
                                }
                            }
                            else 
                                inData.Add(c.field, new CrmDataTypeWrapper(c.value, type));
                        }
                    }


                    //inData.Add("ayu_salesengineer", new CrmDataTypeWrapper(lData.Salesengineer, CrmFieldType.Picklist));
                    //if (lData.Salesengineer.ToString().Length > 0)
                    //{
                    //    inData.Add("ayu_salesperson", new CrmDataTypeWrapper(new Guid(lData.Salesengineer), CrmFieldType.Lookup, "systemuser"));
                    //}
                    Guid contactId = crmSvc.CreateNewRecord("contact", inData);
                    if (contactId != Guid.Empty)
                    {
                        //Console.WriteLine("Account created.");
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = contactId.ToString();
                        pObject.ObjectName = "Contact";
                        pObject.Message = "Contact added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new contact, check mandatory fields", HttpStatusCode.InternalServerError, true);
                    }
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError, true);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
           
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alan365.crm.dynamics.com", userName = "alan@alan365.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

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
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    List<DYContact> listToReturn = new List<DYContact>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "fullname";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue + "%");
                    //filter email
                    //ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    //filterOwnRcd2.AttributeName = "lastname";
                    //filterOwnRcd2.Operator = ConditionOperator.Like;
                    //filterOwnRcd2.Values.Add("%" + SValue + "%");


                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    //filter1.Conditions.Add(filterOwnRcd2);
                    //filter1.FilterOperator = LogicalOperator.Or;
                    QueryExpression query = new QueryExpression("contact");
                    query.ColumnSet.AddColumns("contactid", "firstname", "lastname");
                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            DYContact info = new DYContact();
                            if (z.Attributes.Contains("contactid"))
                                info.contactid = z.Attributes["contactid"].ToString();
                            if (z.Attributes.Contains("firstname"))
                                info.firstname = z.Attributes["firstname"].ToString();
                            if (z.Attributes.Contains("lastname"))
                                info.lastname = z.Attributes["lastname"].ToString();
                            listToReturn.Add(info);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }

    }
}
