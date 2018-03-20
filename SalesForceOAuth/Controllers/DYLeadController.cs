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
            try
            {
                //Test system
                //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId , ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                if (crmSvc != null && crmSvc.IsReady)
                {
                    //create Account object
                    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                    inData.Add("companyname", new CrmDataTypeWrapper(lData.Companyname, CrmFieldType.String));
                    inData.Add("firstname", new CrmDataTypeWrapper(lData.Firstname, CrmFieldType.String));
                    inData.Add("lastname", new CrmDataTypeWrapper(lData.Lastname, CrmFieldType.String));
                    inData.Add("subject", new CrmDataTypeWrapper(lData.Subject, CrmFieldType.String));
                    //inData.Add("address1_city", new CrmDataTypeWrapper(lData.City, CrmFieldType.String));
                    //inData.Add("address1_telephone1", new CrmDataTypeWrapper(lData.Phone, CrmFieldType.String));
                    //inData.Add("emailaddress1", new CrmDataTypeWrapper(lData.Email, CrmFieldType.String));
                    if (lData.CustomFields != null)
                    {
                        foreach (DYCustomObject c in lData.CustomFields)
                        {
                            CrmFieldType type;
                            switch (c.type.ToLower())
                            {
                                case "string":
                                    { type = CrmFieldType.String; break; }
                                case "decimal":
                                    { type = CrmFieldType.CrmDecimal; break; }
                                case "lookup":
                                    { type = CrmFieldType.Lookup; break; }
                                case "bool":
                                    { type = CrmFieldType.CrmBoolean; break; }
                                default:
                                    { type = CrmFieldType.String; break; }
                            }
                            if (type == CrmFieldType.Lookup)
                            {
                                if (c.value.ToString().Length > 0)
                                {
                                    inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));
                                }
                            }
                            else
                                inData.Add(c.field, new CrmDataTypeWrapper(c.value, type));
                        }
                    }


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
                //string ApplicationURL = "https://alive5.crm11.dynamics.com", userName = "alive5@alive5.onmicrosoft.com",
                //password = "Passw0rd1", authType = "Office365";
                //Live system
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
                    List<DYLead> listToReturn = new List<DYLead>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "fullname";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue + "%");
                    //filter email
                    ConditionExpression filterOwnRcd2 = new ConditionExpression();
                    filterOwnRcd2.AttributeName = "emailaddress1";
                    filterOwnRcd2.Operator = ConditionOperator.Like;
                    filterOwnRcd2.Values.Add("%" + SValue + "%");
                    //filter subject
                    ConditionExpression filterOwnRcd3 = new ConditionExpression();
                    filterOwnRcd3.AttributeName = "subject";
                    filterOwnRcd3.Operator = ConditionOperator.Like;
                    filterOwnRcd3.Values.Add("%" + SValue + "%");


                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);
                    filter1.Conditions.Add(filterOwnRcd2);
                    filter1.Conditions.Add(filterOwnRcd3);
                    filter1.FilterOperator = LogicalOperator.Or;
                    QueryExpression query = new QueryExpression("lead");
                    query.ColumnSet.AddColumns("leadid", "address1_city", "subject", "lastname", "telephone1", "emailaddress1", "companyname", "firstname");
                   query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.InternalServerError);

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
        public List<DYCustomObject> CustomFields { get; set; }


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
