using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.CriticalOperation
{
    public class CriticalOperationEditViewModel
    {
        [JsonRequired]
        public int OperationId { get; set; }

        [Required]
        [Display(Name = "Operation Name ")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description ")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Authentication Policies")]
        public string AuthScheme { get; set; }
        public List<SelectListItem> Authlist { get; set; }

        [JsonRequired]
        [Display(Name = "Authentication Required ")]
        public int IsEnable { get; set; }
    }
}
