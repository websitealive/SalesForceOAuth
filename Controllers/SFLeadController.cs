using Newtonsoft.Json;
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
    public class SFLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead([FromBody] LeadData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    string InstanceUrl="", AccessToken ="", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref  ApiVersion, ref InstanceUrl); 

                    ForceClient client = new ForceClient(InstanceUrl.Trim(), AccessToken.Trim(), ApiVersion.Trim());
                    var lead = new Lead { FirstName = lData.FirstName, LastName = lData.LastName, Company = "-", Email = lData.Email, Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Lead", lead);
                    if (sR.Success == true)
                    {
                        outputResponse.StatusCode = HttpStatusCode.Created;
                        outputResponse.Content = new StringContent("Lead added successfully!");
                        return outputResponse;
                    }
                    else
                    {
                        outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                        outputResponse.Content = new StringContent("Lead could not be added!");
                        return outputResponse;
                    }
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Lead could not be added!");
                    return outputResponse;
                }
            }
            outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            outputResponse.Content = new StringContent("Your request isn't authorized!");
            return outputResponse;
        }
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string sObj, string sValue)
        {
            string ValidationKey="", InstanceUrl="", AccessToken="", ApiVersion="";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            var re = Request;
            var headers = re.Headers;
            if (headers.Contains("ValidationKey") && headers.Contains("InstanceUrl") && headers.Contains("AccessToken") && headers.Contains("ApiVersion"))
            {
                ValidationKey = HttpRequestMessageExtensions.GetHeader(re, "ValidationKey"); //headers.GetValues("ValidationKey").First();
                InstanceUrl = HttpRequestMessageExtensions.GetHeader(re, "InstanceUrl"); //headers.GetValues("InstanceUrl").First();
                AccessToken = HttpRequestMessageExtensions.GetHeader(re, "AccessToken"); //headers.GetValues("AccessToken").First();
                ApiVersion = HttpRequestMessageExtensions.GetHeader(re, "ApiVersion"); //headers.GetValues("ApiVersion").First();
            }
            else
            {
                outputResponse.StatusCode = HttpStatusCode.Unauthorized;
                outputResponse.Content = new StringContent("Your request isn't authorized!");
                return outputResponse;
            }
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                List<Lead> myLeads = new List<Lead> { };
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    string objectToSearch = sObj;
                    string objectValue = sValue; 
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, FirstName, LastName, Company, Email, Phone From Lead where " + objectToSearch + " like '%" + objectValue + "%'");
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
                    outputResponse.StatusCode = HttpStatusCode.OK;
                    outputResponse.Content = new StringContent(JsonConvert.SerializeObject(myLeads), Encoding.UTF8, "application/json");
                    return outputResponse;
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Error occured while searching for Leads");
                    return outputResponse;
                }
            }
            else
            {
                outputResponse.StatusCode = HttpStatusCode.Unauthorized;
                outputResponse.Content = new StringContent("Your request isn't authorized!");
                return outputResponse;
            } 
        }

    }
    public class LeadData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
    public class SearchLeadData: SecureInfo
    {
        public string searchObject { get; set; }
        public string searchValue { get; set; }
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
