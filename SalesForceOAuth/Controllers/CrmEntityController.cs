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
using System.Threading.Tasks;
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
            
            //if(Convert.ToDateTime(user.OuthDetail.expires_on) < DateTime.Now)
            if (Convert.ToDateTime(user.OuthDetail.expires_on) < DateTime.Now)
            {
                user.IntegrationConstants = Repository.GetIntegrationConstants(crmEntity.ObjectRef, Request.RequestUri.Authority.ToString(), crmEntity.CrmType, AppType.Alive5);
                user.UrlReferrer = Request.RequestUri.Authority.ToString();
                user.ObjectRef = crmEntity.ObjectRef;
                user.GroupId = crmEntity.GroupId;
                user.CrmType = crmEntity.CrmType;
                user.OuthDetail = HubSpot.RefreshAuthorizationTokens(user);
                Repository.UpdateCrmCreditionals(user);
            }
            bool IsRecordAdded;
            var message = HubSpot.PostNewRecord(user, crmEntity, out IsRecordAdded);
            if (IsRecordAdded)
            {
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.Conflict, false);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntityRecord(string Token, int GroupId, string ObjectRef, string SVAlue, CrmType CrmType, string Entity, string callback)
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
            List<CrmEntity> retRecord = HubSpot.GetRecordList(user, SVAlue);
            return MyAppsDb.ConvertJSONPOutput(callback, retRecord, HttpStatusCode.OK, false);
        }

        //[HttpGet]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityRecord(string Token, int GroupId, string ObjectRef, string SVAlue, CrmType CrmType, string Entity)
        //{
        //    try
        //    {
        //        JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
        //    }
        //    //  Get current user log in detail
        //    CRMUser user = Repository.GetCrmCreditionalsDetail(ObjectRef, GroupId, Request.RequestUri.Authority.ToString(), CrmType);
        //    List<CrmEntity> retRecord = HubSpot.GetRecordList(user, SVAlue);
        //    return MyAppsDb.ConvertJSONPOutput("callback", retRecord, HttpStatusCode.OK, false);
        //    //return MyAppsDb.ConvertJSONOutput(retRecord, HttpStatusCode.OK, false);
        //}

        [HttpPost]
        public async Task<HttpResponseMessage> PostAddMessage(MessageDataCopy lData)
        {
            try
            {
                JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "CRM-IsAuthenticated", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            int chatId = 0;
            // EntitySettings entitySettings = Repository.GetDyEntitySettings(lData.ObjectRef, lData.GroupId, Request.RequestUri.Authority.ToString());

            // var getBackEndFeields = Repository.GetDYBackEndFields(lData.ObjectRef, lData.GroupId, Request.RequestUri.Authority.ToString(), ItemType);
            string ChatId, RowId;
            bool IsChatAdded;
            //long NoteId = 0;
            CRMUser user = Repository.GetCrmCreditionalsDetail(lData.ObjectRef, lData.GroupId, Request.RequestUri.Authority.ToString(), CrmType.HubSpot);
            bool flag = Repository.IsChatExist(lData.EntitytId, lData.EntitytType, lData.App, lData.ObjectRef, Request.RequestUri.Authority.ToString(), out ChatId, out RowId);
            if (flag)
            {
                //HubSpot.UpdateChats(user, lData.Message.Replace("|", "\r\n").Replace("&#39;", "'"), Convert.ToInt32(lData.EntitytId), ChatId, out IsChatAdded);
                HubSpot.UpdateChats(user, lData.Message.Replace("|", "</br>").Replace("&#39;", "'"), Convert.ToInt32(lData.EntitytId), ChatId, out IsChatAdded);
            }
            else
            {
                //HubSpot.PostChats(user, lData.Message.Replace("|", "\r\n").Replace("&#39;", "'"), Convert.ToInt32(lData.EntitytId), out IsChatAdded, out ChatId);
                HubSpot.PostChats(user, lData.Message.Replace("|", "</br>").Replace("&#39;", "'"), Convert.ToInt32(lData.EntitytId), out IsChatAdded, out ChatId);
            }
            if (IsChatAdded)
            {
                Repository.AddChatInfo(lData.ObjectRef, Request.RequestUri.Authority.ToString(), CrmType.HubSpot.ToString(), lData.EntitytId, lData.EntitytType, lData.App, ChatId);
                return MyAppsDb.ConvertJSONOutput("Chat Added Successfully", HttpStatusCode.OK, false);
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput("Unable to add Chat", HttpStatusCode.Conflict, false);
            }
        }

        [HttpPost]
        public HttpResponseMessage SugarNewEntityCreate(CrmEntity crmEntity)
        {
            return null;
        }
    }
}
