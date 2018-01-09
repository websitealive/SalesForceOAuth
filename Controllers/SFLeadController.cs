using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef),urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {       return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode,true);     }
            try
                {
                    string InstanceUrl="", AccessToken ="", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref  ApiVersion, ref InstanceUrl,urlReferrer);
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
                        ownerId = c.Id;  break;
                    }
                    SuccessResponse sR;
                    dynamic newLead = new ExpandoObject();
                    newLead.FirstName = lData.FirstName; newLead.LastName = lData.LastName; newLead.Company = companyName;
                    newLead.Email = lData.Email; newLead.Phone = lData.Phone;
                    //if (ownerId != "" || lData.OwnerEmail != ""
                    if (ownerId != "" && lData.OwnerEmail != "")
                    {
                        MyAppsDb.AddProperty(newLead, "OwnerId", ownerId);
                        //var lead = new Lead { FirstName = lData.FirstName, LastName = lData.LastName, Company = companyName, Email = lData.Email, Phone = lData.Phone };
                        //sR = await client.CreateAsync("Lead", lead).ConfigureAwait(false);
                    }
                    if (lData.CustomFields != null)
                    {
                        foreach (CustomObject c in lData.CustomFields)
                        {
                            MyAppsDb.AddProperty(newLead, c.field, c.value);
                        }
                    }
                    //else
                    //{
                    //    //var leadow = new LeadOW { FirstName = lData.FirstName, LastName = lData.LastName, Email = lData.Email, Phone = lData.Phone, OwnerId = ownerId, Company = companyName };
                    //    //sR = await client.CreateAsync("Lead", leadow).ConfigureAwait(false);
                    //    newLead.OwnerId = ownerId;
                    //}
                    sR = await client.CreateAsync("Lead", newLead).ConfigureAwait(false);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Lead";
                        output.Message = "Lead added successfully!";
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string token, string ObjectRef, int GroupId,string SValue, string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLeads-GetSearchedLeads", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef),urlReferrer); 
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode,true); }
            try
                {
                    List<Lead> myLeads = new List<Lead> { };
                    MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    string objectValue = SValue;
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11; 
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, FirstName, LastName, Company, Email, Phone From Lead " +
                        "where FirstName like '%" + SValue + "%' " +
                        "OR LastName like '%" + SValue + "%' " +
                        "OR Email like '%" + SValue + "%' " +
                        "OR Phone like '%" + SValue + "%' " 
                        ).ConfigureAwait(false);
                    if (cont.Records.Count > 0)
                    {
                        foreach (dynamic c in cont.Records)
                        {
                            Lead l = new Lead();
                            l.Id = c.Id;
                            l.FirstName = c.FirstName;
                            l.LastName = c.LastName;
                            l.Company = c.Company;
                            l.Email = c.Email;
                            l.Phone = c.Phone;
                            myLeads.Add(l);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback,myLeads, HttpStatusCode.OK,false);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.InternalServerError);
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
    }
    
    public class SearchLeadData: SecureInfo
    {
        public string searchObject { get; set; }
        public string searchValue { get; set; }
    }
    public class UserAccounts
    {
        public const String SObjectTypeName = "Users";
        public String Id { get; set; }
        public string Username { get; set; }
        public string Email  { get; set; }
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
