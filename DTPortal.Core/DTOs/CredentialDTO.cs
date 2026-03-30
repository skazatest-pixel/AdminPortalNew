using DTPortal.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class CredentialDTO
    {
        public int Id { get; set; }

        public string? CredentialName { get; set; }
        public string? DisplayName { get; set; }
        public string? CredentialId { get; set; }

        public string? CredentialUId { get; set; }
        public string? Remarks { get; set; }
        public List<int>? Categories {  get; set; }

        public string? VerificationDocType { get; set; }
            
        public List<DataAttributesDTO> DataAttributes { get; set; }

        public string? AuthenticationScheme { get; set; }

        public string? CategoryId { get; set; }
        public int? Validity { get; set; }

        public string? OrganizationId { get; set; }

        public string? TrustUrl { get; set; }

        public List<string>? ServiceDetails { get; set; }

        public string? CredentialOffer {  get; set; }

        public DateTime CreatedDate { get; set; }

        public string? SignedDocument { get; set; }
        public string? Logo { get; set; }
        public string? Status { get; set; }
    }
    public class DataAttributesDTO
    {

        public string DisplayName { get; set; }
        public string Attribute { get; set; } 
        public int DataType { get; set; }


    }
}
