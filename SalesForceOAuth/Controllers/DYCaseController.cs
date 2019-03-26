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
    public class DYCaseController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostCase(Case lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-PostCase", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
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
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    //var test = objser.Retrieve("contact", new Guid(lData.AccountId), new ColumnSet(true));


                    Entity registration = new Entity("incident");
                    registration["title"] = lData.Title;

                    registration["customerid"] = new EntityReference(lData.Customer.Entity, new Guid(lData.Customer.Id));


                    #region Dynamic Inout Fields
                    if (lData.CustomFields != null)
                    {
                        foreach (InputFields inputField in lData.CustomFields)
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
                            }

                        }
                    }
                    #endregion
                    // New Functionality for Custom Fileds
                    var customFields = Repository.GetConstantInputFields(lData.ObjectRef, lData.GroupId, urlReferrer, EntityName.Case);
                    if (customFields != null)
                    {
                        foreach (InputFields inputField in customFields)
                        {
                            if (inputField.Value != null)
                            {
                                registration[inputField.FieldName] = inputField.Value;
                            }

                        }
                    }



                    Guid caseId = objser.Create(registration);
                    if (caseId != Guid.Empty)
                    {
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = caseId.ToString();
                        pObject.ObjectName = "incident";
                        pObject.Message = "Case added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Case, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYCase-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //End connect to SDK
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedCases(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYCase-GetSearchedCase", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                var getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, "Incident", urlReferrer);

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
                    List<Case> listToReturn = new List<Case>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    List<string> opportunityId = new List<string>();
                    // Start Related Entity

                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var csA in getSearchedFileds)
                        {
                            if (csA.FieldType == "relatedEntity")
                            {
                                QueryExpression queryRelatedEntity = new QueryExpression(csA.RelatedEntity);
                                queryRelatedEntity.ColumnSet.AddColumn(csA.RelatedEntityFieldName);
                                FilterExpression relatedEntityFilter = new FilterExpression();
                                ConditionExpression relatedSearchField = new ConditionExpression()
                                {
                                    AttributeName = csA.FieldName,
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
                                        if (item.Attributes.Contains(csA.RelatedEntityFieldName))
                                            opportunityId.Add(((Microsoft.Xrm.Sdk.EntityReference)item.Attributes[csA.RelatedEntityFieldName]).Id.ToString());
                                    }
                                }

                            }
                        }
                    }
                    // End Related Entity

                    //filter name
                    QueryExpression query = new QueryExpression("incident");

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { "incidentid", "title", "customerid" });
                    foreach (var item in defaultSearchedColumn)
                    {
                        query.ColumnSet.AddColumn(item);
                    }
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var field in getSearchedFileds)
                        {
                            if (field.FieldType != "relatedEntity")
                            {
                                query.ColumnSet.AddColumn(field.FieldName);
                            }
                        }

                    }

                    FilterExpression filter1 = new FilterExpression();
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "title";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue.Trim() + "%");

                    filter1.Conditions.Add(filterOwnRcd);

                    //Add Custom Search Filters
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var csA in getSearchedFileds)
                        {
                            if (csA.FieldType == "textbox" || csA.FieldType == "boolean")
                            {
                                ConditionExpression filterOwnRcd4 = new ConditionExpression();
                                filterOwnRcd4.AttributeName = csA.FieldName;
                                filterOwnRcd4.Operator = ConditionOperator.Like;
                                filterOwnRcd4.Values.Add("%" + SValue.Trim() + "%");
                                filter1.Conditions.Add(filterOwnRcd4);
                            }
                        }
                    }
                    filter1.FilterOperator = LogicalOperator.Or;

                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);

                    foreach (var item in opportunityId)
                    {
                        if (result1.Entities.Count > 0)
                        {
                            if (result1.Entities.Where(c => c.Attributes["incidentid"].ToString() == item).FirstOrDefault() == null)
                            {
                                Entity result2 = objser.Retrieve("title", new Guid(item), query.ColumnSet);
                                result1.Entities.Add(result2);
                            }
                        }
                        else
                        {
                            Entity result2 = objser.Retrieve("title", new Guid(item), query.ColumnSet);
                            result1.Entities.Add(result2);
                        }
                    }

                    if (result1.Entities.Count > 0)
                    {
                        foreach (var z in result1.Entities)
                        {
                            Case info = new Case();
                            if (z.Attributes.Contains("incidentid"))
                                info.Id = z.Attributes["incidentid"].ToString();

                            if (z.Attributes.Contains("title"))
                                info.Title = z.Attributes["title"].ToString();

                            if (z.Attributes.Contains("customerid"))
                                info.CustomerName = ((Microsoft.Xrm.Sdk.EntityReference)z.Attributes["customerid"]).Name.ToString();

                            // Start Custom Search Filed
                            List<InputFields> retSearchFields = new List<InputFields>();
                            if (getSearchedFileds.Count > 0)
                            {

                                foreach (var field in getSearchedFileds)
                                {
                                    if (field.FieldType != "relatedEntity")
                                    {
                                        if (z.Attributes.Contains(field.FieldName))
                                        {
                                            InputFields Fields = new InputFields();
                                            Fields.FieldLabel = field.FieldLabel;
                                            if (z.Attributes[field.FieldName].ToString() != "Microsoft.Xrm.Sdk.EntityReference")
                                            {
                                                Fields.Value = z.Attributes[field.FieldName].ToString();
                                            }
                                            else
                                            {
                                                Fields.Value = ((Microsoft.Xrm.Sdk.EntityReference)z.Attributes[field.FieldName]).Name.ToString();
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
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYCase-GetSearchedCase", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

    }
}