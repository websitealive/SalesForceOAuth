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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using SalesForceOAuth.BusinessLogic;
using System.Threading.Tasks;
using SalesForceOAuth.ModelClasses;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SalesForceOAuth.Models;

namespace SalesForceOAuth.Controllers
{
    public class DYDetailedController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> GetDetailView(string token, string ObjectRef, int GroupId, string callback, string entity, string refId)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DyLeads-GetSearchedLeads", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ApplicationURL = "", userName = "", password = "", authType = "";
                string urlReferrer = Request.RequestUri.Authority.ToString();
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

                EntityModel primaryEntityColumn = Repository.GetEntity(urlReferrer, ObjectRef, GroupId, entity, "dy");
                List<EntityColumn> retEntityColumn = BusinessLogic.DynamicCommon.GetDynamicDetailFileds(ObjectRef, GroupId, entity, urlReferrer);
                if (primaryEntityColumn.PrimaryFieldDisplayName != null && primaryEntityColumn.PrimaryFieldUniqueName != null)
                {
                    retEntityColumn.Add(new EntityColumn { FieldLabel = primaryEntityColumn.PrimaryFieldDisplayName, FieldName = primaryEntityColumn.PrimaryFieldUniqueName });
                }
                ClientCredentials credentials = new ClientCredentials();
                ClientCredentials deviceCredentials = new ClientCredentials();

                Uri organizationUri = new Uri(ApplicationURL + "/XRMServices/2011/Organization.svc");
                Uri homeRealmUri = null;

                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
                deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    EntityClass retRecord = new EntityClass();
                    IOrganizationService objser = (IOrganizationService)proxyservice;

                    ColumnSet entityColumn = new ColumnSet();
                    if (retEntityColumn.Count > 0)
                    {
                        foreach (var field in retEntityColumn)
                        {
                            entityColumn.AddColumn(field.FieldName);
                        }

                    }

                    Entity result1 = objser.Retrieve(entity.ToLower(), new Guid(refId), entityColumn);
                    if (result1.Id != Guid.Empty)
                    {
                        if (retEntityColumn.Count > 0)
                        {
                            retRecord.Id = result1.Id.ToString();
                            retRecord.EntityName = entity;
                            foreach (var item in retEntityColumn)
                            {
                                var fieldValue = result1.Attributes
                                            .Where(x => x.Key == item.FieldName)
                                            .Select(y => y.Value).FirstOrDefault();
                                if (fieldValue != null)
                                {
                                    if (fieldValue.ToString() != "Microsoft.Xrm.Sdk.EntityReference")
                                    {
                                        retEntityColumn.Where(x => x.FieldLabel == item.FieldLabel).ToList().ForEach(s => s.Value = fieldValue.ToString());
                                    }
                                    else
                                    {
                                        retEntityColumn.Where(x => x.FieldLabel == item.FieldLabel).ToList().ForEach(s => s.Value = ((Microsoft.Xrm.Sdk.EntityReference)fieldValue).Name.ToString());
                                    }

                                }
                            }
                        }
                    }

                    retRecord.Columns = retEntityColumn.OrderBy(x => x.Sr).ToList();
                    return MyAppsDb.ConvertJSONPOutput(callback, retRecord, HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYLead-GetSearchedLeads", "Unhandled exception", HttpStatusCode.InternalServerError);

            }
        }
    }
}
