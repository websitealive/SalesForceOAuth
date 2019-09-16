using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Dto
{
    public class ResponceContent
    {
        public string message { get; set; }

        public Engagement engagement { get; set; }

        public Metadata metadata { get; set; }
    }

    public class Engagement
    {
        public string id { get; set; }
        public int portalId { get; set; }
        public bool active { get; set; }
        public long createdAt { get; set; }
        public long lastUpdated { get; set; }
        public int createdBy { get; set; }
        public int modifiedBy { get; set; }
        public int ownerId { get; set; }
        public string type { get; set; }
        public long timestamp { get; set; }
        public List<object> allAccessibleTeamIds { get; set; }
        public string bodyPreview { get; set; }
        public List<object> queueMembershipIds { get; set; }
        public bool bodyPreviewIsTruncated { get; set; }
        public string bodyPreviewHtml { get; set; }
        public bool gdprDeleted { get; set; }
    }
    public class Metadata
    {
        public string body { get; set; }
    }
}
