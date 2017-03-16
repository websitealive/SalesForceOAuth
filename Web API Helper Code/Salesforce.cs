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
using SalesForceOAuth.Controllers;


namespace SalesForceOAuth.Web_API_Helper_Code
{
    public class Salesforce
    {
        public static async System.Threading.Tasks.Task<HttpResponseMessage> GetAccessToken(string ObjectRef, int GroupId, string siteRef)
        {
            //get current access token 
            string accessToken = "";
            CRMTokenStatus userTokenStatus;
            userTokenStatus = MyAppsDb.GetAccessTokenSalesForce(ObjectRef, GroupId, ref accessToken);
            //end Live Code 
            if (userTokenStatus == CRMTokenStatus.SUCCESSS) // if a valid token is available
            {
                return MyAppsDb.ConvertStringOutput(accessToken, HttpStatusCode.OK);
            }
            else if (userTokenStatus == CRMTokenStatus.USERNOTFOUND) // if a user account is not found 
            {
                return MyAppsDb.ConvertStringOutput("User not registered to use this application.", HttpStatusCode.NotFound);
            }
            else
            {
                string sf_clientid = "", sf_callback_url = "", sf_consumer_key = "", sf_consumer_secret = "", sf_token_req_end_point = "";
                //MyAppsDb.GetRedirectURLParametersCallBack(ref sf_callback_url, siteRef);
                sf_callback_url = System.Web.HttpUtility.UrlDecode(siteRef);
                MyAppsDb.GetTokenParameters(ref sf_clientid, ref sf_consumer_key, ref sf_consumer_secret, ref sf_token_req_end_point);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                try
                {
                    string SFRefreshToken = "";
                    MyAppsDb.GetCurrentRefreshToken(ObjectRef, GroupId, ref SFRefreshToken);
                    auth.TokenRefreshAsync(sf_clientid, SFRefreshToken, sf_consumer_secret, sf_token_req_end_point).Wait();
                    MyAppsDb.UpdateIntegrationSettingForUser(ObjectRef, GroupId, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                    return MyAppsDb.ConvertJSONOutput("API information updated!", HttpStatusCode.OK);

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONOutput("Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}