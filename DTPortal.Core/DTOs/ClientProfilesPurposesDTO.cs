using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class ClientProfilesPurposesDTO
    {
        public string Scopes { get; set; }
        public string Purposes { get; set; }
        public string ClientId { get; set; }
        public string ApplicationName { get; set; }
        public int Id { get; set; }
    }
}
