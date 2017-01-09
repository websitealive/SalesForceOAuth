using Newtonsoft.Json;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead([FromBody] DYLeadPostData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                string InstanceUrl = "", AccessToken = "", ApiVersion = "", resource = "";
                MyAppsDb.GetAPICredentialsDynamics(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref resource);
                try
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/leads");
                    DYLeadPostValue aData = new DYLeadPostValue();
                    aData.companyname = lData.Companyname;
                    aData.address1_city = lData.City;
                    aData.address1_telephone1 = lData.Phone;
                    aData.emailaddress1 = lData.Email;
                    aData.subject = lData.Subject; 
                    StringContent content = new StringContent(JsonConvert.SerializeObject(aData), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var output = response.Headers.Location.OriginalString;
                        var id = output.Substring(output.IndexOf("(") + 1, 36);
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = id;
                        pObject.ObjectName = "Lead";
                        pObject.Message = "Lead added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Dynamics Error: " + response.StatusCode, HttpStatusCode.InternalServerError);
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
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string ObjectRef, int GroupId, string ValidationKey, string sValue, string callback  )
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "", Resource = "";
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                //MyAppsDb.GetAPICredentialsDynamics(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref Resource);
                try
                {
                    //Test 
                    AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSIsImtpZCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSJ9.eyJhdWQiOiJodHRwczovL1dFQlNJVEVBTElWRVVTLmNybS5keW5hbWljcy5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC85MDI1ZjhjYS1kMjgwLTRiZWYtOWM1MC0wMTYyM2NkODZmOWIvIiwiaWF0IjoxNDgzOTg5NzE2LCJuYmYiOjE0ODM5ODk3MTYsImV4cCI6MTQ4Mzk5MzYxNiwiYWNyIjoiMSIsImFtciI6WyJwd2QiXSwiYXBwaWQiOiIyYTljZTA3My05YTE2LTRlYTgtYTMwNi0yYjYwMTUzN2E0NmMiLCJhcHBpZGFjciI6IjAiLCJlX2V4cCI6MTA4MDAsImZhbWlseV9uYW1lIjoiVGVhbSIsImdpdmVuX25hbWUiOiJEZXYiLCJpcGFkZHIiOiI4MS4xMjguMTkwLjE2NSIsIm5hbWUiOiJEZXYgVGVhbSIsIm9pZCI6IjFhYWE3ZmJlLTE3YTMtNDJhNi1iNzQzLTRjYzM2NDUwOTA5YyIsInBsYXRmIjoiMTQiLCJwdWlkIjoiMTAwMzdGRkU5QzFDODlDNCIsInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiIsInN1YiI6ImlScWRkeW14LXZLT1dVNmxPSTh2SUhaMmNtYzRLeENFbFJvcXhOaHVKaFEiLCJ0aWQiOiI5MDI1ZjhjYS1kMjgwLTRiZWYtOWM1MC0wMTYyM2NkODZmOWIiLCJ1bmlxdWVfbmFtZSI6IkRFVkBXRUJTSVRFQUxJVkVVUy5vbm1pY3Jvc29mdC5jb20iLCJ1cG4iOiJERVZAV0VCU0lURUFMSVZFVVMub25taWNyb3NvZnQuY29tIiwidmVyIjoiMS4wIiwid2lkcyI6WyI2MmU5MDM5NC02OWY1LTQyMzctOTE5MC0wMTIxNzcxNDVlMTAiXX0.HvlJCm3MNNq3ubLK90cgH5-SkECnFZMyeqEapQIuaut6MbK4hfgzui01K3iSl99Aa5DEoB_1AEhob6qwUvi9N5FzeaZlWT5ZKute8Yc5W8z2yWnZaB9EqtR-fFw0mdU-GybIBfVOR1G9t-Me4xGq4M2sT8VIZDBnSfnOhWOfBGQ-GVmeod9Q-ERfZMNZBMmGGIDJ5QT0d23YUC0fKapuJJODSBPX3s8HUW9L46YuxFMFvqZk-q-29qYEGzXEUBmuBrsPx3vL72e06T8S4ml59LRnHRYCvPiN1VMzinOD4K16sezYhzdvaxLoMe3et821FQgG7DZlUxTpEMO8wlSBDQ";

                    //end Test 
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/leads?$select=companyname,emailaddress1,address1_telephone1,subject,address1_city");
                    requestURI.Append("&$top=50");
                    if (!sValue.Equals(""))
                    {
                        requestURI.Append("&$filter=contains(companyname, '" + sValue + "')or contains(subject, '" + sValue + "')");
                        requestURI.Append("or contains(emailaddress1, '" + sValue + "')or contains(address1_city, '" + sValue + "')");
                        requestURI.Append("or contains(address1_telephone1, '" + sValue + "')");
                    }
                    HttpResponseMessage response = client.GetAsync(requestURI.ToString()).Result;
                    List<DYLead> myLeads = new List<DYLead> { };
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var odata = JsonConvert.DeserializeObject<DYLeadOutputContainer>(json);
                        foreach (DYLeadOutput o in odata.value)
                        {
                            DYLead l = new DYLead();
                            l.subject = o.subject;
                            l.leadid = o.leadid;
                            l.address1_city = o.address1_city;
                            l.address1_telephone1 = o.address1_telephone1;
                            l.emailaddress1 = o.emailaddress1;
                            l.companyname = o.companyname; 
                            myLeads.Add(l);
                        }

                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, myLeads, HttpStatusCode.OK);
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

    public class DYLeadPostData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string Companyname { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
    }

    public class DYLeadPostValue
    {
        public string companyname { get; set; }
        public string subject { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
    }

    public class DYLead
    {
        public string leadid { get; set; }
        public string companyname { get; set; }
        public string subject { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
    }

    public class DYLeadOutput : DYLead
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string address1_composites { get; set; }
    }

    public class DYLeadOutputContainer
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYLeadOutput[] value { get; set; }
    }


}
