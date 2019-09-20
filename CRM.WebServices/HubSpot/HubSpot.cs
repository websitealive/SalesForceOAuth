using CRM.Dto;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CRM.WebServices
{
    public class RootObject
    {
        public int vid { get; set; }

        public Properties properties { get; set; }

        public Engagement engagement { get; set; }
        public Associations associations { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Properties
    {
        public Property email { get; set; }
        public Property firstname { get; set; }
        public Property lastname { get; set; }
        public List<Property> properties { get; set; }
    }
    public class Property
    {
        public string property { get; set; }
        public string value { get; set; }
    }

    // Note starts
    public class Engagement
    {
        public bool active { get; set; }
        public int ownerId { get; set; }
        public string type { get; set; }
        public long timestamp { get; set; }
    }

    public class Associations
    {
        public List<int> contactIds { get; set; }
    }

    public class Metadata
    {
        public string body { get; set; }
    }

    public class RootObjectNote
    {
        public Engagement engagement { get; set; }
        public Associations associations { get; set; }
        public Metadata metadata { get; set; }
    }
    // Note End
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
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
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

        public static string PostChats(CRMUser user, string message, out bool IsChatAdded, out string ChatId)
        {
            RootObjectNote n = new RootObjectNote()
            {
                associations = new Associations() { contactIds = new List<int>() { 901 } },
                metadata = new Metadata() { body = message },
                engagement = new Engagement() { active = true, ownerId = 1, timestamp = 1409172644778, type = "NOTE" }
            };
            string d = JsonConvert.SerializeObject(n);
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/engagements/v1/engagements", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            request.AddJsonBody(d);
            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IsChatAdded = true;
                ResponceContent responseContent = JsonConvert.DeserializeObject<ResponceContent>(response.Content);
                ChatId = responseContent.engagement.id;
                return "Record Added Successfully";
            }
            else
            {
                IsChatAdded = false;
                ChatId = null;
                // ResponceContent responseContent = JsonConvert.DeserializeObject<ResponceContent>(response.Content);
                return "Some Thing went wrong. Please Contact Administrator !";
            }
        }

        public static string UpdateChats(CRMUser user, string message, string chatId, out bool IsChatAdded)
        {
            RootObjectNote n = new RootObjectNote()
            {
                associations = new Associations() { contactIds = new List<int>() { 901 } },
                metadata = new Metadata() { body = message },
                engagement = new Engagement() { active = true, ownerId = 1, timestamp = 1409172644778, type = "NOTE" }
            };
            string d = JsonConvert.SerializeObject(n);
            var client = new RestClient(user.ApiUrl);
            RestRequest request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            // Get previous record
            request = new RestRequest("/engagements/v1/engagements/" + chatId, Method.GET);
            var getChat = client.Execute(request);
            if (getChat.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ResponceContent responseContent = JsonConvert.DeserializeObject<ResponceContent>(getChat.Content);
                var chat = responseContent.metadata.body;
                chat = chat + message;
                request = new RestRequest("/engagements/v1/engagements/" + chatId, Method.PATCH);
                request.AddJsonBody(d);
                var updateChat = client.Execute(request);
                if (updateChat.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IsChatAdded = true;
                    return "Record Updated Successfully";
                }
                else
                {
                    IsChatAdded = false;
                    return "Some Thing went wrong. Please Contact Administrator !";
                }
            }
            else
            {
                IsChatAdded = false;
                return "Some Thing went wrong. Please Contact Administrator !";
            }
        }

        public static List<CrmEntity> GetRecordByEmail(CRMUser user, string email)
        {
            List<CrmEntity> retEntityRecord = new List<CrmEntity>();
            var client = new RestClient(user.ApiUrl);

            var request = new RestRequest("/contacts/v1/contact/email/" + email + "/profile?", Method.GET);
            var request2 = new RestRequest("/contacts/v1/search/query?q=" + email);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            request2.AddHeader("Content-Type", "application/json");
            request2.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            var response = client.Execute(request);
            var response2 = client.Execute(request2);

            if (response2.StatusCode == System.Net.HttpStatusCode.OK)
            {
                RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);
                RootObject contact2 = JsonConvert.DeserializeObject<RootObject>(response2.Content);
                var contactttt2 = JsonConvert.SerializeObject(response2.Content);
                var contact3 = JsonConvert.DeserializeObject(response2.Content);

                    foreach (var t in contactttt2)
                {
                    int x = 3;
                }
                
                //retEntityRecord.Email = contact.properties.email.value;
                //retEntityRecord.FirstName = contact.properties.firstname.value;
                //retEntityRecord.LastName = contact.properties.lastname.value;
                return retEntityRecord;
            }
            else
            {
                return retEntityRecord;
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
