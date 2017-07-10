using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesForceOAuth.Web_API_Helper_Code; 

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
                //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                if (crmSvc != null && crmSvc.IsReady)
                {
                    string outStr = "Configuration Complete";
                    return MyAppsDb.ConvertJSONPOutput(callback, outStr, HttpStatusCode.OK,false);
                }
                else
                {
                     return MyAppsDb.ConvertJSONPOutput(callback, "Confirguration Error, check information", HttpStatusCode.OK,true);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYConfig-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.InternalServerError);
            }

        }

    }
}
