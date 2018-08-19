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
    public class SFExportFieldsController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetExportFields(string Token, string ObjectRef, int GroupId, string callback, string EntityForm = null)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SF Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                if (string.IsNullOrEmpty(EntityForm))
                {
                    var FieldsList = Repository.GetSFExportFields(ObjectRef, GroupId, urlReferrer);
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }
                else
                {
                    var FieldsList = Repository.GetSFFormExportFields(ObjectRef, GroupId, urlReferrer, EntityForm);
                    return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
                }

            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "SF GetExportFields", "Message", HttpStatusCode.InternalServerError);
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
                return MyAppsDb.ConvertJSONOutput(ex, "SF Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string message = Repository.AddSFExportFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SF Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
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
                return MyAppsDb.ConvertJSONOutput(ex, "SF Export Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ErrorMessage;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MessageResponce retMessage = new MessageResponce();
                retMessage.Success = Repository.DeleteSFExportFields(Id, ObjectRef, urlReferrer, out ErrorMessage);
                retMessage.Error = ErrorMessage;
                return MyAppsDb.ConvertJSONOutput(retMessage, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SF Export Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }
    }
}
