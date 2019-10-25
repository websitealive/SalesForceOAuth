using Microsoft.Crm.Sdk.Messages;
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
using SalesForceOAuth.ModelClasses;
using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Web.Http;
namespace SalesForceOAuth.Controllers
{
    public class DyEntityController : ApiController
    {
        private OrganizationServiceProxy proxyservice;

        #region Dynamics Added Entity

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityList(string Token, string ObjectRef, int GroupId, string callback)
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
                var entityList = Repository.GetEntityList(urlReferrer, ObjectRef, GroupId, "dy");
                var entityFields = Repository.GetDYFormExportFields(ObjectRef, GroupId, urlReferrer);
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
                if (CreateOneToManyRelationship(lData, out Message))
                {
                    string urlReferrer = Request.RequestUri.Authority.ToString();
                    lData.CrmType = "dy";
                    var messgae = Repository.AddEntity(lData, urlReferrer);
                    return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONPOutput("Error : ", Message, "DYEntity-PostEntity", "Unhandled exception", HttpStatusCode.Conflict);
                }
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

        #region Dynamics Added Entity Operations
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntityRecord(EntityModel lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyEntity-PostAccount", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                //Connect to SDK 
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

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
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    Entity registration = new Entity(lData.EntityUniqueName);
                    //registration[lData.PrimaryFieldUniqueName] = lData.PrimaryFieldValue;

                    #region Dynamic Inout Fields
                    if (lData.CustomFields != null)
                    {
                        foreach (CustomFieldModel inputField in lData.CustomFields)
                        {
                            if (inputField.Value != null)
                            {
                                if (inputField.FieldType == "textbox")
                                {
                                    registration[inputField.FieldName] = inputField.Value;
                                }
                                if (inputField.FieldType == "boolean")
                                {
                                    bool flag = true;
                                    if (inputField.Value == "1")
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                    registration[inputField.FieldName] = flag;
                                }
                                if (inputField.FieldType == "lookup")
                                {
                                    registration[inputField.FieldName] = new EntityReference(inputField.RelatedEntity, new Guid(inputField.Value));
                                }
                                if (inputField.FieldType == "datetime")
                                {
                                    registration[inputField.FieldName] = Convert.ToDateTime(inputField.Value);
                                }
                                if (inputField.FieldType == "currency")
                                {
                                    registration[inputField.FieldName] = new Money(Convert.ToDecimal(inputField.Value));
                                }
                            }

                        }
                    }

                    // New Functionality for Custom Fileds
                    var customFields = Repository.GetConstantInputFields(lData.ObjectRef, lData.GroupId, urlReferrer, lData.EntityUniqueName);
                    if (customFields != null)
                    {
                        foreach (FieldsModel inputField in customFields)
                        {
                            if (inputField.ValueDetail != null)
                            {
                                if (inputField.FieldType == "lookup")
                                {
                                    //lookup 
                                    registration[inputField.FieldName] = new EntityReference(inputField.LookupEntityName, new Guid(inputField.LookupEntityRecordId));
                                }
                                else if (inputField.FieldType == "datetime")
                                {
                                    //Date Time
                                    registration[inputField.FieldName] = Convert.ToDateTime(inputField.ValueDetail);
                                }
                                else if (inputField.FieldType == "currency")
                                {
                                    registration[inputField.FieldName] = new Money(Convert.ToDecimal(inputField.ValueDetail));
                                }
                                else
                                {
                                    registration[inputField.FieldName] = inputField.ValueDetail;
                                }
                            }

                        }
                    }

                    Guid accountId = objser.Create(registration);
                    if (accountId != Guid.Empty)
                    {
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = accountId.ToString();
                        pObject.ObjectName = lData.EntityUniqueName;
                        pObject.Message = lData.EntityUniqueName + " added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Record, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //End connect to SDK
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntityRelationShip(EntityModel lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-PostAccount", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                EntitySettings entitySettings = Repository.GetDyEntitySettings(lData.ObjectRef, lData.GroupId, urlReferrer);
                if (entitySettings.UseAliveChat == 1)
                {
                    Exception Message;
                    if (CreateOneToManyRelationship(lData, out Message))
                    {
                        return MyAppsDb.ConvertJSONOutput("The one-to-many relationship has been created", HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONPOutput("Error : ", Message, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
                    }
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("The one-to-many relationship is not created because Activity Task are used to save chats", HttpStatusCode.OK, false);
                }

            }

            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput("Error : ", ex, "DYEntity-PostEntityRelationShip", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedEntities(string token, string ObjectRef, int GroupId, string Entity, string SValue, string callback, bool IslookupSearch = false, string ExportFieldId = null)
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
                string ApplicationURL = "", userName = "", password = "", authType = "", searchEntity = "", lookupFieldLabel = "", lookupFieldName = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                List<CustomFieldModel> getSearchedFileds = new List<CustomFieldModel>();
                List<EntityColumn> getDetailFields = new List<EntityColumn>();
                FieldsModel getExportFieldForLookup = new FieldsModel();
                if (IslookupSearch)
                {
                    getExportFieldForLookup = Repository.GetDYExportFieldsForLookup(ObjectRef, ExportFieldId, urlReferrer);
                    searchEntity = getExportFieldForLookup.RelatedEntity;
                    lookupFieldLabel = getExportFieldForLookup.OptionalFieldsLabel;
                    lookupFieldName = getExportFieldForLookup.OptionalFieldsName;
                }
                else
                {
                    getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, Entity, urlReferrer);
                    getDetailFields = BusinessLogic.DynamicCommon.GetDynamicDetailFileds(ObjectRef, GroupId, Entity, urlReferrer);
                    searchEntity = Entity;
                }

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
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = searchEntity
                    };
                    RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveEntityRequest);
                    EntityMetadata RetrieveEntityInfo = retrieveEntityResponse.EntityMetadata;
                    

                    List<EntityModel> listToReturn = new List<EntityModel>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    // Start Buliding Querry
                    QueryExpression query = new QueryExpression(searchEntity);
                    Dictionary<string, string> sss = new Dictionary<string, string>();
                    //List<string> searchedColumn = new List<string>();
                    List<string> entityId = new List<string>();
                    FilterExpression filter = new FilterExpression();

                    getSearchedFileds.Add(new CustomFieldModel() { FieldType = "PrimaryIdAttribute", FieldName = RetrieveEntityInfo.PrimaryIdAttribute });
                    getSearchedFileds.Add(new CustomFieldModel() { FieldType = "PrimaryNameAttribute", FieldName = RetrieveEntityInfo.PrimaryNameAttribute });
                    if (IslookupSearch)
                    {
                        if(lookupFieldName != "")
                        {
                            getSearchedFileds.Add(new CustomFieldModel() { FieldType = "OptionalLookupField", FieldName = lookupFieldName });
                        }
                    }
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var field in getSearchedFileds)
                        {
                            ConditionExpression condition = new ConditionExpression();
                            if (IslookupSearch)
                            {
                                query.ColumnSet.AddColumn(field.FieldName);
                                if(field.FieldType != "PrimaryIdAttribute")
                                {
                                    var lookupEntityMetadata = (from e in RetrieveEntityInfo.Attributes where e.LogicalName == field.FieldName select e).FirstOrDefault();

                                    if(lookupEntityMetadata.AttributeType.ToString() == "DateTime")
                                    {
                                        DateTime result;
                                        if (DateTime.TryParse(SValue, out result))
                                        {
                                            condition.AttributeName = field.FieldName;
                                            condition.Operator = ConditionOperator.Like;
                                            condition.Values.Add(result);
                                            filter.Conditions.Add(condition);
                                        }
                                    }
                                    else if (lookupEntityMetadata.AttributeType.ToString() == "Money")
                                    {
                                        int result;
                                        bool parsedSuccessfully = int.TryParse(SValue, out result);
                                        if (int.TryParse(SValue, out result))
                                        {
                                            condition.AttributeName = field.FieldName;
                                            condition.Operator = ConditionOperator.Like;
                                            condition.Values.Add(Convert.ToDecimal(result));
                                        }
                                    }
                                    else if (lookupEntityMetadata.AttributeType.ToString() == "Lookup")
                                    {
                                        string loolupentityName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)lookupEntityMetadata).Targets[0].ToString();
                                        RetrieveEntityRequest retrieveParentEntityRequest = new RetrieveEntityRequest
                                        {
                                            EntityFilters = EntityFilters.Attributes,
                                            LogicalName = loolupentityName
                                        };
                                        RetrieveEntityResponse retrieveParentEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveParentEntityRequest);
                                        EntityMetadata RetrieveParentEntityInfo = retrieveParentEntityResponse.EntityMetadata;

                                        QueryExpression queryLookupEntity = new QueryExpression(loolupentityName);
                                        queryLookupEntity.ColumnSet.AddColumn(RetrieveParentEntityInfo.PrimaryNameAttribute);
                                        FilterExpression lookupEntityFilter = new FilterExpression();
                                        ConditionExpression lookupSearchField = new ConditionExpression()
                                        {
                                            AttributeName = RetrieveParentEntityInfo.PrimaryNameAttribute,
                                            Operator = ConditionOperator.Like,
                                            Values = { "%" + SValue.Trim() + "%" }
                                        };
                                        lookupEntityFilter.Conditions.Add(lookupSearchField);
                                        queryLookupEntity.Criteria.AddFilter(lookupEntityFilter);
                                        EntityCollection lookupResult = objser.RetrieveMultiple(queryLookupEntity);
                                        List<Guid> lookupIds = new List<Guid>();
                                        foreach (var item in lookupResult.Entities)
                                        {
                                            if (item.Attributes.Contains(RetrieveParentEntityInfo.PrimaryIdAttribute))
                                                lookupIds.Add(new Guid(item.Attributes[RetrieveParentEntityInfo.PrimaryIdAttribute].ToString()));
                                        }

                                        if (lookupIds.Count > 0)
                                        {
                                            List<ConditionExpression> lookupCondition = new List<ConditionExpression>();
                                            foreach (var item in lookupIds)
                                            {
                                                ConditionExpression c = new ConditionExpression();
                                                c.AttributeName = field.FieldName;
                                                c.Operator = ConditionOperator.Equal;
                                                c.Values.Add(item);
                                                lookupCondition.Add(c);
                                            }
                                            filter.Conditions.AddRange(lookupCondition);
                                        }
                                    }
                                    else
                                    {
                                        condition.AttributeName = field.FieldName;
                                        condition.Operator = ConditionOperator.Like;
                                        condition.Values.Add("%" + SValue.Trim() + "%");
                                        filter.Conditions.Add(condition);
                                    }
                                }
                            }
                            else
                            {
                                if (field.FieldType == "relatedEntity")
                                {
                                    QueryExpression queryRelatedEntity = new QueryExpression(field.RelatedEntity);
                                    queryRelatedEntity.ColumnSet.AddColumn(field.RelatedEntityFieldName);
                                    FilterExpression relatedEntityFilter = new FilterExpression();
                                    ConditionExpression relatedSearchField = new ConditionExpression()
                                    {
                                        AttributeName = field.FieldName,
                                        Operator = ConditionOperator.Like,
                                        Values = { "%" + SValue.Trim() + "%" }

                                    };
                                    relatedEntityFilter.Conditions.Add(relatedSearchField);
                                    queryRelatedEntity.Criteria.AddFilter(relatedEntityFilter);
                                    EntityCollection result = objser.RetrieveMultiple(queryRelatedEntity);
                                    if (result.Entities.Count > 0)
                                    {
                                        foreach (var item in result.Entities)
                                        {
                                            if (item.Attributes.Contains(field.RelatedEntityFieldName))
                                                entityId.Add(((Microsoft.Xrm.Sdk.EntityReference)item.Attributes[field.RelatedEntityFieldName]).Id.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    query.ColumnSet.AddColumn(field.FieldName);
                                    // if (!(field.FieldType == "lookup" || field.FieldType == "PrimaryIdAttribute"))
                                    if (!(field.FieldType == "PrimaryIdAttribute"))
                                    {
                                        condition.AttributeName = field.FieldName;
                                        if (field.FieldType == "lookup")
                                        {
                                            var lookupFieldMetadata = (from e in RetrieveEntityInfo.Attributes where e.LogicalName == field.FieldName select e).FirstOrDefault();
                                            string loolupentityName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)lookupFieldMetadata).Targets[0].ToString();
                                            RetrieveEntityRequest retrieveParentEntityRequest = new RetrieveEntityRequest
                                            {
                                                EntityFilters = EntityFilters.Attributes,
                                                LogicalName = loolupentityName
                                            };
                                            RetrieveEntityResponse retrieveParentEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveParentEntityRequest);
                                            EntityMetadata RetrieveParentEntityInfo = retrieveParentEntityResponse.EntityMetadata;

                                            QueryExpression queryLookupEntity = new QueryExpression(loolupentityName);
                                            queryLookupEntity.ColumnSet.AddColumn(RetrieveParentEntityInfo.PrimaryNameAttribute);
                                            FilterExpression lookupEntityFilter = new FilterExpression();
                                            ConditionExpression lookupSearchField = new ConditionExpression()
                                            {
                                                AttributeName = RetrieveParentEntityInfo.PrimaryNameAttribute,
                                                Operator = ConditionOperator.Like,
                                                Values = { "%" + SValue.Trim() + "%" }
                                            };
                                            lookupEntityFilter.Conditions.Add(lookupSearchField);
                                            queryLookupEntity.Criteria.AddFilter(lookupEntityFilter);
                                            EntityCollection lookupResult = objser.RetrieveMultiple(queryLookupEntity);
                                            List<Guid> lookupIds = new List<Guid>();
                                            foreach (var item in lookupResult.Entities)
                                            {
                                                if (item.Attributes.Contains(RetrieveParentEntityInfo.PrimaryIdAttribute))
                                                    lookupIds.Add(new Guid(item.Attributes[RetrieveParentEntityInfo.PrimaryIdAttribute].ToString()));
                                            }

                                            if(lookupIds.Count > 0)
                                            {
                                                List<ConditionExpression> lookupCondition = new List<ConditionExpression>();
                                                foreach (var item in lookupIds)
                                                {
                                                    ConditionExpression c = new ConditionExpression();
                                                    c.AttributeName = field.FieldName;
                                                    c.Operator = ConditionOperator.Equal;
                                                    c.Values.Add(item);
                                                    lookupCondition.Add(c);
                                                }
                                                filter.Conditions.AddRange(lookupCondition);
                                            }
                                        }
                                        else if (field.FieldType == "currency")
                                        {
                                            condition.Operator = ConditionOperator.Like;
                                            int result;
                                            bool parsedSuccessfully = int.TryParse(SValue, out result);
                                            if (int.TryParse(SValue, out result))
                                            {
                                                condition.Values.Add(Convert.ToDecimal(result));
                                                filter.Conditions.Add(condition);
                                            }
                                        }
                                        else if (field.FieldType == "datetime")
                                        {

                                            condition.Operator = ConditionOperator.Like;
                                            DateTime result;
                                            if (DateTime.TryParse(SValue, out result))
                                            {
                                                condition.Values.Add(result);
                                                filter.Conditions.Add(condition);
                                            }
                                        }
                                        else
                                        {

                                            condition.Operator = ConditionOperator.Like;
                                            condition.Values.Add("%" + SValue.Trim() + "%");
                                            filter.Conditions.Add(condition);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Add Detail Fileds TO search
                    if (getDetailFields.Count > 0)
                    {
                        foreach (var field in getDetailFields)
                        {
                            var flag = filter.Conditions.Where(x => x.AttributeName == field.FieldName).Select(s => s.AttributeName).FirstOrDefault();
                            if (flag == null)
                            {
                                ConditionExpression condition1 = new ConditionExpression();
                                
                                if (field.FieldType == "lookup")
                                {
                                    var defaultLookupFieldMetadata = (from e in RetrieveEntityInfo.Attributes where e.LogicalName == field.FieldName select e).FirstOrDefault();
                                    string defaultLoolupentityName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)defaultLookupFieldMetadata).Targets[0].ToString();
                                    RetrieveEntityRequest retrieveParentEntityRequest = new RetrieveEntityRequest
                                    {
                                        EntityFilters = EntityFilters.Attributes,
                                        LogicalName = defaultLoolupentityName
                                    };
                                    RetrieveEntityResponse retrieveParentEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveParentEntityRequest);
                                    EntityMetadata RetrieveParentEntityInfo = retrieveParentEntityResponse.EntityMetadata;

                                    QueryExpression queryLookupEntity = new QueryExpression(defaultLoolupentityName);
                                    queryLookupEntity.ColumnSet.AddColumn(RetrieveParentEntityInfo.PrimaryNameAttribute);
                                    FilterExpression lookupEntityFilter = new FilterExpression();
                                    ConditionExpression lookupSearchField = new ConditionExpression()
                                    {
                                        AttributeName = RetrieveParentEntityInfo.PrimaryNameAttribute,
                                        Operator = ConditionOperator.Like,
                                        Values = { "%" + SValue.Trim() + "%" }
                                    };
                                    lookupEntityFilter.Conditions.Add(lookupSearchField);
                                    queryLookupEntity.Criteria.AddFilter(lookupEntityFilter);
                                    EntityCollection lookupResult = objser.RetrieveMultiple(queryLookupEntity);
                                    List<Guid> lookupIds = new List<Guid>();
                                    foreach (var item in lookupResult.Entities)
                                    {
                                        if (item.Attributes.Contains(RetrieveParentEntityInfo.PrimaryIdAttribute))
                                            lookupIds.Add(new Guid(item.Attributes[RetrieveParentEntityInfo.PrimaryIdAttribute].ToString()));
                                    }

                                    if (lookupIds.Count > 0)
                                    {
                                        List<ConditionExpression> lookupCondition = new List<ConditionExpression>();
                                        foreach (var item in lookupIds)
                                        {
                                            ConditionExpression c = new ConditionExpression();
                                            c.AttributeName = field.FieldName;
                                            c.Operator = ConditionOperator.Equal;
                                            c.Values.Add(item);
                                            lookupCondition.Add(c);
                                        }
                                        filter.Conditions.AddRange(lookupCondition);
                                    }
                                }
                                else if (field.FieldType == "currency")
                                {
                                    condition1.AttributeName = field.FieldName;
                                    condition1.Operator = ConditionOperator.Like;
                                    int result;
                                    bool parsedSuccessfully = int.TryParse(SValue, out result);
                                    if (int.TryParse(SValue, out result))
                                    {
                                        condition1.Values.Add(result);
                                        filter.Conditions.Add(condition1);
                                    }
                                }
                                else if (field.FieldType == "datetime")
                                {
                                    condition1.AttributeName = field.FieldName;
                                    condition1.Operator = ConditionOperator.Like;
                                    DateTime result;
                                    if (DateTime.TryParse(SValue, out result))
                                    {
                                        condition1.Values.Add(result);
                                        filter.Conditions.Add(condition1);
                                    }
                                }
                                else
                                {
                                    condition1.AttributeName = field.FieldName;
                                    condition1.Operator = ConditionOperator.Like;
                                    condition1.Values.Add("%" + SValue.Trim() + "%");
                                    filter.Conditions.Add(condition1);
                                }
                            }
                        }
                    }
                    filter.FilterOperator = LogicalOperator.Or;
                    query.Criteria.AddFilter(filter);
                    EntityCollection result1 = objser.RetrieveMultiple(query);

                    foreach (var item in entityId)
                    {
                        if (result1.Entities.Count > 0)
                        {
                            if (result1.Entities.Where(c => c.Attributes[RetrieveEntityInfo.PrimaryIdAttribute].ToString() == item).FirstOrDefault() == null)
                            {
                                Entity result2 = objser.Retrieve(searchEntity, new Guid(item), query.ColumnSet);
                                result1.Entities.Add(result2);
                            }
                        }
                        else
                        {
                            Entity result2 = objser.Retrieve(searchEntity, new Guid(item), query.ColumnSet);
                            result1.Entities.Add(result2);
                        }
                    }

                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            EntityModel info = new EntityModel();
                            info.EntityUniqueName = Entity;
                            if (z.Attributes.Contains(RetrieveEntityInfo.PrimaryIdAttribute))
                                info.EntityPrimaryKey = z.Attributes[RetrieveEntityInfo.PrimaryIdAttribute].ToString();
                            if (z.Attributes.Contains(RetrieveEntityInfo.PrimaryNameAttribute))
                                info.PrimaryFieldValue = z.Attributes[RetrieveEntityInfo.PrimaryNameAttribute].ToString();
                            List<CustomFieldModel> retSearchFields = new List<CustomFieldModel>();
                            if (IslookupSearch)
                            {
                                info.OptionalFieldDisplayName = lookupFieldLabel;
                                if (z.Attributes.Contains(lookupFieldName)){
                                    if (z.Attributes[lookupFieldName].ToString() == "Microsoft.Xrm.Sdk.EntityReference")
                                    {
                                        info.OptionalFieldValue = ((Microsoft.Xrm.Sdk.EntityReference)z.Attributes[lookupFieldName]).Name.ToString();
                                    }
                                    else if (z.Attributes[lookupFieldName].ToString() == "Microsoft.Xrm.Sdk.Money")
                                    {
                                        info.OptionalFieldValue = ((Microsoft.Xrm.Sdk.Money)z.Attributes[lookupFieldName]).Value.ToString();
                                    }
                                    else if (z.Attributes[lookupFieldName].GetType().Name == "DateTime")
                                    {
                                        DateTime date = ((System.DateTime)z.Attributes[lookupFieldName]);
                                        info.OptionalFieldValue = date.Month + "/" + date.Day + "/" + date.Year;
                                    }
                                    else
                                    {
                                        info.OptionalFieldValue = z.Attributes[lookupFieldName].ToString();
                                    }

                                }
                                else{
                                    info.OptionalFieldValue = string.Empty;
                                }
                                
                                listToReturn.Add(info);
                            }
                            else
                            {
                                if (getSearchedFileds.Count > 0)
                                {
                                    foreach (var field in getSearchedFileds)
                                    {
                                        if ((field.FieldType != "relatedEntity") && (field.FieldType == "textbox" || field.FieldType == "boolean" || field.FieldType == "lookup" || field.FieldType == "currency" || field.FieldType == "datetime"))
                                        // if ((field.FieldType != "relatedEntity"))
                                        {
                                            if (z.Attributes.Contains(field.FieldName))
                                            {
                                                CustomFieldModel Fields = new CustomFieldModel();
                                                Fields.FieldLabel = field.FieldLabel;
                                                if (z.Attributes[field.FieldName].ToString() == "Microsoft.Xrm.Sdk.EntityReference")
                                                {
                                                    Fields.Value = ((Microsoft.Xrm.Sdk.EntityReference)z.Attributes[field.FieldName]).Name.ToString();

                                                }
                                                else if (z.Attributes[field.FieldName].ToString() == "Microsoft.Xrm.Sdk.Money")
                                                {
                                                    Fields.Value = ((Microsoft.Xrm.Sdk.Money)z.Attributes[field.FieldName]).Value.ToString();
                                                }
                                                else if (field.FieldType == "datetime")
                                                {
                                                    DateTime date = ((System.DateTime)z.Attributes[field.FieldName]);
                                                    Fields.Value = date.Month + "/" + date.Day + "/" + date.Year;
                                                }
                                                else
                                                {
                                                    Fields.Value = z.Attributes[field.FieldName].ToString();
                                                }
                                                retSearchFields.Add(Fields);
                                            }
                                        }
                                    }

                                }
                                info.CustomFields = retSearchFields;
                                // End Custom Search Filed
                                listToReturn.Add(info);
                            }
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYEntity-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        #endregion

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

        #region Entity Settings
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

        #endregion
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetDefaultFieldSettings(string Token, string ObjectRef, int GroupId, string callback, int Id = 0)
        {
            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                if (Id == 0)
                {
                    var entitySettings = Repository.GetDYDefaultFieldSettings(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
                else
                {
                    var entitySettings = Repository.GetDYDefaultFieldSettingsById(ObjectRef, Id, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYEntity-GetDefaultFieldSettings", "Exception", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostDefaultFieldSettings(DefaultFieldSettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.AddDYDefaultFieldSettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyEntity-PostDefaultFieldSettings", "Exception!", HttpStatusCode.InternalServerError);
            }

        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateDefaultFieldSettings(DefaultFieldSettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateDYDefaultFieldSettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyENtity-UpdateDefaultFieldSettings", "Exception!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteDefaultFieldSettings(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.DeleteDYDefaultFieldSettings(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYEntity-DeleteDefaultFieldSettings", "Exception", HttpStatusCode.InternalServerError);
            }
        }

        #region Default Field Settings

        #endregion

        #region Common Functions
        public bool CreateOneToManyRelationship(EntityModel lData, out Exception Message)
        {
            try
            {
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

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
                using (proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {

                    //Retrieve the One-to-many relationship using the MetadataId.

                    //// This statement is required to enable early-bound type support.
                    proxyservice.EnableProxyTypes();


                    // bool eligibleCreateOneToManyRelationship = EligibleCreateOneToManyRelationship(lData.EntityUniqueName, "ayu_chat");

                    bool isRelationShipExist = IsRelationShipExist("ayu_" + lData.EntityUniqueName + "_ayu_chat");
                    if (!isRelationShipExist)
                    {
                        CreateOneToManyRequest createOneToManyRelationshipRequest =
                            new CreateOneToManyRequest
                            {
                                OneToManyRelationship =
                            new OneToManyRelationshipMetadata
                            {
                                ReferencedEntity = lData.EntityUniqueName,
                                ReferencingEntity = "ayu_chat",
                                SchemaName = "ayu_" + lData.EntityUniqueName + "_ayu_chat",
                                AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                                {
                                    Behavior = AssociatedMenuBehavior.UseLabel,
                                    Group = AssociatedMenuGroup.Details,
                                    Label = new Label(lData.EntityDispalyName, 1033),
                                    Order = 10000
                                },
                                CascadeConfiguration = new CascadeConfiguration
                                {
                                    Assign = CascadeType.NoCascade,
                                    Delete = CascadeType.RemoveLink,
                                    Merge = CascadeType.NoCascade,
                                    Reparent = CascadeType.NoCascade,
                                    Share = CascadeType.NoCascade,
                                    Unshare = CascadeType.NoCascade
                                }
                            },
                                Lookup = new LookupAttributeMetadata
                                {
                                    SchemaName = "ayu_" + lData.EntityUniqueName + "id",
                                    DisplayName = new Label(lData.EntityDispalyName, 1033),
                                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                                    Description = new Label(lData.EntityUniqueName + "Lookup", 1033)
                                }
                            };


                        CreateOneToManyResponse createOneToManyRelationshipResponse =
                            (CreateOneToManyResponse)proxyservice.Execute(
                            createOneToManyRelationshipRequest);

                        var _oneToManyRelationshipId =
                            createOneToManyRelationshipResponse.RelationshipId;

                        // Publish the customization changes.
                        proxyservice.Execute(new PublishAllXmlRequest());
                    }
                }
                Message = null;
                return true;
            }
            catch (Exception ex)
            {
                Message = ex;
                return false;
            }
        }

        public bool EligibleCreateOneToManyRelationship(string referencedEntity,
            string referencingEntity)
        {
            //Checks whether the specified entity can be the primary entity in one-to-many
            //relationship.
            CanBeReferencedRequest canBeReferencedRequest = new CanBeReferencedRequest
            {
                EntityName = referencedEntity
            };

            CanBeReferencedResponse canBeReferencedResponse =
                (CanBeReferencedResponse)proxyservice.Execute(canBeReferencedRequest);

            if (!canBeReferencedResponse.CanBeReferenced)
            {
                Console.WriteLine(
                    "Entity {0} can't be the primary entity in this one-to-many relationship",
                    referencedEntity);
            }

            //Checks whether the specified entity can be the referencing entity in one-to-many
            //relationship.
            CanBeReferencingRequest canBereferencingRequest = new CanBeReferencingRequest
            {
                EntityName = referencingEntity
            };

            CanBeReferencingResponse canBeReferencingResponse =
                (CanBeReferencingResponse)proxyservice.Execute(canBereferencingRequest);

            if (!canBeReferencingResponse.CanBeReferencing)
            {
                Console.WriteLine(
                    "Entity {0} can't be the referencing entity in this one-to-many relationship",
                    referencingEntity);
            }


            if (canBeReferencedResponse.CanBeReferenced == true
                && canBeReferencingResponse.CanBeReferencing == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsRelationShipExist(string RelationShipName)
        {
            try
            {
                RetrieveRelationshipRequest retrieveOneToManyRequest = new RetrieveRelationshipRequest { Name = RelationShipName };
                RetrieveRelationshipResponse retrieveOneToManyResponse = (RetrieveRelationshipResponse)proxyservice.Execute(retrieveOneToManyRequest);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
