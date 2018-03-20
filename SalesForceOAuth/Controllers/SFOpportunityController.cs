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
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFOpportunityController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostOpportunity()
        {
            var re = Request;
            var headers = re.Headers;
            if (headers.Contains("Authorization"))
            {
                string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(_token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                   return MyAppsDb.ConvertJSONOutput(ex, "SFOpportunity-PostOpportunity", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
                }
                JObject values = JObject.Parse(outputPayload); // parse as array  
                OpportunityData lData = new OpportunityData();
                lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                lData.ObjectRef = values.GetValue("ObjectRef").ToString();
                lData.AccountId = values.GetValue("AccountId").ToString();
                lData.Amount = Convert.ToDecimal(values.GetValue("Amount").ToString());
                lData.CloseDate = Convert.ToDateTime( values.GetValue("CloseDate").ToString());
                lData.Name  = values.GetValue("Name").ToString();
                lData.StageName  = values.GetValue("StageName").ToString();
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
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
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SF-PostOpportunity", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,true);
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
