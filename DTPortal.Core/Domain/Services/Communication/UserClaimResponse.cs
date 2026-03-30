using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class UserClaimResponse : BaseResponse<UserClaimListDTO>
    {
        public UserClaimResponse(UserClaimListDTO category) : base(category) { }

        public UserClaimResponse(string message) : base(message) { }

        public UserClaimResponse(UserClaimListDTO category, string message) : base(category, message){ }
    }
}
