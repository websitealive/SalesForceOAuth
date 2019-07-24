using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class OrganizationDetail
    {
        public string Token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }

        public string SiteRef { get; set; }

        public string OwnerEmail { get; set; }

        public CrmType CrmType { get; set; }
    }
}
