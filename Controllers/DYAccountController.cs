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
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://naveedzafar30.crm11.dynamics.com", userName = "naveedzafar30@naveedzafar30.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //string ApplicationURL = "https://websitealive.crmgate.pk/websitealive", userName = "naveed@crmgate.local",
                //    password = "@Abc.123", authType = "IFD";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef,lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                connectionString += "RequireNewInstance=true;";
                CrmServiceClient crmSvc = new CrmServiceClient(connectionString);
                
                if (crmSvc != null && crmSvc.IsReady)
                {
                    //create Account object
                    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                    inData.Add("name", new CrmDataTypeWrapper(lData.Name, CrmFieldType.String));
                    inData.Add("accountnumber", new CrmDataTypeWrapper(lData.AccountNumber, CrmFieldType.String));
                    inData.Add("description", new CrmDataTypeWrapper(lData.Description, CrmFieldType.String));
                    inData.Add("telephone1", new CrmDataTypeWrapper(lData.Phone, CrmFieldType.String));
                    Guid accountId = crmSvc.CreateNewRecord("account", inData);
                    if (accountId != Guid.Empty)
                    {
                        //Console.WriteLine("Account created.");
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = accountId.ToString();
                        pObject.ObjectName = "Account";
                        pObject.Message = "Account added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new account, check mandatory fields", HttpStatusCode.InternalServerError,true);
                    }
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.InternalServerError, true);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
            //End connect to SDK


            //    #region dynamics api call
            //    HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(lData.ObjectRef, lData.GroupId.ToString());
            //    //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
            //    if (msg.StatusCode == HttpStatusCode.OK)
            //    {
            //        AccessToken = msg.Content.ReadAsStringAsync().Result; }
            //    else
            //    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }
            //    HttpClient client = new HttpClient();
            //    client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
            //    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
            //    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            //    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            //    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            //    StringBuilder requestURI = new StringBuilder();
            //    requestURI.Append("/api/data/v8.0/accounts");
            //    DYAccountPostValue aData = new DYAccountPostValue();
            //    aData.name = lData.Name;
            //    aData.description = lData.Description;
            //    aData.accountnumber = lData.AccountNumber;
            //    aData.address1_telephone1 = lData.Phone;
            //    StringContent content = new StringContent(JsonConvert.SerializeObject(aData), Encoding.UTF8, "application/json");
            //    HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var output = response.Headers.Location.OriginalString;
            //        var id = output.Substring(output.IndexOf("(") + 1, 36);
            //        PostedObjectDetail pObject = new PostedObjectDetail();
            //        pObject.Id = id;
            //        pObject.ObjectName = "Account";
            //        pObject.Message = "Account added successfully!";
            //        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK);
            //    }
            //    else
            //    {
            //        return MyAppsDb.ConvertJSONOutput("Dynamics Error: " + response.StatusCode, HttpStatusCode.InternalServerError);
            //    }
            //    #endregion dynamics api call
            //}
            //catch (Exception ex)
            //{
            //    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
            //}
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
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
                    filter1.FilterOperator = LogicalOperator.Or; 
                    QueryExpression query = new QueryExpression("account");
                    query.ColumnSet.AddColumns("accountid", "address1_city", "accountnumber", "telephone1", "emailaddress1", "name");
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
                            listToReturn.Add(info);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.InternalServerError);
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
    }
    
    public class DYAccountOutput: DYAccount
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
