using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.category
{
    public class CatecoryEditViewModel
    {
        [JsonRequired]
        public int Id { get; set; }
        public string CategoryUid { get; set; }
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
    }
}
