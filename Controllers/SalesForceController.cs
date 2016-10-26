using Salesforce.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Salesforce.Common.Models;
using Salesforce.Force;
using Newtonsoft.Json;
using System.Configuration;

namespace SalesForceOAuth.Controllers
{
    public class Contact
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class SalesForceController : ApiController
    {
        /// <summary>
        /// GET: api/SalesForce
        /// </summary>
        /// <returns>string array saying API head line</returns>
        public IEnumerable<string> GetApiInfo()
        {
            return new string[] { "This Api is meant to work with website alive chat application." };
        }


        /// <summary>
        /// GET: api/SalesForce/GetRedirectURL?ValidationKey= ffe06298-22a8-4849-a46c-0284b04f2561
        /// This is the first step of OAuth, getting the URL to redirect to. 
        /// </summary>
        /// <param name="ValidationKey"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetRedirectURL")]
        public string GetRedirectURL(string ValidationKey)
        {
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            { 
                // Response.Write("started TEsting");
                var url =
                Common.FormatAuthUrl(
                    "https://login.salesforce.com/services/oauth2/authorize",
                    ResponseTypes.Code,
                    "3MVG9KI2HHAq33RwXJsqtsEtY.ThMCzS5yZd3S8CzXBArijS0WEQgYACVnQ9SJq0KDdKrQgIxPFNPOIQhuqdK",
                    System.Web.HttpUtility.UrlEncode("http://localhost:56786/About.aspx"));
                return url;
            }
            return "Error: Authenticating App"; 
            
        }

        // GET: api/SalesForce/GetAuthorizationToken?ValidationKey= ffe06298-22a8-4849-a46c-0284b04f2561
        [HttpGet]
        [ActionName("GetAuthorizationToken")]
        public string GetAuthorizationToken(string AuthCode, string ValidationKey)
        {
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                string _consumerKey = "3MVG9KI2HHAq33RwXJsqtsEtY.ThMCzS5yZd3S8CzXBArijS0WEQgYACVnQ9SJq0KDdKrQgIxPFNPOIQhuqdK";
                string _consumerSecret = "7849687416745281703";
                string _callbackUrl = "http://localhost:56786/About.aspx";
                string _tokenRequestEndpointUrl = "https://login.salesforce.com/services/oauth2/token";
                string code = AuthCode;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                try
                {
                    auth.WebServerAsync(_consumerKey, _consumerSecret, _callbackUrl, code, _tokenRequestEndpointUrl).Wait();
                    
                }
                catch(Exception ex)
                {
                    return "Error authenticating" + ex.InnerException; 
                }
                var url = string.Format("/?token={0}&api={1}&instance_url={2}&refresh_token={3}",
                    auth.AccessToken,
                    auth.ApiVersion,
                    auth.InstanceUrl,
                    auth.RefreshToken);
                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(url, UriKind.Relative);

                string output = string.Format("token={0}&api={1}&instance_url={2}",
                    auth.AccessToken,
                    auth.ApiVersion,
                    auth.InstanceUrl
                    ); 
                return output; 
            }
            return "Error: Authenticating App"; 
        }

        // GET: api/SalesForce/GetLeadToken?ValidationKey= ffe06298-22a8-4849-a46c-0284b04f2561
        [HttpGet]
        [ActionName("GetTotalLeads")]
        public async System.Threading.Tasks.Task<List<Lead>> GetAllLeads(string AccessToken, string InstanceUrl, string ApiVersion, string ValidationKey)
        {
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                List<Lead> myLeads = new List<Lead> { };
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, FirstName, LastName, Company From Lead");
                    foreach (dynamic c in cont.Records)
                    {
                        Lead l = new Lead();
                        l.Id = c.Id;
                        l.FirstName = c.FirstName;
                        l.LastName = c.LastName;
                        l.Company = c.Company;
                        myLeads.Add(l); 
                    }
                    return myLeads; 
                }
                catch (Exception ex)
                {
                    return myLeads;
                }
            }
            return null; // if not authenticated 
        }
        // GET: api/SalesForce/GetLeadToken?ValidationKey= ffe06298-22a8-4849-a46c-0284b04f2561
      
        /// <summary>
        /// The function will post chat messages to lead description
        /// </summary>
        /// <param name="lData">An object data type converted to jason format</param>
        /// <returns></returns>
        

    }
  
    public class SecureInfo
    {
        public string AccessToken { get; set; }
        public string InstanceUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ValidationKey { get; set; }
    }
    //public class LeadMessageData: SecureInfo
    //{
    //    public string LeadId { get; set; }
    //    public string Messsage { get; set; }
    //}
    
    
 
}