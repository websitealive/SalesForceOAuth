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

namespace SalesForceOAuth.Controllers
{
    public class DynamicsController : ApiController
    {
        [HttpGet]
        [ActionName("GetRedirectURL")]
        public HttpResponseMessage GetRedirectURL(string ValidationKey, string callback)
        {
            
            string dy_authoize_url = "", dy_clientid = "", dy_redirect_url = "", dy_resource_url="";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    //MyAppsDb.GetRedirectURLParametersDynamics(ref dy_authoize_url, ref dy_clientid, ref dy_redirect_url, ref dy_resource_url);
                    dy_clientid = "1579d88e-bb6c-40ec-81ef-556c87319214";
                    dy_redirect_url = "http://localhost:56786/Contact";
                    dy_resource_url = "https://WEBSITEALIVEUS.onmicrosoft.com/websitealive";
                    dy_authoize_url = "https://login.microsoftonline.com/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize";
                    StringBuilder url = new StringBuilder();
                    //url.Append(dy_authoize_url).Append("?client_id=").Append(dy_clientid);
                    //url.Append("&response_type=code"); 
                    //url.Append("&redirect_uri=").Append(System.Web.HttpUtility.UrlEncode(dy_redirect_url));
                    //url.Append("&response_mode=query");
                    //url.Append("&resource=").Append(System.Web.HttpUtility.UrlEncode(dy_resource_url));
                    //url.Append("&prompt=admin_consent");
                    //javascript attempt
                    https://login.microsoftonline.com/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize?
                    url.Append(dy_authoize_url).Append("?client_id=").Append(dy_clientid);
                    url.Append("&response_type=id_token");
                    url.Append("&redirect_uri=").Append(System.Web.HttpUtility.UrlEncode(dy_redirect_url));
                    url.Append("&resource=").Append(System.Web.HttpUtility.UrlEncode(dy_resource_url));
                    url.Append("&response_mode=form_post&scope=openid&state=12345&nonce=secretCode");

                    return MyAppsDb.ConvertJSONPOutput(callback, url.ToString(), HttpStatusCode.OK);

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


        //GET: api/SalesForce/GetAuthorizationToken? ValidationKey = ffe06298 - 22a8-4849-a46c-0284b04f2561
        [HttpGet]
        [ActionName("GetAuthorizationToken")]
        public HttpResponseMessage GetAuthorizationToken(string ObjectRef, int GroupId, string AuthCode, string ValidationKey, string IsNew, string callback)
        {
            string dy_token_post_url = "", dy_clientid = "", dy_redirect_url = "", dy_resource_url = "";

           HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                // MyAppsDb.GetTokenParametersDynamics(ref dy_clientid, ref dy_redirect_url, ref dy_resource_url, ref dy_token_post_url);
                dy_clientid = "1579d88e-bb6c-40ec-81ef-556c87319214";
                dy_redirect_url = "http://localhost:56786/Contact";
                dy_resource_url = "https://WEBSITEALIVEUS.onmicrosoft.com/websitealive";
                dy_token_post_url = "/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/token"; // "https://login.microsoftonline.com";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                try
                {
                    if (IsNew.Equals("Y"))
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://login.microsoftonline.com"); //");    
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                        var content = new StringContent(
                                "grant_type=authorization_code" + "&client_id=" + dy_clientid + "&code=" + AuthCode+
                                "&redirect_uri=" + System.Web.HttpUtility.UrlEncode(dy_redirect_url) +
                                "&resource=" + System.Web.HttpUtility.UrlEncode(dy_resource_url) +
                                "&client_secret=g3P1ZtRjtZ3hKbw2xP6xcCVJWZfdkRLXG3j3NF4XVLc=",
                                Encoding.UTF8,"application/x-www-form-urlencoded");
                        HttpResponseMessage response = client.PostAsync(dy_token_post_url, content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            string access_token = jObject["access_token"].ToString();
                            string refresh_token = jObject["refresh_token"].ToString();
                            string resource = jObject["resource"].ToString();
                           MyAppsDb.CreateNewIntegrationSettingForDynamicsUser(ObjectRef, GroupId, refresh_token, access_token, resource);
                        }
                    }
                    else
                    {
                        string DYRefreshToken = "", DYResourceURL = "";
                        //MyAppsDb.GetCurrentRefreshTokenDynamics(ObjectRef, GroupId, ref DYRefreshToken, ref DYResourceURL);
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://login.microsoftonline.com");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                        var content = new StringContent(
                                "client_id=" + dy_clientid +
                                "&refresh_token=" + DYRefreshToken +
                                "&grant_type=refresh_token" +
                                "&resource=" + System.Web.HttpUtility.UrlEncode(dy_resource_url) +
                                "&client_secret=g3P1ZtRjtZ3hKbw2xP6xcCVJWZfdkRLXG3j3NF4XVLc=",
                                Encoding.UTF8, "application/x-www-form-urlencoded");
                        //dy_token_post_url = "/WEBSITEALIVEUS.onmicrosoft.com/oauth2/token"; 
                        HttpResponseMessage response = client.PostAsync(dy_token_post_url, content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            string access_token = jObject["access_token"].ToString();
                            string refresh_token = jObject["refresh_token"].ToString();
                            string resource = jObject["resource"].ToString();
                            //MyAppsDb.CreateNewIntegrationSettingForDynamicsUser(ObjectRef, GroupId, refresh_token, access_token, );
                           // MyAppsDb.UpdateIntegrationSettingForUserDynamics(ObjectRef, GroupId, refresh_token, access_token, resource);
                        }

                    }

                    return MyAppsDb.ConvertJSONPOutput(callback, "API information updated!", HttpStatusCode.OK);

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
}
