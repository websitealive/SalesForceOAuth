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
    public class Properties
    {
        public List<Property> properties { get; set; }
    }
    public class Property
    {
        public string property { get; set; }
        public string value { get; set; }
    }
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

        public static string PostNewRecord(CRMUser user, CrmEntity crmEntity, out bool IsRecordAdded)
        {
            List<Property> p1 = new List<Property>();
            p1.Add(new Property() { property = "firstname", value = crmEntity.FirstName });
            p1.Add(new Property() { property = "lastname", value = crmEntity.LastName });
            p1.Add(new Property() { property = "email", value = crmEntity.Email });
            Properties p = new Properties()
            {
                properties = p1
            };
            string d = JsonConvert.SerializeObject(p);
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/contacts/v1/" + crmEntity.EntityName, Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            request.AddJsonBody(d);
            var response = client.Execute(request);
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IsRecordAdded = true;
                return "Record Added Successfully";
            }
            else
            {
                IsRecordAdded = false;
                ResponceContent responseContent = JsonConvert.DeserializeObject<ResponceContent>(response.Content);
                return "Some Thing went wrong. Please Contact Administrator !";
            }
        }

        public static string GetRecordById()
        {
            return "";
        }

        public static string GetRecordList()
        {
            return "";
        }
    }
}
