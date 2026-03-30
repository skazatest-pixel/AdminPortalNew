using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Web.ViewModel.Banners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class BannerConfigurationController : Controller
        {
            private readonly IBannerConfigService _bannerConfigService;
            private readonly ILogger<BannerConfigurationController> _logger;

            public BannerConfigurationController(
                IBannerConfigService bannerConfigService,
                ILogger<BannerConfigurationController> logger)
            {
                _bannerConfigService = bannerConfigService;
                _logger = logger;
            }

        [HttpGet]
        public async Task<IActionResult> BannerConfig()
        {
            try
            {
                string id = null;
                var bannerTexts = await _bannerConfigService
                    .GetLatestBannerTextsAsync(id);

                var model = new BannerConfigViewModel
                {
                    BannerTextList = bannerTexts
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading banner text config");
                return View(new BannerConfigViewModel());
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBannerTextConfig(
        [FromBody] List<BannerTextData> bannerTexts)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (bannerTexts == null || !bannerTexts.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "Banner text list is empty"
                });
            }

            if (bannerTexts.Any(x => x == null))
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid banner text data."
                });
            }

            var request = new UpdateBannerTextRequestDTO
            {
                Id = 1,
                Name = "Home Page Banner Text",
                BannerTexts = bannerTexts,
                UpdatedBy = User?.Identity?.Name ?? "system"
            };

            var result =
                await _bannerConfigService.UpdateBannerTextsAsync(request);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

    }

}
