using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class IntegrationConstants
    {
        public int id { get; set; }

        public string ClientId { get; set; }

        public string SecretKey { get; set; }

        public string RedirectedUrl { get; set; }

        public string AuthorizationUrl { get; set; }

        public string ApiUrl { get; set; }

        public CrmType CrmType { get; set; }

        public AppType AppType { get; set; }
    }
}
