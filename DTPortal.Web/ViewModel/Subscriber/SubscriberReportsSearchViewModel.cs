using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DTPortal.Web.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.Subscriber
{
    public class SubscriberReportsSearchViewModel
    {
        public string Identifier { get; set; }

        public string SubscriberFullname { get; set; }
        [JsonRequired]
        [Required(ErrorMessage = "Select Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Select Transaction Status")]
        public string TransactionStatus { get; set; }

         [Required(ErrorMessage = "Select Date Range")]
        public string DateRange { get; set; }
    }

    public class SubscriberReports
    {
        public string Identifier { get; set; }
        public string SubscriberFullname { get; set; }
        [JsonRequired]
        public int TransactionTypevalue { get; set; }
        public string TransactionStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
