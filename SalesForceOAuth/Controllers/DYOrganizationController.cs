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
using SalesForceOAuth.Models;

namespace SalesForceOAuth.Controllers
{
    public class DYOrganizationController : ApiController
    {

        [HttpGet]
        [ActionName("IsConnected")]
        public HttpResponseMessage IsConnected(string callback, string token, string ObjectRef, int GroupId)
        {
            try
            {
                // Verify Token
                JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);

                string urlReferrer = Request.RequestUri.Authority.ToString();
                bool isAuthenticated;
                if (MyAppsDb.IsDynamicCredentialsExist(ObjectRef, GroupId, urlReferrer))
                {
                    isAuthenticated = true;
                }
                else
                {
                    isAuthenticated = false;
                }
                return MyAppsDb.ConvertJSONPOutput(callback, isAuthenticated, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DyOrganization", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public HttpResponseMessage DeleteOrginization(DYOrganizationDetail orgDetail)
        {
            try
            {
                // Verify Token
                JWT.JsonWebToken.Decode(orgDetail.Token, ConfigurationManager.AppSettings["APISecureKey"], true);

                string urlReferrer = Request.RequestUri.Authority.ToString();
                MyAppsDb.DeleteDynamicCredentials(orgDetail.ObjectRef, orgDetail.GroupId, urlReferrer);
                return MyAppsDb.ConvertJSONOutput("Successfully delete the organization", HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyOrg", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }
    }
}
