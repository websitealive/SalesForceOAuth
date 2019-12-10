using CRM.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
        public Associations associations { get; set; }
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
        public List<Int64> associatedCompanyIds { get; set; }
        public List<Int64> associatedVids { get; set; }
        // Below properties are used for engagments
        public List<Int64> contactIds { get; set; }
        public List<Int64> companyIds { get; set; }
        public List<Int64> dealIds { get; set; }
        public List<Int64> ownerIds { get; set; }
    }

    public class Metadata
    {
        public string body { get; set; }
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

        public static string PostNewRecord(CRMUser user, CrmEntity crmEntity, out bool IsRecordAdded, out Int64? recordPrimaryId)
        {
            dynamic propertiesList = new ExpandoObject();
            List<dynamic> properties = new List<dynamic>();
            dynamic associations = new ExpandoObject();
            foreach (var item in crmEntity.CustomFields)
            {
                if (item.FieldType == "textbox")
                {
                    dynamic property = new ExpandoObject();
                    property.name = item.FieldName;
                    property.property = item.FieldName;
                    property.value = item.Value;
                    properties.Add(property);
                    propertiesList.properties = properties;
                }
                else
                {
                    List<dynamic> ids = new List<dynamic>();
                    ids.Add(item.Value);
                    ((IDictionary<String, Object>)associations)[item.FieldName] = ids;
                    propertiesList.associations = associations;
                }
            }

            string d = JsonConvert.SerializeObject(propertiesList);
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest(crmEntity.SubUrl, Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);

            request.AddJsonBody(d);
            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IsRecordAdded = true;
                dynamic info = JsonConvert.DeserializeObject<dynamic>(response.Content);
                recordPrimaryId = info[crmEntity.EntityPrimaryKey];
                return "Record Added Successfully";
            }
            else
            {
                IsRecordAdded = false;
                dynamic info = JsonConvert.DeserializeObject<dynamic>(response.Content);
                recordPrimaryId = null;
                return info["message"];
            }
        }

        public static string PostChats(CRMUser user, string message, string entityName, Int64 entityId, out bool IsChatAdded, out string ChatId)
        {
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = (long)(DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
            Associations associations = new Associations();
            List<Int64> associatedIds = new List<Int64>();
            associatedIds.Add(entityId);
            if (entityName == "contact")
            {
                associations.contactIds = associatedIds;
            }
            if (entityName == "companies")
            {
                associations.companyIds = associatedIds;
            }
            if (entityName == "deal")
            {
                associations.dealIds = associatedIds;
            }
            RootObject n = new RootObject()
            {
                associations = associations,
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

        public static string UpdateChats(CRMUser user, string message, string entityName, Int64 entityId, string chatId, out bool IsChatAdded)
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
                //RootObjectNote n = new RootObjectNote()
                //{
                //    associations = new Associations() { contactIds = new List<int>() { entityId } },
                //    metadata = new Metadata() { body = chat },
                //    engagement = new Engagement() { active = true, ownerId = 1, timestamp = 1409172644778, type = "NOTE" }
                //};
                DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long timeStamp = (long)(DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
                Associations associations = new Associations();
                List<Int64> associatedIds = new List<Int64>();
                associatedIds.Add(entityId);
                if (entityName == "contact")
                {
                    associations.contactIds = associatedIds;
                }
                if (entityName == "companies")
                {
                    associations.companyIds = associatedIds;
                }
                if (entityName == "deal")
                {
                    associations.dealIds = associatedIds;
                }
                RootObject n = new RootObject()
                {
                    associations = associations,
                    metadata = new Metadata() { body = chat },
                    engagement = new Engagement() { active = true, ownerId = 1, timestamp = timeStamp, type = "NOTE" }
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
                // retEntityRecord. = contact.properties.email.value;
                //retEntityRecord.FirstName = contact.properties.firstname.value;q
                //retEntityRecord.LastName = contact.properties.lastname.value;
                return retEntityRecord;
            }
            else
            {
                return retEntityRecord;
            }
        }

        public static CrmEntity GetRecordByEmail(CRMUser user, string email)
        {
            CrmEntity retEntityRecord = new CrmEntity();
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest("/contacts/v1/contact/email/" + email + "/profile?", Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);
                //retEntityRecord.EntityDispalyName = contact.properties.email.value;
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

        public static List<CrmEntity> GetRecordList(CRMUser user, CrmEntity entityInfo, string sValue)
        {
            List<CrmEntity> retRecordList = new List<CrmEntity>();
            string suburl = string.Empty;
            if (entityInfo.SubUrl.Contains("contact"))
            {
                // suburl = entityInfo.SubUrl + "/email/" + sValue + "/profile?";
                suburl = "contacts/v1/search/query?q=" + sValue;
            } else
            {
                suburl = entityInfo.SubUrl + "/paged?properties=" + entityInfo.PrimaryFieldUniqueName;
            }
            var client = new RestClient(user.ApiUrl);
            var request = new RestRequest(suburl, Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + user.OuthDetail.access_token);
            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic info = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string entitieRecords = entityInfo.SubUrl.IndexOf('/') == 0 ? entityInfo.SubUrl.Substring(1, entityInfo.SubUrl.IndexOf('/', entityInfo.SubUrl.IndexOf('/') + 1) - 1) : entityInfo.SubUrl.Substring(0, entityInfo.SubUrl.IndexOf('/'));
                var ccc = info[entitieRecords];
                foreach (var item in info[entitieRecords])
                {
                    CrmEntity retRecord = new CrmEntity();
                    retRecord.EntityUniqueName = entityInfo.EntityUniqueName;
                    retRecord.EntityDispalyName = entityInfo.EntityPrimaryKey;
                    retRecord.PrimaryFieldUniqueName = entityInfo.PrimaryFieldUniqueName;
                    if (entityInfo.SubUrl.Contains("contact"))
                    {
                        retRecord.EntityPrimaryKeyValue = ((Newtonsoft.Json.Linq.JValue)item[entityInfo.EntityPrimaryKey]).Value.ToString();
                        retRecord.PrimaryFieldValue = ((Newtonsoft.Json.Linq.JValue)item["properties"][entityInfo.PrimaryFieldUniqueName].value).Value.ToString();
                        retRecordList.Add(retRecord);
                    }
                    else
                    {
                        if (((Newtonsoft.Json.Linq.JValue)item["properties"][entityInfo.PrimaryFieldUniqueName].value).Value.ToString().ToLower().Contains(sValue.ToLower()))
                        {
                            retRecord.EntityPrimaryKeyValue = ((Newtonsoft.Json.Linq.JValue)item[entityInfo.EntityPrimaryKey]).Value.ToString();
                            retRecord.PrimaryFieldValue = ((Newtonsoft.Json.Linq.JValue)item["properties"][entityInfo.PrimaryFieldUniqueName].value).Value.ToString();
                            retRecordList.Add(retRecord);
                        }  
                    }
                }
                //if (entityInfo.SubUrl.Contains("contact")) {
                //    RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);
                //    entityInfo.EntityPrimaryKey = contact.vid.ToString();
                //    entityInfo.PrimaryFieldValue = contact.properties.email.value;
                //    retEntityRecord.Add(entityInfo);
                //} else
                //{
                //    dynamic info = JsonConvert.DeserializeObject<dynamic>(response.Content);
                //    string entitieRecords = entityInfo.SubUrl.IndexOf('/') == 0 ? entityInfo.SubUrl.Substring(1, entityInfo.SubUrl.IndexOf('/', entityInfo.SubUrl.IndexOf('/') + 1) - 1) : entityInfo.SubUrl.Substring(0, entityInfo.SubUrl.IndexOf('/'));
                //    foreach (var item in info[entitieRecords])
                //    {
                //        if (((Newtonsoft.Json.Linq.JValue)item["properties"][entityInfo.PrimaryFieldUniqueName].value).Value.ToString().Contains(sValue))
                //        {
                //            entityInfo.EntityPrimaryKey = ((Newtonsoft.Json.Linq.JValue)item[entityInfo.EntityPrimaryKey]).Value.ToString();
                //            entityInfo.PrimaryFieldValue = ((Newtonsoft.Json.Linq.JValue)item["properties"][entityInfo.PrimaryFieldUniqueName].value).Value.ToString();
                //            retEntityRecord.Add(entityInfo);
                //        }
                //    }
                //}
            }
            return retRecordList;
        }
    }
}
