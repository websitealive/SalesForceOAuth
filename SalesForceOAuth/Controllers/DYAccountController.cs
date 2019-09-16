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
    public class DYAccountController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount(DYAccountPostData lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-PostAccount", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                #region New Code

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
                    Entity registration = new Entity("account");
                    registration["name"] = lData.Name;
                    registration["accountnumber"] = lData.AccountNumber;
                    registration["description"] = lData.Description;
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
                            }

                        }
                    }
                    #endregion
                    // New Functionality for Custom Fileds
                    var customFields = Repository.GetConstantInputFields(lData.ObjectRef, lData.GroupId, urlReferrer, EntityName.Account.ToString());
                    if (customFields != null)
                    {
                        foreach (FieldsModel inputField in customFields)
                        {
                            if (inputField.ValueDetail != null)
                            {
                                if(inputField.FieldType == "lookup")
                                {
                                    //lookup 
                                    registration[inputField.FieldName] = new EntityReference(inputField.LookupEntityName, new Guid(inputField.LookupEntityRecordId));
                                }
                                else if(inputField.FieldType == "datetime")
                                {
                                    //Date Time
                                    registration[inputField.FieldName]  = Convert.ToDateTime(inputField.LookupEntityName);
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

                                default:
                                    {
                                        type = CrmFieldType.String;
                                        registration[c.field] = c.value;
                                        break;

                                    }
                            }

                        }
                    }
                    #endregion Custom fields 

                    Guid accountId = objser.Create(registration);
                    if (accountId != Guid.Empty)
                    {
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = accountId.ToString();
                        pObject.ObjectName = "Account";
                        pObject.Message = "Account added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Account, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }

                #endregion

                #region Old Code

                ////Connect to SDK 
                ////Test system
                ////string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                ////    password = "Getthat$$$5", authType = "Office365";
                ////string ApplicationURL = "https://websitealive.crmgate.pk/websitealive", userName = "naveed@crmgate.local",
                ////    password = "@Abc.123", authType = "IFD";
                ////Live system
                //string ApplicationURL = "", userName = "", password = "", authType = "";
                //string urlReferrer = Request.RequestUri.Authority.ToString();
                //int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                //string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                //connectionString += "RequireNewInstance=true;";
                //CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //if (crmSvc != null && crmSvc.IsReady)
                //{
                //    //create Account object
                //    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                //    inData.Add("name", new CrmDataTypeWrapper(lData.Name, CrmFieldType.String));
                //    inData.Add("accountnumber", new CrmDataTypeWrapper(lData.AccountNumber, CrmFieldType.String));
                //    inData.Add("description", new CrmDataTypeWrapper(lData.Description, CrmFieldType.String));
                //    inData.Add("telephone1", new CrmDataTypeWrapper(lData.Phone, CrmFieldType.String));
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
                //            if (type == CrmFieldType.Lookup)
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
                //    Guid accountId = crmSvc.CreateNewRecord("account", inData);
                //    if (accountId != Guid.Empty)
                //    {
                //        //Console.WriteLine("Account created.");
                //        PostedObjectDetail pObject = new PostedObjectDetail();
                //        pObject.Id = accountId.ToString();
                //        pObject.ObjectName = "Account";
                //        pObject.Message = "Account added successfully!";
                //        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                //    }
                //    else
                //    {
                //        return MyAppsDb.ConvertJSONOutput("Could not add new account, check mandatory fields", HttpStatusCode.InternalServerError, true);
                //    }
                //}
                //else
                //{
                //    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError, true);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //End connect to SDK
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string token, string ObjectRef, int GroupId, string SValue, string callback)
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
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                var getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, "Account", urlReferrer);
                List<EntityColumn> getDetailFields = BusinessLogic.DynamicCommon.GetDynamicDetailFileds(ObjectRef, GroupId, "account", urlReferrer);
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
                    List<DYAccount> listToReturn = new List<DYAccount>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    List<string> accountId = new List<string>();
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
                                            accountId.Add(((Microsoft.Xrm.Sdk.EntityReference)item.Attributes[csA.RelatedEntityFieldName]).Id.ToString());
                                    }
                                }

                            }
                        }
                    }
                    // End Related Entity

                    //filter name
                    QueryExpression query = new QueryExpression("account");

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { "accountid", "address1_city", "accountnumber", "telephone1", "emailaddress1", "name", "statuscode" });
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

                    FilterExpression filter = new FilterExpression();
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "name";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue.Trim() + "%");
                    //filter email
                    ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    filterOwnRcd2.AttributeName = "emailaddress1";
                    filterOwnRcd2.Operator = ConditionOperator.Like;
                    filterOwnRcd2.Values.Add("%" + SValue.Trim() + "%");
                    //filter phone
                    ConditionExpression filterOwnRcd1 = new ConditionExpression();
                    filterOwnRcd1.AttributeName = "telephone1";
                    filterOwnRcd1.Operator = ConditionOperator.Like;
                    filterOwnRcd1.Values.Add("%" + SValue.Trim() + "%");

                    filter.Conditions.Add(filterOwnRcd);
                    filter.Conditions.Add(filterOwnRcd1);
                    filter.Conditions.Add(filterOwnRcd2);

                    //Add Custom Search Filters
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var searchField in getSearchedFileds)
                        {
                            ConditionExpression condition = new ConditionExpression();
                            condition.AttributeName = searchField.FieldName;
                            condition.Operator = ConditionOperator.Like;

                            if (searchField.FieldType == "currency")
                            {
                                int result;
                                bool parsedSuccessfully = int.TryParse(SValue, out result);
                                if (int.TryParse(SValue, out result))
                                {
                                    condition.Values.Add(result);
                                    filter.Conditions.Add(condition);
                                }
                            }
                            else if (searchField.FieldType == "datetime")
                            {
                                DateTime result;
                                if (DateTime.TryParse(SValue, out result))
                                {
                                    condition.Values.Add(result);
                                    filter.Conditions.Add(condition);
                                }
                            }
                            else
                            {
                                condition.Values.Add("%" + SValue.Trim() + "%");
                                filter.Conditions.Add(condition);
                            }
                        }
                    }
                    // Add Detail Fileds TO search
                    if (getDetailFields.Count > 0)
                    {
                        foreach (var detailField in getDetailFields)
                        {
                            if (detailField.FieldType != "lookup")
                            {
                                var flag = filter.Conditions.Where(x => x.AttributeName == detailField.FieldName).Select(s => s.AttributeName).FirstOrDefault();
                                if (flag == null)
                                {
                                    ConditionExpression condition1 = new ConditionExpression();
                                    condition1.AttributeName = detailField.FieldName;
                                    condition1.Operator = ConditionOperator.Like;
                                    if (detailField.FieldType == "currency")
                                    {
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
                                        DateTime result;
                                        if (DateTime.TryParse(SValue, out result))
                                        {
                                            condition1.Values.Add(result);
                                            filter.Conditions.Add(condition1);
                                        }
                                    }
                                    else
                                    {
                                        condition1.Values.Add("%" + SValue.Trim() + "%");
                                        filter.Conditions.Add(condition1);
                                    }
                                }
                            }
                        }
                    }
                    filter.FilterOperator = LogicalOperator.Or;

                    query.Criteria.AddFilter(filter);

                    EntityCollection result1 = objser.RetrieveMultiple(query);

                    foreach (var item in accountId)
                    {
                        if (result1.Entities.Count > 0)
                        {
                            if (result1.Entities.Where(c => c.Attributes["accountid"].ToString() == item).FirstOrDefault() == null)
                            {
                                Entity result2 = objser.Retrieve("account", new Guid(item), query.ColumnSet);
                                result1.Entities.Add(result2);
                            }
                        }
                        else
                        {
                            Entity result2 = objser.Retrieve("account", new Guid(item), query.ColumnSet);
                            result1.Entities.Add(result2);
                        }
                    }

                    if (result1.Entities.Count > 0)
                    {
                        foreach (var z in result1.Entities)
                        {
                            if (((Microsoft.Xrm.Sdk.OptionSetValue)z.Attributes["statuscode"]).Value == 1)
                            {
                                DYAccount info = new DYAccount();
                                if (z.Attributes.Contains("accountid"))
                                    info.accountid = z.Attributes["accountid"].ToString();
                                if (z.Attributes.Contains("address1_city"))
                                    info.address1_city = z.Attributes["address1_city"].ToString();
                                if (z.Attributes.Contains("accountnumber"))
                                    info.accountnumber = z.Attributes["accountnumber"].ToString();
                                if (z.Attributes.Contains("telephone1"))
                                    info.address1_telephone1 = z.Attributes["telephone1"].ToString();
                                if (z.Attributes.Contains("emailaddress1"))
                                    info.emailaddress1 = z.Attributes["emailaddress1"].ToString();
                                if (z.Attributes.Contains("name"))
                                    info.name = z.Attributes["name"].ToString();
                                // Start Custom Search Filed
                                List<CustomFieldModel> retSearchFields = new List<CustomFieldModel>();
                                if (getSearchedFileds.Count > 0)
                                {
                                    foreach (var field in getSearchedFileds)
                                    {
                                        if ((field.FieldType != "relatedEntity") && (field.FieldType == "textbox" || field.FieldType == "boolean" || field.FieldType == "lookup" || field.FieldType == "currency" || field.FieldType == "datetime"))
                                        {
                                            if (z.Attributes.Contains(field.FieldName))
                                            {
                                                CustomFieldModel Fields = new CustomFieldModel();
                                                Fields.FieldLabel = field.FieldLabel;
                                                if (z.Attributes[field.FieldName].ToString() != "Microsoft.Xrm.Sdk.EntityReference")
                                                {
                                                    Fields.Value = z.Attributes[field.FieldName].ToString();
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

    public class DYAccountPostData : MyValidation
    {
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public List<CustomObject> CustomFields { get; set; }
        public List<CustomFieldModel> InputFields { get; set; }
    }
    public class DYContactPostData : MyValidation
    {
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<CustomObject> CustomFields { get; set; }
        public List<CustomFieldModel> InputFields { get; set; }

    }

    public class DYAccountPostValue
    {
        public string name { get; set; }
        public string description { get; set; }
        public string accountnumber { get; set; }
        public string address1_telephone1 { get; set; }
    }

    public class CRMInfo
    {
        public string OrgURL { get; set; }
        public string OrgUsername { get; set; }
        public string OrgPassword { get; set; }
        public string SolName { get; set; }
        public string SolVersion { get; set; }
        public string SolID { get; set; }
        public string SolUniquename { get; set; }
        public string SolIsmanaged { get; set; }
    }
    public class DYAccount
    {
        public string accountid { get; set; }
        public string name { get; set; }
        public string accountnumber { get; set; }
        public string address1_city { get; set; }
        public string address1_telephone1 { get; set; }
        public string emailaddress1 { get; set; }
        public string crmtaskassigneduniqueid { get; set; }
        public List<CustomFieldModel> searchFields { get; set; }
    }
    public class DYContact
    {
        public string contactid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public List<CustomFieldModel> searchFields { get; set; }
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string Custom4 { get; set; }
        public string Custom5 { get; set; }
        public string Custom6 { get; set; }
        public string Custom7 { get; set; }
        public string Custom8 { get; set; }
        public string Custom9 { get; set; }
        public string Custom10 { get; set; }
    }
    public class DYAccountOutput : DYAccount
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string address1_composites { get; set; }
    }

    public class DYAccountOutputContainer
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYAccountOutput[] value { get; set; }
    }


}
