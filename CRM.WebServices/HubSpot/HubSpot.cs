using CRM.Dto;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.WebServices
{
    public class HubSpot
    {
        public static OuthDetail GetAuthorizationTokens(CRMUser user)
        {
            OuthDetail outhDetails = new OuthDetail();

            var client = new RestClient(user.IntegrationConstants.ApiUrl);
            var request = new RestRequest("/oauth/v1/token", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", user.IntegrationConstants.ClientId);
            request.AddParameter("client_secret", user.IntegrationConstants.SecretKey);
            request.AddParameter("redirect_uri", user.IntegrationConstants.RedirectedUrl);
            request.AddParameter("code", user.AuthCode);

            //var client = new RestClient(user.ApiUrl);
            //var request = new RestRequest("/oauth/v1/token", Method.POST);
            //var data = new Dictionary<string, string>();
            //data.Add("grant_type", "authorization_code");
            //data.Add("client_id", user.IntegrationConstants.ClientId);
            //data.Add("client_secret", user.IntegrationConstants.SecretKey);
            //data.Add("client_secret", user.IntegrationConstants.SecretKey);
            //data.Add("redirect_uri", user.IntegrationConstants.RedirectedUrl);
            //data.Add("code", user.AuthCode);
            //request.AddJsonBody(data);

            try
            {
                var response = client.Execute(request);
                outhDetails = JsonConvert.DeserializeObject<OuthDetail>(response.Content);
                outhDetails.Is_Authenticated = response.IsSuccessful;
            }
            catch (Exception ex)
            {
                outhDetails.Is_Authenticated = false;
                outhDetails.error_message = "Some Thing Went Wrong Please Try later";
            }
            return outhDetails;
        }
    }
}
