using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
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
                return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Your request isn't authorized!", HttpStatusCode.Conflict);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false);
            }
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
                foreach (dynamic c in cont.Records)
                {
                    ownerId = c.Id; break;
                }
                SuccessResponse sR;
                dynamic newContact = new ExpandoObject();
                newContact.FirstName = lData.FirstName;
                newContact.LastName = lData.LastName;
                newContact.Email = lData.Email;
                newContact.Phone = lData.Phone;
                newContact.accountid = lData.AccountId;
                if (ownerId != "" && lData.OwnerEmail != "")
                {
                    MyAppsDb.AddProperty(newContact, "OwnerId", ownerId);
                }

                #region Dynamic Inout Fields
                if (lData.InputFields != null)
                {
                    foreach (CustomFieldModel inputField in lData.InputFields)
                    {
                        if (inputField.Value != null)
                        {
                            MyAppsDb.AddProperty(newContact, inputField.FieldName, inputField.Value, inputField.FieldType);
                        }

                    }
                }
                #endregion

                if (lData.CustomFields != null)
                {
                    foreach (CustomObject c in lData.CustomFields)
                    {
                        MyAppsDb.AddProperty(newContact, c.field, c.value);
                    }
                }
                sR = await client.CreateAsync("Contact", newContact).ConfigureAwait(false);
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
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.Conflict, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFLead-PostLead", "Unhandled exception", HttpStatusCode.Conflict, false);
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false); }
            try
            {
                List<MyContact> myContacts = new List<MyContact> { };
                string cSearchField = "";
                //string cSearchField = "";
                string cSearchFieldLabels = "";
                MyAppsDb.GetAPICredentialswithCustomSearchFields(ObjectRef, GroupId, "contact", ref AccessToken, ref ApiVersion, ref InstanceUrl, ref cSearchField, ref cSearchFieldLabels, urlReferrer);
                List<FieldsModel> searchFields = Repository.GetSFSearchFieldsByEntity(ObjectRef, GroupId, "contact", urlReferrer);
                List<FieldsModel> detailsFields = Repository.GetSFDetailFieldsByEntity(ObjectRef, GroupId, "contact", urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                StringBuilder filters = new StringBuilder();
                string[] customSearchFieldArray = cSearchField.Split('|');
                string[] customSearchLabelArray = cSearchFieldLabels.Split('|');
                //if (cSearchField.Length > 0)
                //{
                //    foreach (string csA in customSearchFieldArray)
                //    {
                //        columns.Append("," + csA);
                //        filters.Append("OR " + csA + " like '%" + SValue.Trim() + "%' ");
                //    }
                //}
                if (searchFields.Count > 0)
                {
                    foreach (var search in searchFields)
                    {
                        if (!columns.ToString().Contains(search.FieldName))
                        {
                            if (search.FieldType == "datetime")
                            {
                                DateTime result;
                                if (DateTime.TryParse(SValue, out result))
                                {
                                    columns.Append("," + search.FieldName);
                                    filters.Append("OR " + search.FieldName + " equal '%" + result + "%' ");
                                }
                            }
                            else
                            {
                                columns.Append("," + search.FieldName);
                                filters.Append("OR " + search.FieldName + " like '%" + SValue.Trim() + "%' ");
                            }
                        }
                    }
                }
                // Search By details View Fields
                if (detailsFields.Count > 0)
                {
                    foreach (var detail in detailsFields)
                    {
                        if (!columns.ToString().Contains(detail.FieldName))
                        {
                            if (detail.FieldType == "datetime")
                            {
                                DateTime result;
                                if (DateTime.TryParse(SValue, out result))
                                {
                                    columns.Append("," + detail.FieldName);
                                    filters.Append("OR " + detail.FieldName + " equal '%" + result + "%' ");
                                }
                            }
                            else
                            {
                                columns.Append("," + detail.FieldName);
                                filters.Append("OR " + detail.FieldName + " like '%" + SValue.Trim() + "%' ");
                            }
                        }
                    }
                }
                query.Append("SELECT Id, FirstName, LastName, Email, Phone " + columns + ", AccountId, Account.Name From Contact ");
                query.Append("where Name like '%" + SValue.Trim() + "%' ");
                query.Append("OR FirstName like '%" + SValue.Trim() + "%' ");
                query.Append("OR LastName like '%" + SValue.Trim() + "%' ");
                query.Append("OR Email like '%" + SValue.Trim() + "%' ");

                //TODO: Please make sure that user save phone no in proper US format. when done then make changes to below code accord
                if (SValue.Trim().Contains<char>('+'))
                {
                    query.Append("OR Phone like '%" + SValue.Trim() + "%' ");
                    query.Append("OR Phone like '%" + SValue.Trim().Substring(1) + "%' ");
                    query.Append("OR Phone like '%" + SValue.Trim().Substring(2) + "%' ");
                }
                //Note: USA Telephone no has 10 digits without country code
                //If SValue does not have + & characters = 11 means country code exist but without '+'
                else if (!SValue.Trim().Contains<char>('+') && SValue.Trim().Count() == 11)
                {
                    query.Append("OR Phone like '%" + SValue.Trim().Substring(1) + "%' ");
                    query.Append("OR Phone like '%1" + SValue.Trim().Substring(1) + "%' ");
                    query.Append("OR Phone like '%+1" + SValue.Trim().Substring(1) + "%' ");
                }
                //SValue does not have '+' nor 1 => no of digit will be 10 without country code which USA Standard phone no w/o country code
                else
                {
                    query.Append("OR Phone like '%" + SValue.Trim() + "%' ");
                    query.Append("OR Phone like '%1" + SValue.Trim() + "%' ");
                    query.Append("OR Phone like '%+1" + SValue.Trim() + "%' ");
                }
                query.Append(filters.ToString());
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        MyContact l = new MyContact();
                        l.Id = c.Id;
                        l.FirstName = (c.FirstName != null ? c.FirstName : "");
                        l.LastName = (c.LastName != null ? c.LastName : "");
                        l.Email = (c.Email != null ? c.Email : "");
                        l.Phone = (c.Phone != null ? c.Phone : "");
                        l.AccountId = (c.AccountId != null ? c.AccountId : "");
                        if (c.Account != null)
                        {
                            l.AccountName = c.Account.Name;
                        }
                        else l.AccountName = "";
                        if (cSearchField.Length > 0)
                        {
                            int noOfcustomItems = 0; int i = 0;
                            foreach (Newtonsoft.Json.Linq.JProperty item in c)
                            {

                                foreach (string csA in customSearchFieldArray)
                                {
                                    if (item.Name.ToLower() == csA.ToLower())
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(l, customSearchLabelArray[i], item.Value.ToString(), noOfcustomItems);
                                        i++;
                                    }
                                }
                            }
                        }
                        myContacts.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myContacts, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.Conflict, false);
            }

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
        public List<CustomFieldModel> InputFields { get; set; }
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
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string Custom4 { get; set; }
        public string Custom5 { get; set; }
        public string Custom6 { get; set; }
        public string Custom7 { get; set; }
        public string Custom8 { get; set; }
        public string Custom9 { get; set; }
        public string Custom10 { get; set; }
    }
}
