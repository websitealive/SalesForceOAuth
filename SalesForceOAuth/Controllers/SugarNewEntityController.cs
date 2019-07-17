using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace SalesForceOAuth.Controllers
{
    public class SugarNewEntityController : ApiController
    {
        public class SugarNewEntity
        {
            //public string status { get; set; }
            //public string salutation { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            //public string status_description { get; set; }
            //public string alias_c { get; set; }
            //public string birthdate { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage SugarNewEntityCreate(SugarNewEntity Ent)
        {
            var httpClient = new HttpClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://zensyi0756.trial.sugarcrm.com/rest/v10/Leads");

            string auth_token = auth_Token();
            httpRequest.Headers.Add("OAuth-Token", auth_token);

            var values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("first_name", Ent.first_name));
            values.Add(new KeyValuePair<string, string>("last_name", Ent.last_name));

            //values.Add(new KeyValuePair<string, string>("chatdescription_c", ));

            httpRequest.Content = new FormUrlEncodedContent(values);
            //HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            Task<HttpResponseMessage> response = httpClient.SendAsync(httpRequest);
            var result = response.Result;
            var idd = result.Content.ReadAsStringAsync();
            return result;
        }

        [HttpPost]
        public HttpResponseMessage PostSugarChat(MessageData data)
        {
            //int Entity_id = getIDOfEntity(GroupId, ObjectRef, SessionId, Message);
            object Entity_id = getIDOfEntity(data);

            var client = new RestClient("https://zensyi0756.trial.sugarcrm.com/rest/v10/");
            //var request = new RestRequest("Leads/b99bfc08-a812-11e9-b152-024e8f81e1a2", Method.PUT);
            var request = new RestRequest("Leads/" + Entity_id, Method.PUT);

            request.AddHeader("Cache-Control", "no-cache");


            string auth_token = auth_Token();
            request.AddHeader("OAuth-Token", auth_token);

            var updatedFields = new Dictionary<string, string>();
            var message1 = data.Message.Replace("|", "\r\n").Replace("&#39;", "'");
            //updatedFields.Add("chatdescription_c", data.Message);
            updatedFields.Add("chatdescription_c", message1);
            request.AddJsonBody(updatedFields);
            var response = client.Execute(request);

            return null;
        }

        public object getIDOfEntity(MessageData data)
        //public int getIDOfEntity(int GroupId, string ObjectRef, int SessionId, string Message)
        {
            string urlReferrer = Request.RequestUri.Authority.ToString();
            object Entity_id = null;
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, data.ObjectRef/*ObjectRef*/);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_sugar_queue WHERE status = 0 AND  objectref = '" + data.ObjectRef + "' AND groupid = " + data.GroupId.ToString() + " AND sessionid = " + data.SessionId.ToString();
                    //string sql = "SELECT * FROM integration_sugar_queue WHERE status = 0 AND  objectref = '" + ObjectRef + "' AND groupid = " + GroupId.ToString() + " AND sessionid = " + SessionId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                Entity_id = (rdr["object_id"]);
                                //itemType = rdr["object_type"].ToString().Trim();
                                //itemId = rdr["object_id"].ToString().Trim();
                                //ownerEmail = rdr["owner_email"].ToString().Trim();
                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                    return Entity_id;
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }


        public string auth_Token()
        {
            var httpClient = new HttpClient();
            var httpContent = new HttpRequestMessage(HttpMethod.Post, "https://zensyi0756.trial.sugarcrm.com/rest/v10/oauth2/token");
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "password"));
            values.Add(new KeyValuePair<string, string>("client_id", "sugar"));
            values.Add(new KeyValuePair<string, string>("client_secret", ""));
            values.Add(new KeyValuePair<string, string>("username", "admin"));
            values.Add(new KeyValuePair<string, string>("password", "asdf"));
            values.Add(new KeyValuePair<string, string>("platform", "base"));
            httpContent.Content = new FormUrlEncodedContent(values);
            Task<HttpResponseMessage> response = httpClient.SendAsync(httpContent);
            var result = response.Result;
            var idd = result.Content.ReadAsStringAsync();
            var tdsfds = idd.Result;
            var t = idd.Result[0];
            var tt = idd.Result[1];

            var tooString = tdsfds.ToString();


            var json_serializer = new JavaScriptSerializer();
            List<string> routes_list = json_serializer.Deserialize<List<string>>(tooString);
            List<string> routes_list2 = json_serializer.Deserialize<List<string>>(tdsfds);


            var json_serializer1 = new JavaScriptSerializer();
            var routes_list1 = (IDictionary<string, object>)json_serializer.DeserializeObject(tdsfds);
            var auth_token = routes_list1["access_token"];

            return auth_token.ToString();
        }
        [HttpGet]
        public HttpResponseMessage GetTagChat(string token, string ObjectRef, int GroupId, int SessionId, string ObjType, string ObjId, string callback, string OwnerId)
        {
            #region JWT Token 
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DyChat-GetTagChat", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            #endregion JWT Token
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MyAppsDb.TagChatSugar(ObjectRef, GroupId, SessionId, ObjType, ObjId, OwnerId, urlReferrer);
                PostedObjectDetail output = new PostedObjectDetail();
                output.ObjectName = "TagChat";
                output.Message = "Chat Tagged successfully!";
                return MyAppsDb.ConvertJSONPOutput(callback, output, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYChat-GetTagChat", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}