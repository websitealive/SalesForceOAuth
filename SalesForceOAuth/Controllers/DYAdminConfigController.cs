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
    public class DYAdminConfigController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetInstanceType(string token, string ObjectRef, int GroupId, string callback)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYAdminConfig-GetInstanceType", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
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
                if (authType != "")
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, authType, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, "Confirguration Error - Settings Doesn't exist!", HttpStatusCode.OK, true);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAdminConfig-GetInstanceType", "Unhandled exception", HttpStatusCode.InternalServerError);
            }

        }

        [HttpPost]
        public HttpResponseMessage PostCredentialsOnly(DynamicUser lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYAdminConfig-PostCredentialsOnly", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.RecordDynamicsCredentials(lData.ObjectRef, lData.GroupId, lData.OrganizationURL, lData.Username, lData.Password, lData.AuthType, urlReferrer);
                if (output == 1)
                    return MyAppsDb.ConvertJSONOutput("Credentials recorded successfully!", HttpStatusCode.OK, false);
                else
                    return MyAppsDb.ConvertJSONOutput("Credentials exists.", HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYAdminConfig-PostCredentialsOnly", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }

    }
}
