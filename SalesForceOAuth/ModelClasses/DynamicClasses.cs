using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.ModelClasses
{
    public class EntityClass
    {
        public string Id { get; set; }
        public string EntityName { get; set; }
        public List<EntityColumn> Columns { get; set; }
    }
    public class EntityColumn
    {
        public int Sr { get; set; }
        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public string FieldType { get; set; }
        public string Value { get; set; }
    }
}