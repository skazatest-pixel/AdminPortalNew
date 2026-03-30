using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.ESealRegistration;
using DTPortal.Web.ViewModel.WalletConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json; 
 
namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class WalletConfigurationController : Controller
    {
        private readonly IWalletConfigurationService _walletConfigurationService;
        
        public WalletConfigurationController(IWalletConfigurationService walletConfigurationService)
           
            
        {

            
            _walletConfigurationService = walletConfigurationService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _walletConfigurationService.GetWalletConfigurationsAsync();
            if (result == null)
            {
                return NotFound();

            }
            
            var gy = JsonConvert.DeserializeObject<WalletConfigurationResponse>(result.Resource.ToString());

            WalletConfigurationViewModel walletconfig = new WalletConfigurationViewModel
            {
                BindingMethods = gy.BindingMethods,
                CredentialFormats = gy.CredentialFormats
            };
            return View(walletconfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm]WalletConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

         
            WalletConfigurationResponse walletconfig = new WalletConfigurationResponse()
            {
                BindingMethods = model.BindingMethods,
                CredentialFormats = model.CredentialFormats,
            };

            var response = await _walletConfigurationService.UpdateWalletConfigurationsAsync(walletconfig);
            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
            }

            // Pass the updated model back to the view
            return RedirectToAction("Index"); // Ensure the view name matches your view
        }

    }
}
