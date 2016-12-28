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
    public class DYAccountController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount([FromBody] AccountData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ////////////
                    HttpClient client = new HttpClient();
                    //    string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSIsImtpZCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSJ9.eyJhdWQiOiJodHRwczovL1dFQlNJVEVBTElWRVVTLmNybS5keW5hbWljcy5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC85MDI1ZjhjYS1kMjgwLTRiZWYtOWM1MC0wMTYyM2NkODZmOWIvIiwiaWF0IjoxNDgyODU4MTY2LCJuYmYiOjE0ODI4NTgxNjYsImV4cCI6MTQ4Mjg2MjA2NiwiYWNyIjoiMSIsImFtciI6WyJwd2QiXSwiYXBwaWQiOiIxNTc5ZDg4ZS1iYjZjLTQwZWMtODFlZi01NTZjODczMTkyMTQiLCJhcHBpZGFjciI6IjAiLCJlX2V4cCI6MTA4MDAsImZhbWlseV9uYW1lIjoiVGVhbSIsImdpdmVuX25hbWUiOiJEZXYiLCJpcGFkZHIiOiI4MC43Ljc5LjEwNyIsIm5hbWUiOiJEZXYgVGVhbSIsIm9pZCI6IjFhYWE3ZmJlLTE3YTMtNDJhNi1iNzQzLTRjYzM2NDUwOTA5YyIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzN0ZGRTlDMUM4OUM0Iiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwic3ViIjoiaVJxZGR5bXgtdktPV1U2bE9JOHZJSFoyY21jNEt4Q0VsUm9xeE5odUpoUSIsInRpZCI6IjkwMjVmOGNhLWQyODAtNGJlZi05YzUwLTAxNjIzY2Q4NmY5YiIsInVuaXF1ZV9uYW1lIjoiREVWQFdFQlNJVEVBTElWRVVTLm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6IkRFVkBXRUJTSVRFQUxJVkVVUy5vbm1pY3Jvc29mdC5jb20iLCJ2ZXIiOiIxLjAifQ.GIbNfyH9xuvSPkSx6k2CHottBu6jgXIywrpx0WZeKMqFbmqQkTzpX-rT7xDi6z_LlXjIWwoLi9TqPxTeO-0l-efZkbrUY_KP0oTJg2ULwb2hOsxX89rwIcrfmUmsozyFQ_0tfaqbEOSWyd7AMiZ9dhokezeW5LF6hLCLIQkYA0pDenSoLpYicJkCk7YW8LJbPffPIJJEZSMpE6_NRIWU343VlgfA5thOkmGFpBDb9WCss12Cbt1HaEx_W8m_c4FiHMxqI6jvgLDRLJQuMr0QvI7XWU4alwD9Lp3Rl6jLGBlN55JXBGPgs9wDXoVi55V38liAMiPv447wv-aQwaasOQ";
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/accounts");
                    MyAccount lData = new MyAccount();
                    lData.name = "Naveed Account2";
                    lData.description = "this is test description";
                    StringContent content = new StringContent(JsonConvert.SerializeObject(lData), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("new account added");
                        var json = response.Content.ReadAsStringAsync().Result;
                        var output = response.Headers.Location.OriginalString;
                        var id = output.Substring(output.IndexOf("(") + 1, 36);
                        // var odata = JsonConvert.DeserializeObject<DYLeadsRead>(json);
                    }
                    /////////////
                    string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                    MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
                    ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                    var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
                    SuccessResponse sR = await client.CreateAsync("Account", acc);
                    if (sR.Success == true)
                    {
                        PostedObjectDetail output = new PostedObjectDetail();
                        output.Id = sR.Id;
                        output.ObjectName = "Lead";
                        output.Message = "Account added successfully!";
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
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string ObjectRef, int GroupId, string ValidationKey, string sValue, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "", Resource = "";

            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                MyAppsDb.GetAPICredentialsDynamics(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref Resource);
                List<Account> myAccounts = new List<Account> { };
                try
                {
                    //////////////
                    string searchedValue = "Publish";
                    HttpClient client = new HttpClient();
                    //  string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSIsImtpZCI6IlJyUXF1OXJ5ZEJWUldtY29jdVhVYjIwSEdSTSJ9.eyJhdWQiOiJodHRwczovL1dFQlNJVEVBTElWRVVTLmNybS5keW5hbWljcy5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC85MDI1ZjhjYS1kMjgwLTRiZWYtOWM1MC0wMTYyM2NkODZmOWIvIiwiaWF0IjoxNDgyODU4MTY2LCJuYmYiOjE0ODI4NTgxNjYsImV4cCI6MTQ4Mjg2MjA2NiwiYWNyIjoiMSIsImFtciI6WyJwd2QiXSwiYXBwaWQiOiIxNTc5ZDg4ZS1iYjZjLTQwZWMtODFlZi01NTZjODczMTkyMTQiLCJhcHBpZGFjciI6IjAiLCJlX2V4cCI6MTA4MDAsImZhbWlseV9uYW1lIjoiVGVhbSIsImdpdmVuX25hbWUiOiJEZXYiLCJpcGFkZHIiOiI4MC43Ljc5LjEwNyIsIm5hbWUiOiJEZXYgVGVhbSIsIm9pZCI6IjFhYWE3ZmJlLTE3YTMtNDJhNi1iNzQzLTRjYzM2NDUwOTA5YyIsInBsYXRmIjoiMyIsInB1aWQiOiIxMDAzN0ZGRTlDMUM4OUM0Iiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwic3ViIjoiaVJxZGR5bXgtdktPV1U2bE9JOHZJSFoyY21jNEt4Q0VsUm9xeE5odUpoUSIsInRpZCI6IjkwMjVmOGNhLWQyODAtNGJlZi05YzUwLTAxNjIzY2Q4NmY5YiIsInVuaXF1ZV9uYW1lIjoiREVWQFdFQlNJVEVBTElWRVVTLm9ubWljcm9zb2Z0LmNvbSIsInVwbiI6IkRFVkBXRUJTSVRFQUxJVkVVUy5vbm1pY3Jvc29mdC5jb20iLCJ2ZXIiOiIxLjAifQ.GIbNfyH9xuvSPkSx6k2CHottBu6jgXIywrpx0WZeKMqFbmqQkTzpX-rT7xDi6z_LlXjIWwoLi9TqPxTeO-0l-efZkbrUY_KP0oTJg2ULwb2hOsxX89rwIcrfmUmsozyFQ_0tfaqbEOSWyd7AMiZ9dhokezeW5LF6hLCLIQkYA0pDenSoLpYicJkCk7YW8LJbPffPIJJEZSMpE6_NRIWU343VlgfA5thOkmGFpBDb9WCss12Cbt1HaEx_W8m_c4FiHMxqI6jvgLDRLJQuMr0QvI7XWU4alwD9Lp3Rl6jLGBlN55JXBGPgs9wDXoVi55V38liAMiPv447wv-aQwaasOQ";
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/leads?$select=companyname,emailaddress1,address1_telephone1,subject,address1_city");
                    requestURI.Append("&$top=50");
                    if (!searchedValue.Equals(""))
                    {
                        requestURI.Append("&$filter=contains(companyname, '" + searchedValue + "')or contains(subject, '" + searchedValue + "')");
                        requestURI.Append("or contains(emailaddress1, '" + searchedValue + "')or contains(address1_city, '" + searchedValue + "')");
                        requestURI.Append("or contains(address1_telephone1, '" + searchedValue + "')");
                    }

                    HttpResponseMessage response = client.GetAsync(requestURI.ToString()).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var odata = JsonConvert.DeserializeObject<DYLeadsRead>(json);

                        foreach (DYLeads o in odata.value)
                        {
                            string s = o.address1_city + "-" + o.companyname + "-" + o.address1_telephone1 + "-" + o.emailaddress1 + "-" + o.leadid;
                            Console.WriteLine(s);
                        }

                    }






                    /////////////////////////////
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:61250/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    HttpResponseMessage response = client.GetAsync("api/Dynamics/GetAuthorizationToken?ObjectRef=dev0&GroupId=7&AuthCode=" + code + "&ValidationKey=ffe06298-22a8-4849-a46c-0284b04f2561&callback=232434&IsNew=Y").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string outContent = response.Content.ReadAsStringAsync().Result;
                    }


                    foreach (dynamic c in cont.Records)
                    {
                        Account l = new Account();
                        l.Id = c.Id;
                        l.AccountNumber = c.AccountNumber;
                        l.Name = c.Name;
                        l.Phone = c.Phone;
                        myAccounts.Add(l);
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, myAccounts, HttpStatusCode.OK);
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
    public class MyAccount
    {
        public string name { get; set; }
        public string description { get; set; }
    }
    public class DYAccounts
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string accountnumber { get; set; }
        public string name { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
        public string accountid { get; set; }
        public string address1_composites { get; set; }
    }
    public class DYAccountRead
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYAccounts[] value { get; set; }
    }
    public class DYLeads
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string companyname { get; set; }
        public string emailaddress1 { get; set; }
        public string address1_telephone1 { get; set; }
        public string address1_city { get; set; }
        public string subject { get; set; }
        public string leadid { get; set; }
        public string address1_composites { get; set; }
    }
    public class DYLeadsRead
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYLeads[] value { get; set; }
    }
}
