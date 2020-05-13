using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesForceOAuth;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel.Description;

namespace SalesForceOAuth.Controllers
{
    public class DYExportFieldsController : ApiController
    {
        //[HttpGet]
        //public async System.Threading.Tasks.Task<HttpResponseMessage> GetOptionSet(string Token, string ObjectRef, int GroupId, string Entity, string ExportField, string callback)
        //{
        //    //check payload if a right jwt token is submitted
        //    string outputPayload;
        //    try
        //    {
        //        outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
        //    }
        //    try
        //    {
        //        var optionSet = GetOptionSetItems(Entity.ToLower(), ExportField, GetServices(ObjectRef, GroupId));
        //        return MyAppsDb.ConvertJSONPOutput(callback, optionSet, HttpStatusCode.OK, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetOptionSet", "Message", HttpStatusCode.InternalServerError);
        //    }

        //}

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetDropDownOption(string Token, string ObjectRef, int GroupId, string Entity, string ExportField, string DataType, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                List<OptionSet> optionSet = GetOptionSetItems(Entity.ToLower(), ExportField, DataType, GetServices(ObjectRef, GroupId));
                

                return MyAppsDb.ConvertJSONPOutput(callback, optionSet, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetOptionSet", "Message", HttpStatusCode.InternalServerError);
            }

        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetExportFields(string Token, string ObjectRef, int GroupId, string callback, bool IsEntityForm = false)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                if (!IsEntityForm)
                {
                    var FieldsList = Repository.GetDYExportFields(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }
                else
                {
                    List<CustomFields> FieldsList = Repository.GetDYFormExportFields(ObjectRef, GroupId, urlReferrer);
                    foreach (var item in FieldsList)
                    {
                        foreach (var fields in item.CustomFieldsList)
                        {
                            if(fields.FieldType == "optionSet" || fields.FieldType == "statusReason")
                            {
                                fields.OptionSetList = GetOptionSetItems(item.Entity, fields.FieldName, fields.FieldType, GetServices(ObjectRef, GroupId));
                            }
                        }
                    }
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetExportFields", "Message", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetExportFieldByID(string Token, string ObjectRef, int FieldId, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Field By Id", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var FieldsList = Repository.GetDYExportFieldsById(FieldId, ObjectRef, urlReferrer);
                return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetExportFields", "Message", HttpStatusCode.InternalServerError);
            }
        }


        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostExportFields(FieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.AddDYExportFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateExportFields(FieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.UpdateDyExportFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }


        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteExportFields(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ErrorMessage;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MessageResponce retMessage = new MessageResponce();
                retMessage.Success = Repository.DeleteDYExportFields(Id, ObjectRef, urlReferrer, out ErrorMessage);
                retMessage.Error = ErrorMessage;
                return MyAppsDb.ConvertJSONOutput(retMessage, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DY Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        private IOrganizationService GetServices(string ObjectRef, int GroupId)
        {
            string ApplicationURL = "", userName = "", password = "", authType = "";
            int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, Request.RequestUri.Authority.ToString());
            Uri organizationUri, homeRealmUri = null;
            ClientCredentials credentials = new ClientCredentials();
            ClientCredentials deviceCredentials = new ClientCredentials();
            credentials.UserName.UserName = userName;
            credentials.UserName.Password = password;
            deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
            deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
            organizationUri = new Uri(ApplicationURL + "/XRMServices/2011/Organization.svc");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials);
            return proxyservice; 
        }

        private List<OptionSet> GetOptionSetItems(string entityName, string attributeName, string dataType,  IOrganizationService service)
        {
            List<OptionSet> optionList2 = new List<OptionSet>();

            // Create the Attribute Request.
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true,
            };

            // Get the Response and MetaData. Then convert to Option MetaData Array.
            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            OptionMetadata[] optionList;
            if (dataType == "optionSet")
            {
                PicklistAttributeMetadata retrievedPicklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionList = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();
            } else
            {
                StatusAttributeMetadata retrievedPicklistAttributeMetadata = (StatusAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionList = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();
            }

            // Add each item in OptionMetadata array to the ListItemCollection object.
            foreach (OptionMetadata o in optionList)
            {
                OptionSet os = new OptionSet();
                if(dataType == "optionSet")
                {
                    os.Label = o.Label.LocalizedLabels[0].Label;
                    os.Value = o.Value.Value.ToString();
                    optionList2.Add(os);
                } else
                {
                    if (((Microsoft.Xrm.Sdk.Metadata.StatusOptionMetadata)o).State == 0)
                    {
                        os.Label = o.Label.LocalizedLabels[0].Label;
                        os.Value = o.Value.Value.ToString();
                        optionList2.Add(os);
                    }
                }
                
            }

            return optionList2;
        }

        private List<OptionSet> GetStatusReasonItems(string entityName, string optionSetAttributeName, IOrganizationService service)
        {
            List<OptionSet> optionList2 = new List<OptionSet>();
            // Create the Attribute Request.
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = optionSetAttributeName,
                RetrieveAsIfPublished = true
            };

            // Get the Response and MetaData. Then convert to Option MetaData Array.
            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            StatusAttributeMetadata retrievedPicklistAttributeMetadata = (StatusAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
            OptionMetadata[] optionList = retrievedPicklistAttributeMetadata.OptionSet.Options.ToArray();

            // Add each item in OptionMetadata array to the ListItemCollection object.
            foreach (OptionMetadata o in optionList)
            {
                OptionSet os = new OptionSet();
                os.Label = o.Label.LocalizedLabels[0].Label;
                os.Value = o.Value.Value.ToString();
                optionList2.Add(os);
            }

            return optionList2;
        }
    }
}
