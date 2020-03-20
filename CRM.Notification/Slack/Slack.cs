using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Notification
{
    public static class Slack
    {
        public static void SendMessage(string objectRef, int groupID, int sessionID, string objectID, string objectType, string Errormessage)
        {
            // https://hooks.slack.com/services/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
            var urlWithAccessToken = "https://hooks.slack.com/services/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";
            var client = new RestClient(urlWithAccessToken);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            var fields = new Dictionary<string, string>();
            fields.Add("text", "Orginization : " + objectRef + ", Group ID : " + groupID + ", Session ID : " + sessionID + ", Object Type : " + objectType + ", Object ID : " + objectID + "\n*Error is:* " + Errormessage);
            request.AddJsonBody(fields);
            var response = client.Execute(request);
        }
        public static void TestMessage(string TestMessage)
        {
            var urlWithAccessToken = "https://hooks.slack.com/services/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";
            var client = new RestClient(urlWithAccessToken);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            var fields = new Dictionary<string, string>();
            fields.Add("text", "Testing  : " + TestMessage);
            request.AddJsonBody(fields);
            var response = client.Execute(request);
        }
    }
}
