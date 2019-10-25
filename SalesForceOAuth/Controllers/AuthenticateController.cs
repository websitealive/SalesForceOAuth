using CRM.Dto;
using CRM.WebServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SugarUser
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class AuthenticateController : ApiController
    {
        /// <summary>
        /// GET: api/Authenticate/GetRedirectURL
        [HttpGet]
        public HttpResponseMessage GetRedirectedUrl(string Token, string ObjectRef, CrmType CrmType, AppType AppType)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Get-Redirected-URL", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            string urlReferrer = Request.RequestUri.Authority.ToString();
            IntegrationConstants integrationConstants = Repository.GetIntegrationConstants(ObjectRef, urlReferrer, CrmType, AppType);
            string url = string.Empty;
            if (urlReferrer.Contains("localhost"))
            {
                url = integrationConstants.AuthorizationUrl + "?client_id=" + integrationConstants.ClientId + "&scope=contacts%20automation&redirect_uri=" + integrationConstants.RedirectedUrl;
            }
            else
            {
                url = integrationConstants.AuthorizationUrl + "?client_id=" + integrationConstants.ClientId + "&scope=contacts%20automation&redirect_uri=https://app-stage.alive5.com/oauth-hubspot";
            }
            return MyAppsDb.ConvertJSONOutput(url, HttpStatusCode.OK, false);
        }

        /// <summary>
        /// GET: api/Authenticate/GetAuthorizationToken
        [HttpGet]
        public HttpResponseMessage GetAuthorizationToken(string Token, string ObjectRef, CrmType crmType, AppType AppType, string AuthCode)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Get-Authorization-Token", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            string urlReferrer = Request.RequestUri.Authority.ToString();
            CRMUser user = new CRMUser()
            {
                ObjectRef = ObjectRef,
                CrmType = crmType,
                AuthCode = AuthCode
            };
            user.IntegrationConstants = Repository.GetIntegrationConstants(ObjectRef, urlReferrer, crmType, AppType);
            // hub spot crm api
            OuthDetail outhDetails = HubSpot.GetAuthorizationTokens(user);
            
            if (outhDetails.Is_Authenticated)
            {
                user.UrlReferrer = urlReferrer;
                user.OuthDetail = outhDetails;
                user.ApiUrl = user.IntegrationConstants.ApiUrl;
                Repository.AddCrmCreditionals(user);
                return MyAppsDb.ConvertJSONOutput("Successfully Saved Access Tokens", HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput(outhDetails.error_message, HttpStatusCode.Conflict, false);
            }
        }

        /// <summary>
        /// GET: api/Authenticate/IsCRMAuthenticated
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="ObjectRef"></param>
        /// <param name="GroupId"></param>
        /// <param name="CrmType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage IsCRMAuthenticated(string Token, string ObjectRef, int GroupId, CrmType CrmType)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            if (Repository.IsCrmAuthenticated(ObjectRef, GroupId, Request.RequestUri.Authority.ToString(), CrmType))
            {
                return MyAppsDb.ConvertJSONOutput(true, HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput(false, HttpStatusCode.OK, false);
            }

        }

        [HttpPost]
        public HttpResponseMessage CRM(CRMUser user)
        {
            try
            {
                JWT.JsonWebToken.Decode(user.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-Authenticate", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            OuthDetail outhDetails = Sugar.Authenticate(user);
            if (outhDetails.Is_Authenticated)
            {
                user.UrlReferrer = Request.RequestUri.Authority.ToString();
                user.OuthDetail = outhDetails;
                Repository.AddCrmCreditionals(user);
                return MyAppsDb.ConvertJSONOutput("Successfully Authenticated & Recorded Credentials!", HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput(outhDetails.error_message, HttpStatusCode.Conflict, false);
            }
        }

        [HttpDelete]
        public HttpResponseMessage RemoveCRMAuthentication(string Token, string ObjectRef, int GroupId, CrmType CrmType)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-RemoveAuthentication", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                Repository.RemoveCrmAuthentication(ObjectRef, GroupId, Request.RequestUri.Authority.ToString(), CrmType);
                return MyAppsDb.ConvertJSONOutput("Successfully Remove CRM Authentication", HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput("Unable to Remove CRM Authentication Plz try Again", HttpStatusCode.Conflict, false);
            }

        }

    }
}
