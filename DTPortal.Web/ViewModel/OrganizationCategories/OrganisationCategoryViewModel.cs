using System.Collections.Generic;
using System.Text.Json.Serialization;
using DTPortal.Core.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.OrganizationCategories
{
    public class OrganisationCategoryViewModel
    {
        public IEnumerable<SelfServiceCategoryDTO> OrgCatogeryFieldList { get; set; }
        [JsonRequired]
        public int OrgCategoryId { get; set; }
        public string DisplayName { get; set; }
        public string OrgCategoryName { get; set; }
        public List<SelfServiceFieldDTO> organisationFieldDtos { get; set; }
    }
}
