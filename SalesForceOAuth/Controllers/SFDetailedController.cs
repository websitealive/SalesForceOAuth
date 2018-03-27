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
    public class SFDetailedController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetView(string token, string ObjectRef, int GroupId, string entity, string refId ,string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
            try
            {

                List<SFDetailedView> myDView = new List<SFDetailedView> { };
                string sFieldOptional = "";
                string sLabelOptional = "";
                string query = ""; 
                MyAppsDb.GetAPICredentialswithCustomViewFields(ObjectRef, GroupId, entity, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref sFieldOptional,ref sLabelOptional ,ref query, urlReferrer);
                string[] customSearchArray = sFieldOptional.Split('|');
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                query += " where Id ='" + refId + "'";
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        SFDetailedView l = new SFDetailedView();
                        l.Id = c.Id;
                        int noOfcustomItems = 0;
                        if (entity == "lead")
                        {
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Company", c.Company.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                        } else if(entity == "account")
                        {
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Number", c.AccountNumber.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Name", c.Name.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                        }
                        else if (entity == "contact")
                        {
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                            if (c.Account != null)
                            {
                                noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Name", c.Account.Name.ToString(), noOfcustomItems);
                            }
                        }
                        if (sFieldOptional.Length > 0)
                        {
                            
                            foreach (Newtonsoft.Json.Linq.JProperty item in c)
                            {
                                foreach (string csA in customSearchArray)
                                {
                                    if (item.Name == csA)
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(l, item.Name, item.Value.ToString(), noOfcustomItems);
                                    }
                                }
                            }
                        }
                        myDView.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myDView, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.OK);
            }

        }

    }

    public class SFDetailedView
    {
        public String Id { get; set; }
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
        public string Custom11 { get; set; }
        public string Custom12{ get; set; }
        public string Custom13 { get; set; }
        public string Custom14{ get; set; }
        public string Custom15 { get; set; }
    }
}
