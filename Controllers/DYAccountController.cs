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
        //[HttpPost]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> PostAccount([FromBody] AccountData lData)
        //{
        //    HttpResponseMessage outputResponse = new HttpResponseMessage();
        //    if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
        //    {
        //        try
        //        {
        //            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
        //            MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl);
        //            ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
        //            var acc = new Account { Name = lData.Name, AccountNumber = lData.AccountNumber, Phone = lData.Phone };
        //            SuccessResponse sR = await client.CreateAsync("Account", acc);
        //            if (sR.Success == true)
        //            {
        //                PostedObjectDetail output = new PostedObjectDetail();
        //                output.Id = sR.Id;
        //                output.ObjectName = "Lead";
        //                output.Message = "Account added successfully!";
        //                return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK);
        //            }
        //            else
        //            {
        //                return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.InternalServerError);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        //}

        //[HttpGet]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedAccounts(string ObjectRef, int GroupId, string ValidationKey, string sValue, string callback)
        //{
        //    string InstanceUrl = "", AccessToken = "", ApiVersion = "", Resource="";

        //    if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
        //    {
        //        MyAppsDb.GetAPICredentialsDynamics(ObjectRef, GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref Resource);
        //        List<Account> myAccounts = new List<Account> { };
        //        try
        //        {
        //            HttpClient client = new HttpClient();
        //            client.BaseAddress = new Uri("http://localhost:61250/");
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jason"));
        //            HttpResponseMessage response = client.GetAsync("api/Dynamics/GetAuthorizationToken?ObjectRef=dev0&GroupId=7&AuthCode=" + code + "&ValidationKey=ffe06298-22a8-4849-a46c-0284b04f2561&callback=232434&IsNew=Y").Result;
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string outContent = response.Content.ReadAsStringAsync().Result;
        //            }


        //            foreach (dynamic c in cont.Records)
        //            {
        //                Account l = new Account();
        //                l.Id = c.Id;
        //                l.AccountNumber = c.AccountNumber;
        //                l.Name = c.Name;
        //                l.Phone = c.Phone;
        //                myAccounts.Add(l);
        //            }
        //            return MyAppsDb.ConvertJSONPOutput(callback, myAccounts, HttpStatusCode.OK);
        //        }
        //        catch (Exception ex)
        //        {
        //            return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
        //        }
        //    }
        //    else
        //    {
        //        return MyAppsDb.ConvertJSONPOutput(callback, "Your request isn't authorized!", HttpStatusCode.Unauthorized);
        //    }
        //}

    }
}
