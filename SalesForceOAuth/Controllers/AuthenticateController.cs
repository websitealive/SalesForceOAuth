using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class SugarUser
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class AuthenticateController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage PostSugarCreditionals(SugarUser obj)
        {
            var httpClient = new HttpClient();
            var httpContent = new HttpRequestMessage(HttpMethod.Post, "https://zensyi0756.trial.sugarcrm.com/rest/v10/oauth2/token");
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "password"));
            values.Add(new KeyValuePair<string, string>("client_id", "sugar"));
            values.Add(new KeyValuePair<string, string>("client_secret", ""));
            values.Add(new KeyValuePair<string, string>("username", "admin"));
            values.Add(new KeyValuePair<string, string>("password", "asdf"));
            values.Add(new KeyValuePair<string, string>("platform", "base"));
            httpContent.Content = new FormUrlEncodedContent(values);
            Task<HttpResponseMessage> response = httpClient.SendAsync(httpContent);
            var result = response.Result;
            return result;
            return MyAppsDb.ConvertJSONOutput("Credentials exists and working.", HttpStatusCode.OK, false);
        }
    }
}
