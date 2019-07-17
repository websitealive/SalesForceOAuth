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
    public class SFOpportunitiesController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostOpportunity(Opportunity lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFContact-PostContact", "Your request isn't authorized!", HttpStatusCode.Conflict);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.SiteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false); }
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
                dynamic newOpportunity = new ExpandoObject();
                newOpportunity.Name = lData.Name;
                newOpportunity.CloseDate = lData.CloseDate;
                newOpportunity.StageName = lData.Stage;
                if (ownerId != "" && lData.OwnerEmail != "")
                {
                    MyAppsDb.AddProperty(newOpportunity, "OwnerId", ownerId);
                }

                #region Dynamic Inout Fields
                if (lData.CustomFields != null)
                {
                    foreach (CustomFieldModel inputField in lData.CustomFields)
                    {
                        if (inputField.Value != null)
                        {
                            MyAppsDb.AddProperty(newOpportunity, inputField.FieldName, inputField.Value);
                        }

                    }
                }
                #endregion

                sR = await client.CreateAsync("Opportunity", newOpportunity).ConfigureAwait(false);
                if (sR.Success == true)
                {
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.Id = sR.Id;
                    output.ObjectName = "Opportunity";
                    output.Message = "Opportunity added successfully!";
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

        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedOpportunities(string Token, string ObjectRef, int GroupId, string SValue, string SiteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(SiteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false); }
            try
            {
                List<Opportunity> retOpportunity = new List<Opportunity> { };
                string cSearchField = "";
                string cSearchFieldLabels = "";
                MyAppsDb.GetAPICredentialswithCustomSearchFields(ObjectRef, GroupId, "opportunities", ref AccessToken, ref ApiVersion, ref InstanceUrl, ref cSearchField, ref cSearchFieldLabels, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                StringBuilder filters = new StringBuilder();
                string[] customSearchFieldArray = cSearchField.Split('|');
                string[] customSearchLabelArray = cSearchFieldLabels.Split('|');
                if (cSearchField.Length > 0)
                {
                    foreach (string csA in customSearchFieldArray)
                    {
                        columns.Append("," + csA);
                        filters.Append("OR " + csA + " like '%" + SValue + "%' ");
                    }
                }
                query.Append("SELECT Id, Name, CloseDate, StageName From Opportunity ");
                query.Append("where Name like '%" + SValue + "%' ");

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic item in cont.Records)
                    {
                        Opportunity Opp = new Opportunity();
                        Opp.Id = item.Id;
                        Opp.Name = (item.Name != null ? item.Name : "");
                        Opp.CloseDate = (item.CloseDate != null ? item.CloseDate.ToString("MM/dd/yyyy") : "");
                        Opp.Stage = (item.StageName != null ? item.StageName : "");

                        if (cSearchField.Length > 0)
                        {
                            int noOfcustomItems = 0; int i = 0;
                            foreach (Newtonsoft.Json.Linq.JProperty sItem in item)
                            {

                                foreach (string csA in customSearchFieldArray)
                                {
                                    if (sItem.Name.ToLower() == csA.ToLower())
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(Opp, customSearchLabelArray[i], sItem.Value.ToString(), noOfcustomItems);
                                        i++;
                                    }
                                }
                            }
                        }
                        retOpportunity.Add(Opp);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, retOpportunity, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.Conflict, false);
            }

        }
    }
}
