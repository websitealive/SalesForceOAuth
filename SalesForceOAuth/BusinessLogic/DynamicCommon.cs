﻿using MySql.Data.MySqlClient;
using SalesForceOAuth.Controllers;
using SalesForceOAuth.ModelClasses;
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

        public static List<EntityColumn> GetDynamicDetailFileds(string objectRef, int groupId, string entityName, string urlReferrer)
        {
            List<EntityColumn> returnFileds = new List<EntityColumn>();
            var counter = GetDefaultFields(entityName).Count;
            returnFileds.AddRange(GetDefaultFields(entityName));

            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamic_detailedview_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' AND entity_name = '" + entityName + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                EntityColumn detailFields = new EntityColumn();
                                detailFields.Sr = counter;
                                detailFields.FieldLabel = rdr["detail_field_label"].ToString().Trim();
                                detailFields.FieldName = rdr["detail_field_name"].ToString().Trim();
                                returnFileds.Add(detailFields);
                                counter = counter + 1;
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

            return returnFileds;
        }


        public static List<EntityColumn> GetDefaultFields(string entityName)
        {
            List<EntityColumn> defaultColumn = new List<EntityColumn>();

            //Add Default Fields
            if (entityName.ToLower() == "lead")
            {
                defaultColumn.AddRange(
                    new EntityColumn[] {
                        new EntityColumn { Sr = 0, FieldLabel = "Subject", FieldName = "subject" },
                        new EntityColumn { Sr = 1, FieldLabel = "First Name", FieldName = "firstname" },
                        new EntityColumn { Sr = 2, FieldLabel = "Last Name", FieldName = "lastname" },
                        new EntityColumn { Sr = 3, FieldLabel = "Company", FieldName = "companyname" },
                        new EntityColumn { Sr = 4, FieldLabel = "Phone", FieldName = "telephone1" },
                        new EntityColumn { Sr = 5, FieldLabel = "Email", FieldName = "emailaddress1" }
                    }
                );
            }
            if (entityName.ToLower() == "contact")
            {

                defaultColumn.AddRange(
                    new EntityColumn[] {
                        new EntityColumn { Sr = 0, FieldLabel = "First Name", FieldName = "firstname" },
                        new EntityColumn { Sr = 1, FieldLabel = "Last Name", FieldName = "lastname" },
                        new EntityColumn { Sr = 2, FieldLabel = "Email", FieldName = "emailaddress1" },
                        new EntityColumn { Sr = 3, FieldLabel = "Phone", FieldName = "telephone1" }
                    }
                );
            }
            if (entityName.ToLower() == "account")
            {
                defaultColumn.AddRange(
                    new EntityColumn[] {
                        new EntityColumn { Sr = 0, FieldLabel = "First Name", FieldName = "name" },
                        new EntityColumn { Sr = 1, FieldLabel = "Last Name", FieldName = "accountnumber" },
                        new EntityColumn { Sr = 2, FieldLabel = "Phone", FieldName = "telephone1" },
                        new EntityColumn { Sr = 3, FieldLabel = "Email", FieldName = "description" }
                    }
                );
            }
            //End Add Default Fields

            return defaultColumn;
        }
    }
}