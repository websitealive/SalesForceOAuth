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

namespace SalesForceOAuth.Controllers
{
    public class DynamicsController : ApiController
    {
        [HttpGet]
        [ActionName("GetRedirectURL")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetRedirectURL(string ValidationKey, string callback)
        {

            string dy_authoize_url = "", dy_clientid = "", dy_redirect_url = "", dy_resource_url = "";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    //MyAppsDb.GetRedirectURLParametersDynamics(ref dy_authoize_url, ref dy_clientid, ref dy_redirect_url, ref dy_resource_url);
                    dy_clientid = "1579d88e-bb6c-40ec-81ef-556c87319214";
                    dy_redirect_url = "http://localhost:56786/Contact";
                    dy_resource_url = "https://websitealiveus.crm.dynamics.com/";
                    dy_authoize_url = "https://login.microsoftonline.com/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize";
                    StringBuilder url = new StringBuilder();
                    //= https://WEBSITEALIVEUS.crm.dynamics.com/; Username=; Password=Unstoppable.1o; " />
                    //   UserCredential cred = new UserCredential("dev@WEBSITEALIVEUS.onmicrosoft.com", "Unstoppable.1o");
                    //setting up configuration file 
                    string username = "dev@WEBSITEALIVEUS.onmicrosoft.com";
                    string redirecturl = "http://localhost:56786/Contact", serviceurl = "https://WEBSITEALIVEUS.crm.dynamics.com/";
                    var password = "Unstoppable.1o";
                    string clientId = "2a9ce073-9a16-4ea8-a306-2b601537a46c"; 
                    var passwordSecure = new System.Security.SecureString();
                    foreach (char c in password) passwordSecure.AppendChar(c);
                    Web_API_Helper_Code.Configuration _config = null;
                    _config = new Web_API_Helper_Code.Configuration(username, passwordSecure, redirecturl, serviceurl, clientId);
                    string authority = "https://login.windows.net/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize/"; 
                    // authentication class 
                    Web_API_Helper_Code.Authentication _auth = new Authentication(_config, authority);
                   
                    AuthenticationResult res = await _auth.AcquireToken(); 
                    

                    //HttpMessageHandler _clientHandler = null;
                    //AuthenticationContext _context = null;
                    //string _authority = "https://login.windows.net/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize";
                    //_clientHandler = new OAuthMessageHandler(this, new HttpClientHandler());
                    //_context = new AuthenticationContext(Authority, false);


                    //var credentials = new UserPasswordCredential("dev@WEBSITEALIVEUS.onmicrosoft.com", "Unstoppable.1o");
                    //_context.AcquireTokenAsync("https://WEBSITEALIVEUS.crm.dynamics.com/", dy_clientid, credentials); 




                    //url.Append(dy_authoize_url).Append("?client_id=").Append(dy_clientid);
                    //url.Append("&response_type=code"); 
                    //url.Append("&redirect_uri=").Append(System.Web.HttpUtility.UrlEncode(dy_redirect_url));
                    //url.Append("&response_mode=query");
                    //url.Append("&resource=").Append(System.Web.HttpUtility.UrlEncode(dy_resource_url));
                    //url.Append("&prompt=admin_consent");
                    //javascript attempt
                    // https://login.microsoftonline.com/9025f8ca-d280-4bef-9c50-01623cd86f9b/oauth2/authorize?

                    //url.Append(dy_authoize_url).Append("?client_id=").Append(dy_clientid);
                    //url.Append("&response_type=code");
                    //url.Append("&redirect_uri=").Append(System.Web.HttpUtility.UrlEncode(dy_redirect_url));
                    //url.Append("&resource=").Append(System.Web.HttpUtility.UrlEncode(dy_resource_url));
                    //url.Append("&state=b749b6b4-643a-4d71-8909-b976697f881e&nonce=258a431d-e3e7-4a24-a045-c49fbb1b1dcd");

                    return MyAppsDb.ConvertJSONPOutput(callback, res.AccessToken.ToString(), HttpStatusCode.OK);

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
        //        dy_resource_url = "https://websitealiveus.crm.dynamics.com";
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
        //                //dy_token_post_url = "/WEBSITEALIVEUS.onmicrosoft.com/oauth2/token"; 
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
   
}
