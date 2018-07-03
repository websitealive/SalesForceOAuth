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
using Newtonsoft.Json.Linq;
using SalesForceOAuth.Web_API_Helper_Code;
using System.Dynamic;
using System.Collections.Generic;

namespace SalesForceOAuth.Controllers
{

    public class SalesForceController : ApiController
    {
        /// <summary>
        /// GET: api/SalesForce/GetRedirectURL
        [HttpGet]
        [ActionName("GetRedirectURL")]
        public HttpResponseMessage GetRedirectURL(string token, string callback, string siteRef, string objectRef)
        {
            try
            {
                string outputPayload;
                try
                {
                    outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
                }
                catch (Exception ex)
                {
                    return MyAppsDb.ConvertJSONPOutput(callback, ex, "Salesforce-GetRedirectURL", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
                }
                string sf_authoize_url = "", sf_clientid = "", sf_callback_url = "";
                if (siteRef.Contains("localhost"))
                {
                    sf_callback_url = "https://login.salesforce.com/apex/OauthSetup";
                    string urlReferrer = Request.RequestUri.Authority.ToString();
                    MyAppsDb.GetRedirectURLParameters(ref sf_authoize_url, ref sf_clientid, urlReferrer, objectRef);

                    string url = Common.FormatAuthUrl(sf_authoize_url, ResponseTypes.Code, sf_clientid, sf_callback_url, state: siteRef);
                    return MyAppsDb.ConvertJSONPOutput(callback, url, HttpStatusCode.OK, false);
                }
                else
                {
                    sf_callback_url = siteRef;
                    string urlReferrer = Request.RequestUri.Authority.ToString();
                    MyAppsDb.GetRedirectURLParameters(ref sf_authoize_url, ref sf_clientid, urlReferrer, objectRef);

                    string url = Common.FormatAuthUrl(sf_authoize_url, ResponseTypes.Code, sf_clientid, sf_callback_url);
                    return MyAppsDb.ConvertJSONPOutput(callback, url, HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SalesForce-GetRedirectURL", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }

    }


    public class MyAppsDb
    {
        #region SalesForce Methods

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static void GetRedirectURLParameters(ref string sf_authoize_url, ref string sf_clientid, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integrations_constants where id = 1";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                sf_authoize_url = rdr["sf_authoize_url"].ToString();
                                sf_clientid = rdr["sf_clientid"].ToString();
                                //sf_callback_url = rdr["sf_callback_url"].ToString();
                            }
                        }
                        else
                        {
                            Console.WriteLine("No rows found.");
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }

        }

        public static void UpdateIntegrationSettingForUserDynamics(string objectRef, int groupId, string refresh_token, string access_token, string resource, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_settings Set DYAccessToken = '" + access_token + "', SFATCreationDT=now(), resource='" + resource + "'";
                    sql += " WHERE objectRef = '" + objectRef + "' AND groupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetTokenParameters(ref string sf_clientid, ref string sf_consumer_key, ref string sf_consumer_secret, ref string sf_token_req_end_point, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integrations_constants where id = 1";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                sf_clientid = rdr["sf_clientid"].ToString();
                                // sf_callback_url = rdr["sf_callback_url"].ToString();
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void CreateNewIntegrationSettingForUser(string ObjectRef, int GroupId, string SFRefreshToken, string SFAccessToken, string SFApiVersion, string SFInstanceUrl, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_settings WHERE ObjectRef = '" + ObjectRef + "' AND GroupId =" + GroupId.ToString();
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();

                    string sql = "insert into integration_settings(ObjectRef, GroupId, SFRefreshToken,SFRTCreationDT,SFAccessToken, SFApiVersion, SFInstanceUrl, SFATCreationDT)";
                    sql += " values ('" + ObjectRef + "'," + GroupId + ", '" + SFRefreshToken + "', now(), '" + SFAccessToken + "', '" + SFApiVersion + "', '" + SFInstanceUrl + "', now())";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetCurrentRefreshToken(string objectRef, int GroupId, ref string SFRefreshToken, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT SFRefreshToken FROM integration_settings where ObjectRef = '" + objectRef + "' AND GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                SFRefreshToken = rdr["SFRefreshToken"].ToString();
                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void UpdateIntegrationSettingForUser(string ObjectRef, int GroupId, string SFAccessToken, string SFApiVersion, string SFInstanceUrl, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_settings Set SFAccessToken = ' " + SFAccessToken + "', SFApiVersion = ' " + SFApiVersion + "',SFInstanceUrl = ' " + SFInstanceUrl + "', SFATCreationDT=now()";
                    sql += " WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetAPICredentials(string ObjectRef, int GroupId, ref string SFAccessToken, ref string SFApiVersion, ref string SFInstanceUrl, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_settings WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }
        public static void AssignCustomVariableValue(dynamic lead, string label, string value, int itemno)
        {
            switch (itemno)
            {
                case 1:
                    {
                        lead.Custom1 = label + "|" + (value != null ? value : ""); break;
                    }
                case 2:
                    {
                        lead.Custom2 = label + "|" + (value != null ? value : ""); break;
                    }
                case 3:
                    {
                        lead.Custom3 = label + "|" + (value != null ? value : ""); break;
                    }
                case 4:
                    {
                        lead.Custom4 = label + "|" + (value != null ? value : ""); break;
                    }
                case 5:
                    {
                        lead.Custom5 = label + "|" + (value != null ? value : ""); break;
                    }
                case 6:
                    {
                        lead.Custom6 = label + "|" + (value != null ? value : ""); break;
                    }
                case 7:
                    {
                        lead.Custom7 = label + "|" + (value != null ? value : ""); break;
                    }
                case 8:
                    {
                        lead.Custom8 = label + "|" + (value != null ? value : ""); break;
                    }
                case 9:
                    {
                        lead.Custom9 = label + "|" + (value != null ? value : ""); break;
                    }
                case 10:
                    {
                        lead.Custom10 = label + "|" + (value != null ? value : ""); break;
                    }
                case 11:
                    {
                        lead.Custom11 = label + "|" + (value != null ? value : ""); break;
                    }
                case 12:
                    {
                        lead.Custom12 = label + "|" + (value != null ? value : ""); break;
                    }
                case 13:
                    {
                        lead.Custom13 = label + "|" + (value != null ? value : ""); break;
                    }
                case 14:
                    {
                        lead.Custom14 = label + "|" + (value != null ? value : ""); break;
                    }
                case 15:
                    {
                        lead.Custom15 = label + "|" + (value != null ? value : ""); break;
                    }
            }

        }
        public static void GetAPICredentialswithCustomViewFields(string ObjectRef, int GroupId, string entityType, ref string SFAccessToken, ref string SFApiVersion, ref string SFInstanceUrl, ref string customViewFields, ref string sLabelViewFields, ref string query, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    //string sql = "SELECT ints.id,ints.objectref,ints.groupid,ints.SFAccessToken,ints.SFApiVersion,ints.SFInstanceUrl,iscs.entity_type,iscs.label,iscs.sf_variable frOM integration_settings AS ints Left Outer Join integration_salesforce_detailedview_fields AS iscs ON ints.objectref = iscs.objectref AND ints.groupid = iscs.groupid ";

                    //string sql = "SELECT ints.id,ints.objectref,ints.groupid,ints.SFAccessToken,ints.SFApiVersion,ints.SFInstanceUrl,iscs.entity_name,iscs.search_label,iscs.search_field_name frOM integration_settings AS ints Left Outer Join integration_salesforce_custom_search AS iscs ON ints.objectref = iscs.objectref AND ints.groupid = iscs.groupid ";

                    string sql = "SELECT ints.id,ints.objectref,ints.groupid,ints.SFAccessToken,ints.SFApiVersion,ints.SFInstanceUrl,iscs.entity_name,iscs.search_label,iscs.search_field_name frOM integration_settings AS ints Left Outer Join integration_salesforce_custom_search AS iscs ON ints.objectref = iscs.objectref AND ints.groupid = iscs.groupid ";
                    sql += " WHERE ints.ObjectRef = '" + ObjectRef + "' AND ints.GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int row = 0; customViewFields = ""; sLabelViewFields = "";
                    query = "SELECT Id ";
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                SFAccessToken = rdr["SFAccessToken"].ToString().Trim();
                                SFApiVersion = rdr["SFApiVersion"].ToString().Trim();
                                SFInstanceUrl = rdr["SFInstanceUrl"].ToString().Trim();

                                //if (rdr["entity_type"].ToString().Equals(entityType))
                                if (rdr["entity_name"].ToString().ToLower().Equals(entityType.ToLower()))
                                {
                                    if (row == 0)
                                    {
                                        //customViewFields = rdr["sf_variable"].ToString().Trim();
                                        //sLabelViewFields = rdr["label"].ToString().Trim();
                                        //query += ", " + rdr["sf_variable"].ToString().Trim();
                                        customViewFields = rdr["search_field_name"].ToString().Trim();
                                        sLabelViewFields = rdr["search_label"].ToString().Trim();
                                        query += ", " + rdr["search_field_name"].ToString().Trim();
                                        row++;
                                    }
                                    else
                                    {
                                        //query += ", " + rdr["sf_variable"].ToString().Trim();
                                        //customViewFields += "|" + rdr["sf_variable"].ToString().Trim();
                                        //sLabelViewFields += "|" + rdr["label"].ToString().Trim();
                                        query += ", " + rdr["search_field_name"].ToString().Trim();
                                        customViewFields += "|" + rdr["search_field_name"].ToString().Trim();
                                        sLabelViewFields += "|" + rdr["search_label"].ToString().Trim();
                                    }
                                }

                            }
                        }
                        if (entityType.ToLower() == "lead")
                        {
                            query += "," + ConfigurationManager.AppSettings["SFLeadDefault"];
                        }
                        else if (entityType.ToLower() == "account")
                        {
                            query += "," + ConfigurationManager.AppSettings["SFAccountDefault"];
                        }
                        else if (entityType.ToLower() == "contact")
                        {
                            query += "," + ConfigurationManager.AppSettings["SFContactDefault"];
                        }

                        query += " from " + entityType;


                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (MySqlException exs)
                {
                    conn.Close();
                    throw;
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }


        public static void GetAPICredentialswithCustomSearchFields(string ObjectRef, int GroupId, string entityType, ref string SFAccessToken, ref string SFApiVersion, ref string SFInstanceUrl, ref string customSearchFields, ref string customSearchFieldsLabels, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT ints.id,ints.objectref,ints.groupid,ints.SFAccessToken,ints.SFApiVersion,ints.SFInstanceUrl,iscs.entity_name,iscs.search_field_name, iscs.search_label frOM integration_settings AS ints Left Outer Join integration_salesforce_custom_search AS iscs ON ints.objectref = iscs.objectref AND ints.groupid = iscs.groupid ";
                    sql += " WHERE ints.ObjectRef = '" + ObjectRef + "' AND ints.GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int row = 0; customSearchFields = ""; customSearchFieldsLabels = "";
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                SFAccessToken = rdr["SFAccessToken"].ToString().Trim();
                                SFApiVersion = rdr["SFApiVersion"].ToString().Trim();
                                SFInstanceUrl = rdr["SFInstanceUrl"].ToString().Trim();
                                if (rdr["entity_name"].ToString().Equals(entityType))
                                {
                                    if (row == 0)
                                    {
                                        customSearchFields = rdr["search_field_name"].ToString().Trim();
                                        customSearchFieldsLabels = rdr["search_label"].ToString().Trim();
                                        row++;
                                    }
                                    else
                                    {
                                        customSearchFields += "|" + rdr["search_field_name"].ToString().Trim();
                                        customSearchFieldsLabels += "|" + rdr["search_label"].ToString().Trim();
                                    }
                                }

                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (MySqlException exs)
                {
                    conn.Close();
                    throw;
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static int GetDynamicsCredentialswithCustomSearchFields(string objectRef, int groupId, string entityType, ref string applicationURL, ref string userName, ref string password, ref string authType, ref string customSearchFields, string referrerURL)
        {

            string connStr = MyAppsDb.GetConnectionStringbyURL(referrerURL, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT i.ApplicationURL, i.UserName, i.Password, i.AuthType, d.entity_name,d.search_field_name FROM integration_dynamics_settings i  Left Outer Join integration_dynamics_custom_search AS d ON i.objectref = d.objectref AND i.groupid = d.groupid ";
                    sql += " WHERE i.ObjectRef = '" + objectRef + "' AND i.GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int row = 0; customSearchFields = "";
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                applicationURL = rdr["ApplicationURL"].ToString().Trim();
                                userName = Encryption.Decrypt(rdr["UserName"].ToString().Trim());
                                password = Encryption.Decrypt(rdr["Password"].ToString().Trim());
                                authType = rdr["AuthType"].ToString().Trim();
                                if (rdr["entity_name"].ToString().Equals(entityType))
                                {
                                    if (row == 0)
                                    {
                                        customSearchFields = rdr["search_field_name"].ToString().Trim(); row++;
                                    }
                                    else
                                    {
                                        customSearchFields += "|" + rdr["search_field_name"].ToString().Trim();
                                    }
                                }

                            }
                            rdr.Close();
                            conn.Close();
                            return 1;
                        }
                        else
                        {
                            rdr.Close();
                            conn.Close();
                            return -1;
                        }

                    }
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }

        }

        public static HttpResponseMessage ConvertJSONPOutput(string callback, object message, HttpStatusCode code, bool logError)
        {
            //log exception if true
            if (logError)
            {
                LogError(message.ToString());
            }
            if (callback.Equals("internal"))
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

        public static HttpResponseMessage ConvertJSONOutput(object message, HttpStatusCode code, bool logError)
        {
            //log exception if true
            if (logError)
            {
                LogError(message.ToString());
            }
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

        public static void TagChat(string objectRef, int groupId, int sessionId, string objType, string objId, string urlReferrer, string OwnerEmail)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_queue WHERE ObjectRef = '" + objectRef + "' AND GroupId =" + groupId.ToString() + " AND  SessionId = " + sessionId + " AND status = 0";
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();

                    string sql = "insert into integration_salesforce_queue(objectRef, groupid, sessionid,object_type, object_id, timestamp,owner_email)";
                    sql += " values ('" + objectRef + "'," + groupId + ", " + sessionId + ", '" + objType + "', '" + objId + "', now(), '" + OwnerEmail + "')";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetTaggedChatId(string objectRef, int groupId, int sessionId, ref int id, ref string itemId, ref string itemType, ref string ownerEmail, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_salesforce_queue WHERE status = 0 AND  objectref = '" + objectRef + "' AND groupid = " + groupId.ToString() + " AND sessionid = " + sessionId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                id = Convert.ToInt32(rdr["id"]);
                                itemType = rdr["object_type"].ToString().Trim();
                                itemId = rdr["object_id"].ToString().Trim();
                                ownerEmail = rdr["owner_email"].ToString().Trim();
                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void ChatQueueItemAdded(int chatId, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_queue Set status =  1 ";
                    sql += " WHERE id = " + chatId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetRedirectURLParametersDynamics(ref string dy_authoize_url, ref string dy_clientid, ref string dy_redirect_url, ref string dy_resource_url, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integrations_constants where id = 1";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void GetTokenParametersDynamics(ref string dy_clientid, ref string dy_redirect_url, ref string dy_resource_url, ref string dy_token_post_url, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integrations_constants where id = 1";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        internal static void CreateNewIntegrationSettingForDynamicsUser(string objectRef, int groupId, string refreshToken, string accessToken, string resource, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamics_settings WHERE objectRef = '" + objectRef + "' AND groupId =" + groupId.ToString();
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();

                    string sql = "insert into integration_dynamics_settings(objectRef, groupId, DYRefreshToken, DYRTCreationDT, DYAccessToken, DYATCreationDT, resource)";
                    sql += " values ('" + objectRef + "'," + groupId + ", '" + refreshToken.Trim() + "', now(), '" + accessToken.Trim() + "', now(),'" + resource + "')";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        internal static void GetCurrentRefreshTokenDynamics(string objectRef, int groupId, ref string DYRefreshToken, ref string DYResourceURL, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT DYRefreshToken, resource FROM integration_dynamics_settings where objectRef = '" + objectRef + "' AND groupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        internal static void GetAPICredentialsDynamics(string objectRef, int groupId, ref string DYAccessToken, ref string DYApiVersion, ref string DYInstanceUrl, ref string resource, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamics_settings WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void TagChatDynamics(string objectRef, int groupId, int sessionId, string objType, string objId, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }


        }

        public static void GetTaggedChatDynamicsId(string objectRef, int groupId, int sessionId, ref int id, ref string itemId, ref string itemType, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamics_queue WHERE status<>1 AND objectref = '" + objectRef + "' AND groupid = " + groupId.ToString() + " AND sessionid = " + sessionId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static CRMTokenStatus GetAccessTokenDynamics(string objectRef, string groupId, ref string accessToken, ref string username, ref string userPassword,
            ref string clientId, ref string serviceURL, ref DateTime tokenExpiryDT, ref string authority, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamics_settings WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
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
                                    return CRMTokenStatus.TOKENEXPIRED;
                                }
                                tokenExpiryDT = Convert.ToDateTime(rdr["tokenexpirydt"].ToString()).AddHours(1);
                                if (tokenExpiryDT.AddMinutes(-5) > DateTime.Now)
                                {
                                    accessToken = rdr["accesstoken"].ToString().Trim();
                                    return CRMTokenStatus.SUCCESSS;
                                }
                                else
                                {
                                    return CRMTokenStatus.TOKENEXPIRED;
                                }
                            }
                            return CRMTokenStatus.USERNOTFOUND;
                        }
                        else
                        {
                            return CRMTokenStatus.USERNOTFOUND;
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }
        public static string GetConnectionStringbyURL(string url, string objectRef)
        {
            string connectionString = "";
            if (url.Contains("api-apps-dotnet-dev0.websitealive.com") || url.Contains("localhost") || url.Contains("worker-dev0.websitealive.com"))
            {
                connectionString = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;Convert Zero Datetime=True;";

                //connectionString = Environment.GetEnvironmentVariable("devappsConnStr");
            }
            else if (url.Contains("api-apps-dotnet-stage.websitealive.com"))
            {
                //connectionString = Environment.GetEnvironmentVariable("stageappsConnStr");

                connectionString = "server=dbmain.alivechat.websitealive.com;user=apps;database=apps;port=3306;password=wXa8823v123!;";
                // connectionString = connectionString.Replace("alivechat", "alivechat_" + objectRef);
            }
            else if (url.Contains("api-apps-dotnet.websitealive.com"))
            {
                connectionString = "server=dbmain.alivechat.websitealive.com;user=apps;database=apps;port=3306;password=wXa8823v123!;";
                //connectionString = Environment.GetEnvironmentVariable("liveappsConnStr");
                connectionString = connectionString.Replace("alivechat", "alivechat_" + objectRef);
            }
            else
                connectionString = "";
            return connectionString;
        }
        public static int GetDynamicsCredentials(string objectRef, int groupId, ref string applicationURL, ref string userName, ref string password, ref string authType, string referrerURL)
        {

            string connStr = MyAppsDb.GetConnectionStringbyURL(referrerURL, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamics_settings WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                applicationURL = rdr["ApplicationURL"].ToString().Trim();
                                userName = Encryption.Decrypt(rdr["UserName"].ToString().Trim());
                                password = Encryption.Decrypt(rdr["Password"].ToString().Trim());
                                authType = rdr["AuthType"].ToString().Trim();
                                return 1;
                            }
                            rdr.Close();
                            conn.Close();
                            return 0;
                        }
                        else
                        {
                            rdr.Close();
                            conn.Close();
                            return -1;
                        }

                    }
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }

        }

        public static int RecordDynamicsCredentials(string objectRef, int groupId, string organizationURL, string userName, string password, string authType, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamics_settings WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.HasRows)
                        {
                            rdr.Close();
                            string insertSql = "INSERT INTO integration_dynamics_settings (objectref,groupid,username, password, applicationurl, AuthType)" +
                                "VALUES ('" + objectRef + "'," + groupId.ToString() + ",'" + Encryption.Encrypt(userName) + "','" + Encryption.Encrypt(password) + "','" + organizationURL + "','" + authType + "')";
                            MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn);
                            int rows = cmdInsert.ExecuteNonQuery();
                            rdr.Close();
                            conn.Close();
                            return rows;
                        }
                        else
                        {
                            rdr.Close();
                            conn.Close();
                            return 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void UpdateAccessTokenDynamics(string objectRef, string groupId, string accessToken, DateTime expiryDT, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_settings Set accesstoken = '" + accessToken + "', tokenexpirydt = now()";
                    sql += " WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void ChatQueueItemAddedDynamics(int chatId, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_queue Set status =  1 ";
                    sql += " WHERE id = " + chatId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static CRMTokenStatus GetAccessTokenSalesForce(string objectRef, int groupId, ref string accessToken, string urlReferrer)
        {
            //string connStr = "server=dev-rds.cnhwwuo7wmxs.us-west-2.rds.amazonaws.com;user=root;database=apps;port=3306;password=a2387ass;Convert Zero Datetime=True;";
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT  * FROM integration_settings WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                string date = rdr["SFATCreationDT"].ToString();
                                // if no expiry date there
                                if (date.Equals(""))
                                {
                                    rdr.Close(); conn.Close();
                                    return CRMTokenStatus.TOKENEXPIRED;
                                }
                                DateTime tokenExpiryDT = Convert.ToDateTime(rdr["SFATCreationDT"].ToString()).AddHours(1); // add 55 mins
                                if (tokenExpiryDT.AddMinutes(-5) > DateTime.Now)
                                {
                                    accessToken = rdr["SFAccessToken"].ToString().Trim();
                                    rdr.Close(); conn.Close();
                                    return CRMTokenStatus.SUCCESSS;
                                }
                                else
                                {
                                    rdr.Close(); conn.Close();
                                    return CRMTokenStatus.TOKENEXPIRED;
                                }
                            }

                            return CRMTokenStatus.USERNOTFOUND;
                        }
                        else
                        {
                            rdr.Close(); conn.Close();
                            return CRMTokenStatus.USERNOTFOUND;
                        }
                    }

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }

        }

        public static void GetRedirectURLParametersCallBack(ref string sf_callback_url, int id, string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_salesforce_sites where id = " + id.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                sf_callback_url = rdr["sf_callback_url"].ToString();
                            }
                        }
                        else
                        {
                            Console.WriteLine("No rows found.");
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static void LogException(string function, string errorTitle, Exception ex)
        {
            try
            {
                CreateLogFiles log = new CreateLogFiles();
                string InnerException = (ex.InnerException == null ? "" : ex.InnerException.ToString());
                string Message = (ex.Message.ToString() == null ? "" : ex.Message.ToString());
                log.ErrorLog("Function: " + function + " : " + errorTitle + "--Internal exception : " + InnerException + ", Exception Message: " + Message);
            }
            catch (Exception ex1)
            { }
        }

        public static void LogError(string message)
        {
            try
            {
                CreateLogFiles log = new CreateLogFiles();
                log.ErrorLog(message);
            }
            catch (Exception ex1)
            { }

        }

        public static HttpResponseMessage ConvertJSONPOutput(string callback, Exception ex, string function, string errorTitle, HttpStatusCode code)
        {
            //log exception
            CreateLogFiles log = new CreateLogFiles();
            string InnerException = (ex.InnerException == null ? "" : ex.InnerException.ToString());
            string Message = (ex.Message.ToString() == null ? "" : ex.Message.ToString());
            log.ErrorLog("Function: " + function + " : " + errorTitle + "--Internal exception : " + InnerException + ", Exception Message: " + Message);
            //output message
            string outMsg = errorTitle + "--Internal exception : " + InnerException + ", Exception Message: " + Message;
            if (callback.Equals("internal"))
            {
                return ConvertStringOutput(outMsg.ToString(), code);
            }
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = code;
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer js = new JavaScriptSerializer();
            sb.Append(callback + "(");
            sb.Append(js.Serialize(outMsg));
            sb.Append(");");
            response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage ConvertJSONOutput(Exception ex, string function, string errorTitle, HttpStatusCode code)
        {
            //log exception
            CreateLogFiles log = new CreateLogFiles();
            string InnerException = (ex.InnerException == null ? "" : ex.InnerException.ToString());
            string Message = (ex.Message.ToString() == null ? "" : ex.Message.ToString());
            log.ErrorLog("Function: " + function + " : " + errorTitle + "--Internal exception : " + InnerException + ", Exception Message: " + Message);
            //output message
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = code;
            StringBuilder sb = new StringBuilder();
            JavaScriptSerializer js = new JavaScriptSerializer();
            string outMsg = errorTitle + "--Internal exception : " + InnerException + ", Exception Message: " + Message;
            sb.Append(js.Serialize(outMsg));
            response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            return response;
        }


        #endregion SalesForce Methods
    }

    public class Contact
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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