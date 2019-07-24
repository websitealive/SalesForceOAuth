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
    public class Sugar
    {
        public static OuthDetail Authenticate(CRMUser user)
        {
            OuthDetail outhDetails = new OuthDetail();

            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/rest/v10/oauth2/token", Method.POST);
            var loginDetail = new Dictionary<string, string>();
            loginDetail.Add("grant_type", "password");
            loginDetail.Add("client_id", "sugar");
            loginDetail.Add("client_secret", "");
            loginDetail.Add("username", user.UserName);
            loginDetail.Add("password", user.Password);
            loginDetail.Add("platform", "base");
            request.AddJsonBody(loginDetail);

            try
            {
                var response = client.Execute(request);
                outhDetails = JsonConvert.DeserializeObject<OuthDetail>(response.Content);
                outhDetails.Is_Authenticated = response.IsSuccessful;
            }
            catch (Exception ex)
            {
                outhDetails.Is_Authenticated = false;
                outhDetails.error_message = "You must specify a valid username, password and url.";
            }
            return outhDetails;
        }

        public static string UpdateEntity(CRMUser user, CrmEntity crmEntity)
        {
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/rest/v10/" + crmEntity.EntityName + "/" + crmEntity.EntityId, Method.PUT);
            request.AddHeader("Cache-Control", "no-cache");
            OuthDetail OuthDetail = Authenticate(user);
            request.AddHeader("OAuth-Token", OuthDetail.access_token);
            var updatedFields = new Dictionary<string, string>();
            updatedFields.Add("chatdescription_c", crmEntity.Message);
            request.AddJsonBody(updatedFields);

            try
            {
                var response = client.Execute(request);
                return "Chat Added Successfully";
            }
            catch (Exception ex)
            {
                return "Unable to add Chat";
            }


        }

    }
}
