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
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostOpportunity([FromBody] OpportunityData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
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
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Opportunity";
                        output.Message = "Opportunity added successfully!";
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }
        
    }
    public class OpportunityData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
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
