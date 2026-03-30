using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;

namespace DTPortal.Core.DTOs
{
    public class ClientResponseDTO
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ApplicationName { get; set; }
        public string ApplicationNameArabic { get; set; }

        public string ApplicationType { get; set; }

        public string ApplicationUrl { get; set; }

        public string RedirectUri { get; set; }

        public string LogoutUri { get; set; }

        public string GrantTypes { get; set; }

        public string Scopes { get; set; }

        public string Status { get; set; }

        public string OrganizationUid { get; set; }

        public int? AuthScheme { get; set; }

        public bool? WithPkce { get; set; }

        public string PublicKeyCert { get; set; }



        public string UpdatedBy { get; set; }

        public string ResponseTypes { get; set; }

        public string EncryptionCert { get; set; }


        public List<EConsentClient> EConsentClients { get; set; }
        public List<ClientsSaml2> ClientsSaml2s { get; set; } 



    }
}
