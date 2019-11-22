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
    public class RootObject
    {
        public int vid { get; set; }

        public string message { get; set; }

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
        public string name { get; set; }
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
        public string bodyPreview { get; set; }
        public List<object> queueMembershipIds { get; set; }
        public bool bodyPreviewIsTruncated { get; set; }
        public string bodyPreviewHtml { get; set; }
        public bool gdprDeleted { get; set; }
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
                outhDetails.expires_on = DateTime.Now.AddSeconds(outhDetails.expires_in).ToString();
                outhDetails.Is_Authenticated = response.IsSuccessful;
            }
            catch (Exception ex)
            {
                outhDetails.Is_Authenticated = false;
                outhDetails.error_message = "Some Thing Went Wrong Please Try later";
            }
            return outhDetails;
        }

        public static OuthDetail RefreshAuthorizationTokens(CRMUser user)
        {
            OuthDetail outhDetails = new OuthDetail();

            var client = new RestClient(user.IntegrationConstants.ApiUrl);
            var request = new RestRequest("/oauth/v1/token", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", user.IntegrationConstants.ClientId);
            request.AddParameter("client_secret", user.IntegrationConstants.SecretKey);
            request.AddParameter("refresh_token", user.OuthDetail.refresh_token);
            try
            {
                var response = client.Execute(request);
                outhDetails = JsonConvert.DeserializeObject<OuthDetail>(response.Content);
                outhDetails.expires_on = DateTime.Now.AddSeconds(outhDetails.expires_in).ToString();
                outhDetails.Is_Authenticated = response.IsSuccessful;
            }
            catch (Exception ex)
            {
                outhDetails.Is_Authenticated = false;
                outhDetails.error_message = "Some Thing Went Wrong Please Try later";
            }
            return outhDetails;
        }

        public static string PostNewRecord(CRMUser user, CrmEntity crmEntity, out bool IsRecordAdded, out int? recordPrimaryId)
        {
            List<Property> property = new List<Property>();
            foreach (var item in crmEntity.CustomFields)
            {
                property.Add(new Property() { name = item.FieldName, property = item.FieldName, value = item.Value });
            }
            Properties properties = new Properties()
            {
                properties = property
            };
            string d = JsonConvert.SerializeObject(properties);
            var client = new RestClient(user.ApiUrl);
            //var request = new RestRequest("/contacts/v1/" + crmEntity.EntityName, Method.POST);
            var request = new RestRequest(crmEntity.SubUrl, Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            request.AddJsonBody(d);
            var response = client.Execute(request);
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IsRecordAdded = true;
                RootObject responseContent = JsonConvert.DeserializeObject<RootObject>(response.Content);
                recordPrimaryId = responseContent.vid;
                return "Record Added Successfully";
            }
            else
            {
                IsRecordAdded = false;
                RootObject responseContent = JsonConvert.DeserializeObject<RootObject>(response.Content);
                recordPrimaryId = null;
                return responseContent.message;
            }
        }

        public static string PostChats(CRMUser user, string message, int entityId, out bool IsChatAdded, out string ChatId)
        {
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = (long)(DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
            RootObjectNote n = new RootObjectNote() {
                associations = new Associations() { contactIds = new List<int>() { entityId } },
                metadata = new Metadata() { body = message },
                engagement = new Engagement() { active = true, ownerId = 1, timestamp = timeStamp, type = "NOTE" }
            };
            string d = JsonConvert.SerializeObject(n);
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/engagements/v1/engagements" , Method.POST);
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
                return "Some Thing went wrong. Please Contact Administrator !";
            }
        }

        public static string UpdateChats(CRMUser user, string message, int entityId, string chatId, out bool IsChatAdded)
        {
            var client = new RestClient(user.ApiUrl);
            RestRequest request = new RestRequest();
            
            // Get previous record
            request = new RestRequest("/engagements/v1/engagements/" + chatId, Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            var response = client.Execute(request);

            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                RootObject chats = JsonConvert.DeserializeObject<RootObject>(response.Content);
                ResponceContent responseContent = JsonConvert.DeserializeObject<ResponceContent>(response.Content);
                var chat = responseContent.metadata.body;
                chat = chat + message;
                request = new RestRequest("/engagements/v1/engagements/" + chatId, Method.PATCH);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
                RootObjectNote n = new RootObjectNote()
                {
                    associations = new Associations() { contactIds = new List<int>() { entityId } },
                    metadata = new Metadata() { body = chat },
                    engagement = new Engagement() { active = true, ownerId = 1, timestamp = 1409172644778, type = "NOTE" }
                };
                string d = JsonConvert.SerializeObject(n);
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

        public static CrmEntity GetSearchedRecord(CRMUser user, string sValue)
        {
            CrmEntity retEntityRecord = new CrmEntity();
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/contacts/v1/contact/email/" + sValue + "/profile?", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);
                //retEntityRecord.CustomFields.Add(new Dto.Models.EntityFieldsMetaData() { FieldLabel = "adsd", });
                //retEntityRecord.Email = contact.properties.email.value;
                //retEntityRecord.FirstName = contact.properties.firstname.value;q
                //retEntityRecord.LastName = contact.properties.lastname.value;
                return retEntityRecord;
            }
            else
            {
                return retEntityRecord;
            }
        }

        //public static CrmEntity GetRecordByEmail(CRMUser user, string email)
        //{
        //    CrmEntity retEntityRecord = new CrmEntity();
        //    var client = new RestClient(user.ApiUrl);
        //    var request = new RestRequest("/contacts/v1/contact/email/" + email + "/profile?", Method.GET);
        //    request.AddHeader("Content-Type", "application/json");
        //    request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
        //    var response = client.Execute(request);

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);
        //        retEntityRecord.Email = contact.properties.email.value;
        //        retEntityRecord.FirstName = contact.properties.firstname.value;
        //        retEntityRecord.LastName = contact.properties.lastname.value;
        //        return retEntityRecord;
        //    }
        //    else
        //    {
        //        return retEntityRecord;
        //    }
        //}

        public static string GetRecordById()
        {
            return "";
        }

        public static CrmEntity GetRecordList(CRMUser user, CrmEntity entityInfo, string sValue)
        {
            CrmEntity retEntityRecord = new CrmEntity();
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest(entityInfo.SubUrl + "/email/" + sValue + "/profile?", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            var response = client.Execute(request);
            RootObject info = JsonConvert.DeserializeObject<RootObject>(response.Content);
            retEntityRecord.Message = info.message;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                EntityFieldsMetaData rec = new EntityFieldsMetaData()
                {
                    FieldLabel = "Email",
                    FieldName = "email",
                    Value = info.properties.email.value
                };
                try
                {
                    retEntityRecord.CustomFields = new List<EntityFieldsMetaData>();
                    retEntityRecord.CustomFields.Add(rec);
                }
                catch (Exception ex)
                {
                    var obj = ex;
                    throw;
                }
                
                //CrmEntity rec = new CrmEntity()
                //{
                //    Id = contact.vid,
                //    Email = contact.properties.email.value,
                //    FirstName = contact.properties.firstname.value,
                //    LastName = contact.properties.lastname.value,
                //    CrmType = CrmType.HubSpot,
                //    AppType = AppType.Alive5
                //};
                //retEntityRecord.Add(rec);
                return retEntityRecord;
            }
            else
            {
                return retEntityRecord;
            }
        }
    }
}
