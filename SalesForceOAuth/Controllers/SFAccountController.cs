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
    public class SFAccountController : ApiController
    {
        //[HttpPost]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount(AccountData lData)
        //{
        //    string outputPayload;
        //    try
        //    {
        //        outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Your request isn't authorized!", HttpStatusCode.OK);
        //    }
        //    //Access token update
        //    string urlReferrer = Request.RequestUri.Authority.ToString();
        //    HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
        //    if (msg.StatusCode != HttpStatusCode.OK)
        //    {
        //        return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true);
        //    }
        //    try
        //    {
        //        string InstanceUrl = "", AccessToken = "", ApiVersion = "";
        //        MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
        //        ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        //find lead owner user
        //        // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
        //        lData.OwnerEmail = (lData.OwnerEmail == null ? "" : lData.OwnerEmail);
        //        QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
        //            "where Username like '%" + lData.OwnerEmail + "%' " +
        //            "OR Email like '%" + lData.OwnerEmail + "%' ").ConfigureAwait(false);
        //        string ownerId = "";
        //        foreach (dynamic c in cont.Records)
        //        {
        //            ownerId = c.Id;
        //        }
        //        SuccessResponse sR;
        //        dynamic newAccount = new ExpandoObject();
        //        newAccount.Name = lData.Name; newAccount.AccountNumber = lData.AccountNumber; newAccount.Phone = lData.Phone;
        //        if (ownerId != "" && lData.OwnerEmail != "")
        //        {
        //            MyAppsDb.AddProperty(newAccount, "OwnerId", ownerId);
        //        }
        //        if (lData.CustomFields != null)
        //        {
        //            foreach (CustomObject c in lData.CustomFields)
        //            {
        //                MyAppsDb.AddProperty(newAccount, c.field, c.value);
        //            }
        //        }
        //        sR = await client.CreateAsync("Account", newAccount).ConfigureAwait(false);
        //        if (sR.Success == true)
        //        {
        //            PostedObjectDetail output = new PostedObjectDetail();
        //            output.Id = sR.Id;
        //            output.ObjectName = "Account";
        //            output.Message = "Account added successfully!";
        //            return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
        //        }
        //        else
        //        {
        //            return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.OK, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Unhandled exception", HttpStatusCode.OK);
        //    }
        //    //}
        //    //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        //}

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
                return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
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
                // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                lData.OwnerEmail = (lData.OwnerEmail == null ? "" : lData.OwnerEmail);
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>("SELECT Id, Username, Email From User " +
                    "where Username like '%" + lData.OwnerEmail + "%' " +
                    "OR Email like '%" + lData.OwnerEmail + "%' ").ConfigureAwait(false);
                string ownerId = "";
                foreach (dynamic c in cont.Records)
                {
                    ownerId = c.Id;
                }
                SuccessResponse sR;
                dynamic newAccount = new ExpandoObject();
                newAccount.Phone = lData.Phone;
                newAccount.AccountNumber = lData.AccountNumber;
                if (lData.AccountType == 0) //For Business Account
                {
                    newAccount.Name = lData.Name;
                }
                else if (lData.AccountType == 1) // For Personal Account
                {
                    newAccount.FirstName = lData.FirstName;
                    newAccount.LastName = lData.LastName;

                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: Invalid AccountType. For Business Account the AccountType is 0 and for Personal Account AccountType is 1", HttpStatusCode.Conflict, true);
                }

                if (ownerId != "" && lData.OwnerEmail != "")
                {
                    MyAppsDb.AddProperty(newAccount, "OwnerId", ownerId);
                }

                #region Dynamic Inout Fields
                if (lData.InputFields != null)
                {
                    foreach (InputFields inputField in lData.InputFields)
                    {
                        if (inputField.Value != null)
                        {
                            MyAppsDb.AddProperty(newAccount, inputField.FieldName, inputField.Value);
                        }

                    }
                }
                #endregion

                if (lData.CustomFields != null)
                {
                    foreach (CustomObject c in lData.CustomFields)
                    {
                        MyAppsDb.AddProperty(newAccount, c.field, c.value);
                    }
                }
                sR = await client.CreateAsync("Account", newAccount).ConfigureAwait(false);
                if (sR.Success == true)
                {
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.Id = sR.Id;
                    output.ObjectName = "Account";
                    output.Message = "Account added successfully!";
                    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.Conflict, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string token, string ObjectRef, int GroupId, string SValue, string siteRef, string callback, bool isOnlyBusinessAccount = false)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.Conflict, true);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true);
            }
            try
            {
                List<Account> myAccounts = new List<Account> { };
                //MyAppsDb.GetAPICredentials(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                string cSearchField = "";
                string cSearchFieldLabels = "";
                MyAppsDb.GetAPICredentialswithCustomSearchFields(ObjectRef, GroupId, "account", ref AccessToken, ref ApiVersion, ref InstanceUrl, ref cSearchField, ref cSearchFieldLabels, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                StringBuilder filters = new StringBuilder();
                string[] customSearchFieldArray = cSearchField.Split('|');
                //string[] customSearchFieldArray = cSearchField.Split('|');
                string[] customSearchLabelArray = cSearchFieldLabels.Split('|');
                if (cSearchField.Length > 0)
                {
                    foreach (string csA in customSearchFieldArray)
                    {
                        columns.Append("," + csA);
                        filters.Append("OR " + csA + " like '%" + SValue + "%' ");
                    }
                }
                // 1.Issues when Personal Account are not enabled on clint org.
                //query.Append("SELECT Id, AccountNumber, Name, Phone, LastName " + columns + " From Account ");
                query.Append("SELECT Id, AccountNumber, Name, Phone " + columns + " From Account ");
                query.Append("where Name like '%" + SValue + "%' ");
                query.Append("OR Phone like '%" + SValue + "%' ");
                query.Append("OR AccountNumber like '%" + SValue + "%' ");
                query.Append(filters.ToString());
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        Account l = new Account();
                        // 2. Business account have some issues when Personal account are not configured
                        if (isOnlyBusinessAccount)
                        {
                            if (c.FirstName == null && c.LastName == null)
                            {
                                l.Id = c.Id;
                                l.AccountNumber = (c.AccountNumber != null ? c.AccountNumber : "");
                                l.Name = (c.Name != null ? c.Name : "");
                                l.Phone = (c.Phone != null ? c.Phone : "");
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
                                myAccounts.Add(l);
                            }
                        }
                        else
                        {
                            l.Id = c.Id;
                            l.AccountNumber = (c.AccountNumber != null ? c.AccountNumber : "");
                            l.Name = (c.Name != null ? c.Name : "");
                            l.Phone = (c.Phone != null ? c.Phone : "");
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
                            myAccounts.Add(l);
                        }
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myAccounts, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.Conflict, true);
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
        public int AccountType { get; set; } // 0 for Business Account & 1 for Personal Account.
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string OwnerEmail { get; set; }
        public List<CustomObject> CustomFields { get; set; }
        public List<InputFields> InputFields { get; set; }
    }
    public class Account
    {
        public String Id { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
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
    public class AccountOW
    {
        public String Id { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string OwnerId { get; set; }
    }
}
