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

        public static List<ExportFields> GetDYFormExportFields(string objectRef, int groupId, string urlReferrer)
        {
            List<ExportFields> returnFileds = new List<ExportFields>();
            //var counter = GetDefaultFields(entityName, Crm.Dynamic).Count;
            //returnFileds.AddRange(GetDefaultFields(entityName, Crm.Dynamic));
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string fieldType = "user_input_field";
                    string sql = "SELECT * from integration_dynamics_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' AND valuetype = '" + fieldType + "'  ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            ExportFields contactExportFields = new ExportFields();
                            contactExportFields.Entity = "Contact";
                            ExportFields leadExportFields = new ExportFields();
                            leadExportFields.Entity = "Lead";
                            ExportFields accountExportFields = new ExportFields();
                            accountExportFields.Entity = "Account";

                            List<ExportFieldModel> contactFieldsList = new List<ExportFieldModel>();
                            List<ExportFieldModel> leadFieldsList = new List<ExportFieldModel>();
                            List<ExportFieldModel> accountFieldsList = new List<ExportFieldModel>();

                            while (rdr.Read())
                            {
                                if (rdr["entityname"].ToString().Trim().ToLower() == "contact")
                                {
                                    ExportFieldModel contactExportFieldsList = new ExportFieldModel();
                                    // contactExportFieldsList.FieldType = "Custom";
                                    contactExportFieldsList.Id = Convert.ToInt32(rdr["id"].ToString().Trim());
                                    contactExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    contactExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    contactExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    contactExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());

                                    contactExportFieldsList.FieldType = rdr["fieldtype"].ToString().Trim();
                                    contactExportFieldsList.RelatedEntity = rdr["relatedentity"].ToString().Trim();
                                    contactFieldsList.Add(contactExportFieldsList);
                                }
                                if (rdr["entityname"].ToString().Trim().ToLower() == "lead")
                                {
                                    ExportFieldModel leadExportFieldsList = new ExportFieldModel();
                                    // leadExportFieldsList.FieldType = "Custom";
                                    leadExportFieldsList.Id = Convert.ToInt32(rdr["id"].ToString().Trim());
                                    leadExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    leadExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    leadExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    leadExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());

                                    leadExportFieldsList.FieldType = rdr["fieldtype"].ToString().Trim();
                                    leadExportFieldsList.RelatedEntity = rdr["relatedentity"].ToString().Trim();
                                    leadFieldsList.Add(leadExportFieldsList);
                                }
                                if (rdr["entityname"].ToString().Trim().ToLower() == "account")
                                {
                                    ExportFieldModel accountExportFieldsList = new ExportFieldModel();
                                    // accountExportFieldsList.FieldType = "Custom";
                                    accountExportFieldsList.Id = Convert.ToInt32(rdr["id"].ToString().Trim());
                                    accountExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    accountExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    accountExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    accountExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());

                                    accountExportFieldsList.FieldType = rdr["fieldtype"].ToString().Trim();
                                    accountExportFieldsList.RelatedEntity = rdr["relatedentity"].ToString().Trim();
                                    accountFieldsList.Add(accountExportFieldsList);
                                }

                            }
                            contactExportFields.ExportFieldsList = contactFieldsList;
                            returnFileds.Add(contactExportFields);
                            leadExportFields.ExportFieldsList = leadFieldsList;
                            returnFileds.Add(leadExportFields);
                            accountExportFields.ExportFieldsList = accountFieldsList;
                            returnFileds.Add(accountExportFields);
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

        public static FieldsModel GetDYExportFieldsById(int ExportFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_custom_fields where id = '" + ExportFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["inputfieldlabel"].ToString().Trim();
                                returnFileds.FieldName = rdr["fieldname"].ToString().Trim();
                                returnFileds.FieldType = rdr["fieldtype"].ToString().Trim();
                                returnFileds.EntityType = rdr["entityname"].ToString().Trim();
                                returnFileds.ValueType = rdr["valuetype"].ToString().Trim();
                                returnFileds.ValueDetail = rdr["valuedetail"].ToString().Trim();
                                returnFileds.RelatedEntity = rdr["relatedentity"].ToString().Trim();
                                returnFileds.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                returnFileds.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
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
                                exportFields.FieldLabel = rdr["inputfieldlabel"].ToString().Trim();
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

        public static List<InputFields> GetConstantInputFields(string objectRef, int groupId, string urlReferrer, EntityName entity)
        {
            List<InputFields> returnFileds = new List<InputFields>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string fieldType = "user_constant";
                    string sql = "SELECT * from integration_dynamics_custom_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' AND valuetype = '" + fieldType + "'   ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                if (rdr["entityname"].ToString().Trim().ToLower() == entity.ToString().ToLower())
                                {
                                    InputFields customInputFields = new InputFields();
                                    customInputFields.FieldName = rdr["fieldname"].ToString().Trim();
                                    customInputFields.Value = rdr["valuedetail"].ToString().Trim();
                                    returnFileds.Add(customInputFields);
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
                        string sql = "INSERT INTO integration_dynamics_custom_fields (objectref, groupid, integration_id, fieldname, entityname, valuetype, valuedetail, inputfieldlabel, businessrequired, maxlength, fieldtype, relatedentity)";
                        sql += "VALUES ('" + ExportFields.ObjectRef + "'," + ExportFields.GroupId.ToString() + ",'" + integrationId + "','" + ExportFields.FieldName + "','" + ExportFields.EntityType + "','" + ExportFields.ValueType + "','" + ExportFields.ValueDetail + "','" + ExportFields.FieldLabel + "','" + ExportFields.BusinessRequired + "','" + ExportFields.MaxLength + "','" + ExportFields.FieldType + "','" + ExportFields.RelatedEntity + "' )";
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

        public static string UpdateDyExportFields(FieldsModel ExportFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ExportFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_custom_fields Set fieldname = '" + ExportFields.FieldName + "', entityname = '" + ExportFields.EntityType + "', valuetype = '" + ExportFields.ValueType + "', valuedetail = '" + ExportFields.ValueDetail + "', inputfieldlabel = '" + ExportFields.FieldLabel + "', businessrequired = '" + ExportFields.BusinessRequired + "', maxlength = '" + ExportFields.MaxLength + "', fieldtype = '" + ExportFields.FieldType + "', relatedentity = '" + ExportFields.RelatedEntity + "'";
                    sql += " WHERE id = " + ExportFields.ID;
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                    return "Export Fields Updated Successfully";
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
                                FieldsModel searchFields = new FieldsModel();
                                searchFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                searchFields.FieldLabel = rdr["search_field_label"].ToString().Trim();
                                searchFields.FieldName = rdr["search_field_name"].ToString().Trim();
                                searchFields.EntityType = rdr["entity_name"].ToString().Trim();

                                returnFileds.Add(searchFields);
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

        public static FieldsModel GetDYSearchFieldsById(int SearchFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_custom_search where id = '" + SearchFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["search_field_label"].ToString().Trim();
                                returnFileds.FieldName = rdr["search_field_name"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity_name"].ToString().Trim();
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
                    string sql = "INSERT INTO integration_dynamics_custom_search (objectref, groupid, entity_name, search_field_name, search_field_label, search_field_type, related_entity_name, related_entity_field_name)";
                    sql += "VALUES ('" + SearchFields.ObjectRef + "'," + SearchFields.GroupId.ToString() + ",'" + SearchFields.EntityType + "','" + SearchFields.FieldName + "','" + SearchFields.FieldLabel + "','" + SearchFields.ValueType + "','" + SearchFields.RelatedEntity + "','" + SearchFields.RelatedField + "' )";
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

        public static string UpdateDySearchFields(FieldsModel SearchFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, SearchFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_custom_search Set entity_name = '" + SearchFields.EntityType + "', search_field_name = '" + SearchFields.FieldName + "', search_field_label = '" + SearchFields.FieldLabel + "'";
                    sql += " WHERE id = " + SearchFields.ID;
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                    return "Search Fields Updated Successfully";
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
                                FieldsModel detailFields = new FieldsModel();
                                detailFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                detailFields.FieldLabel = rdr["detail_field_label"].ToString().Trim();
                                detailFields.FieldName = rdr["detail_field_name"].ToString().Trim();
                                detailFields.EntityType = rdr["entity_name"].ToString().Trim();

                                returnFileds.Add(detailFields);
                            }
                            rdr.Close();
                        }
                        conn.Close();
                    }
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

        public static FieldsModel GetDYDetailFieldsById(int DetailFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamic_detailedview_fields where id = '" + DetailFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["detail_field_label"].ToString().Trim();
                                returnFileds.FieldName = rdr["detail_field_name"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity_name"].ToString().Trim();
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
                    sql += "VALUES ('" + DetailFields.ObjectRef + "'," + DetailFields.GroupId.ToString() + ",'" + DetailFields.EntityType + "','" + DetailFields.FieldName + "','" + DetailFields.FieldLabel + "' )";
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

        public static string UpdateDyDetailFields(FieldsModel DetailFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DetailFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamic_detailedview_fields Set entity_name = '" + DetailFields.EntityType + "', detail_field_name = '" + DetailFields.FieldName + "', detail_field_label = '" + DetailFields.FieldLabel + "'";
                    sql += " WHERE id = " + DetailFields.ID;
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Updated Successfully";
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

        public static List<FieldsModel> GetDYBackEndFields(string objectRef, int groupId, string urlReferrer, string entity = null)
        {
            List<FieldsModel> returnFields = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql;
                    if (entity == null)
                    {
                        sql = "SELECT * from integration_dynamics_backend_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    }
                    else
                    {
                        sql = "SELECT * from integration_dynamics_backend_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' AND entity = '" + entity + "' ";
                    }
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel backendFields = new FieldsModel();
                                backendFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                backendFields.FieldName = rdr["backend_field_name"].ToString().Trim();
                                backendFields.ValueDetail = rdr["backend_field_value"].ToString().Trim();
                                backendFields.EntityType = rdr["entity"].ToString().Trim();

                                returnFields.Add(backendFields);
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
            return returnFields;
        }

        public static FieldsModel GetDYBackEndFieldsById(int BackEndFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_backend_fields where id = '" + BackEndFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldName = rdr["backend_field_name"].ToString().Trim();
                                returnFileds.ValueDetail = rdr["backend_field_value"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity"].ToString().Trim();
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

        public static string AddDYBackEndFields(FieldsModel BackEndFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, BackEndFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_dynamics_backend_fields (objectref, groupid, entity, backend_field_name, backend_field_value)";
                    sql += "VALUES ('" + BackEndFields.ObjectRef + "'," + BackEndFields.GroupId.ToString() + ",'" + BackEndFields.EntityType + "','" + BackEndFields.FieldName + "','" + BackEndFields.ValueDetail + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Back End Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateDYBackEndFields(FieldsModel BackEndFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, BackEndFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_backend_fields Set backend_field_name = '" + BackEndFields.FieldName + "', backend_field_value = '" + BackEndFields.ValueDetail + "', entity = '" + BackEndFields.EntityType + "'";
                    sql += " WHERE id = " + BackEndFields.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDYBackEndFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamics_backend_fields WHERE id = " + Id;
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

        public static DefaultFieldSettings GetDYDefaultFieldSettings(string ObjectRef, int GroupId, string UrlReferrer)
        {
            DefaultFieldSettings defaultField = new DefaultFieldSettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_default_fields WHERE objectref = '" + ObjectRef + "' AND groupid = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                defaultField.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                defaultField.IsAccountPhoneRequired = Convert.ToInt32(rdr["is_visible_account_phone"].ToString().Trim());
                                defaultField.IsAccountDescriptionRequired = Convert.ToInt32(rdr["is_visible_account_description"].ToString().Trim());
                                defaultField.IsContactEmailRequired = Convert.ToInt32(rdr["is_visible_contact_email"].ToString().Trim());
                                defaultField.IsContactPhoneRequired = Convert.ToInt32(rdr["is_visible_contact_phone"].ToString().Trim());
                                defaultField.IsLeadEmailRequired = Convert.ToInt32(rdr["is_visible_lead_email"].ToString().Trim());
                                defaultField.IsLeadPhoneRequired = Convert.ToInt32(rdr["is_visible_lead_phone"].ToString().Trim());
                                defaultField.IsLeadCompanyRequired = Convert.ToInt32(rdr["is_visible_lead_company"].ToString().Trim());
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
            return defaultField;
        }

        public static DefaultFieldSettings GetDYDefaultFieldSettingsById(string ObjectRef, int Id, string UrlReferrer)
        {
            DefaultFieldSettings defaultField = new DefaultFieldSettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamics_default_fields WHERE id = '" + Id + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                defaultField.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                defaultField.IsAccountPhoneRequired = Convert.ToInt32(rdr["is_visible_account_phone"].ToString().Trim());
                                defaultField.IsAccountDescriptionRequired = Convert.ToInt32(rdr["is_visible_account_description"].ToString().Trim());
                                defaultField.IsContactEmailRequired = Convert.ToInt32(rdr["is_visible_contact_email"].ToString().Trim());
                                defaultField.IsContactPhoneRequired = Convert.ToInt32(rdr["is_visible_contact_phone"].ToString().Trim());
                                defaultField.IsLeadEmailRequired = Convert.ToInt32(rdr["is_visible_lead_email"].ToString().Trim());
                                defaultField.IsLeadPhoneRequired = Convert.ToInt32(rdr["is_visible_lead_phone"].ToString().Trim());
                                defaultField.IsLeadCompanyRequired = Convert.ToInt32(rdr["is_visible_lead_company"].ToString().Trim());
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
            return defaultField;
        }

        public static string AddDYDefaultFieldSettings(DefaultFieldSettings DefaultFieldSettings, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DefaultFieldSettings.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_dynamics_default_fields (objectref, groupid, is_visible_account_phone, is_visible_account_description, is_visible_contact_email, is_visible_contact_phone, is_visible_lead_email, is_visible_lead_phone, is_visible_lead_company)";
                    sql += "VALUES ('" + DefaultFieldSettings.ObjectRef + "','" + DefaultFieldSettings.GroupId + "','" + DefaultFieldSettings.IsAccountPhoneRequired + "','" + DefaultFieldSettings.IsAccountDescriptionRequired + "','" + DefaultFieldSettings.IsContactEmailRequired + "','" + DefaultFieldSettings.IsContactPhoneRequired + "','" + DefaultFieldSettings.IsLeadEmailRequired + "','" + DefaultFieldSettings.IsLeadPhoneRequired + "','" + DefaultFieldSettings.IsLeadCompanyRequired + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entiity Default Field Settings Added Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateDYDefaultFieldSettings(DefaultFieldSettings DefaultFieldSettings, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DefaultFieldSettings.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamics_default_fields Set is_visible_account_phone = '" + DefaultFieldSettings.IsAccountPhoneRequired + "', is_visible_account_description = '" + DefaultFieldSettings.IsAccountDescriptionRequired + "', is_visible_contact_email = '" + DefaultFieldSettings.IsContactEmailRequired + "', is_visible_contact_phone = '" + DefaultFieldSettings.IsContactPhoneRequired + "', is_visible_lead_email = '" + DefaultFieldSettings.IsLeadEmailRequired + "', is_visible_lead_phone = '" + DefaultFieldSettings.IsLeadPhoneRequired + "', is_visible_lead_company = '" + DefaultFieldSettings.IsLeadCompanyRequired + "'";
                    sql += " WHERE id = " + DefaultFieldSettings.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entity Settings Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDYDefaultFieldSettings(string ObjectRef, string UrlReferrer, int RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamics_default_fields  WHERE id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static EntitySettings GetDyEntitySettings(string ObjectRef, int GroupId, string UrlReferrer)
        {
            EntitySettings returnEntitySettings = new EntitySettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamic_entity WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnEntitySettings.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                returnEntitySettings.IsAccountRequired = Convert.ToInt32(rdr["accounts_required"].ToString().Trim());
                                returnEntitySettings.IsContactRequired = Convert.ToInt32(rdr["contacts_required"].ToString().Trim());
                                returnEntitySettings.IsLeadRequired = Convert.ToInt32(rdr["leads_required"].ToString().Trim());
                                returnEntitySettings.AllowAccountCreation = Convert.ToInt32(rdr["allow_accounts_creation"].ToString().Trim());
                                returnEntitySettings.AllowContactCreation = Convert.ToInt32(rdr["allow_contacts_creation"].ToString().Trim());
                                returnEntitySettings.AllowLeadCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
                                returnEntitySettings.UseAliveChat = Convert.ToInt32(rdr["use_alive_chat"].ToString().Trim());
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
            return returnEntitySettings;
        }

        public static EntitySettings GetDyEntitySettingsById(string ObjectRef, int Id, string UrlReferrer)
        {
            EntitySettings returnEntitySettings = new EntitySettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_dynamic_entity WHERE id = '" + Id + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnEntitySettings.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                returnEntitySettings.IsAccountRequired = Convert.ToInt32(rdr["accounts_required"].ToString().Trim());
                                returnEntitySettings.IsContactRequired = Convert.ToInt32(rdr["contacts_required"].ToString().Trim());
                                returnEntitySettings.IsLeadRequired = Convert.ToInt32(rdr["leads_required"].ToString().Trim());

                                returnEntitySettings.AllowAccountCreation = Convert.ToInt32(rdr["allow_accounts_creation"].ToString().Trim());
                                returnEntitySettings.AllowContactCreation = Convert.ToInt32(rdr["allow_contacts_creation"].ToString().Trim());
                                returnEntitySettings.AllowLeadCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
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
            return returnEntitySettings;
        }

        public static string AddDyEntitySettings(EntitySettings EntitySettingsName, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntitySettingsName.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_dynamic_entity (objectref, groupid, leads_required, contacts_required, accounts_required, allow_leads_creation, allow_contacts_creation, allow_accounts_creation)";
                    sql += "VALUES ('" + EntitySettingsName.ObjectRef + "','" + EntitySettingsName.GroupId + "','" + EntitySettingsName.IsLeadRequired + "','" + EntitySettingsName.IsContactRequired + "','" + EntitySettingsName.IsAccountRequired + "','" + EntitySettingsName.AllowLeadCreation + "','" + EntitySettingsName.AllowContactCreation + "','" + EntitySettingsName.AllowAccountCreation + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entiity Settings Added Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateDyEntitySettings(EntitySettings EntitySettingsName, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntitySettingsName.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_dynamic_entity Set leads_required = '" + EntitySettingsName.IsLeadRequired + "', contacts_required = '" + EntitySettingsName.IsContactRequired + "', accounts_required = '" + EntitySettingsName.IsAccountRequired + "', allow_leads_creation = '" + EntitySettingsName.AllowLeadCreation + "', allow_contacts_creation = '" + EntitySettingsName.AllowContactCreation + "', allow_accounts_creation = '" + EntitySettingsName.AllowAccountCreation + "'";
                    sql += " WHERE id = " + EntitySettingsName.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entity Settings Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteDyEntitySettings(string ObjectRef, string UrlReferrer, int RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_dynamic_entity  WHERE id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
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

        public static List<ExportFields> GetSFFormExportFields(string objectRef, int groupId, string urlReferrer)
        {
            List<ExportFields> returnFileds = new List<ExportFields>();
            var counter = 1;
            // var counter = GetDefaultFields(entityName, Crm.SalesForce).Count;
            // returnFileds.AddRange(GetDefaultFields(entityName, Crm.SalesForce));
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
                            ExportFields contactExportFields = new ExportFields();
                            contactExportFields.Entity = "Contact";
                            ExportFields leadExportFields = new ExportFields();
                            leadExportFields.Entity = "Lead";
                            ExportFields accountExportFields = new ExportFields();
                            accountExportFields.Entity = "Account";

                            List<ExportFieldModel> contactFieldsList = new List<ExportFieldModel>();
                            List<ExportFieldModel> leadFieldsList = new List<ExportFieldModel>();
                            List<ExportFieldModel> accountFieldsList = new List<ExportFieldModel>();


                            while (rdr.Read())
                            {
                                if (rdr["entity_name"].ToString().Trim().ToLower() == "contact")
                                {
                                    ExportFieldModel contactExportFieldsList = new ExportFieldModel();
                                    contactExportFieldsList.FieldType = "Custom";
                                    contactExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    contactExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    contactExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    contactExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
                                    contactFieldsList.Add(contactExportFieldsList);
                                }
                                if (rdr["entity_name"].ToString().Trim().ToLower() == "lead")
                                {
                                    ExportFieldModel leadExportFieldsList = new ExportFieldModel();
                                    leadExportFieldsList.FieldType = "Custom";
                                    leadExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    leadExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    leadExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    leadExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
                                    leadFieldsList.Add(leadExportFieldsList);
                                }
                                if (rdr["entity_name"].ToString().Trim().ToLower() == "account")
                                {
                                    ExportFieldModel accountExportFieldsList = new ExportFieldModel();
                                    accountExportFieldsList.FieldType = "Custom";
                                    accountExportFieldsList.FiledLabel = rdr["inputfieldlabel"].ToString().Trim();
                                    accountExportFieldsList.FieldName = rdr["fieldname"].ToString().Trim();
                                    accountExportFieldsList.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                    accountExportFieldsList.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
                                    accountFieldsList.Add(accountExportFieldsList);
                                }
                            }
                            contactExportFields.ExportFieldsList = contactFieldsList;
                            returnFileds.Add(contactExportFields);
                            leadExportFields.ExportFieldsList = leadFieldsList;
                            returnFileds.Add(leadExportFields);
                            accountExportFields.ExportFieldsList = accountFieldsList;
                            returnFileds.Add(accountExportFields);
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
                                exportFields.FieldLabel = rdr["inputfieldlabel"].ToString().Trim();
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

        public static FieldsModel GetSFExportFieldsById(int ExportFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_custom_fields where id = '" + ExportFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["inputfieldlabel"].ToString().Trim();
                                returnFileds.FieldName = rdr["fieldname"].ToString().Trim();
                                // returnFileds.FieldType = rdr["fieldtype"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity_name"].ToString().Trim();
                                returnFileds.ValueType = rdr["valuetype"].ToString().Trim();
                                returnFileds.ValueDetail = rdr["valuedetail"].ToString().Trim();
                                returnFileds.BusinessRequired = Convert.ToInt32(rdr["businessrequired"].ToString().Trim());
                                returnFileds.MaxLength = Convert.ToInt32(rdr["maxlength"].ToString().Trim());
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
                        sql += "VALUES ('" + ExportFields.ObjectRef + "'," + ExportFields.GroupId.ToString() + ",'" + integrationId + "','" + ExportFields.EntityType + "','" + ExportFields.FieldName + "','" + ExportFields.ValueType + "','" + ExportFields.ValueDetail + "','" + ExportFields.FieldLabel + "','" + ExportFields.BusinessRequired + "','" + ExportFields.MaxLength + "' )";
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

        public static string UpdateSFExportFields(FieldsModel ExportFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ExportFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_custom_fields Set fieldname = '" + ExportFields.FieldName + "', entity_name = '" + ExportFields.EntityType + "', valuetype = '" + ExportFields.ValueType + "', valuedetail = '" + ExportFields.ValueDetail + "', inputfieldlabel = '" + ExportFields.FieldLabel + "', businessrequired = '" + ExportFields.BusinessRequired + "', maxlength = '" + ExportFields.MaxLength + "'";
                    sql += " WHERE id = " + ExportFields.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Export Fields Updated Successfully";

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
                                exportFields.FieldLabel = rdr["search_label"].ToString().Trim();
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

        public static FieldsModel GetSFSearchFieldsById(int SearchFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_custom_search where id = '" + SearchFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["search_label"].ToString().Trim();
                                returnFileds.FieldName = rdr["search_field_name"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity_name"].ToString().Trim();
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
                    sql += "VALUES ('" + SearchFields.ObjectRef + "'," + SearchFields.GroupId.ToString() + ",'" + SearchFields.EntityType + "','" + SearchFields.FieldName + "','" + SearchFields.FieldLabel + "' )";
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

        public static string UpdateSFSearchFields(FieldsModel SearchFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, SearchFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_custom_search Set search_field_name = '" + SearchFields.FieldName + "', entity_name = '" + SearchFields.EntityType + "', search_label = '" + SearchFields.FieldLabel + "'";
                    sql += " WHERE id = " + SearchFields.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Search Fields Updated Successfully";

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
                                exportFields.FieldLabel = rdr["label"].ToString().Trim();
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

        public static FieldsModel GetSFDetailFieldsById(int DetailFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_detailedview_fields where id = '" + DetailFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldLabel = rdr["label"].ToString().Trim();
                                returnFileds.FieldName = rdr["sf_variable"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity_type"].ToString().Trim();
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
                    sql += "VALUES ('" + DetailFields.ObjectRef + "'," + DetailFields.GroupId.ToString() + ",'" + DetailFields.EntityType + "','" + DetailFields.FieldName + "','" + DetailFields.FieldLabel + "' )";
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

        public static string UpdateSFDetailFields(FieldsModel DetailFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DetailFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_detailedview_fields Set sf_variable = '" + DetailFields.FieldName + "', entity_type = '" + DetailFields.EntityType + "', label = '" + DetailFields.FieldLabel + "'";
                    sql += " WHERE id = " + DetailFields.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Updated Successfully";
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

        public static List<FieldsModel> GetSFBackEndFields(string objectRef, int groupId, string urlReferrer, string entity = null)
        {
            List<FieldsModel> returnFields = new List<FieldsModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql;
                    if (entity == null)
                    {
                        sql = "SELECT * from integration_salesforce_backend_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' ";
                    }
                    else
                    {
                        sql = "SELECT * from integration_salesforce_backend_fields where objectref = '" + objectRef + "' AND groupid = '" + groupId + "' AND entity = '" + entity + "' ";
                    }
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                FieldsModel backendFields = new FieldsModel();
                                backendFields.ID = int.Parse(rdr["id"].ToString().Trim());
                                backendFields.FieldName = rdr["backend_field_name"].ToString().Trim();
                                backendFields.ValueDetail = rdr["backend_field_value"].ToString().Trim();
                                backendFields.EntityType = rdr["entity"].ToString().Trim();
                                backendFields.FieldType = rdr["backend_field_type"].ToString().Trim();

                                returnFields.Add(backendFields);
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
            return returnFields;
        }

        public static FieldsModel GetSFBackEndFieldsById(int BackEndFieldID, string ObjectRef, string urlReferrer)
        {
            FieldsModel returnFileds = new FieldsModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_backend_fields where id = '" + BackEndFieldID + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnFileds.ID = int.Parse(rdr["id"].ToString().Trim());
                                returnFileds.FieldName = rdr["backend_field_name"].ToString().Trim();
                                returnFileds.ValueDetail = rdr["backend_field_value"].ToString().Trim();
                                returnFileds.EntityType = rdr["entity"].ToString().Trim();
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

        public static string AddSFBackEndFields(FieldsModel BackEndFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, BackEndFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_salesforce_backend_fields (objectref, groupid, entity, backend_field_name, backend_field_value)";
                    sql += "VALUES ('" + BackEndFields.ObjectRef + "'," + BackEndFields.GroupId.ToString() + ",'" + BackEndFields.EntityType + "','" + BackEndFields.FieldName + "','" + BackEndFields.ValueDetail + "' )";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Back End Fields Added Successfully";

                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateSFBackEndFields(FieldsModel BackEndFields, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, BackEndFields.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_backend_fields Set backend_field_name = '" + BackEndFields.FieldName + "', backend_field_value = '" + BackEndFields.ValueDetail + "', entity = '" + BackEndFields.EntityType + "'";
                    sql += " WHERE id = " + BackEndFields.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Detail Fields Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSFBackEndFields(int Id, string ObjectRef, string urlReferrer, out string ErrorMessage)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_backend_fields WHERE id = " + Id;
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

        public static DefaultFieldSettings GetSFDefaultFieldSettings(string ObjectRef, int GroupId, string UrlReferrer)
        {
            DefaultFieldSettings defaultField = new DefaultFieldSettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_saleforce_default_fields WHERE objectref = '" + ObjectRef + "' AND groupid = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                defaultField.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                defaultField.IsAccountPhoneRequired = Convert.ToInt32(rdr["is_visible_account_phone"].ToString().Trim());
                                defaultField.IsContactEmailRequired = Convert.ToInt32(rdr["is_visible_contact_email"].ToString().Trim());
                                defaultField.IsContactPhoneRequired = Convert.ToInt32(rdr["is_visible_contact_phone"].ToString().Trim());
                                defaultField.IsLeadEmailRequired = Convert.ToInt32(rdr["is_visible_lead_email"].ToString().Trim());
                                defaultField.IsLeadPhoneRequired = Convert.ToInt32(rdr["is_visible_lead_phone"].ToString().Trim());
                                defaultField.IsLeadCompanyRequired = Convert.ToInt32(rdr["is_visible_lead_company"].ToString().Trim());
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
            return defaultField;
        }

        public static DefaultFieldSettings GetSFDefaultFieldSettingsById(string ObjectRef, int Id, string UrlReferrer)
        {
            DefaultFieldSettings defaultField = new DefaultFieldSettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_saleforce_default_fields WHERE id = '" + Id + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                defaultField.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                defaultField.IsAccountPhoneRequired = Convert.ToInt32(rdr["is_visible_account_phone"].ToString().Trim());
                                defaultField.IsContactEmailRequired = Convert.ToInt32(rdr["is_visible_contact_email"].ToString().Trim());
                                defaultField.IsContactPhoneRequired = Convert.ToInt32(rdr["is_visible_contact_phone"].ToString().Trim());
                                defaultField.IsLeadEmailRequired = Convert.ToInt32(rdr["is_visible_lead_email"].ToString().Trim());
                                defaultField.IsLeadPhoneRequired = Convert.ToInt32(rdr["is_visible_lead_phone"].ToString().Trim());
                                defaultField.IsLeadCompanyRequired = Convert.ToInt32(rdr["is_visible_lead_company"].ToString().Trim());
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
            return defaultField;
        }

        public static string AddSFDefaultFieldSettings(DefaultFieldSettings DefaultFieldSettings, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DefaultFieldSettings.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_saleforce_default_fields (objectref, groupid, is_visible_account_phone, is_visible_contact_email, is_visible_contact_phone, is_visible_lead_email, is_visible_lead_phone, is_visible_lead_company)";
                    sql += "VALUES ('" + DefaultFieldSettings.ObjectRef + "','" + DefaultFieldSettings.GroupId + "','" + DefaultFieldSettings.IsAccountPhoneRequired + "','" + DefaultFieldSettings.IsContactEmailRequired + "','" + DefaultFieldSettings.IsContactPhoneRequired + "','" + DefaultFieldSettings.IsLeadEmailRequired + "','" + DefaultFieldSettings.IsLeadPhoneRequired + "','" + DefaultFieldSettings.IsLeadCompanyRequired + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entiity Default Field Settings Added Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateSFDefaultFieldSettings(DefaultFieldSettings DefaultFieldSettings, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, DefaultFieldSettings.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_saleforce_default_fields Set is_visible_account_phone = '" + DefaultFieldSettings.IsAccountPhoneRequired + "', is_visible_contact_email = '" + DefaultFieldSettings.IsContactEmailRequired + "', is_visible_contact_phone = '" + DefaultFieldSettings.IsContactPhoneRequired + "', is_visible_lead_email = '" + DefaultFieldSettings.IsLeadEmailRequired + "', is_visible_lead_phone = '" + DefaultFieldSettings.IsLeadPhoneRequired + "', is_visible_lead_company = '" + DefaultFieldSettings.IsLeadCompanyRequired + "'";
                    sql += " WHERE id = " + DefaultFieldSettings.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entity Settings Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSFDefaultFieldSettings(string ObjectRef, string UrlReferrer, int RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_saleforce_default_fields  WHERE id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
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

        #region Alive 5 Chats
        public static bool IsChatExist(string EntityId, string EntityType, string App, string objectRef, string urlReferrer, out string ChatId, out string RowId)
        {
            ChatId = string.Empty;
            RowId = string.Empty;
            bool flag = false;
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_crm_chats where entity_id = '" + EntityId + "' AND entity_type = '" + EntityType + "' AND app = '" + App + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                flag = true;
                                RowId = rdr["id"].ToString();
                                ChatId = rdr["chat_id"].ToString();
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
            return flag;
        }

        public static void AddChatInfo(string objectRef, string urlReferrer, string CRM, string EntityId, string EntityType, string App, string ChatId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_crm_chats (crm, entity_type, entity_id, chat_id, is_active, app)";
                    sql += "VALUES ('" + CRM + "','" + EntityType + "','" + EntityId + "','" + ChatId + "','" + true + "','" + App + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteChatInfo(string objectRef, string urlReferrer, string RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_crm_chats WHERE chat_id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
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

        #region Saleforce Entity Setting
        public static EntitySettings GetSfEntitySettings(string ObjectRef, int GroupId, string UrlReferrer)
        {
            EntitySettings returnEntitySettings = new EntitySettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_entity WHERE objectref = '" + ObjectRef + "' AND groupid = " + GroupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnEntitySettings.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                returnEntitySettings.IsAccountRequired = Convert.ToInt32(rdr["accounts_required"].ToString().Trim());
                                returnEntitySettings.IsContactRequired = Convert.ToInt32(rdr["contacts_required"].ToString().Trim());
                                returnEntitySettings.IsLeadRequired = Convert.ToInt32(rdr["leads_required"].ToString().Trim());

                                returnEntitySettings.AllowAccountCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
                                returnEntitySettings.AllowContactCreation = Convert.ToInt32(rdr["allow_contacts_creation"].ToString().Trim());
                                returnEntitySettings.AllowLeadCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
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
            return returnEntitySettings;
        }

        public static EntitySettings GetSfEntitySettingsById(string ObjectRef, int Id, string UrlReferrer)
        {
            EntitySettings returnEntitySettings = new EntitySettings();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_salesforce_entity WHERE id = '" + Id + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnEntitySettings.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                returnEntitySettings.IsAccountRequired = Convert.ToInt32(rdr["accounts_required"].ToString().Trim());
                                returnEntitySettings.IsContactRequired = Convert.ToInt32(rdr["contacts_required"].ToString().Trim());
                                returnEntitySettings.IsLeadRequired = Convert.ToInt32(rdr["leads_required"].ToString().Trim());
                                returnEntitySettings.AllowAccountCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
                                returnEntitySettings.AllowContactCreation = Convert.ToInt32(rdr["allow_contacts_creation"].ToString().Trim());
                                returnEntitySettings.AllowLeadCreation = Convert.ToInt32(rdr["allow_leads_creation"].ToString().Trim());
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
            return returnEntitySettings;
        }

        public static string AddSfEntitySettings(EntitySettings EntitySettingsName, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntitySettingsName.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_salesforce_entity (objectref, groupid, leads_required, contacts_required, accounts_required, allow_leads_creation, allow_contacts_creation, allow_accounts_creation)";
                    sql += "VALUES ('" + EntitySettingsName.ObjectRef + "','" + EntitySettingsName.GroupId + "','" + EntitySettingsName.IsLeadRequired + "','" + EntitySettingsName.IsContactRequired + "','" + EntitySettingsName.IsAccountRequired + "','" + EntitySettingsName.AllowLeadCreation + "','" + EntitySettingsName.AllowContactCreation + "','" + EntitySettingsName.AllowAccountCreation + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entiity Settings Added Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateSfEntitySettings(EntitySettings EntitySettingsName, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntitySettingsName.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_salesforce_entity Set leads_required = '" + EntitySettingsName.IsLeadRequired + "', contacts_required = '" + EntitySettingsName.IsContactRequired + "', accounts_required = '" + EntitySettingsName.IsAccountRequired + "', allow_leads_creation = '" + EntitySettingsName.AllowLeadCreation + "', allow_contacts_creation = '" + EntitySettingsName.AllowContactCreation + "', allow_accounts_creation = '" + EntitySettingsName.AllowAccountCreation + "'";
                    sql += " WHERE id = " + EntitySettingsName.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Entity Settings Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteSfEntitySettings(string ObjectRef, string UrlReferrer, int RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_salesforce_entity  WHERE id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
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

        #region CRM Entity

        public static string AddEntity(EntityModel EntityDetail, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntityDetail.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO integration_crm_entity (objectref, groupid, crm_type, entity_name, entity_display_name, entity_primary_field_name, entity_primary_field_display_name, allow_entity_record_creation)";
                    sql += "VALUES ('" + EntityDetail.ObjectRef + "','" + EntityDetail.GroupId + "','" + EntityDetail.CrmType + "','" + EntityDetail.EntityUniqueName + "','" + EntityDetail.EntityDispalyName + "','" + EntityDetail.PrimaryFieldUniqueName + "','" + EntityDetail.PrimaryFieldDisplayName + "','" + EntityDetail.AllowRecordCreation + "')";
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Dynamics Entity Added Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static string UpdateEntity(EntityModel EntityDetail, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, EntityDetail.ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "Update integration_crm_entity Set entity_name = '" + EntityDetail.EntityUniqueName + "', entity_display_name = '" + EntityDetail.EntityDispalyName + "', entity_primary_field_name = '" + EntityDetail.PrimaryFieldUniqueName + "', entity_primary_field_display_name = '" + EntityDetail.PrimaryFieldDisplayName + "', allow_entity_record_creation = '" + EntityDetail.AllowRecordCreation + "'";
                    sql += " WHERE id = " + EntityDetail.ID;
                    MySqlCommand cmd1 = new MySqlCommand(sql, conn);
                    int rows = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return "Dynamics Entity Updated Successfully";
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static bool DeleteEntity(string ObjectRef, string UrlReferrer, int RowId)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sqlDel = "DELETE FROM integration_crm_entity  WHERE id = " + RowId;
                    MySqlCommand cmd1 = new MySqlCommand(sqlDel, conn);
                    int rowsDeleted = cmd1.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    conn.Close();
                    throw;
                }
            }
        }

        public static List<EntityModel> GetEntityList(string UrlReferrer, string ObjectRef, int GroupId, string crmType)
        {
            List<EntityModel> returnEntityList = new List<EntityModel>();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_crm_entity WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = '" + GroupId.ToString() + "' AND crm_type = '" + crmType + "'  ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                EntityModel entity = new EntityModel();
                                entity.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                entity.EntityUniqueName = rdr["entity_name"].ToString().Trim();
                                entity.EntityDispalyName = rdr["entity_display_name"].ToString().Trim();
                                entity.PrimaryFieldUniqueName = rdr["entity_primary_field_name"].ToString().Trim();
                                entity.PrimaryFieldDisplayName = rdr["entity_primary_field_display_name"].ToString().Trim();
                                returnEntityList.Add(entity);
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
            return returnEntityList;
        }

        public static EntityModel GetEntityById(string UrlReferrer, string ObjectRef, int EntityId)
        {
            EntityModel returnEntity = new EntityModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_crm_entity WHERE id = '" + EntityId + "' ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                returnEntity.ID = Convert.ToInt32(rdr["id"].ToString().Trim());
                                returnEntity.EntityUniqueName = rdr["entity_name"].ToString().Trim();
                                returnEntity.EntityDispalyName = rdr["entity_display_name"].ToString().Trim();
                                returnEntity.PrimaryFieldUniqueName = rdr["entity_primary_field_name"].ToString().Trim();
                                returnEntity.PrimaryFieldDisplayName = rdr["entity_primary_field_display_name"].ToString().Trim();
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
            return returnEntity;
        }

        public static EntityModel GetEntity(string UrlReferrer, string ObjectRef, int GroupId, string Entity, string crmType)
        {
            EntityModel returnEntity = new EntityModel();
            string connStr = MyAppsDb.GetConnectionStringbyURL(UrlReferrer, ObjectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * from integration_crm_entity WHERE ObjectRef = '" + ObjectRef + "' AND GroupId = '" + GroupId.ToString() + "' AND entity_name = '" + Entity + "' AND crm_type = '" + crmType + "'  ";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                //returnEntitySettings.EntityUniqueName = rdr["entity_name"].ToString().Trim();
                                //returnEntitySettings.EntityDispalyName = rdr["entity_display_name"].ToString().Trim();
                                returnEntity.PrimaryFieldUniqueName = rdr["entity_primary_field_name"].ToString().Trim();
                                // returnEntitySettings.PrimaryFieldValue = rdr["entity_primary_field_display_name"].ToString().Trim();

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
            return returnEntity;
        }

        #endregion

        public static int RecordDynamicsSettings(string objectRef, int groupId, int isUsingAliveChat, string urlReferrer)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM integration_dynamic_entity WHERE ObjectRef = '" + objectRef + "' AND GroupId = " + groupId.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.HasRows)
                        {
                            // Insert the record
                            rdr.Close();
                            string insertSql = "INSERT INTO integration_dynamic_entity (objectref, groupid, use_alive_chat)";
                            insertSql += "VALUES ('" + objectRef + "','" + groupId + "','" + isUsingAliveChat + "')";
                            MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn);
                            int rows = cmdInsert.ExecuteNonQuery();
                            conn.Close();
                            return rows;
                        }
                        else
                        {
                            // Update the record
                            var id = Convert.ToInt32(rdr["id"]);
                            rdr.Close();
                            string updateSql = "Update integration_dynamic_entity Set use_alive_chat = '" + isUsingAliveChat + "'";
                            updateSql += " WHERE id = " + id;
                            MySqlCommand cmd1 = new MySqlCommand(updateSql, conn);
                            int rows = cmd1.ExecuteNonQuery();
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
    }
}