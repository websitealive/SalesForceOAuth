﻿using Newtonsoft.Json;
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
    public class DYAccountController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount([FromBody] DYAccountPostData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                string AccessToken = "";
                try
                {
                    HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], lData.ObjectRef, lData.GroupId.ToString(), "internal");
                    if (msg.StatusCode == HttpStatusCode.OK)
                    { AccessToken = msg.Content.ReadAsStringAsync().Result; }
                    else
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/accounts");
                    DYAccountPostValue aData = new DYAccountPostValue();
                    aData.name = lData.Name;
                    aData.description = lData.Description;
                    aData.accountnumber = lData.AccountNumber;
                    aData.address1_telephone1 = lData.Phone;
                    StringContent content = new StringContent(JsonConvert.SerializeObject(aData), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(requestURI.ToString(), content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var output = response.Headers.Location.OriginalString;
                        var id = output.Substring(output.IndexOf("(") + 1, 36);
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = id;
                        pObject.ObjectName = "Account";
                        pObject.Message = "Account added successfully!";
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
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string ObjectRef, int GroupId, string ValidationKey, string sValue, string callback)
        {
            string AccessToken = "";
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    HttpResponseMessage msg = await new DynamicsController().GetAccessToken(ConfigurationManager.AppSettings["APISecureKey"], ObjectRef, GroupId.ToString(), "internal");
                    if (msg.StatusCode == HttpStatusCode.OK)
                    { AccessToken = msg.Content.ReadAsStringAsync().Result; }
                    else
                    { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode); }

                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri("https://WEBSITEALIVEUS.crm.dynamics.com");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    StringBuilder requestURI = new StringBuilder();
                    requestURI.Append("/api/data/v8.0/accounts?$select=accountnumber,name,emailaddress1,address1_telephone1,address1_city,crmtaskassigneduniqueid");
                    requestURI.Append("&$top=50");
                    if (!sValue.Equals(""))
                    {
                        requestURI.Append("&$filter=contains(name, '" + sValue + "')or contains(accountnumber, '" + sValue + "')");
                        requestURI.Append("or contains(emailaddress1, '" + sValue + "')or contains(address1_city, '" + sValue + "')");
                        requestURI.Append("or contains(address1_telephone1, '" + sValue + "')");
                    }
                    HttpResponseMessage response = client.GetAsync(requestURI.ToString()).Result;
                    List<DYAccount> myAccounts = new List<DYAccount> { };
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var odata = JsonConvert.DeserializeObject<DYAccountOutputContainer>(json);
                        foreach (DYAccountOutput o in odata.value)
                        {
                            string s = o.accountid + "-" + o.address1_city + "-" + o.name + "-" + o.address1_telephone1 + "-" + o.emailaddress1 + "-" + o.accountnumber;
                            Console.WriteLine(s);
                            DYAccount l = new DYAccount();
                            l.accountid = o.accountid;
                            l.address1_city = o.address1_city;
                            l.name = o.name;
                            l.address1_telephone1 = o.address1_telephone1;
                            l.emailaddress1 = o.emailaddress1;
                            l.accountnumber = o.accountnumber; 
                            myAccounts.Add(l);
                        }

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

    public class DYAccountPostData : MyValidation
    {
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
    }

    public class DYAccountPostValue
    {
        public string name { get; set; }
        public string description { get; set; }
        public string accountnumber { get; set; }
        public string address1_telephone1 { get; set; }
    }

    public class DYAccount
    {
        public string accountid { get; set; }
        public string name { get; set; }
        public string accountnumber { get; set; }
        public string address1_city { get; set; }
        public string address1_telephone1 { get; set; }
        public string emailaddress1 { get; set; }
        public string crmtaskassigneduniqueid { get; set; }
    }
    
    public class DYAccountOutput: DYAccount
    {
        [JsonProperty("odata.etag")]
        public string etag { get; set; }
        public string address1_composites { get; set; }
    }

    public class DYAccountOutputContainer
    {
        [JsonProperty("odata.context")]
        public string context { get; set; }
        public DYAccountOutput[] value { get; set; }
    }

    
}
