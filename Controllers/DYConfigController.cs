using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesForceOAuth.Web_API_Helper_Code;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.ServiceModel.Description;

namespace SalesForceOAuth.Controllers
{
    public class DYConfigController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetConfigurationStatus(string token, string ObjectRef, int GroupId, string callback)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYConfig-GetConfigurationStatus", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
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
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {

                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //ConditionExpression password = new ConditionExpression();

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
                        string outStr = "Managed Solution Found - Configuration Complete";
                        return MyAppsDb.ConvertJSONPOutput(callback, outStr, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback, "Confirguration Error - Managed solution Not Found, check information", HttpStatusCode.OK, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYConfig-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.InternalServerError);
            }

        }

        [HttpPost]
        public HttpResponseMessage PostCredentials(DynamicUser lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostCredentials", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                Uri organizationUri;
                Uri homeRealmUri;
                ClientCredentials credentials = new ClientCredentials();
                ClientCredentials deviceCredentials = new ClientCredentials();
                credentials.UserName.UserName = lData.Username;
                credentials.UserName.Password = lData.Password;
                deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
                organizationUri = new Uri(lData.OrganizationURL + "/XRMServices/2011/Organization.svc");
                homeRealmUri = null;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {

                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //ConditionExpression password = new ConditionExpression();

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
                        string outStr = "Managed Solution Found - Configuration Complete";
                        int output = MyAppsDb.RecordDynamicsCredentials(lData.ObjectRef, lData.GroupId, lData.OrganizationURL, lData.Username, lData.Password, lData.AuthType, urlReferrer);
                        if (output == 1)
                            return MyAppsDb.ConvertJSONOutput("Managed Solution Found - Credentials recorded successfully!", HttpStatusCode.OK, false);
                        else
                            return MyAppsDb.ConvertJSONOutput("Credentials exists and working.", HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Confirguration Error - Managed solution Not Found, check information", HttpStatusCode.OK, true);
                    }
                }
                
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostLead", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}
