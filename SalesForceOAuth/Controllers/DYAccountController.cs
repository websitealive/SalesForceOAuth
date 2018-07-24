using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
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
                        foreach (InputFields inputField in lData.InputFields)
                        {
                            if (inputField.Value != null)
                            {
                                registration[inputField.FieldName] = inputField.Value;
                            }

                        }
                    }
                    #endregion

                    #region custom fields 
                    if (lData.CustomFields != null)
                    {
                        foreach (DYCustomObject c in lData.CustomFields)
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
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alan365.crm.dynamics.com", userName = "alan@alan365.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Test system IFD
                //string ApplicationURL = "https://msdynamics.websitealive.com", userName = @"wsa\administrator",
                //    password = "bX9bTkYv)Td", authType = "IFD";
                //Live system
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
                    List<DYAccount> listToReturn = new List<DYAccount>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "name";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue + "%");
                    //filter email
                    ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    filterOwnRcd2.AttributeName = "emailaddress1";
                    filterOwnRcd2.Operator = ConditionOperator.Like;
                    filterOwnRcd2.Values.Add("%" + SValue + "%");


                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    filter1.Conditions.Add(filterOwnRcd2);
                    //Add Custom Search Filters
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var csA in getSearchedFileds)
                        {
                            ConditionExpression filterOwnRcd4 = new ConditionExpression();
                            filterOwnRcd4.AttributeName = csA.FieldName;
                            filterOwnRcd4.Operator = ConditionOperator.Like;
                            filterOwnRcd4.Values.Add("%" + SValue + "%");
                            filter1.Conditions.Add(filterOwnRcd4);
                        }
                    }
                    filter1.FilterOperator = LogicalOperator.Or;
                    QueryExpression query = new QueryExpression("account");

                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { "accountid", "address1_city", "accountnumber", "telephone1", "emailaddress1", "name" });
                    foreach (var item in defaultSearchedColumn)
                    {
                        query.ColumnSet.AddColumn(item);
                    }
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var field in getSearchedFileds)
                        {
                            query.ColumnSet.AddColumn(field.FieldName);
                        }

                    }

                    //query.ColumnSet.AddColumns("accountid", "address1_city", "accountnumber", "telephone1", "emailaddress1", "name");
                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
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
                            List<InputFields> retSearchFields = new List<InputFields>();
                            if (getSearchedFileds.Count > 0)
                            {

                                foreach (var field in getSearchedFileds)
                                {
                                    if (z.Attributes.Contains(field.FieldName))
                                    {
                                        InputFields Fields = new InputFields();
                                        Fields.FieldLabel = field.FieldLabel;
                                        Fields.Value = z.Attributes[field.FieldName].ToString();

                                        retSearchFields.Add(Fields);
                                    }
                                }

                            }

                            info.searchFields = retSearchFields;
                            // End Custom Search Filed

                            listToReturn.Add(info);
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
        public List<DYCustomObject> CustomFields { get; set; }
        public List<InputFields> InputFields { get; set; }
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
        public List<DYCustomObject> CustomFields { get; set; }
        public List<InputFields> InputFields { get; set; }
        //public string Salesengineer { get; set; }

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
        public List<InputFields> searchFields { get; set; }
    }
    public class DYContact
    {
        public string contactid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public List<InputFields> searchFields { get; set; }
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
