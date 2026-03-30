using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class LicenseInfoDTO
    {
        public int Id { get; set; }
        public string Ouid { get; set; }
        public string AppId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public string LicenseInfo { get; set; }
        public DateTime? IssuedOn { get; set; }
        public DateTime? ValidUpTo { get; set; }
        public string LicenseType { get; set; }
        public DateTime? LastActivated { get; set; }
        public DateTime? FirstActivated { get; set; }
        public string LicenseStatus { get; set; }
        public string ApplicationName { get; set; }
        public string OrganizationName { get; set; }
    }
}
