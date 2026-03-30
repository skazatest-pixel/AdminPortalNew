using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class AuthSchemeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int PriAuthSchCnt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsPrimaryAuthscheme { get; set; }
        public string DisplayName { get; set; }
        public string Hash { get; set; }
        public int SupportsProvisioning { get; set; }

        public IEnumerable<object> NorAuthSchemes { get; set; }

    }
}
