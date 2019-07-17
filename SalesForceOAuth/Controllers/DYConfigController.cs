using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SalesForceOAuth.Web_API_Helper_Code;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace SalesForceOAuth.Controllers
{
    public sealed class ManagedTokenOrganizationServiceProxy : OrganizationServiceProxy
    {
        private AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService> _proxyManager;

        public ManagedTokenOrganizationServiceProxy(Uri serviceUri, ClientCredentials userCredentials)
            : base(serviceUri, null, userCredentials, null)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement,
            SecurityTokenResponse securityTokenRes)
            : base(serviceManagement, securityTokenRes)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement,
            ClientCredentials userCredentials)
            : base(serviceManagement, userCredentials)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        protected override void AuthenticateCore()
        {
            this._proxyManager.PrepareCredentials();
            base.AuthenticateCore();
        }

        protected override void ValidateAuthentication()
        {
            this._proxyManager.RenewTokenIfRequired();
            base.ValidateAuthentication();
        }
    }

    ///
    /// Class that wraps acquiring the security token for a service
    /// </summary>

    public sealed class AutoRefreshSecurityToken<TProxy, TService>
        where TProxy : ServiceProxy<TService>
        where TService : class
    {
        private TProxy _proxy;

        ///
        /// Instantiates an instance of the proxy class
        /// </summary>

        /// <param name="proxy">Proxy that will be used to authenticate the user</param>
        public AutoRefreshSecurityToken(TProxy proxy)
        {
            if (null == proxy)
            {
                throw new ArgumentNullException("proxy");
            }

            this._proxy = proxy;
        }

        ///
        /// Prepares authentication before authenticated
        /// </summary>

        public void PrepareCredentials()
        {
            if (null == this._proxy.ClientCredentials)
            {
                return;
            }

            switch (this._proxy.ServiceConfiguration.AuthenticationType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    this._proxy.ClientCredentials.UserName.UserName = null;
                    this._proxy.ClientCredentials.UserName.Password = null;
                    break;
                case AuthenticationProviderType.Federation:
                case AuthenticationProviderType.LiveId:
                    this._proxy.ClientCredentials.Windows.ClientCredential = null;
                    break;
                default:
                    return;
            }
        }

        ///
        /// Renews the token (if it is near expiration or has expired)
        /// </summary>

        public void RenewTokenIfRequired()
        {
            if (null != this._proxy.SecurityTokenResponse &&
            DateTime.UtcNow.AddMinutes(15) >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
            {
                try
                {
                    this._proxy.Authenticate();
                }
                catch (CommunicationException)
                {
                    if (null == this._proxy.SecurityTokenResponse ||
                        DateTime.UtcNow >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
                    {
                        throw;
                    }

                    // Ignore the exception 
                }
            }
        }
    }

    public class DYConfigController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetConfigurationStatus(string token, string ObjectRef, int GroupId, string callback)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DYConfig-GetConfigurationStatus", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
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
                int output = MyAppsDb.GetDynamicsCredentials(ObjectRef, GroupId, ref ApplicationURL, ref userName, ref password, ref authType, urlReferrer);

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
                    //ConditionExpression password = new ConditionExpression();

                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "uniquename";
                    filterOwnRcd.Operator = ConditionOperator.Equal;
                    filterOwnRcd.Values.Add(ConfigurationManager.AppSettings["DynamicsManagedSolName"].ToString());

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);


                    QueryExpression query = new QueryExpression("solution");
                    query.ColumnSet.AddColumns("solutionid", "friendlyname", "version", "ismanaged", "uniquename");
                    query.Criteria.AddFilter(filter1);

                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {
                        string outStr = "Managed Solution Found - Configuration Complete";
                        return MyAppsDb.ConvertJSONPOutput(callback, outStr, HttpStatusCode.OK, false);
                    }
                    else
                    {
                        return MyAppsDb.ConvertJSONPOutput(callback, "Managed solution Not Found, Actitiy Task are use Instead - Configuration Complete", HttpStatusCode.OK, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DYConfig-GetConfigurationStatus", "Unhandled exception", HttpStatusCode.InternalServerError, false);
            }

        }

        [HttpPost]
        public HttpResponseMessage PostCredentials(DynamicUser lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostCredentials", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                Uri organizationUri;
                Uri homeRealmUri;
                ClientCredentials credentials = new ClientCredentials();
                ClientCredentials deviceCredentials = new ClientCredentials();
                credentials.UserName.UserName = lData.Username;
                credentials.UserName.Password = lData.Password;
                deviceCredentials.UserName.UserName = ConfigurationManager.AppSettings["dusername"];
                deviceCredentials.UserName.Password = ConfigurationManager.AppSettings["duserid"];
                organizationUri = new Uri(lData.OrganizationURL + "/XRMServices/2011/Organization.svc");
                homeRealmUri = null;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // testing Start

                //IServiceManagement<IOrganizationService> management = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(organizationUri);

                ////ClientCredentials credentials = new ClientCredentials();
                ////credentials.UserName.UserName = lData.Username;
                ////credentials.UserName.Password = lData.Password;

                //// OrganizationServiceProxy _serviceproxy = new OrganizationServiceProxy(management, credentials);

                //AuthenticationCredentials authCredentials = management.Authenticate(new AuthenticationCredentials { ClientCredentials = credentials });
                //SecurityTokenResponse securityTokenResponse = authCredentials.SecurityTokenResponse;

                //ManagedTokenOrganizationServiceProxy _serviceproxy = new ManagedTokenOrganizationServiceProxy(management, securityTokenResponse);

                using (OrganizationServiceProxy proxyservice = new OrganizationServiceProxy(organizationUri, homeRealmUri, credentials, deviceCredentials))
                {
                    IOrganizationService objser = (IOrganizationService)proxyservice;
                    //ConditionExpression password = new ConditionExpression();

                    ConditionExpression filterOwnRcd = new ConditionExpression();
                    filterOwnRcd.AttributeName = "uniquename";
                    filterOwnRcd.Operator = ConditionOperator.Equal;
                    filterOwnRcd.Values.Add(ConfigurationManager.AppSettings["DynamicsManagedSolName"].ToString());

                    FilterExpression filter1 = new FilterExpression();
                    filter1.Conditions.Add(filterOwnRcd);


                    QueryExpression query = new QueryExpression("solution");
                    query.ColumnSet.AddColumns("solutionid", "friendlyname", "version", "ismanaged", "uniquename");
                    query.Criteria.AddFilter(filter1);
                    EntityCollection result1 = objser.RetrieveMultiple(query);
                    if (result1.Entities.Count > 0)
                    {
                        Repository.RecordDynamicsSettings(lData.ObjectRef, lData.GroupId, 1, urlReferrer);
                    }
                    else
                    {
                        Repository.RecordDynamicsSettings(lData.ObjectRef, lData.GroupId, 0, urlReferrer);
                    }
                    int output = MyAppsDb.RecordDynamicsCredentials(lData.ObjectRef, lData.GroupId, lData.OrganizationURL, lData.Username, lData.Password, lData.AuthType, urlReferrer);
                    if (output == 1)
                        return MyAppsDb.ConvertJSONOutput("Credentials recorded successfully!", HttpStatusCode.OK, false);
                    else
                        return MyAppsDb.ConvertJSONOutput("Credentials exists and working.", HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DyLead-PostLead", "Unhandled exception", HttpStatusCode.InternalServerError);
            }
        }
    }
}
