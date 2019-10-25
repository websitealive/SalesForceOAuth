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
using System.ServiceModel.Description;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYContactController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact(DYContactPostData lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-PostContact", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            try
            {
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alive5.crm11.dynamics.com", userName = "alive5@alive5.onmicrosoft.com",
                //password = "Passw0rd1", authType = "Office365";
                //string ApplicationURL = "https://websitealive.crmgate.pk/websitealive", userName = "naveed@crmgate.local",
                //    password = "@Abc.123", authType = "IFD";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                //string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                //connectionString = "url=https://msdynamics.websitealive.com/MSDynamics;username=wsa\\administrator;password=bX9bTkYv)Td;Domain=wsa;authtype=IFD;RequireNewInstance=true;"; 

                //connectionString += "RequireNewInstance=true;";


                //new code start

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
                    Entity registration = new Entity("contact");
                    registration["firstname"] = lData.FirstName;
                    registration["lastname"] = lData.LastName;
                    registration["emailaddress1"] = lData.Email;
                    registration["telephone1"] = lData.Phone;

                    #region Dynamic Inout Fields
                    if (lData.InputFields != null)
                    {
                        foreach (CustomFieldModel inputField in lData.InputFields)
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
                    #endregion

                    // New Functionality for Custom Fileds
                    var customFields = Repository.GetConstantInputFields(lData.ObjectRef, lData.GroupId, urlReferrer, EntityName.Contact.ToString());
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
                                    registration[inputField.FieldName] = Convert.ToDateTime(inputField.LookupEntityName);
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

                    // Custom Fields Region will be remove as we handle these on server site. i.e these fields are not posted form client sides.
                    #region custom fields 
                    if (lData.CustomFields != null)
                    {
                        foreach (CustomObject c in lData.CustomFields)
                        {
                            if (c.type != null)
                            {
                                CrmFieldType type;
                                switch (c.type.ToLower())
                                {
                                    //case "string":
                                    //    {
                                    //        type = CrmFieldType.String;break;
                                    //    }
                                    case "optionset":
                                        {
                                            type = CrmFieldType.CrmDecimal;
                                            int option = Convert.ToInt32(c.value);
                                            registration[c.field] = new OptionSetValue(option);
                                            break;
                                        }
                                    case "lookup":
                                        {
                                            type = CrmFieldType.Lookup;
                                            if (c.value.ToString().Length > 0)
                                            {
                                                registration[c.field] = new EntityReference("User", new Guid(c.value));//inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));// registration[c.field] = new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table).Value;
                                            }
                                            break;
                                        }
                                    //case "bool":
                                    //    { type = CrmFieldType.CrmBoolean; break; }
                                    default:
                                        {
                                            type = CrmFieldType.String;
                                            registration[c.field] = c.value;
                                            break;

                                        }
                                }
                            }

                        }
                    }
                    #endregion Custom fields 

                    Guid contactId = objser.Create(registration);
                    if (contactId != Guid.Empty)
                    {
                        //Console.WriteLine("Account created.");
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = contactId.ToString();
                        pObject.ObjectName = "Contact";
                        pObject.Message = "Contact added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new contact, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }

                //end code start


                //userName = "wsa\administrator";
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CrmServiceClient crmSvc = new CrmServiceClient(connectionString);

                //if (crmSvc != null && crmSvc.IsReady)
                //{
                //    //create Account object
                //    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                //    inData.Add("firstname", new CrmDataTypeWrapper(lData.FirstName, CrmFieldType.String));
                //    inData.Add("lastname", new CrmDataTypeWrapper(lData.LastName, CrmFieldType.String));
                //    if (lData.CustomFields != null)
                //    {
                //        foreach (DYCustomObject c in lData.CustomFields)
                //        {
                //            CrmFieldType type;
                //            switch (c.type.ToLower())
                //            {
                //                case "string":
                //                    { type = CrmFieldType.String; break; }
                //                case "decimal":
                //                    { type = CrmFieldType.CrmDecimal; break; }
                //                case "lookup":
                //                    { type = CrmFieldType.Lookup; break; }
                //                case "bool":
                //                    { type = CrmFieldType.CrmBoolean; break; }
                //                default:
                //                    { type = CrmFieldType.String; break; }
                //            }
                //            if(type ==  CrmFieldType.Lookup)
                //            {
                //                if (c.value.ToString().Length > 0)
                //                {
                //                    inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));
                //                }
                //            }
                //            else 
                //                inData.Add(c.field, new CrmDataTypeWrapper(c.value, type));
                //        }
                //    }


                //    //inData.Add("ayu_salesengineer", new CrmDataTypeWrapper(lData.Salesengineer, CrmFieldType.Picklist));
                //    //if (lData.Salesengineer.ToString().Length > 0)
                //    //{
                //    //    inData.Add("ayu_salesperson", new CrmDataTypeWrapper(new Guid(lData.Salesengineer), CrmFieldType.Lookup, "systemuser"));
                //    //}
                //    //Guid contactId = crmSvc.CreateNewRecord("contact", inData);
                //    //if (contactId != Guid.Empty)
                //    //{
                //    //    //Console.WriteLine("Account created.");
                //    //    PostedObjectDetail pObject = new PostedObjectDetail();
                //    //    pObject.Id = contactId.ToString();
                //    //    pObject.ObjectName = "Contact";
                //    //    pObject.Message = "Contact added successfully!";
                //    //    return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                //    //}
                //    //else
                //    //{
                //    //    return MyAppsDb.ConvertJSONOutput("Could not add new contact, check mandatory fields", HttpStatusCode.OK, true);
                //    //}
                //}
                //else
                //{
                //    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.OK, true);
                //}
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.Conflict);
            }

        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            try
            {
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string sFieldOptional = "";
                //int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);
                int output = MyAppsDb.GetDynamicsCredentialswithCustomSearchFields(ObjectRef, GroupId, "contact", ref ApplicationURL, ref userName, ref password, ref authType, ref sFieldOptional, urlReferrer);

                var getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, "Contact", urlReferrer);
                List<EntityColumn> getDetailFields = BusinessLogic.DynamicCommon.GetDynamicDetailFileds(ObjectRef, GroupId, "contact", urlReferrer);

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
                        LogicalName = "contact"
                    };
                    RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)proxyservice.Execute(retrieveEntityRequest);
                    EntityMetadata RetrieveEntityInfo = retrieveEntityResponse.EntityMetadata;

                    List<DYContact> listToReturn = new List<DYContact>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    List<string> contactId = new List<string>();
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
                                            contactId.Add(((Microsoft.Xrm.Sdk.EntityReference)item.Attributes[csA.RelatedEntityFieldName]).Id.ToString());
                                    }
                                }

                            }
                        }
                    }
                    // End Related Entity

                    QueryExpression query = new QueryExpression("contact");

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { "contactid", "firstname", "lastname", "emailaddress1", "telephone1", "statuscode" });
                    // Add Default Searched Column
                    foreach (var item in defaultSearchedColumn)
                    {
                        query.ColumnSet.AddColumn(item);
                    }
                    // Add Custom Searched Column
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

                    FilterExpression filter = new FilterExpression();

                    //filter name 
                    ConditionExpression filterOwnRcd1 = new ConditionExpression();
                    filterOwnRcd1.AttributeName = "fullname";
                    filterOwnRcd1.Operator = ConditionOperator.Like;
                    filterOwnRcd1.Values.Add("%" + SValue.Trim() + "%");
                    //Filter Email
                    ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    filterOwnRcd2.AttributeName = "emailaddress1";
                    filterOwnRcd2.Operator = ConditionOperator.Like;
                    filterOwnRcd2.Values.Add("%" + SValue.Trim() + "%");

                    //Filter Phone
                    ConditionExpression filterOwnRcd3 = new ConditionExpression();
                    filterOwnRcd3.AttributeName = "telephone1";
                    filterOwnRcd3.Operator = ConditionOperator.Like;
                    filterOwnRcd3.Values.Add("%" + SValue.Trim() + "%");

                    //filter1.Conditions.Add(filterOwnRcd);
                    filter.Conditions.Add(filterOwnRcd1);
                    filter.Conditions.Add(filterOwnRcd2);
                    filter.Conditions.Add(filterOwnRcd3);

                    // get list of custom fields 
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var searchField in getSearchedFileds)
                        {
                            ConditionExpression condition = new ConditionExpression();
                            

                            if (searchField.FieldType == "lookup")
                            {
                                var lookupFieldMetaData = (from e in RetrieveEntityInfo.Attributes where e.LogicalName == searchField.FieldName select e).FirstOrDefault();
                                string loolupentityName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)lookupFieldMetaData).Targets[0].ToString();
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
                                        c.AttributeName = searchField.FieldName;
                                        c.Operator = ConditionOperator.Equal;
                                        c.Values.Add(item);
                                        lookupCondition.Add(c);
                                    }
                                    filter.Conditions.AddRange(lookupCondition);
                                }
                            }
                            else if (searchField.FieldType == "currency")
                            {
                                condition.AttributeName = searchField.FieldName;
                                condition.Operator = ConditionOperator.Like;
                                int result;
                                bool parsedSuccessfully = int.TryParse(SValue, out result);
                                if (int.TryParse(SValue, out result))
                                {
                                    condition.Values.Add(Convert.ToDecimal(result));
                                    filter.Conditions.Add(condition);
                                }
                            }
                            else if (searchField.FieldType == "datetime")
                            {
                                condition.AttributeName = searchField.FieldName;
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
                                condition.AttributeName = searchField.FieldName;
                                condition.Operator = ConditionOperator.Like;
                                condition.Values.Add("%" + SValue.Trim() + "%");
                                filter.Conditions.Add(condition);
                            }
                        }
                    }
                    // Add Detail Fields TO search
                    if (getDetailFields.Count > 0)
                    {
                        foreach (var detailField in getDetailFields)
                        {
                            var flag = filter.Conditions.Where(x => x.AttributeName == detailField.FieldName).Select(s => s.AttributeName).FirstOrDefault();
                            if (flag == null)
                            {
                                ConditionExpression condition1 = new ConditionExpression();

                                if (detailField.FieldType == "lookup")
                                {
                                    var lookupFieldMetaData = (from e in RetrieveEntityInfo.Attributes where e.LogicalName == detailField.FieldName select e).FirstOrDefault();
                                    string loolupentityName = ((Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata)lookupFieldMetaData).Targets[0].ToString();
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
                                            c.AttributeName = detailField.FieldName;
                                            c.Operator = ConditionOperator.Equal;
                                            c.Values.Add(item);
                                            lookupCondition.Add(c);
                                        }
                                        filter.Conditions.AddRange(lookupCondition);
                                    }
                                }
                                else if (detailField.FieldType == "currency")
                                {
                                    condition1.AttributeName = detailField.FieldName;
                                    condition1.Operator = ConditionOperator.Like;
                                    int result;
                                    bool parsedSuccessfully = int.TryParse(SValue, out result);
                                    if (int.TryParse(SValue, out result))
                                    {
                                        condition1.Values.Add(result);
                                        filter.Conditions.Add(condition1);
                                    }
                                }
                                else if (detailField.FieldType == "datetime")
                                {
                                    condition1.AttributeName = detailField.FieldName;
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
                                    condition1.AttributeName = detailField.FieldName;
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

                    foreach (var item in contactId)
                    {
                        if (result1.Entities.Count > 0)
                        {
                            if (result1.Entities.Where(c => c.Attributes["contactid"].ToString() == item).FirstOrDefault() == null)
                            {
                                Entity result2 = objser.Retrieve("contact", new Guid(item), query.ColumnSet);
                                result1.Entities.Add(result2);
                            }
                        }
                        else
                        {
                            Entity result2 = objser.Retrieve("contact", new Guid(item), query.ColumnSet);
                            result1.Entities.Add(result2);
                        }
                    }

                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            if(((Microsoft.Xrm.Sdk.OptionSetValue)z.Attributes["statuscode"]).Value == 1)
                            {
                                DYContact info = new DYContact();
                                if (z.Attributes.Contains("contactid"))
                                {
                                    info.contactid = z.Attributes["contactid"].ToString();
                                }
                                if (z.Attributes.Contains("firstname"))
                                {
                                    info.firstname = z.Attributes["firstname"].ToString();
                                }
                                if (z.Attributes.Contains("lastname"))
                                {
                                    info.lastname = z.Attributes["lastname"].ToString();
                                }
                                if (z.Attributes.Contains("emailaddress1"))
                                {
                                    info.email = z.Attributes["emailaddress1"].ToString();
                                }
                                if (z.Attributes.Contains("telephone1"))
                                {
                                    info.phone = z.Attributes["telephone1"].ToString();
                                }

                                // Start Custom Search Field
                                List<CustomFieldModel> retSearchFields = new List<CustomFieldModel>();
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
                                                if (z.Attributes[field.FieldName].ToString() != "Microsoft.Xrm.Sdk.EntityReference")
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
                                                    Fields.Value = ((Microsoft.Xrm.Sdk.EntityReference)z.Attributes[field.FieldName]).Name.ToString();
                                                }

                                                retSearchFields.Add(Fields);
                                            }
                                        }
                                    }

                                }

                                info.searchFields = retSearchFields;
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

    }
}
