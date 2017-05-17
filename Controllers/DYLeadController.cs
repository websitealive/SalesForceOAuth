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
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYLeadController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostLead(DYLeadPostData lData)
        {
            string AccessToken = "";
            //var re = Request;
            //var headers = re.Headers;
            //if (headers.Contains("Authorization"))
            //{
                try
                {
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
                    JObject values = JObject.Parse(outputPayload); // parse as array  
                    //DYLeadPostData lData = new DYLeadPostData();
                    //lData.GroupId = Convert.ToInt32(values.GetValue("GroupId").ToString());
                    //lData.ObjectRef = values.GetValue("ObjectRef").ToString();
                    //lData.City = values.GetValue("City").ToString();
                    //lData.Phone = values.GetValue("Phone").ToString();
                    //lData.Companyname = values.GetValue("Companyname").ToString();
                    //lData.Email = values.GetValue("Email").ToString();
                    //lData.Subject = values.GetValue("Subject").ToString();
                    #region dynamics api call
                    //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
                    HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(lData.ObjectRef, lData.GroupId.ToString());
                    if (msg.StatusCode == HttpStatusCode.OK)
                    { AccessToken = msg.Content.ReadAsStringAsync().Result; }
                    else
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
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
                    #endregion dynamics api call
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
                }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedLeads(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {
            string AccessToken = "";
            //var re = Request;
            //var headers = re.Headers;
           // string GroupId = "", ObjectRef = "", SValue = "";
            //if (headers.Contains("Authorization"))
            //{
                try
                {
                    #region JWT Token 
                    //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                    string outputPayload;
                    try
                    {
                        outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                    }
                    catch (Exception ex)
                    {
                        return MyAppsDb.ConvertJSONOutput(ex.InnerException, HttpStatusCode.InternalServerError);
                    }
                    #endregion JWT Token
                    //JObject values = JObject.Parse(outputPayload); // parse as array  
                    //GroupId = values.GetValue("GroupId").ToString();
                    //ObjectRef = values.GetValue("ObjectRef").ToString();
                    //SValue = values.GetValue("SValue").ToString();
                    #region dynamics api call 
                    //HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], ObjectRef,GroupId.ToString(), "internal");
                    HttpResponseMessage msg = await Web_API_Helper_Code.Dynamics.GetAccessToken(ObjectRef, GroupId.ToString());
                    if (msg.StatusCode == HttpStatusCode.OK)
                    {   AccessToken = msg.Content.ReadAsStringAsync().Result;   }
                    else
                    {   return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode);   }

                    //end Test 
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://websitealive.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/leads?$select=companyname,emailaddress1,address1_telephone1,subject,address1_city");
                    requestURI.Append("&$top=50");
                    if (!SValue.Equals(""))
                    {
                        requestURI.Append("&$filter=contains(companyname, '" + SValue + "')or contains(subject, '" + SValue + "')");
                        requestURI.Append("or contains(emailaddress1, '" + SValue + "')or contains(address1_city, '" + SValue + "')");
                        requestURI.Append("or contains(address1_telephone1, '" + SValue + "')");
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
                    #endregion dynamics api call 
                    return MyAppsDb.ConvertJSONPOutput(callback, myLeads, HttpStatusCode.OK);
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

    public class DYLeadPostData : MyValidation
    {
        public string token { get; set; }
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
