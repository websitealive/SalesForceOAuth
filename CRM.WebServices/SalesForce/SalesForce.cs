using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.WebServices.SalesForce
{
    class SalesForce
    {
        //public static List<string> GetPickListItem(string sValue)
        //{
        //    List<string> retEntityRecord = new List<string>();
        //    var client = new RestClient("apiURL");
        //    var request = new RestRequest("/contacts/v1/contact/email/" + sValue + "/profile?", Method.GET);
        //    request.AddHeader("Content-Type", "application/json");
        //    request.AddHeader("Authorization", "Bearer " + "acc_token");
        //    var response = client.Execute(request);

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        RootObject contact = JsonConvert.DeserializeObject<RootObject>(response.Content);

        //        return retEntityRecord;
        //    }
        //    else
        //    {
        //        return retEntityRecord;
        //    }
        //}
    }
}
