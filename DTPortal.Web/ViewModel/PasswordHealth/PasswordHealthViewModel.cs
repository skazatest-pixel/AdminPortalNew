using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DTPortal.Web.ViewModel.PasswordHealth
{
    public class PasswordHealthViewModel
    {
        [JsonRequired]
        [Required]
        public int Id { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "*Last 'n' number of passwords which should not match the new password")]
        public int PasswordHistory { get; set; }

        //[Required]
        //[Display(Name = " Minimum password age [in Days]")]
        //public int MinimumPwdAge { get; set; }

        //[Required]
        //[Display(Name = "Maximum password age [in Days]")]
        //public int MaximumPwdAge { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Minimum password length")]
        public int MinimumPwdLength { get; set; }
        [JsonRequired]
        [Required]
        [Display(Name = "Maximum password length")]
        public int MaximumPwdLength { get; set; }

        //[Required]
        //[Display(Name = "Store password using reversible encryption")]
        //public bool IsReversibleEncryption { get; set; }

        [Required]
        [Display(Name = "Password Contains")]
        public int? PwdContains { get; set; }

        public List<SelectListItem> PwdContainsList { get; } = new List<SelectListItem>
        {
            //new SelectListItem { Value = "1", Text = "Only letter" },
            //new SelectListItem { Value = "2", Text = "Only numbers" },
            //new SelectListItem { Value = "3", Text = "Alpha numeric" },
            new SelectListItem { Value = "5", Text = "Alpha numeric with atleast one uppercase letter, lowercase letter, digit and special symbols" },
            //new SelectListItem { Value = "4", Text = "Alpha numeric with atleast one Special symbol"  },
        };
        //[Required]
        //[Display(Name = "Bad password count")]
        //public int? BadPwdCount { get; set; }
        [JsonRequired]
        public int enforcepwdhistory { get; set; }
    }
}
