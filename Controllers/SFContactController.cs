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
    public class SFContactController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact(ContactData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    var cont = new MyContact {AccountId = lData.AccountId, FirstName = lData.FirstName,LastName = lData.LastName,
                        Email = lData.Email  , Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Contact", cont);
                    if (sR.Success == true)
                    {
                        outputResponse.StatusCode = HttpStatusCode.Created;
                        outputResponse.Content = new StringContent("Contact added successfully!");
                        return outputResponse;
                    }
                    else
                    {
                        outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                        outputResponse.Content = new StringContent("Contact could not be added!");
                        return outputResponse;
                    }
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Contact could not be added!");
                    return outputResponse;
                }
            }
            outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            outputResponse.Content = new StringContent("Your request isn't authorized!");
            return outputResponse;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string sObj, string sValue)
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
                List<MyContact> myContacts = new List<MyContact> { };
                try
                {
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    string objectToSearch = sObj;
                    string objectValue = sValue;
                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(
                        "SELECT Id, FirstName, LastName, Email, Phone, AccountId From Contact where " +  objectToSearch + " like '%" + objectValue + "%'");
                    foreach (dynamic c in cont.Records)
                    {
                        MyContact mc = new MyContact();
                        mc.Id = c.Id; mc.FirstName = c.FirstName;mc.LastName = c.LastName; mc.Email = c.Email;
                        mc.AccountId = c.AccountId; mc.Phone = c.Phone;
                        myContacts.Add(mc);
                    }
                    outputResponse.StatusCode = HttpStatusCode.OK;
                    outputResponse.Content = new StringContent(JsonConvert.SerializeObject(myContacts), Encoding.UTF8, "application/json");
                    return outputResponse;
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent("Error occured while searching for Contacts");
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
    public class ContactData : SecureInfo
    {
        public string AccountId { get; set; } // for reference
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
    public class MyContact
    {
        public String Id { get; set; }
        public string AccountId { get; set; } // for reference
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
