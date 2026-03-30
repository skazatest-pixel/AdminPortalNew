using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.DTOs
{
    public class DefaultAuthenticationResponseDTO
    {
        public List<AuthenticationSchemeDto> AuthenticationSchemesList { get; set; }
        public string AuthSchemeId { get; set; }
    }

    public class AuthenticationSchemeDto
    {
        public bool Disabled { get; set; }
        public string Group { get; set; }
        public bool Selected { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }
}