using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    public class InputFields
    {
        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public string Value { get; set; }
        public string FieldType { get; set; }
        public string RelatedEntity { get; set; }
        public string RelatedEntityFieldName { get; set; }
        public string Table { get; set; }
    }
}