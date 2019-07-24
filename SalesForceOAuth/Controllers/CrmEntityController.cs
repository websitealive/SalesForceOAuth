using CRM.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SalesForceOAuth.Controllers
{
    public class CrmEntityController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage SugarNewEntityCreate(CrmEntity crmEntity)
        {
            return null;
        }
    }
}
