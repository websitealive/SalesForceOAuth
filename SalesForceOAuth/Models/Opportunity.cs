using SalesForceOAuth.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    public class Opportunity : OrganizationDetail
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string CloseDate { get; set; }

        public string Stage { get; set; }

        public List<CustomFieldModel> CustomFields { get; set; }
    }
}