using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
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
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
            try
            {
                string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //find lead owner user
                lData.OwnerEmail = (lData.OwnerEmail == null ? "" : lData.OwnerEmail);
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
                    "where Username like '%" + lData.OwnerEmail + "%' " +
                    "OR Email like '%" + lData.OwnerEmail + "%' ").ConfigureAwait(false);
                string ownerId = "";
                //string companyName = (lData.Company == "" || lData.Company == null ? "NA" : lData.Company);
                foreach (dynamic c in cont.Records)
                {
                    ownerId = c.Id; break;
                }
                SuccessResponse sR;
                dynamic newLead = new ExpandoObject();
                newLead.FirstName = lData.FirstName; newLead.LastName = lData.LastName; newLead.Email = lData.Email; newLead.Phone = lData.Phone;
                newLead.accountid = lData.AccountId;

                if (ownerId != "" && lData.OwnerEmail != "")
                {
                    MyAppsDb.AddProperty(newLead, "OwnerId", ownerId);
                }
                if (lData.CustomFields != null)
                {
                    foreach (CustomObject c in lData.CustomFields)
                    {
                        MyAppsDb.AddProperty(newLead, c.field, c.value);
                    }
                }
                sR = await client.CreateAsync("Contact", newLead).ConfigureAwait(false);
                if (sR.Success == true)
                {
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.Id = sR.Id;
                    output.ObjectName = "Contact";
                    output.Message = "Contact added successfully!";
                    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError, true);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Unhandled exception", HttpStatusCode.InternalServerError);
            }



            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
            //    string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            //    string outputPayload;
            //    try
            //    {
            //        outputPayload = JWT.JsonWebToken.Decode(_token, ConfigurationManager.AppSettings["APISecureKey"], true);
            //    }
            //    catch (Exception ex)
            //    {
            //        return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            //    }
            //    string urlReferrer = Request.RequestUri.Authority.ToString();
            //    JObject values = JObject.Parse(outputPayload); // parse as array  
            //    ContactData lData = new ContactData();
            //    lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
            //    lData.ObjectRef = values.GetValue("ObjectRef").ToString();
            //    lData.FirstName = values.GetValue("FirstName").ToString();
            //    lData.LastName = values.GetValue("LastName").ToString();
            //    lData.Email = values.GetValue("Email").ToString();
            //    lData.Phone = values.GetValue("Phone").ToString();
            //    lData.AccountId = values.GetValue("AccountId").ToString();
            //    try
            //    {
            //        string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            //        MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
            //        ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
            //        var cont = new MyContact {AccountId = lData.AccountId, FirstName = lData.FirstName, LastName = lData.LastName,
            //            Email = lData.Email  , Phone = lData.Phone };
            //        SuccessResponse sR = await client.CreateAsync("Contact", cont);
            //        if (sR.Success == true)
            //        {
            //            PostedObjectDetail output = new PostedObjectDetail();
            //            output.Id = sR.Id;
            //            output.ObjectName = "Contact";
            //            output.Message = "Contact added successfully!";
            //            return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK,false);
            //        }
            //        else
            //        {
            //            return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError,true);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Unhandled exception", HttpStatusCode.InternalServerError);
            //    }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,false);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string token, string ObjectRef, int GroupId, string SValue, string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
            try
            {
                List<MyContact> myContacts = new List<MyContact> { };
                MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                //SELECT Id, FirstName, LastName, Email, Phone From Contact
                //SELECT AccountID, Name, (SELECT Id, FirstName, LastName, Email, Phone, from Contact)  From Account
                QueryResult <dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, FirstName, LastName, Email, Phone, AccountId, Account.Name From Contact " +
                    "where FirstName like '%" + SValue + "%' " +
                    "OR LastName like '%" + SValue + "%' " +
                    "OR Email like '%" + SValue + "%' " +
                    "OR Phone like '%" + SValue + "%' "
                    ).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        MyContact l = new MyContact();
                        l.Id = c.Id;
                        l.FirstName = c.FirstName;
                        l.LastName = c.LastName;
                        l.Email = c.Email;
                        l.Phone = c.Phone;
                        l.AccountId = c.AccountId;
                        l.AccountName = c.Account.Name;
                        //foreach (dynamic acc in c.Contact.records)
                        //{
                        //    l.Id = acc.Id;
                        //    l.FirstName =acc.FirstName;
                        //    l.LastName = acc.LastName;
                        //    l.Email = acc.Email;
                        //    l.Phone = acc.Phone;
                        //    //    l.AccountId = acc.Id;
                        //    //    l.AccountName = acc.Name;
                        //    break;
                        //}
                        myContacts.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myContacts, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
            //    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            //    string ObjectRef = "", SValue = "";
            //    int GroupId = 0;
            //    string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
            //    string outputPayload;
            //    try
            //    {
            //        outputPayload = JWT.JsonWebToken.Decode(_token, ConfigurationManager.AppSettings["APISecureKey"], true);
            //    }
            //    catch (Exception ex)
            //    {
            //        return MyAppsDb.ConvertJSONOutput(ex, "DYConfig-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            //    }
            //    JObject values = JObject.Parse(outputPayload); // parse as array  
            //    GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
            //    ObjectRef = values.GetValue("ObjectRef").ToString();
            //    SValue = values.GetValue("SValue").ToString();
            //    List<MyContact> myContacts = new List<MyContact> { };
            //    string urlReferrer = Request.RequestUri.Authority.ToString();
            //    try
            //    {
            //        MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl,urlReferrer);
            //        ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);

            //        QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(
            //            "SELECT Id, FirstName, LastName, Email, Phone, AccountId From Contact " +
            //            "where FirstName like '%" + SValue + "%' " +
            //            "OR LastName like '%" + SValue + "%' " +
            //            "OR Email like '%" + SValue + "%' " +
            //            "OR Phone like '%" + SValue + "%' "
            //            );
            //        foreach (dynamic c in cont.Records)
            //        {
            //            MyContact mc = new MyContact();
            //            mc.Id = c.Id; mc.FirstName = c.FirstName;mc.LastName = c.LastName; mc.Email = c.Email;
            //            mc.AccountId = c.AccountId; mc.Phone = c.Phone;
            //            myContacts.Add(mc);
            //        }
            //        return MyAppsDb.ConvertJSONOutput(myContacts, HttpStatusCode.OK,false);
            //    }
            //    catch (Exception ex)
            //    {
            //        return MyAppsDb.ConvertJSONOutput(ex, "SF-PostContact", "Unhandled exception", HttpStatusCode.InternalServerError);
            //    }
            //}
            //else
            //{
            //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized,true);
            //}
        }
    }

    public class ContactData : MyValidation
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OwnerEmail { get; set; }
        public List<CustomObject> CustomFields { get; set; }
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
