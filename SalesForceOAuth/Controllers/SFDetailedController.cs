﻿using CRM.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;


namespace SalesForceOAuth.Controllers
{
    public class SFDetailedController : ApiController
    {
        //    [HttpGet]
        //    public async System.Threading.Tasks.Task<HttpResponseMessage> GetView(string token, string ObjectRef, int GroupId, string entity, string refId, string siteRef, string callback)
        //    {
        //        string InstanceUrl = "", AccessToken = "", ApiVersion = "";
        //        string outputPayload;
        //        try
        //        {
        //            outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
        //        }
        //        catch (Exception ex)
        //        {
        //            return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.OK);
        //        }
        //        //Access token update
        //        string urlReferrer = Request.RequestUri.Authority.ToString();
        //        HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
        //        if (msg.StatusCode != HttpStatusCode.OK)
        //        { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
        //        try
        //        {

        //            List<SFDetailedView> myDView = new List<SFDetailedView> { };
        //            string sFieldOptional = "";
        //            string sLabelOptional = "";
        //            string query = "";
        //            MyAppsDb.GetAPICredentialswithCustomViewFields(ObjectRef, GroupId, entity, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref sFieldOptional, ref sLabelOptional, ref query, urlReferrer);
        //            string[] customSearchArray = sFieldOptional.Split('|');
        //            string[] customSearchLabelArray = sLabelOptional.Split('|');
        //            ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
        //            query += " where Id ='" + refId + "'";
        //            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //            QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
        //            if (cont.Records.Count > 0)
        //            {
        //                foreach (dynamic c in cont.Records)
        //                {
        //                    SFDetailedView l = new SFDetailedView();
        //                    l.Id = c.Id;
        //                    int noOfcustomItems = 0;
        //                    if (entity == "lead")
        //                    {
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Company", c.Company.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
        //                    }
        //                    else if (entity == "account")
        //                    {
        //                        //noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Number", c.AccountNumber.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Name", c.Name.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
        //                    }
        //                    else if (entity == "contact")
        //                    {
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
        //                        if (c.Account != null)
        //                        {
        //                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Name", c.Account.Name.ToString(), noOfcustomItems);
        //                        }
        //                    }
        //                    else if (entity == "opportunity")
        //                    {
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Name", c.Name.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Close Date", c.CloseDate.ToString(), noOfcustomItems);
        //                        noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Stage", c.StageName.ToString(), noOfcustomItems);
        //                    }
        //                    //if (sFieldOptional.Length > 0)
        //                    //{

        //                    //    foreach (Newtonsoft.Json.Linq.JProperty item in c)
        //                    //    {
        //                    //        foreach (string csA in customSearchArray)
        //                    //        {
        //                    //            if (item.Name == csA)
        //                    //            {
        //                    //                //code to add to custom list
        //                    //                noOfcustomItems++;
        //                    //                MyAppsDb.AssignCustomVariableValue(l, item.Name, item.Value.ToString(), noOfcustomItems);
        //                    //            }
        //                    //        }
        //                    //    }
        //                    //}
        //                    if (sFieldOptional.Length > 0)
        //                    {
        //                        int i = 0;
        //                        foreach (Newtonsoft.Json.Linq.JProperty item in c)
        //                        {

        //                            foreach (string csA in customSearchArray)
        //                            {
        //                                if (item.Name.ToLower() == csA.ToLower())
        //                                {
        //                                    //code to add to custom list
        //                                    noOfcustomItems++;
        //                                    MyAppsDb.AssignCustomVariableValue(l, customSearchLabelArray[i], item.Value.ToString(), noOfcustomItems);
        //                                    i++;
        //                                }//codde
        //                            }
        //                        }
        //                    }
        //                    myDView.Add(l);
        //                }
        //            }
        //            return MyAppsDb.ConvertJSONPOutput(callback, myDView, HttpStatusCode.OK, false);
        //        }
        //        catch (Exception ex)
        //        {
        //            return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.OK);
        //        }

        //    }

        //}
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetView(string token, string ObjectRef, int GroupId, string entity, string refId, string siteRef, string callback)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(siteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
            try
            {

                List<SFDetailedView> myDView = new List<SFDetailedView> { };
                CrmEntity dynamicEntity = new CrmEntity();
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                MyAppsDb.GetSaleForceAPICredentials(ObjectRef, GroupId, entity, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                List<FieldsModel> detailsFields = Repository.GetSFDetailFieldsByEntity(ObjectRef, GroupId, entity, urlReferrer);

                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                if (entity == "lead")
                {
                    columns.Append("Id, FirstName, LastName, Company, Email, Phone ");
                }
                else if (entity == "account")
                {
                    columns.Append("Id,  AccountNumber, Name, Phone, LastName");
                }
                else if (entity == "contact")
                {
                    columns.Append("Id, FirstName, LastName, Email, Phone ");
                }
                else
                {
                    // Get The dynamics Entity Info
                    dynamicEntity = Repository.GetEntity(urlReferrer, ObjectRef, GroupId, entity, "sf");
                    columns.Append("Id, " + dynamicEntity.PrimaryFieldUniqueName);
                }
                if (detailsFields.Count > 0)
                {
                    foreach (var detail in detailsFields)
                    {
                        if (!columns.ToString().Contains(detail.FieldName))
                        {
                            columns.Append("," + detail.FieldName);
                        }
                    }
                }
                query.Append("SELECT " + columns + " From " + entity);
                query.Append(" where Id ='" + refId + "'");
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        SFDetailedView l = new SFDetailedView();
                        l.Id = c.Id;
                        int noOfcustomItems = 0;
                        if (entity == "lead")
                        {
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Company", c.Company.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                        }
                        else if (entity == "account")
                        {
                            //noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Number", c.AccountNumber.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Name", c.Name.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                        }
                        else if (entity == "contact")
                        {
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "First Name", c.FirstName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Last Name", c.LastName.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Email", c.Email.ToString(), noOfcustomItems);
                            noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Phone", c.Phone.ToString(), noOfcustomItems);
                            if (c.Account != null)
                            {
                                noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, "Account Name", c.Account.Name.ToString(), noOfcustomItems);
                            }
                        }
                        else
                        {
                            FieldsModel d = new FieldsModel();
                            d.FieldName = dynamicEntity.PrimaryFieldUniqueName;
                            d.FieldLabel = dynamicEntity.PrimaryFieldDisplayName;
                            detailsFields.Add(d);
                            // noOfcustomItems++; MyAppsDb.AssignCustomVariableValue(l, dynamicEntity.PrimaryFieldUniqueName, c.Name.ToString(), noOfcustomItems);

                        }
                        if (detailsFields.Count > 0)
                        {
                            int i = 0;
                            foreach (Newtonsoft.Json.Linq.JProperty item in c)
                            {
                                foreach (var detail in detailsFields)
                                {
                                    if (item.Name.ToLower() == detail.FieldName.ToLower())
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(l, detail.FieldLabel, item.Value.ToString(), noOfcustomItems);
                                        i++;
                                    }//codde
                                }
                            }
                        }
                        myDView.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, myDView, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.OK);
            }

        }
    }

    public class SFDetailedView
    {
        public String Id { get; set; }
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string Custom4 { get; set; }
        public string Custom5 { get; set; }
        public string Custom6 { get; set; }
        public string Custom7 { get; set; }
        public string Custom8 { get; set; }
        public string Custom9 { get; set; }
        public string Custom10 { get; set; }
        public string Custom11 { get; set; }
        public string Custom12 { get; set; }
        public string Custom13 { get; set; }
        public string Custom14 { get; set; }
        public string Custom15 { get; set; }
    }
}
