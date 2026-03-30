using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
   

        public class ClientDTO
        {
         
            public int Id { get; set; }

            public string UUID { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }

            public string ApplicationName { get; set; }
        public string ApplicationNameArabic { get; set; }

            public string ApplicationType { get; set; }

            public string ApplicationUrl { get; set; }

            public string RedirectUri { get; set; }

            public string GrantTypes { get; set; }

            public string Scopes { get; set; }

            public string LogoutUri { get; set; }

            public string OrganizationUid { get; set; }

            public int AuthScheme { get; set; }

            public string PublicKeyCert { get; set; }

            public string Profiles { get; set; }

            public string Purposes { get; set; }
        }
}
