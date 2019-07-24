using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class CRMUser : OrganizationDetail
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ApiUrl { get; set; }
        public string UrlReferrer { get; set; }
        public string AuthCode { get; set; }
        public OuthDetail OuthDetail { get; set; }
        public IntegrationConstants IntegrationConstants { get; set; }
    }
}
