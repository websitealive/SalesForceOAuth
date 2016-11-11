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
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact([FromBody] ContactData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var cont = new MyContact {AccountId = lData.AccountId, FirstName = lData.FirstName,LastName = lData.LastName,
                        Email = lData.Email  , Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Contact", cont);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Contact";
                        output.Message = "Contact added successfully!";
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
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string ObjectRef, int GroupId, string ValidationKey, string sObj, string sValue, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                List<MyContact> myContacts = new List<MyContact> { };
                try
                {
                    MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
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
                    return MyAppsDb.ConvertJSONPOutput(callback, myContacts, HttpStatusCode.OK);
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
