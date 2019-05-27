using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using MySql.Data.MySqlClient;
using SalesForceOAuth.Web_API_Helper_Code;
using Microsoft.Xrm.Sdk.Query;
using System.Dynamic;
using SalesForceOAuth.Models;

namespace SalesForceOAuth.Controllers
{
    public class SFChatLiveFiveController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageDataCopy lData)
        {
            string outputPayload;
            try
            {
                outputPayload = JWT.JsonWebToken.Decode(lData.token, ConfigurationManager.AppSettings["APISecureKey"], true);
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput(ex, "SfChats-PostChats", "Your request isn't authorized!", HttpStatusCode.OK);
            }
            string AccessToken = "";
            string urlReferrer = Request.RequestUri.Authority.ToString();
            //Access token update
            try
            {
                HttpResponseMessage msg = await Web_API_Helper_Code.Salesforce.GetAccessToken(lData.ObjectRef, lData.GroupId, System.Web.HttpUtility.UrlDecode(lData.siteRef), urlReferrer);
                if (msg.StatusCode != HttpStatusCode.OK)
                { return MyAppsDb.ConvertJSONOutput(msg.Content.ReadAsStringAsync().Result, msg.StatusCode, true); }
            }
            catch (Exception eee)
            {
                return MyAppsDb.ConvertJSONOutput("--Internal Exception: " + eee.Message, HttpStatusCode.OK, true);
            }
            try
            {
                string InstanceUrl = "", ApiVersion = "";
                string ChatId, RowId;
                string HeadingSms = " App: " + lData.App + " |  Name: " + lData.OwnerName + " |  E-mail: " + lData.OwnerEmail + " |  Phone Number: " + lData.OwnerPhone + " | | Chat Content |";
                MyAppsDb.GetAPICredentials(lData.ObjectRef, lData.GroupId, ref AccessToken, ref ApiVersion, ref InstanceUrl, urlReferrer);
                bool flag = Repository.IsChatExist(lData.EntitytId, lData.EntitytType, lData.App, lData.ObjectRef, urlReferrer, out ChatId, out RowId);
                ForceClient client = new ForceClient(InstanceUrl, AccessToken, ApiVersion);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                PostedObjectDetail output = new PostedObjectDetail();
                SuccessResponse sR;
                dynamic lTemp = new ExpandoObject();
                lTemp.Subject = lData.Subject;
                lTemp.Description = HeadingSms.Replace("|", "\r\n") + lData.Message.Replace("|", "\r\n").Replace("&#39;", "'");
                lTemp.Status = "Completed";
                if (lData.EntitytType.ToLower() == "lead" || lData.EntitytType.ToLower() == "contact") lTemp.WhoId = lData.EntitytId; else lTemp.WhatId = lData.EntitytId;

                // Get Back End Fields and create object for update
                var getBackEndFeields = Repository.GetSFBackEndFields(lData.ObjectRef, lData.GroupId, urlReferrer, lData.EntitytType.ToLower());
                dynamic UpdateRecord = new ExpandoObject();
                foreach (var item in getBackEndFeields)
                {
                    MyAppsDb.AddProperty(UpdateRecord, item.FieldName, item.ValueDetail);
                }

                if (string.IsNullOrEmpty(ChatId))
                {
                    sR = await client.CreateAsync("Task", lTemp).ConfigureAwait(false);
                    if (getBackEndFeields.Count > 0)
                    {
                        await client.UpdateAsync(lData.EntitytType, lData.EntitytId, UpdateRecord);
                    }
                    output.Id = sR.Id;
                    output.ObjectName = "Chat";
                    output.Message = "Chat added successfully!";
                    Repository.AddChatInfo(lData.ObjectRef, urlReferrer, "SaleForce", lData.EntitytId, lData.EntitytType, lData.App, sR.Id.ToString());
                }
                else
                {
                    try
                    {
                        sR = await client.UpdateAsync("Task", ChatId, lTemp).ConfigureAwait(false);
                        if (getBackEndFeields.Count > 0)
                        {
                            await client.UpdateAsync(lData.EntitytType, lData.EntitytId, UpdateRecord);
                        }
                        output.Id = sR.Id;
                        output.ObjectName = "Chat";
                        output.Message = "Chat updated successfully!";
                    }
                    catch (Exception)
                    {
                        sR = await client.CreateAsync("Task", lTemp).ConfigureAwait(false);
                        if (getBackEndFeields.Count > 0)
                        {
                            await client.UpdateAsync(lData.EntitytType, lData.EntitytId, UpdateRecord);
                        }
                        output.Id = sR.Id;
                        output.ObjectName = "Chat";
                        output.Message = "Chat added successfully!";
                        Repository.DeleteChatInfo(lData.ObjectRef, urlReferrer, RowId);
                        Repository.AddChatInfo(lData.ObjectRef, urlReferrer, "SaleForce", lData.EntitytId, lData.EntitytType, lData.App, sR.Id.ToString());
                    }

                }

                if (sR.Success == true)
                {
                    return MyAppsDb.ConvertJSONOutput(output, HttpStatusCode.OK, false);
                }
                else
                {
                    return MyAppsDb.ConvertJSONOutput("SalesForce Error: " + sR.Errors, HttpStatusCode.OK, true);
                }
            }
            catch (Exception ex)
            {
                return MyAppsDb.ConvertJSONOutput("Internal Exception: " + ex.Message, HttpStatusCode.OK, true);
            }
        }

    }
}
