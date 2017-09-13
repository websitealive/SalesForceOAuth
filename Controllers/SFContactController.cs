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
    public class SFContactController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact()
        {
            var re = Request;
            var headers = re.Headers;
            if (headers.Contains("Authorization"))
            {
                string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(_token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
                }
                string urlReferrer = Request.RequestUri.Authority.ToString();
                JObject values = JObject.Parse(outputPayload); // parse as array  
                ContactData lData = new ContactData();
                lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                lData.ObjectRef = values.GetValue("ObjectRef").ToString();
                lData.FirstName = values.GetValue("FirstName").ToString();
                lData.LastName = values.GetValue("LastName").ToString();
                lData.Email = values.GetValue("Email").ToString();
                lData.Phone = values.GetValue("Phone").ToString();
                lData.AccountId = values.GetValue("AccountId").ToString();
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var cont = new MyContact {AccountId = lData.AccountId, FirstName = lData.FirstName, LastName = lData.LastName,
                        Email = lData.Email  , Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Contact", cont);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Contact";
                        output.Message = "Contact added successfully!";
                        return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
            }
            return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,false);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts()
        {
            var re = Request;
            var headers = re.Headers;
            if (headers.Contains("Authorization"))
            {
                string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                string ObjectRef = "", SValue = "";
                int GroupId = 0;
                string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(_token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "DYConfig-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
                }
                JObject values = JObject.Parse(outputPayload); // parse as array  
                GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                ObjectRef = values.GetValue("ObjectRef").ToString();
                SValue = values.GetValue("SValue").ToString();
                List<MyContact> myContacts = new List<MyContact> { };
                string urlReferrer = Request.RequestUri.Authority.ToString();
                try
                {
                    MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);

                    QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(
                        "SELECT Id, FirstName, LastName, Email, Phone, AccountId From Contact " +
                        "where FirstName like '%" + SValue + "%' " +
                        "OR LastName like '%" + SValue + "%' " +
                        "OR Email like '%" + SValue + "%' " +
                        "OR Phone like '%" + SValue + "%' "
                        );
                    foreach (dynamic c in cont.Records)
                    {
                        MyContact mc = new MyContact();
                        mc.Id = c.Id; mc.FirstName = c.FirstName;mc.LastName = c.LastName; mc.Email = c.Email;
                        mc.AccountId = c.AccountId; mc.Phone = c.Phone;
                        myContacts.Add(mc);
                    }
                    return MyAppsDb.ConvertJSONOutput(myContacts, HttpStatusCode.OK,false);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput(ex, "SF-PostContact", "Unhandled exception", HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,true);
            }
        }
    }
    public class ContactData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
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
