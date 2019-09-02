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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using SalesForceOAuth.BusinessLogic;
using System.Threading.Tasks;
using SalesForceOAuth.ModelClasses;
using SalesForceOAuth.Models;

namespace SalesForceOAuth.Controllers
{
    public class DYLeadController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> PostLead(DYLeadPostData lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostLead", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
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
                    Entity registration = new Entity("lead");
                    registration["firstname"] = lData.Firstname;
                    registration["lastname"] = lData.Lastname;
                    registration["companyname"] = lData.Companyname;
                    registration["subject"] = lData.Subject;
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
                            }

                        }
                    }
                    #endregion

                    // New Functionality for Custom Fileds
                    var customFields = Repository.GetConstantInputFields(lData.ObjectRef, lData.GroupId, urlReferrer, EntityName.Lead);
                    if (customFields != null)
                    {
                        foreach (CustomFieldModel inputField in customFields)
                        {
                            if (inputField.Value != null)
                            {
                                registration[inputField.FieldName] = inputField.Value;
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

                    Guid leadId = objser.Create(registration);
                    if (leadId != Guid.Empty)
                    {
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = leadId.ToString();
                        pObject.ObjectName = "Lead";
                        pObject.Message = "Lead added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Lead, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }
                #endregion

                #region Old Code

                ////Test system
                ////string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                ////    password = "Getthat$$$5", authType = "Office365";
                ////Live system
                //string ApplicationURL = "", userName = "", password = "", authType = "";
                //string urlReferrer = Request.RequestUri.Authority.ToString();
                //int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                //string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                //connectionString += "RequireNewInstance=true;";
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                //if (crmSvc != null && crmSvc.IsReady)
                //{
                //    //create Account object
                //    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                //    inData.Add("companyname", new CrmDataTypeWrapper(lData.Companyname, CrmFieldType.String));
                //    inData.Add("firstname", new CrmDataTypeWrapper(lData.Firstname, CrmFieldType.String));
                //    inData.Add("lastname", new CrmDataTypeWrapper(lData.Lastname, CrmFieldType.String));
                //    inData.Add("subject", new CrmDataTypeWrapper(lData.Subject, CrmFieldType.String));
                //    //inData.Add("address1_city", new CrmDataTypeWrapper(lData.City, CrmFieldType.String));
                //    inData.Add("telephone1", new CrmDataTypeWrapper(lData.Phone, CrmFieldType.String));
                //    inData.Add("emailaddress1", new CrmDataTypeWrapper(lData.Email, CrmFieldType.String));
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


                //    Guid accountId = crmSvc.CreateNewRecord("lead", inData);
                //    if (accountId != Guid.Empty)
                //    {
                //        //Console.WriteLine("Account created.");
                //        PostedObjectDetail pObject = new PostedObjectDetail();
                //        pObject.Id = accountId.ToString();
                //        pObject.ObjectName = "Lead";
                //        pObject.Message = "Lead added successfully!";
                //        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                //    }
                //    else
                //    {
                //        return MyAppsDb.ConvertJSONOutput("Could not add new Lead, check mandatory fields", HttpStatusCode.InternalServerError, true);
                //    }
                //}
                //else
                //{
                //    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError, true);
                //}

                #region dynamics rest api call code obsolete
                //JObject values = JObject.Parse(outputPayload); // parse as array  
                //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
                //HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(lData.ObjectRef, lData.GroupId.ToString());
                //if (msg.StatusCode == HttpStatusCode.OK)
                //{ AccessToken = msg.Content.ReadAsStringAsync().Result; }
                //else
                //{ return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                //client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                //client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                //StringBuilder requestURI = new StringBuilder();
                //requestURI.Append("/api/data/v8.0/leads");
                //DYLeadPostValue aData = new DYLeadPostValue();
                //aData.companyname = lData.Companyname;
                //aData.address1_city = lData.City;
                //aData.address1_telephone1 = lData.Phone;
                //aData.emailaddress1 = lData.Email;
                //aData.subject = lData.Subject; 
                //StringContent content = new StringContent(JsonConvert.SerializeObject(aData), Encoding.UTF8, "application/json");
                //HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                //if (response.IsSuccessStatusCode)
                //{
                //    var output = response.Headers.Location.OriginalString;
                //    var id = output.Substring(output.IndexOf("(") + 1, 36);
                //    PostedObjectDetail pObject = new PostedObjectDetail();
                //    pObject.Id = id;
                //    pObject.ObjectName = "Lead";
                //    pObject.Message = "Lead added successfully!";
                //    return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK);
                //}
                //else
                //{
                //    return MyAppsDb.ConvertJSONOutput("Dynamics Error: " + response.StatusCode, HttpStatusCode.InternalServerError);
                //}
                #endregion dynamics rest api call code obsolete

                #endregion
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostLead", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetSearchedLeads(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DyLeads-GetSearchedLeads", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                var getSearchedFileds = BusinessLogic.DynamicCommon.GetDynamicSearchFileds(ObjectRef, GroupId, "Lead", urlReferrer);
                List<EntityColumn> getDetailFields = BusinessLogic.DynamicCommon.GetDynamicDetailFileds(ObjectRef, GroupId, "lead", urlReferrer);

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
                    List<DYLead> listToReturn = new List<DYLead>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    List<string> leadId = new List<string>();
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
                                            leadId.Add(((Microsoft.Xrm.Sdk.EntityReference)item.Attributes[csA.RelatedEntityFieldName]).Id.ToString());
                                    }
                                }

                            }
                        }
                    }
                    // End Related Entity

                    //filter name
                    QueryExpression query = new QueryExpression("lead");
                    List<string> defaultSearchedColumn = new List<string>();
                    defaultSearchedColumn.AddRange(new string[] { "leadid", "address1_city", "subject", "lastname", "telephone1", "emailaddress1", "companyname", "firstname" });
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

                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "fullname";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue.Trim() + "%");
                    //filter phone
                    ConditionExpression filterOwnRcd1 = new ConditionExpression();
                    filterOwnRcd1.AttributeName = "telephone1";
                    filterOwnRcd1.Operator = ConditionOperator.Like;
                    filterOwnRcd1.Values.Add("%" + SValue.Trim() + "%");
                    //filter email
                    ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    filterOwnRcd2.AttributeName = "emailaddress1";
                    filterOwnRcd2.Operator = ConditionOperator.Like;
                    filterOwnRcd2.Values.Add("%" + SValue.Trim() + "%");
                    //filter subject
                    ConditionExpression filterOwnRcd3 = new ConditionExpression();
                    filterOwnRcd3.AttributeName = "subject";
                    filterOwnRcd3.Operator = ConditionOperator.Like;
                    filterOwnRcd3.Values.Add("%" + SValue.Trim() + "%");

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    filter1.Conditions.Add(filterOwnRcd1);
                    filter1.Conditions.Add(filterOwnRcd2);
                    filter1.Conditions.Add(filterOwnRcd3);
                    //Add Custom Search Filters
                    if (getSearchedFileds.Count > 0)
                    {
                        foreach (var csA in getSearchedFileds)
                        {
                            if (csA.FieldType != "lookup")
                            {
                                ConditionExpression filterOwnRcd4 = new ConditionExpression();
                                filterOwnRcd4.AttributeName = csA.FieldName;
                                filterOwnRcd4.Operator = ConditionOperator.Like;
                                filterOwnRcd4.Values.Add("%" + SValue + "%");
                                filter1.Conditions.Add(filterOwnRcd4);
                            }
                        }
                    }
                    // Add Detail Fileds TO search
                    if (getDetailFields.Count > 0)
                    {
                        foreach (var field in getDetailFields)
                        {
                            if (field.FieldType == "textbox" || field.FieldType == "boolean")
                            {
                                var flag = filter1.Conditions.Where(x => x.AttributeName == field.FieldName).Select(s => s.AttributeName).FirstOrDefault();
                                if (flag == null)
                                {
                                    ConditionExpression filterOwnRcd5 = new ConditionExpression();
                                    filterOwnRcd5.AttributeName = field.FieldName;
                                    filterOwnRcd5.Operator = ConditionOperator.Like;
                                    filterOwnRcd5.Values.Add("%" + SValue.Trim() + "%");
                                    filter1.Conditions.Add(filterOwnRcd5);
                                }
                            }
                        }
                    }
                    filter1.FilterOperator = LogicalOperator.Or;

                    //query.ColumnSet.AddColumns("leadid", "address1_city", "subject", "lastname", "telephone1", "emailaddress1", "companyname", "firstname");
                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);

                    foreach (var item in leadId)
                    {
                        if (result1.Entities.Count > 0)
                        {
                            if (result1.Entities.Where(c => c.Attributes["leadid"].ToString() == item).FirstOrDefault() == null)
                            {
                                Entity result2 = objser.Retrieve("lead", new Guid(item), query.ColumnSet);
                                result1.Entities.Add(result2);
                            }
                        }
                        else
                        {
                            Entity result2 = objser.Retrieve("lead", new Guid(item), query.ColumnSet);
                            result1.Entities.Add(result2);
                        }
                    }

                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            DYLead info = new DYLead();
                            if (z.Attributes.Contains("leadid"))
                                info.leadid = z.Attributes["leadid"].ToString();
                            if (z.Attributes.Contains("address1_city"))
                                info.address1_city = z.Attributes["address1_city"].ToString();
                            if (z.Attributes.Contains("subject"))
                                info.subject = z.Attributes["subject"].ToString();
                            if (z.Attributes.Contains("lastname"))
                                info.lastname = z.Attributes["lastname"].ToString();
                            if (z.Attributes.Contains("emailaddress1"))
                                info.emailaddress1 = z.Attributes["emailaddress1"].ToString();
                            if (z.Attributes.Contains("companyname"))
                                info.companyname = z.Attributes["companyname"].ToString();
                            if (z.Attributes.Contains("firstname"))
                                info.firstname = z.Attributes["firstname"].ToString();
                            if (z.Attributes.Contains("telephone1"))
                                info.address1_telephone1 = z.Attributes["telephone1"].ToString();

                            // Start Custom Search Filed
                            List<CustomFieldModel> retSearchFields = new List<CustomFieldModel>();
                            if (getSearchedFileds.Count > 0)
                            {

                                foreach (var field in getSearchedFileds)
                                {
                                    if (z.Attributes.Contains(field.FieldName))
                                    {
                                        CustomFieldModel Fields = new CustomFieldModel();
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

                            info.searchFields = retSearchFields;
                            // End Custom Search Filed

                            listToReturn.Add(info);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
                #region dynamics api call 
                ////HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], ObjectRef,GroupId.ToString(), "internal");
                //HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(ObjectRef, GroupId.ToString());
                //if (msg.StatusCode == HttpStatusCode.OK)
                //{   AccessToken = msg.Content.ReadAsStringAsync().Result;   }
                //else
                //{   return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode);   }

                ////end Test 
                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                //client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                //client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                //StringBuilder requestURI = new StringBuilder();
                //requestURI.Append("/api/data/v8.0/leads?$select=companyname,emailaddress1,address1_telephone1,subject,address1_city");
                //requestURI.Append("&$top=50");
                //if (!SValue.Equals(""))
                //{
                //    requestURI.Append("&$filter=contains(companyname, '" + SValue + "')or contains(subject, '" + SValue + "')");
                //    requestURI.Append("or contains(emailaddress1, '" + SValue + "')or contains(address1_city, '" + SValue + "')");
                //    requestURI.Append("or contains(address1_telephone1, '" + SValue + "')");
                //}
                //HttpResponseMessage response = client.GetAsync(requestURI.ToString()).Result;
                //List<DYLead> myLeads = new List<DYLead> { };
                //if (response.IsSuccessStatusCode)
                //{
                //    var json = response.Content.ReadAsStringAsync().Result;
                //    var odata = JsonConvert.DeserializeObject<DYLeadOutputContainer>(json);
                //    foreach (DYLeadOutput o in odata.value)
                //    {
                //        DYLead l = new DYLead();
                //        l.subject = o.subject;
                //        l.leadid = o.leadid;
                //        l.address1_city = o.address1_city;
                //        l.address1_telephone1 = o.address1_telephone1;
                //        l.emailaddress1 = o.emailaddress1;
                //        l.companyname = o.companyname; 
                //        myLeads.Add(l);
                //    }

                //}
                #endregion dynamics api call 
                //return MyAppsDb.ConvertJSONPOutput(callback, myLeads, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.Conflict);

            }

        }


    }
    public class DynamicUser
    {
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string OrganizationURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthType { get; set; }
    }

    public class DYLeadPostData : MyValidation
    {
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Companyname { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<CustomObject> CustomFields { get; set; }
        public List<CustomFieldModel> InputFields { get; set; }


        //public string Email { get; set; }
        //public string Phone { get; set; }
        //public string City { get; set; }
    }

    public class DYLeadPostValue
    {
        public string companyname { get; set; }
        public string subject { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
    }

    public class DYLead
    {
        public string leadid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string companyname { get; set; }
        public string subject { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
        public List<CustomFieldModel> searchFields { get; set; }
    }

    public class DYLeadOutput : DYLead
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string address1_composites { get; set; }
    }

    public class DYLeadOutputContainer
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYLeadOutput[] value { get; set; }
    }


}
