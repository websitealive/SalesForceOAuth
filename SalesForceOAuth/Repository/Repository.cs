using MySql.Data.MySqlClient;
using SalesForceOAuth.Controllers;
using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SalesForceOAuth
{
    public class Repository
    {
        public enum Crm
        {
            SalesForce,
            Dynamic
        };

        public static List<ExportFieldModel> GetDefaultFields(string entityName, Crm crmType)
        {
            List<ExportFieldModel> defaultColumn = new List<ExportFieldModel>();

            //Add Default Fields
            if (entityName.ToLower() == "lead")
            {
                if (crmType == Crm.Dynamic)
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "Subject", FieldName = "subject" , FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25 },
                        new ExportFieldModel { Sr = 1, FiledLabel = "First Name", FieldName = "firstname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Last Name", FieldName = "lastname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25 },
                        new ExportFieldModel { Sr = 3, FiledLabel = "Company", FieldName = "companyname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25 },
                        new ExportFieldModel { Sr = 4, FiledLabel = "Phone", FieldName = "telephone1", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25 },
                        new ExportFieldModel { Sr = 5, FiledLabel = "Email", FieldName = "emailaddress1", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25 }
                    });
                }
                else
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "FirstName", FieldName = "FirstName" , FieldType = "Default"  , BusinessRequired = 1 , MaxLength = 25 },
                        new ExportFieldModel { Sr = 1, FiledLabel = "Last Name", FieldName = "LastName", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Company", FieldName = "Company", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 3, FiledLabel = "Email", FieldName = "Email", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 4, FiledLabel = "Phone", FieldName = "Phone", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                    });
                }

            }
            if (entityName.ToLower() == "contact")
            {
                if (crmType == Crm.Dynamic)
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "First Name", FieldName = "firstname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 1, FiledLabel = "Last Name", FieldName = "lastname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Email", FieldName = "emailaddress1", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 3, FiledLabel = "Phone", FieldName = "telephone1", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  }
                    });
                }
                else
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "FirstName", FieldName = "FirstName" , FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 1, FiledLabel = "Last Name", FieldName = "LastName", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Last Name", FieldName = "lastname", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 3, FiledLabel = "Email", FieldName = "Email", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 4, FiledLabel = "Phone", FieldName = "Phone", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 5, FiledLabel = "AccountId", FieldName = "AccountId", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  }
                    });
                }
            }
            if (entityName.ToLower() == "account")
            {
                if (crmType == Crm.Dynamic)
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "Account Name", FieldName = "name", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 1, FiledLabel = "Account Number", FieldName = "accountnumber", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Phone", FieldName = "telephone1", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 3, FiledLabel = "Description", FieldName = "description", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  }
                    });
                }
                else
                {
                    defaultColumn.AddRange(
                    new ExportFieldModel[] {
                        new ExportFieldModel { Sr = 0, FiledLabel = "AccountNumber", FieldName = "AccountNumber" , FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 1, FiledLabel = "Account Name", FieldName = "Name", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                        new ExportFieldModel { Sr = 2, FiledLabel = "Phone", FieldName = "Phone", FieldType = "Default" , BusinessRequired = 1 , MaxLength = 25  },
                    });
                }

            }
            //End Add Default Fields

            return defaultColumn;
        }

        #region Dynamic Custom Fields



        public static List<ExportFieldModel> GetDYFormExportFields(string objectRef, int groupId, string urlReferrer, string entityName)
        {
            List<ExportFieldModel> returnFileds = new List<ExportFieldModel>();
            var counter = GetDefaultFields(entityName, Crm.Dynamic).Count;
            returnFileds.AddRange(GetDefaultFields(entityName, Crm.Dynamic));
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                ExportFieldModel exportFields = new ExportFieldModel();
                                exportFields.Sr = counter;
                                exportFields.FieldType = "Custom";
                                exportFields.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                exportFields.FieldName = rdr["fieldname"].ToString().Trim();
                                exportFields.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                exportFields.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
                                returnFileds.Add(exportFields);
                                counter = counter + 1;
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
            return returnFileds;
        }

        public static List<FieldsModel> GetDYExportFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                exportFields.FieldName = rdr["fieldname"].ToString().Trim();
                                exportFields.EntityType = rdr["entityname"].ToString().Trim();
                                exportFields.ValueType = rdr["valuetype"].ToString().Trim();
                                exportFields.ValueDetail = rdr["valuedetail"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddDYExportFields(FieldsModel ExportFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ExportFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    bool flag = false;
                    string integrationId = null;
                    string sqlFetchIntegration = "SELECT id FROM integration_dynamics_settings WHERE ObjectRef = '" + ExportFields.ObjectRef + "' AND GroupId = " + ExportFields.GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sqlFetchIntegration, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                integrationId = rdr["id"].ToString().Trim();
                                flag = true;
                            }
                        }
                        rdr.Close();
                    }

                    if (flag)
                    {
                        string sql = "INSERT INTO integration_dynamics_custom_fields (objectref, groupid, integration_id, fieldname, entityname, valuetype, valuedetail, inputfieldlabel, businessrequired, maxlength)";
                        sql += "VALUES ('" + ExportFields.ObjectRef + "'," + ExportFields.GroupId.ToString() + ",'" + integrationId + "','" + ExportFields.FieldName + "','" + ExportFields.EntityType + "','" + ExportFields.ValueType + "','" + ExportFields.ValueDetail + "','" + ExportFields.FiledLabel + "','" + ExportFields.BusinessRequired + "','" + ExportFields.MaxLength + "' )";
                        MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                        int rows = cmd1.ExecuteNonQuery();
                        conn.Close();
                        return "Export Fields Added Successfully";
                    }
                    else
                    {
                        conn.Close();
                        return "MS Dynamic Account In not Configured";
                    }

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDYExportFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamics_custom_fields WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMessage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static List<FieldsModel> GetDYSearchFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
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
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["search_field_label"].ToString().Trim();
                                exportFields.FieldName = rdr["search_field_name"].ToString().Trim();
                                exportFields.EntityType = rdr["entity_name"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddDYSearchFields(FieldsModel SearchFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, SearchFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_dynamics_custom_search (objectref, groupid, entity_name, search_field_name, search_field_label)";
                    sql += "VALUES ('" + SearchFields.ObjectRef + "'," + SearchFields.GroupId.ToString() + ",'" + SearchFields.EntityType + "','" + SearchFields.FieldName + "','" + SearchFields.FiledLabel + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Search Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDYSearchFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamics_custom_search WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMessage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static List<FieldsModel> GetDYDetailFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamic_detailedview_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["detail_field_label"].ToString().Trim();
                                exportFields.FieldName = rdr["detail_field_name"].ToString().Trim();
                                exportFields.EntityType = rdr["entity_name"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddDYDetailFields(FieldsModel DetailFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DetailFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_dynamic_detailedview_fields (objectref, groupid, entity_name, detail_field_name, detail_field_label)";
                    sql += "VALUES ('" + DetailFields.ObjectRef + "'," + DetailFields.GroupId.ToString() + ",'" + DetailFields.EntityType + "','" + DetailFields.FieldName + "','" + DetailFields.FiledLabel + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDYDetailFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {

            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamic_detailedview_fields WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMessage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        #endregion

        #region SaleForce Custom Fields

        public static List<ExportFieldModel> GetSFFormExportFields(string objectRef, int groupId, string urlReferrer, string entityName)
        {
            List<ExportFieldModel> returnFileds = new List<ExportFieldModel>();
            var counter = GetDefaultFields(entityName, Crm.SalesForce).Count;
            returnFileds.AddRange(GetDefaultFields(entityName, Crm.SalesForce));
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                ExportFieldModel exportFields = new ExportFieldModel();
                                exportFields.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                exportFields.FieldName = rdr["fieldname"].ToString().Trim();
                                exportFields.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                exportFields.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }


        public static List<FieldsModel> GetSFExportFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                exportFields.FieldName = rdr["fieldname"].ToString().Trim();
                                exportFields.EntityType = rdr["entity_name"].ToString().Trim();
                                exportFields.ValueType = rdr["valuetype"].ToString().Trim();
                                exportFields.ValueDetail = rdr["valuedetail"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddSFExportFields(FieldsModel ExportFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ExportFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    bool flag = false;
                    string integrationId = null;
                    string sqlFetchIntegration = "SELECT * FROM integration_settings WHERE objectref = '" + ExportFields.ObjectRef + "' AND groupid = " + ExportFields.GroupId;
                    MySqlCommand cmd = new MySqlCommand(sqlFetchIntegration, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                integrationId = rdr["id"].ToString().Trim();
                                flag = true;
                            }
                        }
                        rdr.Close();
                    }

                    if (flag)
                    {
                        string sql = "INSERT INTO integration_salesforce_custom_fields (objectref, groupid, integration_id, entity_name, fieldname, valuetype, valuedetail, inputfieldlabel, businessrequired, maxlength)";
                        sql += "VALUES ('" + ExportFields.ObjectRef + "'," + ExportFields.GroupId.ToString() + ",'" + integrationId + "','" + ExportFields.EntityType + "','" + ExportFields.FieldName + "','" + ExportFields.ValueType + "','" + ExportFields.ValueDetail + "','" + ExportFields.FiledLabel + "','" + ExportFields.BusinessRequired + "','" + ExportFields.MaxLength + "' )";
                        MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                        int rows = cmd1.ExecuteNonQuery();
                        conn.Close();
                        return "Export Fields Added Successfully";
                    }
                    else
                    {
                        conn.Close();
                        return "MS Dynamic Account In not Configured";
                    }

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSFExportFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMesssage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_custom_fields WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMesssage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static List<FieldsModel> GetSFSearchFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_custom_search where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["search_label"].ToString().Trim();
                                exportFields.FieldName = rdr["search_field_name"].ToString().Trim();
                                exportFields.EntityType = rdr["entity_name"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddSFSearchFields(FieldsModel SearchFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, SearchFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_salesforce_custom_search (objectref, groupid, entity_name, search_field_name, search_label)";
                    sql += "VALUES ('" + SearchFields.ObjectRef + "'," + SearchFields.GroupId.ToString() + ",'" + SearchFields.EntityType + "','" + SearchFields.FieldName + "','" + SearchFields.FiledLabel + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Search Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSFSearchFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_custom_search WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMessage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static List<FieldsModel> GetSFDetailFields(string objectRef, int groupId, string urlReferrer)
        {
            List<FieldsModel> returnFileds = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_detailedview_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel exportFields = new FieldsModel();
                                exportFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                exportFields.FiledLabel = rdr["label"].ToString().Trim();
                                exportFields.FieldName = rdr["sf_variable"].ToString().Trim();
                                exportFields.EntityType = rdr["entity_type"].ToString().Trim();

                                returnFileds.Add(exportFields);
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
            return returnFileds;
        }

        public static string AddSFDetailFields(FieldsModel DetailFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DetailFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_salesforce_detailedview_fields (objectref, groupid, entity_type, sf_variable, label)";
                    sql += "VALUES ('" + DetailFields.ObjectRef + "'," + DetailFields.GroupId.ToString() + ",'" + DetailFields.EntityType + "','" + DetailFields.FieldName + "','" + DetailFields.FiledLabel + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSFDetailFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_detailedview_fields WHERE id = " + Id;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    ErrorMessage = null;
                    return true;

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        #endregion
    }
}