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

namespace SalesForceOAuth.Controllers
{
    public class Contact
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class SalesForceController : ApiController
    {
        /// <summary>
        /// GET: api/SalesForce/GetRedirectURL
        /// ValidationKey in header
        /// This is the first step of OAuth, getting the URL to redirect to. 
        /// </summary>
        /// <param name="ValidationKey"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ActionName("GetRedirectURL")]
        public HttpResponseMessage GetRedirectURL(string ValidationKey, string callback)
        {

            string sf_authoize_url = "", sf_clientid = "", sf_callback_url = "";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    MyAppsDb.GetRedirectURLParameters(ref sf_authoize_url, ref sf_clientid, ref sf_callback_url);

                    //var url =
                    //Common.FormatAuthUrl(
                    //    "https://login.salesforce.com/services/oauth2/authorize",
                    //    ResponseTypes.Code,
                    //    "3MVG9KI2HHAq33RwXJsqtsEtY.ThMCzS5yZd3S8CzXBArijS0WEQgYACVnQ9SJq0KDdKrQgIxPFNPOIQhuqdK",
                    //    System.Web.HttpUtility.UrlEncode("http://localhost:56786/About.aspx"));

                    string url = Common.FormatAuthUrl(sf_authoize_url, ResponseTypes.Code, sf_clientid, System.Web.HttpUtility.UrlEncode(sf_callback_url));
                    return MyAppsDb.ConvertJSONPOutput(callback, url, HttpStatusCode.OK);
                }
                catch(Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
           }
            else
            {
               return MyAppsDb.ConvertJSONPOutput(callback, "Your request isn't authorized!", HttpStatusCode.Unauthorized);
            }
        }

        
        //GET: api/SalesForce/GetAuthorizationToken? ValidationKey = ffe06298 - 22a8-4849-a46c-0284b04f2561
        [HttpGet]
        [ActionName("GetAuthorizationToken")]
        public HttpResponseMessage GetAuthorizationToken(string ObjectRef, int GroupId, string AuthCode , string ValidationKey, string IsNew, string callback)
        {
            string sf_clientid = "", sf_callback_url = "", sf_consumer_key = "", sf_consumer_secret = "", sf_token_req_end_point = "";
            HttpResponseMessage outputResponse = new HttpResponseMessage();
            if (ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                MyAppsDb.GetTokenParameters(ref sf_clientid, ref sf_callback_url, ref sf_consumer_key, ref sf_consumer_secret, ref sf_token_req_end_point);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var auth = new AuthenticationClient();
                try
                {
                    if (IsNew.Equals("Y"))
                    {
                        auth.WebServerAsync(sf_consumer_key, sf_consumer_secret, sf_callback_url, AuthCode, sf_token_req_end_point).Wait();
                        MyAppsDb.CreateNewIntegrationSettingForUser(ObjectRef, GroupId, auth.RefreshToken, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl);
                    }
                    else
                    {
                        string SFRefreshToken="";
                        MyAppsDb.GetCurrentRefreshToken(ObjectRef, GroupId, ref SFRefreshToken);
                        auth.TokenRefreshAsync(sf_clientid, SFRefreshToken, sf_consumer_secret, sf_token_req_end_point).Wait();
                        MyAppsDb.UpdateIntegrationSettingForUser(ObjectRef, GroupId, auth.AccessToken, auth.ApiVersion, auth.InstanceUrl); 
                    }

                    return MyAppsDb.ConvertJSONPOutput(callback, "API information updated!", HttpStatusCode.OK);

                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, "Internal Error: " + ex.InnerException, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return MyAppsDb.ConvertJSONPOutput(callback, "Your request isn't authorized!", HttpStatusCode.Unauthorized);
            }
        }
    }
    public class MyAppsDb
    {
        #region SalesForce Methods
        public static void GetRedirectURLParameters(ref string sf_authoize_url, ref string sf_clientid, ref string sf_callback_url)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integrations_constants where id = 1";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        sf_authoize_url = rdr["sf_authoize_url"].ToString();
                        sf_clientid = rdr["sf_clientid"].ToString();
                        sf_callback_url = rdr["sf_callback_url"].ToString();
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void UpdateIntegrationSettingForUserDynamics(string objectRef, int groupId, string refresh_token, string access_token, string resource)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "Update integration_settings_dynamics Set DYAccessToken = '" + access_token + "', SFATCreationDT=now(), resource='" + resource + "'";
                sql += " WHERE objectRef = '" + objectRef + "' AND groupId = " + groupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetTokenParameters(ref string sf_clientid, ref string sf_callback_url, ref string sf_consumer_key, ref string sf_consumer_secret, ref string sf_token_req_end_point)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integrations_constants where id = 1";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        sf_clientid = rdr["sf_clientid"].ToString();
                        sf_callback_url = rdr["sf_callback_url"].ToString();
                        sf_consumer_key = rdr["sf_consumer_key"].ToString();
                        sf_consumer_secret = rdr["sf_consumer_secret"].ToString();
                        sf_token_req_end_point = rdr["sf_token_req_end_point"].ToString();
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void CreateNewIntegrationSettingForUser(string ObjectRef,int GroupId, string SFRefreshToken, string SFAccessToken, string SFApiVersion, string SFInstanceUrl)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sqlDel = "DELETE FROM integration_settings WHERE ObjectRef = '" + ObjectRef + "' AND GroupId =" + GroupId.ToString();
                MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                int rowsDeleted = cmd1.ExecuteNonQuery();

                string sql = "insert into integration_settings(ObjectRef, GroupId, SFRefreshToken,SFRTCreationDT,SFAccessToken, SFApiVersion, SFInstanceUrl, SFATCreationDT)";
                sql += " values ('" + ObjectRef + "',"+ GroupId + ", '" + SFRefreshToken + "', now(), '"+ SFAccessToken + "', '" + SFApiVersion + "', '" + SFInstanceUrl + "', now())";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();
                
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetCurrentRefreshToken(string objectRef, int GroupId,  ref string SFRefreshToken)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT SFRefreshToken FROM integration_settings where ObjectRef = '" + objectRef + "' AND GroupId = " + GroupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        SFRefreshToken = rdr["SFRefreshToken"].ToString();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void UpdateIntegrationSettingForUser(string ObjectRef, int GroupId,  string SFAccessToken, string SFApiVersion, string SFInstanceUrl)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "Update integration_settings Set SFAccessToken = ' " + SFAccessToken + "', SFApiVersion = ' " + SFApiVersion + "',SFInstanceUrl = ' " + SFInstanceUrl + "', SFATCreationDT=now()";
                sql += " WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = " + GroupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetAPICredentials(string ObjectRef, int GroupId,ref  string SFAccessToken,ref string SFApiVersion,ref string SFInstanceUrl)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integration_settings WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = " + GroupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        SFAccessToken = rdr["SFAccessToken"].ToString().Trim();
                        SFApiVersion = rdr["SFApiVersion"].ToString().Trim();
                        SFInstanceUrl = rdr["SFInstanceUrl"].ToString().Trim();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static HttpResponseMessage ConvertJSONPOutput(string callback, object message, HttpStatusCode code)
        {
            if(callback.Equals("internal"))
            {
                return ConvertStringOutput(message.ToString(), code); 
            }
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = code;
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer js = new JavaScriptSerializer();
            sb.Append(callback + "(");
            sb.Append(js.Serialize(message));
            sb.Append(");");
            response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage ConvertJSONOutput(object message, HttpStatusCode code)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = code;
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer js = new JavaScriptSerializer();
            sb.Append(js.Serialize(message));
            response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            return response;
        }
        public static HttpResponseMessage ConvertStringOutput(string message, HttpStatusCode code)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = code;
            response.Content = new StringContent(message, Encoding.UTF8, "application/json");
            return response;
        }

        public static void TagChat(string objectRef, int groupId, int sessionId, string objType, string objId)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sqlDel = "DELETE FROM integration_salesforce_queue WHERE ObjectRef = '" + objectRef + "' AND GroupId =" + groupId.ToString() + " AND  SessionId = " + sessionId + " AND status = 0";
                MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                int rowsDeleted = cmd1.ExecuteNonQuery();

                string sql = "insert into integration_salesforce_queue(objectRef, groupid, sessionid,object_type, object_id, timestamp)";
                sql += " values ('" + objectRef + "'," + groupId + ", " + sessionId + ", '" + objType + "', '" + objId + "', now())";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetTaggedChatId(string objectRef, int groupId, int sessionId,ref int id,  ref string itemId, ref string itemType)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integration_salesforce_queue WHERE objectref = '" + objectRef + "' AND groupid = " + groupId.ToString() + " AND sessionid = " + sessionId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        id = Convert.ToInt32(rdr["id"]); 
                        itemType = rdr["object_type"].ToString().Trim();
                        itemId = rdr["object_id"].ToString().Trim();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void ChatQueueItemAdded(int chatId)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "Update integration_salesforce_queue Set status =  1 ";
                sql += " WHERE id = " + chatId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetRedirectURLParametersDynamics(ref string dy_authoize_url, ref string dy_clientid, ref string dy_redirect_url, ref string dy_resource_url)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integrations_constants where id = 1";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        dy_authoize_url = rdr["dy_authorize_url"].ToString();
                        dy_clientid = rdr["dy_clientid"].ToString();
                        dy_redirect_url = rdr["dy_redirect_url"].ToString();
                        dy_resource_url = rdr["dy_resource_url"].ToString();
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void GetTokenParametersDynamics(ref string dy_clientid, ref string dy_redirect_url, ref string dy_resource_url, ref string dy_token_post_url)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integrations_constants where id = 1";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        dy_clientid = rdr["dy_clientid"].ToString();
                        dy_redirect_url = rdr["dy_redirect_url"].ToString();
                        dy_resource_url = rdr["dy_resource_url"].ToString();
                        dy_token_post_url = rdr["dy_token_post_url"].ToString();
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        internal static void CreateNewIntegrationSettingForDynamicsUser(string objectRef, int groupId, string refreshToken, string accessToken, string resource)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sqlDel = "DELETE FROM integration_settings_dynamics WHERE objectRef = '" + objectRef + "' AND groupId =" + groupId.ToString();
                MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                int rowsDeleted = cmd1.ExecuteNonQuery();

                string sql = "insert into integration_settings_dynamics(objectRef, groupId, DYRefreshToken, DYRTCreationDT, DYAccessToken, DYATCreationDT, resource)";
                sql += " values ('" + objectRef + "'," + groupId + ", '" + refreshToken.Trim() + "', now(), '" + accessToken.Trim() + "', now(),'" + resource+"')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        internal static void GetCurrentRefreshTokenDynamics(string objectRef, int groupId, ref string DYRefreshToken, ref string DYResourceURL)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT DYRefreshToken, resource FROM integration_settings_dynamics where objectRef = '" + objectRef + "' AND groupId = " + groupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        DYRefreshToken = rdr["DYRefreshToken"].ToString();
                        DYResourceURL = rdr["resource"].ToString();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        internal static void GetAPICredentialsDynamics(string objectRef, int groupId, ref string DYAccessToken, ref string DYApiVersion, ref string DYInstanceUrl, ref string resource)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integration_settings_dynamics WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        DYAccessToken = rdr["DYAccessToken"].ToString().Trim();
                        //DYApiVersion = rdr["DYApiVersion"].ToString().Trim();
                       // DYInstanceUrl = rdr["DYInstanceUrl"].ToString().Trim();
                        resource = rdr["resource"].ToString().Trim();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        internal static void TagChatDynamics(string objectRef, int groupId, int sessionId, string objType, string objId)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sqlDel = "DELETE FROM integration_dynamics_queue WHERE ObjectRef = '" + objectRef + "' AND GroupId =" + groupId.ToString() + " AND  SessionId = " + sessionId + " AND status = 0";
                MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                int rowsDeleted = cmd1.ExecuteNonQuery();

                string sql = "insert into integration_dynamics_queue(objectRef, groupid, sessionid,object_type, object_id, timestamp)";
                sql += " values ('" + objectRef + "'," + groupId + ", " + sessionId + ", '" + objType + "', '" + objId + "', now())";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }
        public static void GetTaggedChatDynamicsId(string objectRef, int groupId, int sessionId, ref int id, ref string itemId, ref string itemType)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integration_dynamics_queue WHERE status=1 AND objectref = '" + objectRef + "' AND groupid = " + groupId.ToString() + " AND sessionid = " + sessionId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        id = Convert.ToInt32(rdr["id"]);
                        itemType = rdr["object_type"].ToString().Trim();
                        itemId = rdr["object_id"].ToString().Trim();
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static DYTokenStatus GetAccessTokenDynamics(string objectRef, string groupId, ref string accessToken, ref string username, ref string userPassword, 
            ref string clientId, ref string serviceURL, ref DateTime tokenExpiryDT, ref string authority)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;Convert Zero Datetime=True;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "SELECT * FROM integration_settings_dynamics WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        username = rdr["username"].ToString().Trim();
                        userPassword = rdr["userpassword"].ToString().Trim();
                        clientId = rdr["clientid"].ToString().Trim();
                        serviceURL = rdr["serviceurl"].ToString().Trim();
                        authority = rdr["authority"].ToString().Trim();
                        string date = rdr["tokenexpirydt"].ToString(); 
                        // if no expiry date there
                        if (date.Equals(""))
                        {
                            return DYTokenStatus.TOKENEXPIRED;
                        }
                        tokenExpiryDT = Convert.ToDateTime(rdr["tokenexpirydt"].ToString()).AddHours(1);
                        if (tokenExpiryDT.AddMinutes(-5) > DateTime.Now)
                        {
                            accessToken = rdr["accesstoken"].ToString().Trim();
                            return DYTokenStatus.SUCCESSS;
                        }
                        else
                        {
                            return DYTokenStatus.TOKENEXPIRED; 
                        }
                    }
                    return DYTokenStatus.USERNOTFOUND;
                }
                else
                {
                    return DYTokenStatus.USERNOTFOUND;
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                return DYTokenStatus.USERNOTFOUND;
            }
            conn.Close();
        }

        public static void UpdateAccessTokenDynamics(string objectRef, string groupId, string accessToken, DateTime expiryDT)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "Update integration_settings_dynamics Set accesstoken = '" + accessToken + "', tokenexpirydt = now()";
                sql += " WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }

        public static void ChatQueueItemAddedDynamics(int chatId)
        {
            string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                string sql = "Update integration_dynamics_queue Set status =  1 ";
                sql += " WHERE id = " + chatId.ToString();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rows = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
            }
            conn.Close();
        }
        #endregion SalesForce Methods
    }

    public class MyValidation
    {
        public string ValidationKey { get; set; }
    }
    public class SecureInfo
    {
        public string AccessToken { get; set; }
        public string InstanceUrl { get; set; }
        public string ApiVersion { get; set; }
        public string ValidationKey { get; set; }
    }
    //public class LeadMessageData: SecureInfo
    //{
    //    public string LeadId { get; set; }
    //    public string Messsage { get; set; }
    //}
    
    
 
}