using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class CrmEntity : OrganizationDetail
    {
        //public int Id { get; set; }
        //public string EntityId { get; set; }
        //public string EntityName { get; set; }
        //public string EntityPrimaryFieldName { get; set; }
        //public string SubUrl { get; set; }
        ////public string FirstName { get; set; }
        ////public string LastName { get; set; }
        ////public string Email { get; set; }
        //public string Message { get; set; }
        //public List<EntityFieldsMetaData> CustomFields { get; set; }

        public int ID { get; set; }
        public string EntityId { get; set; }
        public string EntityUniqueName { get; set; }
        public string EntityDispalyName { get; set; }
        public string EntityPrimaryKey { get; set; }
        public string EntityPrimaryKeyValue { get; set; }
        public string PrimaryFieldUniqueName { get; set; }
        public string PrimaryFieldDisplayName { get; set; }
        public string PrimaryFieldValue { get; set; }
        public string OptionalFieldDisplayName { get; set; }
        public string OptionalFieldValue { get; set; }
        public int AllowRecordSearch { get; set; }
        public int AllowRecordCreation { get; set; }
        public string SubUrl { get; set; }
        public string Message { get; set; }
        public List<EntityFieldsMetaData> CustomFields { get; set; }
    }
}
