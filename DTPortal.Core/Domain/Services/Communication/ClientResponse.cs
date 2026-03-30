using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Services.Communication
{
    public class ClientResponse : BaseResponse<ClientsAllDTO>
    {
        public ClientResponse (ClientsAllDTO category) : base(category) { }

        public ClientResponse(string message) : base(message) { }

        public ClientResponse(ClientsAllDTO category, string message) :
            base(category, message) { }
    }

    public class ClientRequest
    {
        public ClientsAllDTO client { get; set; }
        public ClientsSaml2 ClientSaml2 { get; set; }
    }

}
