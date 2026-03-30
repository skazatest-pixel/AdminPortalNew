using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Services.Communication;
namespace DTPortal.Core.DTOs
{
    public class ApplicationConfigurationDTO
    {
         
        public SSOConfig SsoConfig { get; set; }
        public adminportal_config AdminPortalConfig { get; set; }
        public idp_configuration IdpConfig { get; set; }
        public string UUID { get; set; }
    }
}
