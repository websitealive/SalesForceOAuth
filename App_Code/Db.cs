﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using SalesForceOAuth.Controllers; 

namespace SalesForceOAuth.App_Code
{
    
    public class Db
        {
            //
            private MySqlConnection connect;

            //
            private MySqlCommand command;

            //
            private MySqlDataReader reader;

            //
            private string connectiondetails = "";

        
        public static void GetRedirectURLParameters(ref string sf_authoize_url, ref string sf_clientid,ref string sf_callback_url, string urlReferrer, string objectRef)
            {
                string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
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

        public static void GetTokenParameters(ref string sf_callback_url, ref string sf_consumer_key, ref string sf_consumer_secret, ref string sf_token_req_end_point,string urlReferrer, string objectRef)
        {
            string connStr = MyAppsDb.GetConnectionStringbyURL(urlReferrer, objectRef);
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

        //
        private bool bConnected = false;

            //
            private DataTable Table;

            //
            private int affected_rows;

            //
            private string squery;

            //
            private List<string> parameters;

            public Db()
            {
                Connect();
                Table = Table = new DataTable();
                parameters = new List<string>();
            }

            private void Connect()
            {
                try
                {
                    MySqlConnection connection = new MySqlConnection(connectiondetails);
                    connection.Open();
                    connect = connection;
                    bConnected = true;
                }
                catch (MySqlException ex)
                {
                    string exception = "Exception : " + ex.Message.ToString() + "\n\rApplication will close now. \n\r" + squery;
                    Debug(exception);

                    Environment.Exit(1);
                }
            }

            public void CloseConn()
            {
                bConnected = false;
                connect.Close();
                connect.Dispose();
            }

            private void Init(string Query, string[] bindings = null)
            {
                // No connection with database? make connection
                if (bConnected == false)
                {
                    Connect();
                }

                // Automatically disposes the MySQLcommand instance
                using (command = new MySqlCommand(Query, connect))
                {
                    // 
                    bind(bindings);

                    // Prevents SQL injection
                    if (parameters.Count > 0)
                    {
                        parameters.ForEach(delegate (string parameter)
                        {
                            string[] sparameters = parameter.ToString().Split('\x7F');
                            command.Parameters.AddWithValue(sparameters[0], sparameters[1]);
                        });
                    }

                    this.squery = Query.ToLower();

                    if (squery.Contains("select"))
                    {
                        this.Table = execDatatable();
                    }
                    if (squery.Contains("delete") || squery.Contains("update") || squery.Contains("insert"))
                    {
                        this.affected_rows = execNonquery();
                    }

                    // Clear de parameters, 
                    this.parameters.Clear();
                }
            }

            public void bind(string field, string value)
            {
                parameters.Add("@" + field + "\x7F" + value);
            }

            public void bind(string[] fields)
            {
                if (fields != null)
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        bind(fields[i], fields[i + 1]);
                        i += 1;
                    }
                }
            }

            // Example:
            // qBind(new string[] { "12", "John" });
            // nQuery("SELECT * FROM User WHERE ID=@0 AND Name=@1");
            /// <summary>
            /// Bind multiple fields/values without identifier.
            /// </summary>
            /// <param name="fields"></param>
            public void qBind(string[] fields)
            {
                if (fields != null)
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        bind(i.ToString(), fields[i]);
                    }
                }
            }

            private DataTable execDatatable()
            {
                DataTable dt = new DataTable();
                try
                {
                    reader = command.ExecuteReader();
                    dt.Load(reader);
                }
                catch (MySqlException my)
                {
                    string exception = "Exception : " + my.Message.ToString() + "\n\r SQL Query : \n\r" + squery;

                    //MessageBox.Show(exception, "Uncaught MYSQL Exception");

                    Debug(exception);
                }

                return dt;
            }

            private int execNonquery()
            {

                int affected = 0;

                try
                {
                    affected = command.ExecuteNonQuery();
                }
                catch (MySqlException my)
                {
                    string exception = "Exception : " + my.Message.ToString() + "\n\r SQL Query : \n\r" + squery;

                    //MessageBox.Show(exception, "Uncaught MYSQL Exception");

                    Debug(exception);
                }

                return affected;
            }

            public DataTable table(string table, string[] bindings = null)
            {
                Init("SELECT * FROM " + table, bindings);
                return this.Table;
            }

            public List<object> table(string table, Type t)
            {
                return new List<object>();
            }

            public DataTable query(string query, string[] bindings = null)
            {
                Init(query, bindings);
                return this.Table;
            }

            public int nQuery(string query, string[] bindings = null)
            {
                Init(query, bindings);
                return this.affected_rows;
            }

            public string single(string query, string[] bindings = null)
            {
                Init(query, bindings);

                if (Table.Rows.Count > 0)
                {
                    return Table.Rows[0][0].ToString();
                }

                return string.Empty;
            }

            public string[] row(string query, string[] bindings = null)
            {
                Init(query, bindings);

                string[] row = new string[Table.Columns.Count];

                if (Table.Rows.Count > 0)
                    for (int i = 0; i++ < Table.Columns.Count; row[i - 1] = Table.Rows[0][i - 1].ToString()) ;

                return row;
            }

            public List<string> column(string query, string[] bindings = null)
            {
                Init(query, bindings);

                List<string> column = new List<string>();

                for (int i = 0; i++ < Table.Rows.Count; column.Add(Table.Rows[i - 1][0].ToString())) ;

                return column;
            }


            public void Debug(string error)
            {
                Console.WriteLine(error + "/n/r");
            }
        }
   
}