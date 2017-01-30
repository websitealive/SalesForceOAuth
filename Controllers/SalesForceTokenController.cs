using Salesforce.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Salesforce.Common.Models;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
namespace SalesForceOAuth.Controllers
{
    public class SalesForceTokenController : ApiController
    {
        //GET: api/SalesForce/GetAuthorizationToken
        [HttpGet]
        [ActionName("GetAuthorizationToken")]
        public HttpResponseMessage GetAuthorizationToken(string token, string callback)
        {
            //string ObjectRef, int GroupId, string AuthCode, string IsNew, string callback, string ValidationKey, 
            //var re = Request;
            //var headers = re.Headers;
            string ObjectRef = "", AuthCode = "", IsNew = "";
            int GroupId = 0; 
            //if (headers.Contains("Authorization"))
            //{
               // string _token = HttpRequestMessageExtensions.GetHeader(re, "Authorization");
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback,ex.InnerException, HttpStatusCode.InternalServerError);
                }
                JObject values = JObject.Parse(outputPayload); // parse as array  
                GroupId = Convert.ToInt32(values.GetValue("GroupId"));
                ObjectRef = values.GetValue("ObjectRef").ToString();
                AuthCode = values.GetValue("AuthCode").ToString();
                IsNew = values.GetValue("IsNew").ToString();

                string sf_clientid = "", sf_callback_url = "", sf_consumer_key = "", sf_consumer_secret = "", sf_token_req_end_point = "";
                MyAppsDb.GetTokenParameters(ref sf_clientid, ref sf_callback_url, ref sf_consumer_key, ref sf_consumer_secret, ref sf_token_req_end_point);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                try
                {
                    if (IsNew.Equals("Y"))
                    {
                        auth.WebServerAsync(sf_consumer_key, sf_consumer_secret, sf_callback_url, AuthCode, sf_token_req_end_point).Wait();
                        MyAppsDb.CreateNewIntegrationSettingForUser(ObjectRef, GroupId, auth.RefreshToken, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                    }
                    else
                    {
                        string SFRefreshToken = "";
                        MyAppsDb.GetCurrentRefreshToken(ObjectRef, GroupId, ref SFRefreshToken);
                        auth.TokenRefreshAsync(sf_clientid, SFRefreshToken, sf_consumer_secret, sf_token_req_end_point).Wait();
                        MyAppsDb.UpdateIntegrationSettingForUser(ObjectRef, GroupId, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                    }

                    return MyAppsDb.ConvertJSONPOutput(callback,"API information updated!", HttpStatusCode.OK);

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback ,"Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            //}
            //else
            //{
            //    return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
            //}
        }

    }
}
