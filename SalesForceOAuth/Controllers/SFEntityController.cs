using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFEntityController : ApiController
    {
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
    }
}
