using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.CredentialApproval;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class CredentialApprovalController : BaseController
    {
        private readonly ICredentialService _credentialService;
        private readonly ICategoryService _categoryService;
        private readonly IOrganizationService _organizationService;
        public CredentialApprovalController(ICredentialService credentialService,
            ICategoryService categoryService,
            IOrganizationService organizationService,
            ILogClient logClient) : base(logClient)
        {
            _credentialService = credentialService;

            _categoryService = categoryService;
            _organizationService = organizationService;
        }
        public async Task<IActionResult> Index()
        {
            //var response = await _credentialService.GetCredentialList();
            var response = await _credentialService.GetCredentialListAsync();
            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credentialList = JsonConvert.DeserializeObject<List<CredentialDTO>>(response.Resource.ToString());


            //var credentialList = (List<CredentialDTO>)response.Resource;

            var viewModel = new List<CredentialListViewModel>();

            foreach (var credential in credentialList)
            {
                var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.OrganizationId);
                var OrganizationName = "";
                if (organizationDetails != null && organizationDetails.Success)
                {
                    var organization = (OrganizationDTO)organizationDetails.Resource;
                    OrganizationName = organization.OrganizationName;
                }
                viewModel.Add(new CredentialListViewModel
                {
                    Id = credential.Id,
                    CredentialName = credential.DisplayName,
                    organizationName = OrganizationName,
                    createdDate = credential.CreatedDate,
                    authenticationScheme = credential.AuthenticationScheme,
                    verificationDocType = credential.VerificationDocType,
                    status = credential.Status
                });


            }

            return View(viewModel);
        }

        public async Task<IActionResult> CredentialDetails(int Id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _credentialService.GetCredentialByIdsync(Id);

            if (response != null && !response.Success)
            {
                return NotFound();
            }

            var credential = JsonConvert.DeserializeObject<CredentialDTO>(response.Resource.ToString());

            //var credential = (CredentialDTO)response.Resource;

            var model = new CredentialDetailsViewModel()
            {
                Id = credential.Id,
                credentialUId = credential.CredentialUId,
                DisplayName = credential.DisplayName,
                CredentialName = credential.CredentialName,
                authenticationScheme = credential.AuthenticationScheme,
                verificationDocType = credential.VerificationDocType,
                status = credential.Status,
                categoryName = credential.CategoryId,
                organizationName = credential.OrganizationId,
                remarks = credential.Remarks,
                logo = credential.Logo,
                createdDate = credential.CreatedDate.ToString("yyyy-MM-dd"),
                dataAttributes = credential.DataAttributes,
                signedDocument = credential.SignedDocument,
                trustUrl = credential.TrustUrl

            };

            var category = await _categoryService.GetCredentialCategoryByIdAsync(credential.Id);

            if (category != null)
            {
                model.categoryName = category.Name;
            }

            var organizationDetails = await _organizationService.GetOrganizationDetailsByUIdAsync(credential.OrganizationId);

            if (organizationDetails != null && organizationDetails.Success)
            {
                var organization = (OrganizationDTO)organizationDetails.Resource;
                model.organizationName = organization.OrganizationName;
            }

            return View(model);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string uid)
        {

            //var response = await _credentialService.ActivateCredential(uid);
            var response = await _credentialService.ActiveCredentialAsync(uid);


            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = false, message = "Failed to approve the credential." });
            }

            Alert successAlert = new Alert { IsSuccess = true, Message = response.Message };
            TempData["Alert"] = JsonConvert.SerializeObject(successAlert);
            return Json(new { success = true, message = "Credential approved successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string uid, string remarks)
        {
            var cred = new
            {
                Id = uid,
                Remarks = remarks
            };

            //var response = await _credentialService.RejectCredential(uid, remarks);
            var response = await _credentialService.RejectCredentialAsync(uid, remarks);

            if (response == null || !response.Success)
            {
                Alert alert = new Alert { IsSuccess = false, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = false, message = "Failed to reject the credential." });
            }
            else
            {
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return Json(new { success = true, message = "Credential rejected successfully!" });
            }
        }
    }
}
