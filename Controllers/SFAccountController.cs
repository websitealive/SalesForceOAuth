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
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount(AccountData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Account", acc);
                    if (sR.Success == true)
                    {
                        outputResponse.StatusCode = HttpStatusCode.Created;
                        outputResponse.Content = new StringContent("Account added successfully!");
                        return outputResponse;
                    }
                    else
                    {
                        outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                        outputResponse.Content = new StringContent("Account could not be added!");
                        return outputResponse;
                    }
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Account could not be added!");
                    return outputResponse;
                }
            }
            outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            outputResponse.Content = new StringContent("Your request isn't authorized!");
            return outputResponse;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string sObj, string sValue)
        {
            string ValidationKey = "", InstanceUrl = "", AccessToken = "", ApiVersion = "";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            var re = Request;
            var headers = re.Headers;
            if (headers.Contains("ValidationKey") && headers.Contains("InstanceUrl") && headers.Contains("AccessToken") && headers.Contains("ApiVersion"))
            {
                ValidationKey = HttpRequestMessageExtensions.GetHeader(re, "ValidationKey"); 
                InstanceUrl = HttpRequestMessageExtensions.GetHeader(re, "InstanceUrl"); 
                AccessToken = HttpRequestMessageExtensions.GetHeader(re, "AccessToken"); 
                ApiVersion = HttpRequestMessageExtensions.GetHeader(re, "ApiVersion"); ;
            }
            else
            {
                outputResponse.StatusCode = HttpStatusCode.Unauthorized;
                outputResponse.Content = new StringContent("Your request isn't authorized!");
                return outputResponse;
            }
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                List<Account> myAccounts = new List<Account> { };
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    string objectToSearch = sObj;
                    string objectValue = sValue;
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, AccountNumber, Name, Phone From Account where " + objectToSearch + " like '%" + objectValue + "%'");
                    foreach (dynamic c in cont.Records)
                    {
                        Account l = new Account();
                        l.Id = c.Id;
                        l.AccountNumber = c.AccountNumber;
                        l.Name = c.Name; 
                        l.Phone = c.Phone;
                        myAccounts.Add(l);
                    }
                    outputResponse.StatusCode = HttpStatusCode.OK;
                    outputResponse.Content = new StringContent(JsonConvert.SerializeObject(myAccounts), Encoding.UTF8, "application/json");
                    return outputResponse;
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Error occured while searching for Accounts");
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
    public class AccountData : SecureInfo
    {
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
