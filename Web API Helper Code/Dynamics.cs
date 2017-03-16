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
    public class Dynamics
    {
        public static async System.Threading.Tasks.Task<HttpResponseMessage> GetAccessToken(string ObjectRef, string GroupId)
        {
                try
                {
                    //Live Code 
                    string accessToken = "", username = "", serviceURL = "", userPassword = "", clientId = "", authority = "";
                    DateTime tokenExpiryDT = DateTime.Now.AddDays(-1);
                    CRMTokenStatus userTokenStatus;
                    userTokenStatus = MyAppsDb.GetAccessTokenDynamics(ObjectRef, GroupId, ref accessToken, ref username, ref userPassword, ref clientId, ref serviceURL, ref tokenExpiryDT, ref authority);
                    //end Live Code 
                    if (userTokenStatus == CRMTokenStatus.SUCCESSS) // if a valid token is available
                    {
                        return MyAppsDb.ConvertStringOutput(accessToken, HttpStatusCode.OK);
                    }
                    else if (userTokenStatus == CRMTokenStatus.USERNOTFOUND) // if a user account is not found 
                    {
                    return MyAppsDb.ConvertStringOutput("User not registered to use this application.", HttpStatusCode.NotFound);
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
                        MyAppsDb.UpdateAccessTokenDynamics(ObjectRef, GroupId, res.AccessToken.ToString(), expiryDT);
                        return MyAppsDb.ConvertStringOutput(res.AccessToken.ToString(), HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertStringOutput("Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
        }

    }
}