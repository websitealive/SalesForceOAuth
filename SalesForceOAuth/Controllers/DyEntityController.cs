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
    public class DyEntityController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> IsEntityRecordExist(string token, string ObjectRef, int GroupId, string Entity, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
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
                    RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = Entity
                    };
                    RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveEntityRequest);
                    EntityMetadata RetrieveEntityInfo = retrieveEntityResponse.EntityMetadata;

                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    if (Entity == "account")
                    {
                        filterOwnRcd.AttributeName = "accountnumber";
                    }
                    else
                    {
                        filterOwnRcd.AttributeName = "emailaddress1";
                    }
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue.Trim() + "%");

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    //Add Custom Search Filters

                    filter1.FilterOperator = LogicalOperator.Or;
                    QueryExpression query = new QueryExpression(Entity);

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { RetrieveEntityInfo.PrimaryIdAttribute, RetrieveEntityInfo.PrimaryNameAttribute });
                    foreach (var item in defaultSearchedColumn)
                    {
                        query.ColumnSet.AddColumn(item);
                    }

                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback, "true", HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback, "false", HttpStatusCode.OK, false);
                    }

                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedEntities(string token, string ObjectRef, int GroupId, string Entity, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
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
                    RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = Entity
                    };
                    RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveEntityRequest);
                    EntityMetadata RetrieveEntityInfo = retrieveEntityResponse.EntityMetadata;


                    List<EntityModel> listToReturn = new List<EntityModel>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = RetrieveEntityInfo.PrimaryNameAttribute;
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue.Trim() + "%");

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    //Add Custom Search Filters

                    filter1.FilterOperator = LogicalOperator.Or;
                    QueryExpression query = new QueryExpression(Entity);

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { RetrieveEntityInfo.PrimaryIdAttribute, RetrieveEntityInfo.PrimaryNameAttribute });
                    foreach (var item in defaultSearchedColumn)
                    {
                        query.ColumnSet.AddColumn(item);
                    }

                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            EntityModel info = new EntityModel();
                            if (z.Attributes.Contains(RetrieveEntityInfo.PrimaryIdAttribute))
                                info.EntityPrimaryKey = z.Attributes[RetrieveEntityInfo.PrimaryIdAttribute].ToString();
                            if (z.Attributes.Contains(RetrieveEntityInfo.PrimaryNameAttribute))
                                info.EntityPrimaryName = z.Attributes[RetrieveEntityInfo.PrimaryNameAttribute].ToString();

                            listToReturn.Add(info);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntitySettings(string Token, string ObjectRef, int GroupId, string callback, int Id = 0)
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
                if (Id == 0)
                {
                    var entitySettings = Repository.GetDyEntitySettings(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
                else
                {
                    var entitySettings = Repository.GetDyEntitySettingsById(ObjectRef, Id, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntitySettings(EntitySettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.AddDyEntitySettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateEntitySettings(EntitySettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-UpdateEntitySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateDyEntitySettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteEntitySettings(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Detail Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.DeleteDyEntitySettings(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
                //
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DY Detail Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

    }
}
