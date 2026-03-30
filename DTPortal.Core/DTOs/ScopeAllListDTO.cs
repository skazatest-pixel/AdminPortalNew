using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class ScopeAllListDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool UserConsent { get; set; }

        public bool DefaultScope { get; set; }

        public bool MetadataPublish { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public string Status { get; set; }

        public string ClaimsList { get; set; }

        public bool IsClaimsPresent { get; set; }

        public bool SaveConsent { get; set; }

        public string Version { get; set; }

        public string DisplayNameArabic { get; set; }


    }
}
