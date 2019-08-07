using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class CrmEntity : OrganizationDetail
    {
        public int Id { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityPrimaryFieldName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
