using System;
using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.category
{
    public class CategoryAddViewModel
    {
       
        public string CategoryUid { get; set; }
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Description")]
        [MaxLength(300, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
       
    }
}
