using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SFChatController : ApiController
    {
        [HttpPost]
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostAddMessage(MessageData lData)
        {
            HttpResponseMessage outputResponse = new HttpResponseMessage(); 
            if (lData.ValidationKey == ConfigurationManager.AppSettings["APISecureKey"])
            {
                try
                {
                    ForceClient client = new ForceClient(lData.InstanceUrl, lData.AccessToken, lData.ApiVersion);
                    TaskLogACall lTemp = new TaskLogACall();
                    lTemp.Subject = "WebsiteAlive-Chat1";
                    lTemp.Description = lData.Messsage;
                    if (lData.ItemType == "Lead" || lData.ItemType == "Contact")
                        lTemp.WhoId = lData.ItemId;
                    else
                        lTemp.WhatId = lData.ItemId;
                    lTemp.Status = "Completed"; 
                    var lACall = lTemp ;
                    SuccessResponse sR = await client.CreateAsync("Task", lACall);
                    if (sR.Success == true)
                    {
                        outputResponse.StatusCode = HttpStatusCode.Created;
                        outputResponse.Content = new StringContent( lData.ItemType + " chat added successfully!"); 
                        return outputResponse;
                    } 
                    else
                    {
                        outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                        outputResponse.Content = new StringContent(lData.ItemType + " chat could not be added!");
                        return outputResponse;
                    }
                }
                catch (Exception ex)
                {
                    outputResponse.StatusCode = HttpStatusCode.InternalServerError;
                    outputResponse.Content = new StringContent(lData.ItemType + " chat could not be added!");
                    return outputResponse;
                }
            }
            outputResponse.StatusCode = HttpStatusCode.Unauthorized;
            outputResponse.Content = new StringContent("Your request isn't authorized!");
            return outputResponse;
        }

    }
    public class TaskLogACall
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string WhoId { get; set; }
        public string WhatId { get; set; }
        public string Status { get; set; }
    }
    public class MessageData:SecureInfo
    {
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public string Messsage { get; set; }

    }
  
}
