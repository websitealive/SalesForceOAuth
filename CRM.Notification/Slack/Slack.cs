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
            var client = new RestClient("https://slack.com/api");
            var request = new RestRequest("/chat.postMessage", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", "Bearer xoxp-4353695722-631074034307-705854250514-e65d5d86fa970bf200ae0110b3c06eb7");
            request.AddParameter("channel", "CLYLUR6MT");
            request.AddParameter("text", "*Orginization* : " + objectRef + ", *Group ID* : " + groupID + ", *Session ID* : " + sessionID + ", *Object Type* : " + objectType + ", *Object ID* : " + objectID + "\n*Error is:* " + Errormessage);
            client.Execute(request);
        }
    }
}
