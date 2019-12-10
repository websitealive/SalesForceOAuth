using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class EntityFieldsMetaData
    {
        public int Sr { get; set; }
        public string Id { get; set; }
        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public string Value { get; set; }
        public string FieldType { get; set; }
        public int BusinessRequired { get; set; }
        public int MaxLength { get; set; }
        public string RelatedEntity { get; set; }
        public string RelatedEntityFieldName { get; set; }
        public string Table { get; set; }
    }
}
