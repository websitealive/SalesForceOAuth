﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.Text;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYContactController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostContact(DYContactPostData lData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-PostContact", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            try
            {
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alive5.crm11.dynamics.com", userName = "alive5@alive5.onmicrosoft.com",
                //password = "Passw0rd1", authType = "Office365";
                //string ApplicationURL = "https://websitealive.crmgate.pk/websitealive", userName = "naveed@crmgate.local",
                //    password = "@Abc.123", authType = "IFD";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(lData.ObjectRef, lData.GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                //string connectionString = string.Format("url={0};username={1};password={2};authtype={3};", ApplicationURL, userName, password, authType);
                //connectionString = "url=https://msdynamics.websitealive.com/MSDynamics;username=wsa\\administrator;password=bX9bTkYv)Td;Domain=wsa;authtype=IFD;RequireNewInstance=true;"; 

                //connectionString += "RequireNewInstance=true;";


                //new code start

                Uri organizationUri;
                Uri homeRealmUri;
                ClientCredentials credentials = new ClientCredentials();
                ClientCredentials deviceCredentials = new ClientCredentials();
                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
                deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
                organizationUri = new Uri(ApplicationURL + "/XRMServices/2011/Organization.svc");
                homeRealmUri = null;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    Entity registration = new Entity("contact");
                    registration["firstname"] = lData.FirstName;
                    registration["lastname"] = lData.LastName;
                    #region custom fields 
                    if (lData.CustomFields != null)
                    {
                        foreach (DYCustomObject c in lData.CustomFields)
                        {
                            CrmFieldType type;
                            switch (c.type.ToLower())
                            {
                                //case "string":
                                //    {
                                //        type = CrmFieldType.String;break;
                                //    }
                                case "optionset":
                                    {
                                        type = CrmFieldType.CrmDecimal;
                                        int option = Convert.ToInt32(c.value); 
                                        registration[c.field] = new OptionSetValue(option);
                                        break;
                                    }
                                case "lookup":
                                    {
                                        type = CrmFieldType.Lookup;
                                        if (c.value.ToString().Length > 0)
                                        {
                                            registration[c.field] = new EntityReference("User", new Guid(c.value));//inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));// registration[c.field] = new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table).Value;
                                        }
                                        break;
                                    }
                                //case "bool":
                                //    { type = CrmFieldType.CrmBoolean; break; }
                                default:
                                    {
                                        type = CrmFieldType.String;
                                        registration[c.field] = c.value;
                                        break;
                                        
                                    }
                            }
                            //if (type != CrmFieldType.Lookup)
                            //{
                            //    //  inData.Add(c.field, new CrmDataTypeWrapper(c.value, type));
                            //    registration[c.field] = c.value; // new CrmDataTypeWrapper(c.value, type); 
                            //}
                        }
                    }
                    #endregion Custom fields 

                    Guid contactId = objser.Create(registration);
                    if (contactId != Guid.Empty)
                    {
                        //Console.WriteLine("Account created.");
                        PostedObjectDetail pObject = new PostedObjectDetail();
                        pObject.Id = contactId.ToString();
                        pObject.ObjectName = "Contact";
                        pObject.Message = "Contact added successfully!";
                        return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONOutput("Could not add new contact, check mandatory fields", HttpStatusCode.OK, true);
                    }
                }

                //end code start


                //userName = "wsa\administrator";
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CrmServiceClient crmSvc = new CrmServiceClient(connectionString);

                //if (crmSvc != null && crmSvc.IsReady)
                //{
                //    //create Account object
                //    Dictionary<string, CrmDataTypeWrapper> inData = new Dictionary<string, CrmDataTypeWrapper>();
                //    inData.Add("firstname", new CrmDataTypeWrapper(lData.FirstName, CrmFieldType.String));
                //    inData.Add("lastname", new CrmDataTypeWrapper(lData.LastName, CrmFieldType.String));
                //    if (lData.CustomFields != null)
                //    {
                //        foreach (DYCustomObject c in lData.CustomFields)
                //        {
                //            CrmFieldType type;
                //            switch (c.type.ToLower())
                //            {
                //                case "string":
                //                    { type = CrmFieldType.String; break; }
                //                case "decimal":
                //                    { type = CrmFieldType.CrmDecimal; break; }
                //                case "lookup":
                //                    { type = CrmFieldType.Lookup; break; }
                //                case "bool":
                //                    { type = CrmFieldType.CrmBoolean; break; }
                //                default:
                //                    { type = CrmFieldType.String; break; }
                //            }
                //            if(type ==  CrmFieldType.Lookup)
                //            {
                //                if (c.value.ToString().Length > 0)
                //                {
                //                    inData.Add(c.field, new CrmDataTypeWrapper(new Guid(c.value), CrmFieldType.Lookup, c.table));
                //                }
                //            }
                //            else 
                //                inData.Add(c.field, new CrmDataTypeWrapper(c.value, type));
                //        }
                //    }


                //    //inData.Add("ayu_salesengineer", new CrmDataTypeWrapper(lData.Salesengineer, CrmFieldType.Picklist));
                //    //if (lData.Salesengineer.ToString().Length > 0)
                //    //{
                //    //    inData.Add("ayu_salesperson", new CrmDataTypeWrapper(new Guid(lData.Salesengineer), CrmFieldType.Lookup, "systemuser"));
                //    //}
                //    //Guid contactId = crmSvc.CreateNewRecord("contact", inData);
                //    //if (contactId != Guid.Empty)
                //    //{
                //    //    //Console.WriteLine("Account created.");
                //    //    PostedObjectDetail pObject = new PostedObjectDetail();
                //    //    pObject.Id = contactId.ToString();
                //    //    pObject.ObjectName = "Contact";
                //    //    pObject.Message = "Contact added successfully!";
                //    //    return MyAppsDb.ConvertJSONOutput(pObject, HttpStatusCode.OK, false);
                //    //}
                //    //else
                //    //{
                //    //    return MyAppsDb.ConvertJSONOutput("Could not add new contact, check mandatory fields", HttpStatusCode.OK, true);
                //    //}
                //}
                //else
                //{
                //    return MyAppsDb.ConvertJSONOutput("Internal Exception: Dynamics setup is incomplete or login credentials are not right. ", HttpStatusCode.OK, true);
                //}
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyContact-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.OK);
            }
           
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedContacts(string token, string ObjectRef, int GroupId, string SValue, string callback)
        {

            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYContacts-GetSearchedContacts", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            try
            {
                //Connect to SDK 
                //Test system
                //string ApplicationURL = "https://alan365.crm.dynamics.com", userName = "alan@alan365.onmicrosoft.com",
                //    password = "Getthat$$$5", authType = "Office365";
                //Live system
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string sFieldOptional = "";
                //int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);
                int output = MyAppsDb.GetDynamicsCredentialswithCustomSearchFields(ObjectRef, GroupId, "contact", ref ApplicationURL, ref userName, ref password, ref authType, ref sFieldOptional, urlReferrer);

                Uri organizationUri;
                Uri homeRealmUri;
                ClientCredentials credentials = new ClientCredentials();
                ClientCredentials deviceCredentials = new ClientCredentials();
                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
                deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
                organizationUri = new Uri(ApplicationURL + "/XRMServices/2011/Organization.svc");
                homeRealmUri = null;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    List<DYContact> listToReturn = new List<DYContact>();
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    QueryExpression query = new QueryExpression("contact");
                    query.ColumnSet.AddColumns("contactid", "firstname", "lastname");
                    FilterExpression filter1 = new FilterExpression();
                    
                    string[] customSearchArray = sFieldOptional.Split('|');
                    if (sFieldOptional.Length > 0)
                    {
                        foreach (string csA in customSearchArray)
                        {
                            query.ColumnSet.AddColumn(csA);
                        }
                    }
                    //filter name 
                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "fullname";
                    filterOwnRcd.Operator = ConditionOperator.Like;
                    filterOwnRcd.Values.Add("%" + SValue + "%");
                    filter1.Conditions.Add(filterOwnRcd);
                    // get list of custom fields 
                    if (sFieldOptional.Length > 0)
                    {
                        foreach (string csA in customSearchArray)
                        {
                            ConditionExpression filterOwnRcd2 = new ConditionExpression();
                            filterOwnRcd2.AttributeName = csA;
                            filterOwnRcd2.Operator = ConditionOperator.Like;
                            filterOwnRcd2.Values.Add("%" + SValue + "%");
                            filter1.Conditions.Add(filterOwnRcd2);
                        }
                    }
                    filter1.FilterOperator = LogicalOperator.Or;
                    query.Criteria.AddFilter(filter1);
                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {

                        foreach (var z in result1.Entities)
                        {
                            DYContact info = new DYContact();
                            if (z.Attributes.Contains("contactid"))
                            {
                                info.contactid = z.Attributes["contactid"].ToString();
                            }
                            if (z.Attributes.Contains("firstname"))
                            {
                                info.firstname = z.Attributes["firstname"].ToString(); 
                            }
                            if (z.Attributes.Contains("lastname"))
                            {
                                info.lastname = z.Attributes["lastname"].ToString(); 
                            }
                            int noOfcustomItems = 0;
                            if (sFieldOptional.Length > 0)
                            {
                                foreach (string csA in customSearchArray)
                                {
                                    if (z.Attributes.Contains(csA))
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(info, csA, z.Attributes[csA].ToString(), noOfcustomItems);
                                    }
                                    else
                                    {
                                        noOfcustomItems++; // if no value found return empty string 
                                        MyAppsDb.AssignCustomVariableValue(info, csA,"", noOfcustomItems);
                                    }
                                }
                            }
                            listToReturn.Add(info);
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedAccounts", "Unhandled exception", HttpStatusCode.OK);
            }
        }

    }
}
