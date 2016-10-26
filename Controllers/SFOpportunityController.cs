using Newtonsoft.Json;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFOpportunityController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostOpportunity(OpportunityData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    var opp = new MyOpportunity
                    {
                        AccountId = lData.AccountId,
                        Name = lData.Name, 
                        CloseDate = lData.CloseDate , 
                        Amount = lData.Amount, 
                        StageName = "Prospecting"
                    };
                    SuccessResponse sR = await client.CreateAsync("Opportunity", opp);
                    if (sR.Success == true)
                    {
                        outputResponse.StatusCode = HttpStatusCode.Created;
                        outputResponse.Content = new StringContent("Opportunity added successfully!");
                        return outputResponse;
                    }
                    else
                    {
                        outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                        outputResponse.Content = new StringContent("Opportunity could not be added!");
                        return outputResponse;
                    }
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Opportunity could not be added!");
                    return outputResponse;
                }
            }
            outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            outputResponse.Content = new StringContent("Your request isn't authorized!");
            return outputResponse;
        }
        
    }
    public class OpportunityData : SecureInfo
    {
        public string AccountId { get; set; } // for reference
        public string Name { get; set; }
        public DateTime CloseDate { get; set; }
        public string StageName { get; set; }
        public decimal Amount { get; set; }
    }
    public class MyOpportunity
    {
        public String Id { get; set; }
        public string AccountId { get; set; } // for reference
        public string Name { get; set; }
        public DateTime CloseDate { get; set; }
        public string StageName { get; set; }
        public decimal Amount { get; set; }
    }
}
