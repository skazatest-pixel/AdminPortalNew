using System;
using System.ComponentModel.DataAnnotations;
using DTPortal.Core;
using DTPortal.Core.DTOs;
using DTPortal.Web.CustomValidations;
using DTPortal.Web.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace DTPortal.Web.ViewModel.Reports
{
    public class ReportsSearchViewModel
    {
        public string ReportType { get; set; }

        public string[] ReportTypes = new[] { "RA", "Onboarding"};

        [Display(Name = "Subscriber Identifier")]
        public string Identifier { get; set; }

        // To Change later
        [Display(Name = "Operation")]
        public string ServiceName { get; set; }

        [Display(Name = "Transaction Type")]
        public TransactionType? TransactionType { get; set; }

        [Display(Name = "Signature Type")]
        public SignatureType? SignatureType { get; set; }
        [JsonRequired]
        [Display(Name = "ESeal Used?")]
        public bool ESealUsed { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        [DateGreaterThanAttribute(otherPropertyName = "StartDate", ErrorMessage = "End date must be greater than start date")]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Show Records (Per Page)")]
        public int? PerPage { get; set; }

        public PaginatedList<LogReportDTO> Reports { get; set; }
    }
}
