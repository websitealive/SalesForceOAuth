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
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
                //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex.InnerException, HttpStatusCode.InternalServerError);
            }

            //Access token update
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef));
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

            //JObject values = JObject.Parse(outputPayload); // parse as array  
            //AccountData lData = new AccountData();
            //lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
            //lData.ObjectRef = values.GetValue("ObjectRef").ToString();
            //lData.Name = values.GetValue("Name").ToString();
            //lData.Phone = values.GetValue("Phone").ToString();
            //lData.AccountNumber = values.GetValue("AccountNumber").ToString();
            try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                SuccessResponse sR = await client.CreateAsync("Account", acc);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Account";
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
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string token, string ObjectRef, int GroupId, string SValue, string siteRef, string callback)
        {
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
                //string ObjectRef = "", SValue = "";
                //int GroupId = 0; 
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback,ex.InnerException, HttpStatusCode.InternalServerError);
            }
            //Access token update
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef));
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }
            //JObject values = JObject.Parse(outputPayload); // parse as array  
            //GroupId = Convert.ToInt32( values.GetValue("GroupId").ToString());
            //ObjectRef = values.GetValue("ObjectRef").ToString();
            //SValue = values.GetValue("SValue").ToString();

            MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
            List<Account> myAccounts = new List<Account> { };
            try
            {
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, AccountNumber, Name, Phone From Account " +
                    "where AccountNumber like '%" + SValue + "%' " 
                    + "OR Name like '%" + SValue + "%' "
                    + "OR Phone like '%" + SValue + "%'" ); 
                foreach (dynamic c in cont.Records)
                {
                    Account l = new Account();
                    l.Id = c.Id;
                    l.AccountNumber = c.AccountNumber;
                    l.Name = c.Name; 
                    l.Phone = c.Phone;
                    myAccounts.Add(l);
                }
                return MyAppsDb.ConvertJSONPOutput(callback,myAccounts, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
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
