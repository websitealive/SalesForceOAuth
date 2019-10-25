using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class OuthDetail
    {
        public bool Is_Authenticated { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string error { get; set; }
        public string error_message { get; set; }
        public string localendpoint { get; set; }
        public double expires_in { get; set; }
        public string expires_on { get; set; }
    }
}
