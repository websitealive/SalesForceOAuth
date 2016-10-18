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
            if (ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
            { 
                // Response.Write("started TEsting");
                var url =
                Common.FormatAuthUrl(
                    "https://login.salesforce.com/services/oauth2/authorize",
                    ResponseTypes.Code,
                    "3MVG9HxRZv05HarTzRb2msaMZ2puUcXjnYXV1FQAcWN3zVPJ0BZE65IFJ9TzL8Oar5tOFmSkZlH8iGfvIy2wR",
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
            if (ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
            {
                string _consumerKey = "3MVG9HxRZv05HarTzRb2msaMZ2puUcXjnYXV1FQAcWN3zVPJ0BZE65IFJ9TzL8Oar5tOFmSkZlH8iGfvIy2wR";
                string _consumerSecret = "7781222679202251199";
                string _callbackUrl = "http://localhost:56786/About.aspx";
                string _tokenRequestEndpointUrl = "https://login.salesforce.com/services/oauth2/token";

                string code = AuthCode;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                auth.WebServerAsync(_consumerKey, _consumerSecret, _callbackUrl, code, _tokenRequestEndpointUrl).Wait();

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
            if (ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
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
        [HttpGet]
        [ActionName("GetAddNewLead")]
        public async System.Threading.Tasks.Task<string> GetAddNewLead(string ParamFirstName, string ParamLastName, string AccessToken, string InstanceUrl, string ApiVersion, string ValidationKey)
        {
            if (ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
            { 
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);

                    var lead = new Lead { FirstName = ParamFirstName, LastName = ParamLastName, Company = "-"};
                    SuccessResponse sR = await client.CreateAsync("Lead",lead);
                    if (sR.Success == true)
                    {
                        return "Added Successfully";
                    }
                    else
                        return "Not Added Successfully"; 
                }
                catch (Exception ex)
                {
                    return ex.InnerException.ToString();
                }
            }
            return "Error: Authenticating App";
        }

        //[HttpPost]
        //[ActionName("PostNewLead")]
        //public async System.Threading.Tasks.Task<string> PostNewLead(LeadData lData)
        //{
        //    if (lData.ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
        //    {
        //        try
        //        {
        //            ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);

        //            var lead = new Lead { FirstName = lData.FirstName, LastName = lData.LastName, Company = lData.Company };
        //            SuccessResponse sR = await client.CreateAsync("Lead", lead);
        //            if (sR.Success == true)
        //            {
        //                return "Added Successfully";
        //            }
        //            else
        //                return "Not Added Successfully";
        //        }
        //        catch (Exception ex)
        //        {
        //            return ex.InnerException.ToString();
        //        }
        //    }
        //    return "Error: Authenticating App";
        //}

        /// <summary>
        /// The function will post chat messages to lead description
        /// </summary>
        /// <param name="lData">An object data type converted to jason format</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("PostAddLeadMessage")]
        public async System.Threading.Tasks.Task<string> PostAddLeadMessage(LeadMessageData lData)
        {
            if (lData.ValidationKey ==  Environment.GetEnvironmentVariable("APISecureKey"))
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    var readLead = await client.QueryAsync<LeadDescription>("SELECT Id, Description From Lead where Id ='" + lData.LeadId + "'");
                    if (readLead.TotalSize > 0)
                    {
                        LeadDescription lDesc = readLead.Records[0];
                        var lACall = new TaskLogACall { Subject = "WebsiteAlive-Chat", Description = lData.Messsage,WhoId= lData.LeadId, Status="Completed"};
                        SuccessResponse sR = await client.CreateAsync("Task", lACall);

                        //SuccessResponse sR = await client.UpdateAsync("Lead", lDesc.Id, new { Description = lData.Messsage + "\r\n" + lDesc.Description });
                        if (sR.Success == true)
                        {
                            return "Update Lead Successfully";
                        }
                    }
                    return "Not Added Feed Successfully";
                }
                catch (Exception ex)
                {
                    return ex.InnerException.ToString();
                }
            }
            return "Error: Authenticating App";
        }

    }

    public class TaskLogACall
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string WhoId { get; set; }
        public string Status { get; set; }
    }
    public class SecureInfo
    {
        public string AccessToken { get; set; }
        public string InstanceUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ValidationKey { get; set; }
    }
    public class LeadMessageData: SecureInfo
    {
        public string LeadId { get; set; }
        public string Messsage { get; set; }
    }
    public class LeadData : SecureInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
    }
    public class Lead
    {
        public const String SObjectTypeName = "Lead";
        public String Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
    }
    public class LeadDescription
    {
        public String Id { get; set; }
        public string Description { get; set; }
    }
}
//00Q0Y0000017bUsUAI