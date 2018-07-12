using MySql.Data.MySqlClient;
using SalesForceOAuth.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.BusinessLogic
{
    public static class DynamicCommon
    {
        public static List<InputFields> GetDynamicSearchFileds(string objectRef, int groupId, string entityName, string urlReferrer)
        {
            List<InputFields> returnFieldList = new List<InputFields>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    //string sql = "SELECT ints.id,ints.objectref,ints.groupid from integration_settings AS ints Left Outer Join integration_dynamics_custom_search AS iscs ON ints.objectref = iscs.objectref AND ints.groupid = iscs.groupid ";
                    //sql += " WHERE ints.ObjectRef = '" + objectRef + "' AND ints.GroupId = " + groupId.ToString();
                    string sql = "SELECT * from integration_dynamics_custom_search where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                if (rdr["entity_name"].ToString().ToLower().Equals(entityName.ToLower()))
                                {
                                    InputFields searchFields = new InputFields();
                                    searchFields.FieldLabel = rdr["search_field_label"].ToString().Trim();
                                    searchFields.FieldName = rdr["search_field_name"].ToString().Trim();
                                    returnFieldList.Add(searchFields);
                                }

                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                catch (MySqlException ex)
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

            return returnFieldList;
        }
    }
}