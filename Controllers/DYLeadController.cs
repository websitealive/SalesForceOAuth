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
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead(DYLeadPostData lData)
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
            //string AccessToken = "";
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
            try
            {
                //Test system
                //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                if (crmSvc != null && crmSvc.IsReady)
                {
                    //create Account object
                    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                    inData.Add("companyname", new CrmDataTypeWrapper(lData.Companyname, CrmFieldType.String));
                    inData.Add("firstname", new CrmDataTypeWrapper(lData.Firstname, CrmFieldType.String));
                    inData.Add("lastname", new CrmDataTypeWrapper(lData.Lastname, CrmFieldType.String));
                    inData.Add("address1_city", new CrmDataTypeWrapper(lData.City, CrmFieldType.String));
                    inData.Add("address1_telephone1", new CrmDataTypeWrapper(lData.Phone, CrmFieldType.String));
                    inData.Add("emailaddress1", new CrmDataTypeWrapper(lData.Email, CrmFieldType.String));
                    inData.Add("subject", new CrmDataTypeWrapper(lData.Subject, CrmFieldType.String));
                    //aData.companyname = lData.Companyname;
                    //aData.address1_city = lData.City;
                    //aData.address1_telephone1 = lData.Phone;
                    //aData.emailaddress1 = lData.Email;
                    //aData.subject = lData.Subject; 
                    Guid accountId = crmSvc.CreateNewRecord("lead", inData);
                    if (accountId != Guid.Empty)
                    {
                        //Console.WriteLine("Account created.");
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = accountId.ToString();
                        pObject.ObjectName = "Lead";
                        pObject.Message = "Lead added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new Lead, check mandatory fields", HttpStatusCode.InternalServerError,true);
                    }
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError,true);
                }

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
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostLead", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {
            // string AccessToken = "";
            // //var re = Request;
            // //var headers = re.Headers;
            //// string GroupId = "", ObjectRef = "", SValue = "";
            // //if (headers.Contains("Authorization"))
            // //{
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
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                if (crmSvc != null && crmSvc.IsReady)
                {
                    Dictionary<string, Dictionary<string, object>> outData = new Dictionary<string, Dictionary<string, object>>();
                    //search conditions 
                    CrmServiceClient.CrmFilterConditionItem condition1 = new CrmServiceClient.CrmFilterConditionItem();
                    condition1.FieldName = "subject";
                    condition1.FieldOperator = Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith;
                    condition1.FieldValue = SValue;
                    CrmServiceClient.CrmFilterConditionItem condition2 = new CrmServiceClient.CrmFilterConditionItem();
                    condition2.FieldName = "companyname";
                    condition2.FieldOperator = Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith;
                    condition2.FieldValue = SValue;
                    CrmServiceClient.CrmFilterConditionItem condition3 = new CrmServiceClient.CrmFilterConditionItem();
                    condition3.FieldName = "emailaddress1";
                    condition3.FieldOperator = Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith;
                    condition3.FieldValue = SValue;
                    CrmServiceClient.CrmFilterConditionItem condition4 = new CrmServiceClient.CrmFilterConditionItem();
                    condition4.FieldName = "firstname";
                    condition4.FieldOperator = Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith;
                    condition4.FieldValue = SValue;
                    CrmServiceClient.CrmFilterConditionItem condition5 = new CrmServiceClient.CrmFilterConditionItem();
                    condition5.FieldName = "lastname";
                    condition5.FieldOperator = Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith;
                    condition5.FieldValue = SValue;
                    //search filters
                    CrmServiceClient.CrmSearchFilter filter1 = new CrmServiceClient.CrmSearchFilter();
                    filter1.SearchConditions.Add(condition1);
                    filter1.SearchConditions.Add(condition2);
                    filter1.SearchConditions.Add(condition3);
                    filter1.SearchConditions.Add(condition4);
                    filter1.SearchConditions.Add(condition5);
                    filter1.FilterOperator = Microsoft.Xrm.Sdk.Query.LogicalOperator.Or;
                    //searchFilters list
                    List<CrmServiceClient.CrmSearchFilter> searchFilters = new List<CrmServiceClient.CrmSearchFilter>();
                    searchFilters.Add(filter1);


                    //list of columns required in the output 
                    List<string> outputList = new List<string>();
                    outputList.Add("leadid"); outputList.Add("address1_city"); outputList.Add("subject"); outputList.Add("lastname");
                    outputList.Add("telephone1"); outputList.Add("emailaddress1"); outputList.Add("companyname"); outputList.Add("firstname");
                    //search function call 
                    outData = crmSvc.GetEntityDataBySearchParams("lead", searchFilters, CrmServiceClient.LogicalSearchOperator.Or, outputList);
                    List<DYLead> myLeads = new List<DYLead> { };
                    if (outData != null)
                    {
                        foreach (var pair in outData)
                        {
                            DYLead l = new DYLead();
                            foreach (var fields in pair.Value)
                            {
                                if (fields.Key == "subject") { l.subject = fields.Value.ToString(); }
                                else if (fields.Key == "leadid") { l.leadid = fields.Value.ToString(); }
                                else if (fields.Key == "firstname") { l.firstname = fields.Value.ToString(); }
                                else if (fields.Key == "lastname") { l.lastname = fields.Value.ToString(); }
                                else if (fields.Key == "address1_city") { l.address1_city = fields.Value.ToString(); }
                                else if (fields.Key == "telephone1") { l.address1_telephone1 = fields.Value.ToString(); }
                                else if (fields.Key == "emailaddress1") { l.emailaddress1 = fields.Value.ToString(); }
                                else if (fields.Key == "companyname") { l.companyname = fields.Value.ToString(); }
                            }
                            myLeads.Add(l);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, myLeads, HttpStatusCode.OK,false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONPOutput(callback,"Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError,true);
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.InternalServerError);

            }

        }


    }

    public class DYLeadPostData : MyValidation
    {
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string Companyname { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
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
