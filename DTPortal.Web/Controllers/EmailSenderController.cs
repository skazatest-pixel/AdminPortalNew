using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class EmailSenderController : BaseController
    {
        private readonly IEmailSenderService _emailSenderService;
        public EmailSenderController(ILogClient logClient,
            IEmailSenderService emailSenderService) : base(logClient)
        {
            _emailSenderService = emailSenderService;
        }

        [HttpGet]
        [Route("send")]
        public IActionResult Index()
        {
            return Ok(new APIResponse
            {
                Success = true,
                Message = "",
                Result = null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sendemail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDTO keyValues)
        {
            APIResponse response;

            var result = await _emailSenderService.SendEmailAsync(keyValues);
           
            
                response = new APIResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Result = result.Resource
                };
            
             

            return Ok(response);
        }
    }
}
