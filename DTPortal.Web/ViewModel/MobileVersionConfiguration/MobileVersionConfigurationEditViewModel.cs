using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.MobileVersionConfiguration
{
    public class MobileVersionConfigurationEditViewModel : BaseMobileVersionConfigurationViewModel
    {
        [JsonRequired]
        public int Id { get; set; }

        [Required]
        [Display(Name = "OS Version")]
        public string OSVersion { get; set; }

        [Required]
        [Display(Name = "Latest Version")]
        public string LatestVersion { get; set; }

        [Required]
        [Display(Name = "Minimum Version")]
        public string MinimumVersion { get; set; }

        [Display(Name = "Update Link")]
        public string UpdateLink { get; set; }
    }
}
