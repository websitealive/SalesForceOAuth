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
        public HttpResponseMessage PostEntityRecord(CrmEntity crmEntity)
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
            int? recordPrimaryId;
            var message = HubSpot.PostNewRecord(user, crmEntity, out IsRecordAdded, out recordPrimaryId);
            if (IsRecordAdded)
            {
                return MyAppsDb.ConvertJSONOutput(new CrmEntity() { EntityId = recordPrimaryId.ToString(), Message = message }, HttpStatusCode.OK, false);
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
            if (Convert.ToDateTime(user.OuthDetail.expires_on) < DateTime.Now)
            {
                user.IntegrationConstants = Repository.GetIntegrationConstants(ObjectRef, Request.RequestUri.Authority.ToString(), CrmType, AppType.Alive5);
                user.UrlReferrer = Request.RequestUri.Authority.ToString();
                user.ObjectRef = ObjectRef;
                user.GroupId = GroupId;
                user.CrmType = CrmType;
                user.OuthDetail = HubSpot.RefreshAuthorizationTokens(user);
                Repository.UpdateCrmCreditionals(user);
            }
            var entityInfo = Repository.GetEntity(Request.RequestUri.Authority.ToString(), ObjectRef, GroupId, Entity, CrmType.ToString());
            CrmEntity retRecord = HubSpot.GetRecordList(user, entityInfo, SVAlue);
            return MyAppsDb.ConvertJSONPOutput(callback, retRecord, HttpStatusCode.OK, false);
        }

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
            CRMUser user = Repository.GetCrmCreditionalsDetail(lData.ObjectRef, lData.GroupId, Request.RequestUri.Authority.ToString(), CrmType.HubSpot);
            if (Convert.ToDateTime(user.OuthDetail.expires_on) < DateTime.Now)
            {
                user.IntegrationConstants = Repository.GetIntegrationConstants(lData.ObjectRef, Request.RequestUri.Authority.ToString(), CrmType.HubSpot, AppType.Alive5);
                user.UrlReferrer = Request.RequestUri.Authority.ToString();
                user.ObjectRef = lData.ObjectRef;
                user.GroupId = lData.GroupId;
                user.CrmType = CrmType.HubSpot;
                user.OuthDetail = HubSpot.RefreshAuthorizationTokens(user);
                Repository.UpdateCrmCreditionals(user);
            }
            bool flag = Repository.IsChatExist(lData.EntitytId, lData.EntitytType, lData.App, lData.ObjectRef, Request.RequestUri.Authority.ToString(), out ChatId, out RowId);
            if (flag)
            {
                HubSpot.UpdateChats(user, lData.Message.Replace("|", "</br>").Replace("&#39;", "'"), Convert.ToInt32(lData.EntitytId), ChatId, out IsChatAdded);
            }
            else
            {
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

        #region Dynamics Added Entities

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityList(string Token, string ObjectRef, int GroupId, CrmType Crmtype, string callback)
        {

            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                var entityList = Repository.GetEntityList(urlReferrer, ObjectRef, GroupId, Crmtype.ToString());
                var entityFields = Repository.GetFormCustomFields(ObjectRef, GroupId, urlReferrer);
                foreach (var entity in entityList)
                {
                    foreach (var fields in entityFields)
                    {
                        if (entity.EntityUniqueName.ToLower() == fields.Entity.ToLower())
                        {
                            entity.CustomFields = fields.CustomFieldsList;
                        }
                    }

                }
                return MyAppsDb.ConvertJSONPOutput(callback, entityList, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityById(string Token, string ObjectRef, int EntityId, string callback)
        {

            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                var entitySettings = Repository.GetEntityById(urlReferrer, ObjectRef, EntityId);
                return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntity(EntityModel lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                Exception Message;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                lData.CrmType = CrmType.HubSpot.ToString();
                var messgae = Repository.AddEntity(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyEntity-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateEntity(EntityModel lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateEntity(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyEntity-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteEntity(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.DeleteEntity(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Delete Entity", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Customs Fields
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetExportFields(string Token, string ObjectRef, int GroupId, string callback, bool IsEntityForm = false)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                if (!IsEntityForm)
                {
                    var FieldsList = Repository.GetCustomFields(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }
                else
                {
                    var FieldsList = Repository.GetDYFormExportFields(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetExportFields", "Message", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetExportFieldByID(string Token, string ObjectRef, int FieldId, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Field By Id", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var FieldsList = Repository.GetDYExportFieldsById(FieldId, ObjectRef, urlReferrer);
                return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetExportFields", "Message", HttpStatusCode.InternalServerError);
            }
        }


        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostExportFields(FieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.AddCustomFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateExportFields(FieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.UpdateCustomFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }


        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteExportFields(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ErrorMessage;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MessageResponce retMessage = new MessageResponce();
                retMessage.Success = Repository.DeleteCustomFields(Id, ObjectRef, urlReferrer, out ErrorMessage);
                retMessage.Error = ErrorMessage;
                return MyAppsDb.ConvertJSONOutput(retMessage, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DY Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}
