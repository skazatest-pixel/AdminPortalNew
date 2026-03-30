using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Models
{
    public class ApprovedScopes
    {
        public string scope { get; set; }
        public bool permission { get; set;}
        public string version { get; set; }
        public string created_date { get; set; }
        public List<string> attributes { get; set; }
    }
    public class Scopes
    {
        public IList<ApprovedScopes> approved_scopes { get; set; }
    }
}
