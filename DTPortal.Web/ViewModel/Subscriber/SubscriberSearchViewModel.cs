using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DTPortal.Core.DTOs;
using DTPortal.Web.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.Subscriber
{
    public class SubscriberSearchViewModel
    {
        [JsonRequired]
        [Display(Name ="Search By")]
        [Required(ErrorMessage ="Select type")]
        public SubscriberIdentifier IdentifierType { get; set; }

        [Display(Name = "Value")]
        [Required(ErrorMessage = "Value is required")]
        public string IdentifierValue { get; set; }

        public SubscriberDetailsDTO SubscriberDetails { get; set; }
    }
}
