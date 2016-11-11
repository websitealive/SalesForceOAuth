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
    public class SFAccountController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount([FromBody] AccountData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Account", acc);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Lead";
                        output.Message = "Account added successfully!";
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

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string ObjectRef, int GroupId, string ValidationKey,string sValue, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                List<Account> myAccounts = new List<Account> { };
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    string objectValue = sValue;
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, AccountNumber, Name, Phone From Account " +
                        "where AccountNumber like '%" + sValue + "%' " 
                        + "OR Name like '%" + sValue + "%' "
                        + "OR Phone like '%" + sValue + "%'" ); 
                    foreach (dynamic c in cont.Records)
                    {
                        Account l = new Account();
                        l.Id = c.Id;
                        l.AccountNumber = c.AccountNumber;
                        l.Name = c.Name; 
                        l.Phone = c.Phone;
                        myAccounts.Add(l);
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, myAccounts, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return MyAppsDb.ConvertJSONPOutput(callback, "Your request isn't authorized!", HttpStatusCode.Unauthorized);
            }
        }
    }
    public class AccountData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
    public class Account
    {
        public String Id { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
