using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace DTPortal.Web.ViewModel.RoleManagement
{
    public class RoleManagementNewViewModel
    {
        [JsonRequired]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Role Name ")]
        public string RoleName { get; set; }

        [Required]
        [Display(Name = "Display Name ")]
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Role Description ")]
        public string Description { get; set; }

        // public List<ActivityListItem> Activities { get; set; }
        [Display(Name = "Activities")]
        public Object Activities { get; set; }

        public string Activitie { get; set; }

        public IEnumerable<CheckerListItem> CheckerActivitie { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}
