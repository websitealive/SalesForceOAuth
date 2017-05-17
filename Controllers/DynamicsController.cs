using Salesforce.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Salesforce.Common.Models;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SalesForceOAuth.Web_API_Helper_Code;
using JWT; 
namespace SalesForceOAuth.Controllers
{
    public class DynamicsController : ApiController
    {

        [HttpGet]
        [ActionName("GetAccessToken")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAccessToken(string token, string ObjectRef, int GroupId, string callback)
        {
            //var re = Request;
            //var headers = re.Headers;
           
            //if (headers.Contains("Authorization") )
            //{
                //string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string outputPayload; 
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch(Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, ex.InnerException, HttpStatusCode.InternalServerError);
                }

                //JObject values = JObject.Parse(outputPayload); // parse as array  
                //GroupId = values.GetValue("GroupId").ToString();
                //ObjectRef = values.GetValue("ObjectRef").ToString();
                try
                {
                    //Live Code 
                    string accessToken = "", username = "", serviceURL = "", userPassword = "", clientId = "", authority = "";
                    DateTime tokenExpiryDT = DateTime.Now.AddDays(-1);
                    CRMTokenStatus userTokenStatus;
                    userTokenStatus = MyAppsDb.GetAccessTokenDynamics(ObjectRef, GroupId.ToString(), ref accessToken, ref username, ref userPassword, ref clientId, ref serviceURL, ref tokenExpiryDT, ref authority);
                    //end Live Code 
                    if (userTokenStatus == CRMTokenStatus.SUCCESSS) // if a valid token is available
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback,accessToken, HttpStatusCode.OK);
                    }
                    else if (userTokenStatus == CRMTokenStatus.USERNOTFOUND) // if a user account is not found 
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback,"User not registered to use this application.", HttpStatusCode.NotFound);
                    }
                    else // if user acccount found but token is expired, code to refresh token  ---- DYTokenStatus.TOKENEXPIRED
                    {
                        var passwordSecure = new System.Security.SecureString();
                        foreach (char c in userPassword) passwordSecure.AppendChar(c);
                        Web_API_Helper_Code.Configuration _config = null;
                        _config = new Web_API_Helper_Code.Configuration(username, passwordSecure, serviceURL, clientId);

                        // authentication class 
                        Web_API_Helper_Code.Authentication _auth = new Authentication(_config, authority);
                        AuthenticationResult res = await _auth.AcquireToken();
                        DateTime expiryDT = res.ExpiresOn.DateTime;
                        MyAppsDb.UpdateAccessTokenDynamics(ObjectRef, GroupId.ToString(), res.AccessToken.ToString(), expiryDT);
                        return MyAppsDb.ConvertJSONPOutput(callback,res.AccessToken.ToString(), HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback,"Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            //}
            //else
            //{
            //    HttpResponseMessage outputResponse = new HttpResponseMessage();
            //    outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            //    outputResponse.Content = new StringContent("Your request isn't authorized!");
            //    return outputResponse;
            //}
        }


        //[HttpGet]
        //[ActionName("GetAccessToken")]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> GetAccessToken(string ValidationKey, string ObjectRef, string GroupId, string callback)
        //{
        //    HttpResponseMessage outputResponse = new HttpResponseMessage();
        //    if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
        //    {
        //        try
        //        {
        //            //Test Code
        //            //string accessToken = ""; 
        //            //string username = "dev@websitealive.onmicrosoft.com";
        //            //string serviceURL = "https://websitealive.crm.dynamics.com/";
        //            //string userPassword = "Unstoppable.1o";
        //            //string clientId = "2a9ce073-9a16-4ea8-a306-2b601537a46c";
        //            //DYTokenStatus userTokenStatus = DYTokenStatus.TOKENEXPIRED;
        //            //string authority = "https://login.windows.net/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize/";
        //            //End Test Code 
        //            //Live Code 
        //            string accessToken = "", username = "", serviceURL = "", userPassword = "", clientId = "", authority = "";
        //            DateTime tokenExpiryDT = DateTime.Now.AddDays(-1);
        //            DYTokenStatus userTokenStatus;
        //            userTokenStatus = MyAppsDb.GetAccessTokenDynamics(ObjectRef, GroupId, ref accessToken, ref username, ref userPassword, ref clientId, ref serviceURL, ref tokenExpiryDT, ref authority);
        //            //end Live Code 


        //            if (userTokenStatus == DYTokenStatus.SUCCESSS) // if a valid token is available
        //            {
        //                return MyAppsDb.ConvertJSONPOutput(callback, accessToken, HttpStatusCode.OK);
        //            }
        //            else if (userTokenStatus == DYTokenStatus.USERNOTFOUND) // if a user account is not found 
        //            {
        //                return MyAppsDb.ConvertJSONPOutput(callback, "User not registered to use this application.", HttpStatusCode.NotFound);
        //            }
        //            else // if user acccount found but token is expired, code to refresh token  ---- DYTokenStatus.TOKENEXPIRED
        //            {
        //                var passwordSecure = new System.Security.SecureString();
        //                foreach (char c in userPassword) passwordSecure.AppendChar(c);
        //                Web_API_Helper_Code.Configuration _config = null;
        //                _config = new Web_API_Helper_Code.Configuration(username, passwordSecure, serviceURL, clientId);

        //                // authentication class 
        //                Web_API_Helper_Code.Authentication _auth = new Authentication(_config, authority);
        //                AuthenticationResult res = await _auth.AcquireToken();
        //                DateTime expiryDT = res.ExpiresOn.DateTime;
        //                MyAppsDb.UpdateAccessTokenDynamics(ObjectRef, GroupId, res.AccessToken.ToString(), expiryDT);
        //                return MyAppsDb.ConvertJSONPOutput(callback, res.AccessToken.ToString(), HttpStatusCode.OK);
        //            }
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


        //GET: api/SalesForce/GetAuthorizationToken? ValidationKey = ffe06298 - 22a8-4849-a46c-0284b04f2561
        //[HttpGet]
        //[ActionName("GetAuthorizationToken")]
        //public HttpResponseMessage GetAuthorizationToken(string ObjectRef, int GroupId, string AuthCode, string ValidationKey, string IsNew, string callback)
        //{
        //    string dy_token_post_url = "", dy_clientid = "", dy_redirect_url = "", dy_resource_url = "";

        //   HttpResponseMessage outputResponse = new HttpResponseMessage();
        //    if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
        //    {
        //        // MyAppsDb.GetTokenParametersDynamics(ref dy_clientid, ref dy_redirect_url, ref dy_resource_url, ref dy_token_post_url);
        //        dy_clientid = "1579d88e-bb6c-40ec-81ef-556c87319214";
        //        dy_redirect_url = "http://localhost:56786/Contact";
        //        dy_resource_url = "https://websitealive.crm.dynamics.com";
        //        dy_token_post_url = "/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/token"; // "https://login.microsoftonline.com";
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        var auth = new AuthenticationClient();
        //        try
        //        {
        //            if (IsNew.Equals("Y"))
        //            {
        //                HttpClient client = new HttpClient();
        //                client.BaseAddress = new Uri("https://login.windows.net/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/token"); //");    
        //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        //                var content = new StringContent(
        //                        "grant_type=authorization_code" + "&client_id=" + dy_clientid + "&code=" + AuthCode+
        //                        "&redirect_uri=" + System.Web.HttpUtility.UrlEncode(dy_redirect_url) +
        //                        "&resource=" + System.Web.HttpUtility.UrlEncode(dy_resource_url) +
        //                        "&client_secret=g3P1ZtRjtZ3hKbw2xP6xcCVJWZfdkRLXG3j3NF4XVLc=",
        //                        Encoding.UTF8,"application/x-www-form-urlencoded");


        //                  HttpResponseMessage response = client.PostAsync(dy_token_post_url, content).Result;
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
        //                    string access_token = jObject["access_token"].ToString();
        //                    string refresh_token = jObject["refresh_token"].ToString();
        //                    string resource = jObject["resource"].ToString();
        //                  // MyAppsDb.CreateNewIntegrationSettingForDynamicsUser(ObjectRef, GroupId, refresh_token, access_token, resource);
        //                }
        //            }
        //            else
        //            {
        //                string DYRefreshToken = "", DYResourceURL = "";
        //                //MyAppsDb.GetCurrentRefreshTokenDynamics(ObjectRef, GroupId, ref DYRefreshToken, ref DYResourceURL);
        //                HttpClient client = new HttpClient();
        //                client.BaseAddress = new Uri("https://login.microsoftonline.com");
        //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        //                var content = new StringContent(
        //                        "client_id=" + dy_clientid +
        //                        "&refresh_token=" + DYRefreshToken +
        //                        "&grant_type=refresh_token" +
        //                        "&resource=" + System.Web.HttpUtility.UrlEncode(dy_resource_url) +
        //                        "&client_secret=g3P1ZtRjtZ3hKbw2xP6xcCVJWZfdkRLXG3j3NF4XVLc=",
        //                        Encoding.UTF8, "application/x-www-form-urlencoded");
        //                //dy_token_post_url = "/websitealive.onmicrosoft.com/oauth2/token"; 
        //                HttpResponseMessage response = client.PostAsync(dy_token_post_url, content).Result;
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
        //                    string access_token = jObject["access_token"].ToString();
        //                    string refresh_token = jObject["refresh_token"].ToString();
        //                    string resource = jObject["resource"].ToString();
        //                    //MyAppsDb.CreateNewIntegrationSettingForDynamicsUser(ObjectRef, GroupId, refresh_token, access_token, );
        //                   // MyAppsDb.UpdateIntegrationSettingForUserDynamics(ObjectRef, GroupId, refresh_token, access_token, resource);
        //                }

        //            }

        //            return MyAppsDb.ConvertJSONPOutput(callback, "API information updated!", HttpStatusCode.OK);

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
    public class DynamicsToken 
    {
        string access_token { get; set; }
        string token_type { get; set; }
        string expires_in { get; set; }
        string expires_on { get; set; }
        string resource { get; set; }
        string refresh_token { get; set; }
        string scope{ get; set; }
        string id_token { get; set; }
        string ext_expires_in { get; set; }
        string not_before { get; set; }
      
    }
    public enum CRMTokenStatus
    {
        USERNOTFOUND = -1, SUCCESSS = 1 ,TOKENEXPIRED = 2,
    }
    
}
