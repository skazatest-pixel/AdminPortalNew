using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DTPortal.Web.ViewModel.Scopes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.Scopes
{
    public class ScopesEditViewModel
    {
        [JsonRequired]
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
        [Required]
        [Display(Name = "Display Name(Arabic)")]
        public string DisplayNameArabic { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Require user consent for this scope")]
        public bool UserConsent { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Set as default scope")]
        public bool DefaultScope { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Include in public metadata")]
        public bool Metadata { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Required Attributes")]
        public bool isClaimPresent { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Save Consent Data")]
        public bool SaveConsent { get; set; }

        public string claims { get; set; }
        public IEnumerable<ClaimListItem> ClaimLists { get; set; }
    }
}
