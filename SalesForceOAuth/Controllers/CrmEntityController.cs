using CRM.Dto;
using CRM.WebServices;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
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
    public class CrmEntityController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage GetMsDynamicsUser(string Token, int GroupId, string ObjectRef, string CrmUserId)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                var getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, "Account", urlReferrer);

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
                    ColumnSet entityColumn = new ColumnSet();
                    entityColumn.AddColumn("fullname");
                    Entity chk = proxyservice.Retrieve("systemuser", new Guid(CrmUserId), entityColumn);
                    return MyAppsDb.ConvertJSONOutput(chk.Attributes["fullname"], HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex.Message, HttpStatusCode.NotFound, false);
            }
        }

        [HttpPost]
        public HttpResponseMessage PostNewEntityRecord(CrmEntity crmEntity)
        {
            try
            {
                JWT.JsonWebToken.Decode(crmEntity.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //  Get current user log in detail
            CRMUser user = Repository.GetCrmCreditionalsDetail(crmEntity.ObjectRef, crmEntity.GroupId, Request.RequestUri.Authority.ToString(), crmEntity.CrmType);
            bool IsRecordAdded;
            var message = HubSpot.PostNewRecord(user, crmEntity, out IsRecordAdded);
            if(IsRecordAdded)
            {
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.Conflict, false);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEntityRecord(string Token, int GroupId, string ObjectRef, string SVAlue, CrmType CrmType, string Entity)
        {
            try
            {
                JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //  Get current user log in detail
            CRMUser user = Repository.GetCrmCreditionalsDetail(ObjectRef, GroupId, Request.RequestUri.Authority.ToString(), CrmType);
            CrmEntity retRecord = HubSpot.GetRecordByEmail(user, SVAlue);
            return MyAppsDb.ConvertJSONOutput(retRecord, HttpStatusCode.NotFound, false);
        }

        [HttpPost]
        public HttpResponseMessage SugarNewEntityCreate(CrmEntity crmEntity)
        {
            return null;
        }
    }
}
