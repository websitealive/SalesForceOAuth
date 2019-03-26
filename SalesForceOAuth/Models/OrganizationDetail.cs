using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    public class OrganizationDetail
    {
        public string Token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }

        public string SiteRef { get; set; }

        public string OwnerEmail { get; set; }
    }
}