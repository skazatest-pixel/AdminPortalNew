using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class ScopeResponse : BaseResponse<ScopeAllListDTO>
    {
        public ScopeResponse(ScopeAllListDTO category) : base(category) { }

        public ScopeResponse(string message) : base(message) { }

        public ScopeResponse(ScopeAllListDTO category, string message) : base(category, message){ }
    }
}
