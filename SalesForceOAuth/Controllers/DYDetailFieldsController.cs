﻿using SalesForceOAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class DYDetailFieldsController : ApiController
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetDetailFields(string token, string ObjectRef, int GroupId, string callback)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Detail Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                var FieldsList = Repository.GetDYDetailFields(ObjectRef, GroupId, urlReferrer);
                return MyAppsDb.ConvertJSONPOutput(callback, FieldsList, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONPOutput(callback, ex, "Dy GetSearchFields", "Message", HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostDetailFields(ExportFieldsModel ExportFieldData)
        {
            //check payload if a right jwt token is submitted
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(ExportFieldData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Detail Fields", "Your request isn't authorized!", HttpStatusCode.InternalServerError);
            }
            try
            {
                string urlReferrer = Request.RequestUri.Authority.ToString();
                string message = Repository.AddDYDetailFields(ExportFieldData, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "Dy Detail Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteDetailFields(string Token, int Id, string ObjectRef)
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
                string message = Repository.DeleteDYDetailFields(Id, ObjectRef, urlReferrer);
                return MyAppsDb.ConvertJSONOutput(message, HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "DY Detail Fields", "Unable to add Export Fields", HttpStatusCode.InternalServerError);
            }
        }
    }
}