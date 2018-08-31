using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
using SalesForceOAuth.Web_API_Helper_Code;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead(LeadData lData)
        {

            string outputPayload;//
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false); }
            try
            {
                string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //find lead owner user
                lData.OwnerEmail = (lData.OwnerEmail == null ? "" : lData.OwnerEmail);
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
                    "where Username like '%" + lData.OwnerEmail + "%' " +
                    "OR Email like '%" + lData.OwnerEmail + "%' ").ConfigureAwait(false);
                string ownerId = "";
                string companyName = (lData.Company == "" || lData.Company == null ? "NA" : lData.Company);
                foreach (dynamic c in cont.Records)
                {
                    ownerId = c.Id; break;
                }
                SuccessResponse sR;
                dynamic newLead = new ExpandoObject();


                newLead.FirstName = lData.FirstName; newLead.LastName = lData.LastName; newLead.Company = companyName;
                newLead.Email = lData.Email;
                lData.Phone = lData.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
                newLead.Phone = String.Format("{0:(###) ###-####}", lData.Phone);

                #region Dynamic Inout Fields
                if (lData.InputFields != null)
                {
                    foreach (InputFields inputField in lData.InputFields)
                    {
                        if (inputField.Value != null)
                        {
                            MyAppsDb.AddProperty(newLead, inputField.FieldName, inputField.Value);
                            //dictionaryLead.Add(inputField.FieldName, inputField.Value);
                        }

                    }
                }
                #endregion

                Decimal value;
                if (Decimal.TryParse(lData.Phone, out value))
                    newLead.Phone = String.Format("{0:(###) ###-####}", value);
                else
                    return MyAppsDb.ConvertJSONOutput(new Exception("Phone number not in right format", null), "SFLead-PostLead", "Unhandled exception", HttpStatusCode.OK);

                if (ownerId != "" && lData.OwnerEmail != "")
                {
                    MyAppsDb.AddProperty(newLead, "OwnerId", ownerId);
                }
                if (lData.CustomFields != null)
                {
                    foreach (CustomObject c in lData.CustomFields)
                    {
                        MyAppsDb.AddProperty(newLead, c.field, c.value);
                    }
                }
                sR = await client.CreateAsync("Lead", newLead).ConfigureAwait(false);
                if (sR.Success == true)
                {
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.Id = sR.Id;
                    output.ObjectName = "Lead";
                    output.Message = "Lead added successfully!";
                    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.Conflict, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Unhandled exception", HttpStatusCode.Conflict, false);
            }
        }
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string token, string ObjectRef, int GroupId, string SValue, string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLeads-GetSearchedLeads", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false);
            }
            try
            {
                List<Lead> myLeads = new List<Lead> { };
                string cSearchField = "";
                string cSearchFieldLabels = "";
                MyAppsDb.GetAPICredentialswithCustomSearchFields(ObjectRef, GroupId, "lead", ref AccessToken, ref ApiVersion, ref InstanceUrl, ref cSearchField, ref cSearchFieldLabels, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                StringBuilder filters = new StringBuilder();
                string[] customSearchFieldArray = cSearchField.Split('|');
                string[] customSearchLabelArray = cSearchFieldLabels.Split('|');
                if (cSearchField.Length > 0)
                {
                    foreach (string csA in customSearchFieldArray)
                    {
                        columns.Append("," + csA);
                        filters.Append("OR " + csA + " like '%" + SValue + "%' ");
                    }
                }
                //Id, FirstName, LastName, Company, Email, Phone
                query.Append("SELECT Id, FirstName, LastName, Company, Email, Phone " + columns.ToString() + " From Lead ");
                query.Append("where Name like '%" + SValue + "%' ");
                query.Append("OR FirstName like '%" + SValue + "%' ");
                query.Append("OR LastName like '%" + SValue + "%' ");
                query.Append("OR Email like '%" + SValue + "%' ");
                query.Append("OR Phone like '%" + SValue + "%' ");
                query.Append(filters.ToString());
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        Lead l = new Lead();
                        l.Id = c.Id;
                        l.FirstName = (c.FirstName != null ? c.FirstName : "");
                        l.LastName = (c.LastName != null ? c.LastName : "");
                        l.Company = (c.Company != null ? c.Company : "");
                        l.Email = (c.Email != null ? c.Email : "");
                        l.Phone = (c.Phone != null ? c.Phone : "");


                        if (cSearchField.Length > 0)
                        {
                            int noOfcustomItems = 0; int i = 0;
                            foreach (Newtonsoft.Json.Linq.JProperty item in c)
                            {

                                foreach (string csA in customSearchFieldArray)
                                {
                                    if (item.Name.ToLower() == csA.ToLower())
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        //MyAppsDb.AssignCustomVariableValue(l, item.Name, item.Value.ToString(), noOfcustomItems);
                                        MyAppsDb.AssignCustomVariableValue(l, customSearchLabelArray[i], item.Value.ToString(), noOfcustomItems);
                                        i++;
                                    }

                                }
                            }
                        }
                        myLeads.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myLeads, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.Conflict, false);
            }
        }
    }


    public class PostedObjectDetail
    {
        public string ObjectName { get; set; }
        public string Id { get; set; }
        public string Message { get; set; }
    }
    public class CustomObject
    {
        public string field { get; set; }
        public string value { get; set; }
    }
    public class DYCustomObject
    {
        public string field { get; set; }
        public string type { get; set; }
        public string table { get; set; }
        public string value { get; set; }
    }
    public class CustomFields
    {
        public string field { get; set; }
    }



    public class LeadData : MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OwnerEmail { get; set; }
        public List<CustomObject> CustomFields { get; set; }
        public List<InputFields> InputFields { get; set; }
    }

    public class SearchLeadData : SecureInfo
    {
        public string searchObject { get; set; }
        public string searchValue { get; set; }
    }
    public class UserAccounts
    {
        public const String SObjectTypeName = "Users";
        public String Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
    public class Lead
    {
        public const String SObjectTypeName = "Lead";
        public String Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
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
        public dynamic AccountId { get; internal set; }
    }
    public class LeadOW
    {
        public const String SObjectTypeName = "Lead";
        public String Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OwnerId { get; set; }
    }
    public class HttpActionResult : IHttpActionResult
    {
        private readonly string _message;
        private readonly HttpStatusCode _statusCode;

        public HttpActionResult(HttpStatusCode statusCode, string message)
        {
            _statusCode = statusCode;
            _message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_message)
            };
            return Task.FromResult(response);
        }
    }

    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// Returns a dictionary of QueryStrings that's easier to work with 
        /// than GetQueryNameValuePairs KevValuePairs collection.
        /// 
        /// If you need to pull a few single values use GetQueryString instead.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryStrings(HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns an individual querystring value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryString(this HttpRequestMessage request, string key)
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
                return null;

            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, true) == 0);
            if (string.IsNullOrEmpty(match.Value))
                return null;

            return match.Value;
        }

        /// <summary>
        /// Returns an individual HTTP Header value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetHeader(this HttpRequestMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return null;

            return keys.First();
        }

        /// <summary>
        /// Retrieves an individual cookie from the cookies collection
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookie(this HttpRequestMessage request, string cookieName)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault();
            if (cookie != null)
                return cookie[cookieName].Value;

            return null;
        }

    }

}
