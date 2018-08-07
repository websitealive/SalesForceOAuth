using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    // string token, string ObjectRef, int GroupId
    public class DYOrganizationDetail
    {
        public string token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }
    }
}