using System;

namespace DTPortal.Web.ViewModel.ESealRegistration
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public string LabelName { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
