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
    public class SFEntityController : ApiController
    {
        #region Dynamics Added Entity

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityList(string Token, string ObjectRef, int GroupId, string callback)
        {

            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFEntity-GetEntityList", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                var entities = Repository.GetEntityList(urlReferrer, ObjectRef, GroupId, "sf");
                var entityFields = Repository.GetSFFormExportFields(ObjectRef, GroupId, urlReferrer);
                foreach (var entity in entities)
                {
                    foreach (var fields in entityFields)
                    {
                        if (entity.EntityUniqueName.ToLower() == fields.Entity.ToLower())
                        {
                            entity.CustomFields = fields.CustomFieldsList;
                        }
                    }

                }
                return MyAppsDb.ConvertJSONPOutput(callback, entities, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFEntity-GetEntityList", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntityById(string Token, string ObjectRef, int EntityId, string callback)
        {

            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFEntity-GetEntityById", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                var entitySettings = Repository.GetEntityById(urlReferrer, ObjectRef, EntityId);
                return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFEntity-GetEntityById", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntity(EntityModel lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                lData.CrmType = "sf";
                var messgae = Repository.AddEntity(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateEntity(EntityModel lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateEntity(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteEntity(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.DeleteEntity(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-DeleteEntity", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntityRecord(EntityModel lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-PostEntity", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.SiteRef), urlReferrer);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false);
            }
            try
            {
                string InstanceUrl = "", AccessToken = "", ApiVersion = "";
                MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                SuccessResponse sR;
                dynamic newEntity = new ExpandoObject();

                #region Dynamic Inout Fields
                if (lData.CustomFields != null)
                {
                    foreach (CustomFieldModel inputField in lData.CustomFields)
                    {
                        if (inputField.Value != null)
                        {
                            MyAppsDb.AddProperty(newEntity, inputField.FieldName, inputField.Value, inputField.FieldType);
                        }

                    }
                }
                #endregion

                //if (lData.CustomFields != null)
                //{
                //    foreach (CustomObject c in lData.CustomFields)
                //    {
                //        MyAppsDb.AddProperty(newAccount, c.field, c.value);
                //    }
                //}
                sR = await client.CreateAsync(lData.EntityUniqueName, newEntity).ConfigureAwait(false);
                if (sR.Success == true)
                {
                    PostedObjectDetail output = new PostedObjectDetail();
                    output.Id = sR.Id;
                    output.ObjectName = lData.PrimaryFieldUniqueName;
                    output.Message = lData.PrimaryFieldUniqueName + " added successfully!";
                    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.Conflict, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFAccount-PostAccount", "Unhandled exception", HttpStatusCode.Conflict);
            }
            //}
            //return MyAppsDb.ConvertJSONOutput("Your request isn't authorized!", HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetEntitySettings(string Token, string ObjectRef, int GroupId, string callback, int Id = 0)
        {
            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccounts-GetSearchedAccounts", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                if (Id == 0)
                {
                    var entitySettings = Repository.GetSfEntitySettings(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
                else
                {
                    var entitySettings = Repository.GetSfEntitySettingsById(ObjectRef, Id, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYAccount-GetSearchedEntities", "Unhandled exception", HttpStatusCode.Conflict);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostEntitySettings(EntitySettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-PostEntiySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.AddSfEntitySettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateEntitySettings(EntitySettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyAccount-UpdateEntitySettings", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateSfEntitySettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteEntitySettings(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Detail Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.DeleteSfEntitySettings(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
                //
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DY Detail Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetSearchedEntities(string Token, string ObjectRef, int GroupId, string SiteRef, string Entity, string SValue, string callback, bool IslookupSearch = false, string ExportFieldId = null)
        {
            string InstanceUrl = "", AccessToken = "", ApiVersion = "";
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLeads-GetSearchedLeads", "Your request isn't authorized!", HttpStatusCode.Conflict, false);
            }
            //Access token update
            string urlReferrer = Request.RequestUri.Authority.ToString();
            HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(ObjectRef, GroupId, System.Web.HttpUtility.UrlDecode(SiteRef), urlReferrer);

            if (msg.StatusCode != HttpStatusCode.OK)
            {
                return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, false);
            }
            try
            {
                string searchEntity = "", lookupFieldLabel = "", lookupFieldName = "";

                List<EntityModel> listToReturn = new List<EntityModel>();
                FieldsModel getExportFieldForLookup = new FieldsModel();

                if (IslookupSearch)
                {
                    if (ExportFieldId != null)
                    {
                        getExportFieldForLookup = Repository.GetSFExportFieldsForLookup(ObjectRef, ExportFieldId, urlReferrer);
                        searchEntity = getExportFieldForLookup.RelatedEntity;
                        //lookupFieldLabel = getExportFieldForLookup.OptionalFieldsLabel;
                        //lookupFieldName = getExportFieldForLookup.OptionalFieldsName;
                    }
                    searchEntity = Entity;
                }

                string cSearchField = "";
                string cSearchFieldLabels = "";
                //Below line 1st checks credentials if OK then returns list of custom search fields if any
                MyAppsDb.GetAPICredentialswithCustomSearchFields(ObjectRef, GroupId, Entity, ref AccessToken, ref ApiVersion, ref InstanceUrl, ref cSearchField, ref cSearchFieldLabels, urlReferrer);

                //Below line Get a list of custom entities if any
                EntityModel dynamicEntity = Repository.GetEntity(urlReferrer, ObjectRef, GroupId, Entity, "sf");
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                string objectValue = SValue;
                StringBuilder query = new StringBuilder();
                StringBuilder columns = new StringBuilder();
                StringBuilder filters = new StringBuilder();
                string[] customSearchFieldArray = cSearchField.Split('|');
                string[] customSearchLabelArray = cSearchFieldLabels.Split('|');
                //If custom search fields exist on admin side
                if (cSearchField.Length > 0)
                {
                    foreach (string csA in customSearchFieldArray)
                    {
                        columns.Append("," + csA);
                        filters.Append("OR " + csA + " like '%" + SValue.Trim() + "%' ");
                    }
                }
                if (IslookupSearch)
                {
                    query.Append("SELECT Id, name From " + Entity + " where Name like '%" + SValue.Trim() + "%' ");
                }
                else
                {
                    query.Append("SELECT Id, " + dynamicEntity.PrimaryFieldUniqueName + " " + columns.ToString() + " From " + Entity);
                    query.Append(" where Name like '%" + SValue.Trim() + "%' ");
                    query.Append(filters.ToString());
                }
                //Id, FirstName, LastName, Company, Email, Phone

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                QueryResult<dynamic> cont = await client.QueryAsync<dynamic>(query.ToString()).ConfigureAwait(false);
                if (cont.Records.Count > 0)
                {
                    foreach (dynamic c in cont.Records)
                    {
                        EntityModel l = new EntityModel();
                        l.EntityPrimaryKey = c.Id;
                        l.EntityDispalyName = c.Name;
                        l.EntityUniqueName = Entity;
                        if (!IslookupSearch)
                        {
                            var chk = dynamicEntity.PrimaryFieldUniqueName;
                            l.PrimaryFieldValue = c[chk];
                        }
                        if (cSearchField.Length > 0)
                        {
                            int noOfcustomItems = 0; int i = 0;
                            foreach (Newtonsoft.Json.Linq.JProperty item in c)
                            {

                                foreach (string csA in customSearchFieldArray)
                                {
                                    if (item.Name.ToLower() == csA.ToLower())
                                    {
                                        //code to add to custom list
                                        noOfcustomItems++;
                                        MyAppsDb.AssignCustomVariableValue(l, customSearchLabelArray[i], item.Value.ToString(), noOfcustomItems);
                                        i++;
                                    }

                                }
                            }
                        }
                        listToReturn.Add(l);
                    }
                }
                return MyAppsDb.ConvertJSONPOutput(callback, listToReturn, HttpStatusCode.OK, false);

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFLead-GetSearchedEntity", "Unhandled exception", HttpStatusCode.Conflict, false);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetDefaultFieldSettings(string Token, string ObjectRef, int GroupId, string callback, int Id = 0)
        {
            string urlReferrer = Request.RequestUri.Authority.ToString();
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                if (Id == 0)
                {
                    var entitySettings = Repository.GetSFDefaultFieldSettings(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
                else
                {
                    var entitySettings = Repository.GetSFDefaultFieldSettingsById(ObjectRef, Id, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, entitySettings, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SFEntity-GetDefaultFieldSettings", "Exception", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostDefaultFieldSettings(DefaultFieldSettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.AddSFDefaultFieldSettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-PostDefaultFieldSettings", "Exception!", HttpStatusCode.InternalServerError);
            }

        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateDefaultFieldSettings(DefaultFieldSettings lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var messgae = Repository.UpdateSFDefaultFieldSettings(lData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(messgae, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFENtity-UpdateDefaultFieldSettings", "Exception!", HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteDefaultFieldSettings(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var message = Repository.DeleteSFDefaultFieldSettings(ObjectRef, urlReferrer, Id);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SFEntity-DeleteDefaultFieldSettings", "Exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}
