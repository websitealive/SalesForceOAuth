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
    public class SFAccountController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount(AccountData lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef),urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode,true);
            }
            try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                SuccessResponse sR = await client.CreateAsync("Account", acc).ConfigureAwait(false);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Account";
                        output.Message = "Account added successfully!";
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string token, string ObjectRef, int GroupId, string SValue, string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
               return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef),urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode,true);
            }
            List<Account> myAccounts = new List<Account> { };
            try
            {
                MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                
                string objectValue = SValue;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, AccountNumber, Name, Phone From Account " +
                    "where AccountNumber like '%" + SValue + "%' " 
                    + "OR Name like '%" + SValue + "%' "
                    + "OR Phone like '%" + SValue + "%'" ).ConfigureAwait(false); 
                foreach (dynamic c in cont.Records)
                {
                    Account l = new Account();
                    l.Id = c.Id;
                    l.AccountNumber = c.AccountNumber;
                    l.Name = c.Name; 
                    l.Phone = c.Phone;
                    myAccounts.Add(l);
                }
                return MyAppsDb.ConvertJSONPOutput(callback,myAccounts, HttpStatusCode.OK,true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
            //}
            //else
            //{
            //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
            //}
        }
    }

    public class PostToken
    {
        public string token { get; set; }
    }

    public class AccountData : MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
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
