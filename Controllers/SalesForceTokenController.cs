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
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAuthorizationToken(string token, string ObjectRef, string AuthCode, int GroupId, string IsNew, string siteRef, string callback)
        {
            //string ObjectRef, int GroupId, string AuthCode, string IsNew, string callback, string ValidationKey, 
            //var re = Request;
            //var headers = re.Headers;
            //string ObjectRef = "", AuthCode = "", IsNew = "";
            //int GroupId = 0; 
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
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SalesForceToken-GetAuthorizationToken", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            //JObject values = JObject.Parse(outputPayload); // parse as array  
            //GroupId = Convert.ToInt32(GroupId);
            //ObjectRef = values.GetValue("ObjectRef").ToString();
            //AuthCode = values.GetValue("AuthCode").ToString();
            //IsNew = values.GetValue("IsNew").ToString();
            //int groupId = Convert.ToInt32(GroupId);

            string sf_clientid = "", sf_callback_url = "", sf_consumer_key = "", sf_consumer_secret = "", sf_token_req_end_point = "";
            //MyAppsDb.GetRedirectURLParametersCallBack(ref sf_callback_url, siteRef);
            sf_callback_url = System.Web.HttpUtility.UrlDecode(siteRef);
            try
            {
                MyAppsDb.GetTokenParameters(ref sf_clientid, ref sf_consumer_key, ref sf_consumer_secret, ref sf_token_req_end_point);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SalesForceToken-GetAuthorizationToken", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var auth = new AuthenticationClient();
            try
            {
                if (IsNew.Equals("Y"))
                {
                    await auth.WebServerAsync(sf_consumer_key, sf_consumer_secret, sf_callback_url, AuthCode, sf_token_req_end_point).ConfigureAwait(false);
                    MyAppsDb.CreateNewIntegrationSettingForUser(ObjectRef, GroupId, auth.RefreshToken, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                }
                else
                {
                    string SFRefreshToken = "";
                    MyAppsDb.GetCurrentRefreshToken(ObjectRef, GroupId, ref SFRefreshToken);
                    await auth.TokenRefreshAsync(sf_clientid, SFRefreshToken, sf_consumer_secret, sf_token_req_end_point).ConfigureAwait(false);
                    MyAppsDb.UpdateIntegrationSettingForUser(ObjectRef, GroupId, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                }
                return MyAppsDb.ConvertJSONPOutput(callback, "API information updated!", HttpStatusCode.OK,false);

            }
            catch (Exception ex)
            {
                //return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal Error: " + ex.Message);
                return MyAppsDb.ConvertJSONPOutput(callback,ex, "SalesForceToken-GetAuthorizationToken", "Unhandled exception", HttpStatusCode.InternalServerError);
            }

        }

    }
}
