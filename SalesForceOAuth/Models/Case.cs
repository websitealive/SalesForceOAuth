using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    public class Case : OrganizationDetail
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public LookupField Customer { get; set; }

        public string CustomerName { get; set; }

        public List<CustomFieldModel> CustomFields { get; set; }
    }
}