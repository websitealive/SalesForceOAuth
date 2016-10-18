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
    public class SFLeadsController : ApiController
    {

           // GET: api/SalesForce/GetAddNewLead?ValidationKey= ffe06298-22a8-4849-a46c-0284b04f2561
        [HttpGet]
        [ActionName("GetAddNewLead")]
        public async System.Threading.Tasks.Task<string> GetAddNewLead(string ParamFirstName, string ParamLastName, string AccessToken, string InstanceUrl, string ApiVersion, string ValidationKey)
        {
            if (ValidationKey == "ffe06298-22a8-4849-a46c-0284b04f2561")
            {
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);

                    var lead = new Lead { FirstName = ParamFirstName, LastName = ParamLastName };
                    SuccessResponse sR = await client.CreateAsync(Lead.SObjectTypeName, lead);
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


        [HttpPost]
        [ActionName("PostAddLeadMessage")]
        public async System.Threading.Tasks.Task<string> PostLogACallTask(LogACall lData)
        {
            if (lData.ValidationKey == Environment.GetEnvironmentVariable("APISecureKey"))
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    var readLead = await client.QueryAsync<LeadDescription>("SELECT Id, Description From Lead where Id ='" + lData.LeadId + "'");
                    if (readLead.TotalSize > 0)
                    {
                        LeadDescription lDesc = readLead.Records[0];
                        SuccessResponse sR = await client.UpdateAsync("Lead", lDesc.Id, new { Description = lData.Messsage + "\r\n" + lDesc.Description });
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


        // PUT: api/SFLeads/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SFLeads/5
        public void Delete(int id)
        {
        }
    }
    public class LogACall : SecureInfo
    {
        public string LeadId { get; set; }
        public string Messsage { get; set; }
    }
}
