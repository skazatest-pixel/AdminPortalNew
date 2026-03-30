using System.ComponentModel.DataAnnotations;

namespace DTPortal.Web.ViewModel.Agent
{
    public class AgentEditViewModel
    {
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "MobileNumber")]
        public string MobileNumber { get; set; }
    }
}
