using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using SalesForceOAuth.Models;
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
    public class DYBackEndFieldsController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetBackEndFields(string Token, string ObjectRef, int GroupId, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF Detail Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var FieldsList = Repository.GetDYBackEndFields(ObjectRef, GroupId, urlReferrer);
                return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DF GetBackEndFields", "Message", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetBackEndFieldsById(string Token, string ObjectRef, int FieldId, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var FieldsList = Repository.GetDYBackEndFieldsById(FieldId, ObjectRef, urlReferrer);
                return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "DF GetBackEndFields", "Message", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostBackEndFields(FieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string message = Repository.AddDYBackEndFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateBackEndFields(FieldsModel BackEndFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(BackEndFieldData.Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Update Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string message = Repository.UpdateDYBackEndFields(BackEndFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Update Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteBackEndFields(string Token, int Id, string ObjectRef)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(Token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF Back End Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string ErrorMessage;
                string urlReferrer = Request.RequestUri.Authority.ToString();
                MessageResponce retMessage = new MessageResponce();
                retMessage.Success = Repository.DeleteDYBackEndFields(Id, ObjectRef, urlReferrer, out ErrorMessage);
                retMessage.Error = ErrorMessage;
                return MyAppsDb.ConvertJSONOutput(retMessage, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DF BackEnd Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }
    }
}