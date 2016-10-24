using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead(LeadData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
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

        [HttpPost]
        public async System.Threading.Tasks.Task<List<Lead>> SearchedLeads(SearchLeadData sData)
        {
            if (sData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                List<Lead> myLeads = new List<Lead> { };
                try
                {
                    ForceClient client = new ForceClient(sData.InstanceUrl, sData.AccessToken, sData.ApiVersion);
                    string objectToSearch = sData.searchObject;
                    string objectValue = sData.searchValue; 
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

    }
    public class LeadData : SecureInfo
    {
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
}
