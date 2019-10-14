using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    public class EntityModel : DYOrganizationDetail
    {
        public int ID { get; set; }

        public string EntityPrimaryKey { get; set; }

        public string EntityUniqueName { get; set; }

        public string EntityDispalyName { get; set; }

        public string PrimaryFieldUniqueName { get; set; }

        public string PrimaryFieldDisplayName { get; set; }

        public string PrimaryFieldValue { get; set; }

        public string OptionalFieldDisplayName { get; set; }

        public string OptionalFieldValue { get; set; }

        public int AllowRecordSearch { get; set; }

        public int AllowRecordCreation { get; set; }

        public string CrmType { get; set; }

        public List<CustomFieldModel> CustomFields { get; set; }

    }
}