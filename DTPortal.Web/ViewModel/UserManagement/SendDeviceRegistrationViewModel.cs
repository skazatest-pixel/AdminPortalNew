using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DTPortal.Web.ViewModel.UserManagement
{
    public class SendDeviceRegistrationViewModel
    {
        public string Uuid { get; set; }

        public string RegistrationType { get; set; }
        [JsonRequired]
        public int id { get; set; }

        public DateTime? Expiry { get; set; }
    }
}
