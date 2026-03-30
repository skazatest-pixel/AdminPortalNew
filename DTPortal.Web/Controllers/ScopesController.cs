using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.Attribute;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize(Roles = "Profiles")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public class ScopesController : BaseController
    {
        private readonly IUserClaimService _userClaimService;
        private readonly IScopeService _scopeService;
         
        public ScopesController(ILogClient logClient, IUserClaimService userClaimService, IScopeService scopeService) : base(logClient)
        {
            _scopeService = scopeService;
            _userClaimService = userClaimService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var viewModel = new List<ScopesListViewModel>();

            //var ScopeList = await _scopeService.ListScopeAsync();
            var ScopeList = await _scopeService.GetProfileListAsync();
            if (ScopeList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all Scopes Configuration List", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration list");
                return NotFound();
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all Scopes Configuration List", LogMessageType.SUCCESS.ToString(), "Get Scopes Configuration list success");

                foreach (var item in ScopeList)
                {
                    viewModel.Add(new ScopesListViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        DisplayName = item.DisplayName,
                        Description = item.Description
                    });
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var Claimslist = await GetClaimList(false);
            if (Claimslist == null)
            {
                return NotFound();
            }
            var viewModel = new ScopesNewViewModel
            {
                ClaimLists = Claimslist,
            };
            return View(viewModel);
        }

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{

        //    var scopeInDb = await _scopeService.GetScopeAsync(id);
        //    if (scopeInDb == null)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
        //        return NotFound();
        //    }

        //    var Claimslist = await GetClaimList(scopeInDb.IsClaimsPresent, scopeInDb.ClaimsList);
        //    if (Claimslist == null)
        //    {
        //        return NotFound();
        //    }

        //    var ScopeEditViewModel = new ScopesEditViewModel
        //    {
        //        Id = scopeInDb.Id,
        //        Name = scopeInDb.Name,
        //        DisplayName = scopeInDb.DisplayName,
        //        Description = scopeInDb.Description,
        //        UserConsent = scopeInDb.UserConsent,
        //        DefaultScope = scopeInDb.DefaultScope,
        //        Metadata = scopeInDb.MetadataPublish,
        //        ClaimLists = Claimslist,
        //        claims = scopeInDb.ClaimsList,
        //        isClaimPresent = scopeInDb.IsClaimsPresent,
        //        SaveConsent = scopeInDb.SaveConsent,
        //    };

        //    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration details", LogMessageType.SUCCESS.ToString(), "Get Scopes Configuration details of " + scopeInDb.DisplayName + " successfully ");

        //    return View(ScopeEditViewModel);
        //}

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (id <= 0)
            { 
                return BadRequest(new { success = false, message = "Id is Invalid." }); 
            }

            var scopeData = await _scopeService.GetProfileByIdAsync(id);
            if (scopeData== null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration client details", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
                return NotFound();
            }

            var Claimslist = await GetClaimList(scopeData.IsClaimsPresent, scopeData.ClaimsList);
            if (Claimslist == null)
            {
                return NotFound();
            }

            var ScopeEditViewModel = new ScopesEditViewModel
            {
                Id = scopeData.Id,
                Name = scopeData.Name,
                DisplayName = scopeData.DisplayName,
                DisplayNameArabic=scopeData.DisplayNameArabic,
                Description = scopeData.Description,
                UserConsent = scopeData.UserConsent,
                DefaultScope = scopeData.DefaultScope,
                Metadata = scopeData.MetadataPublish,
                ClaimLists = Claimslist,
                claims = scopeData.ClaimsList,
                isClaimPresent = scopeData.IsClaimsPresent,
                SaveConsent = scopeData.SaveConsent,
            };

            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "View Scopes Configuration details", LogMessageType.SUCCESS.ToString(), "Get Scopes Configuration details of " + scopeData.DisplayName + " successfully ");

            return View(ScopeEditViewModel);
        }


        //[HttpPost]
        //public async Task<IActionResult> Save(ScopesNewViewModel viewModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var Claimslist = await GetClaimList(true, viewModel.claims);
        //        if (Claimslist == null)
        //        {
        //            return NotFound();
        //        }
        //        viewModel.ClaimLists = Claimslist;
        //        return View("New", viewModel);
        //    }

        //    var scope = new Scope()
        //    {
        //        Name = viewModel.Name,
        //        DisplayName = viewModel.DisplayName,
        //        Description = viewModel.Description,
        //        UserConsent = viewModel.UserConsent,
        //        DefaultScope = viewModel.DefaultScope,
        //        MetadataPublish = viewModel.Metadata,
        //        CreatedBy = UUID,
        //        UpdatedBy = UUID,
        //        ClaimsList = viewModel.claims,
        //        IsClaimsPresent = true,
        //        SaveConsent = viewModel.SaveConsent
        //    };

        //    //var CliamList = viewModel.claims.Split(",")
        //    //      .Select(x => new { claimId = int.Parse(x.Split(":")[0]), isSelected = x.Split(":")[1].Equals("true") })
        //    //      .ToDictionary(x => x.claimId, x => x.isSelected);

        //    //foreach (var item in CliamList)
        //    //{
        //    //    var ScopeClaims = new ScopeClaim
        //    //    {
        //    //        ClaimId = item.Key,
        //    //        IsEnabled = item.Value
        //    //    };
        //    //    scope.ScopeClaims.Add(ScopeClaims);
        //    //}

        //    var response = await _scopeService.CreateScopeAsync(scope);
        //    if (response == null || !response.Success)
        //    {
        //        var Claimslist = await GetClaimList(true, viewModel.claims);
        //        if (Claimslist == null)
        //        {
        //            return NotFound();
        //        }
        //        viewModel.ClaimLists = Claimslist;
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to create Scopes Configuration");
        //        Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
        //        TempData["Alert"] = JsonConvert.SerializeObject(alert);
        //        return View("New", viewModel);
        //    }
        //    else
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Created New Scopes Configuration with name " + viewModel.DisplayName + " Successfully");
        //        Alert alert = new Alert { IsSuccess = true, Message = response.Message };
        //        TempData["Alert"] = JsonConvert.SerializeObject(alert);

        //        return RedirectToAction("List");
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ScopesNewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var Claimslist = await GetClaimList(true, viewModel.claims);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.ClaimLists = Claimslist;
                return View("New", viewModel);
            }

            var scope = new ScopeAllListDTO()
            {
                Name = viewModel.Name,
                DisplayName = viewModel.DisplayName,
                DisplayNameArabic=viewModel.DisplayNameArabic,
                Description = viewModel.Description,
                UserConsent = viewModel.UserConsent,
                DefaultScope = viewModel.DefaultScope,
                MetadataPublish = viewModel.Metadata,
                CreatedBy = UUID,
                UpdatedBy = UUID,
                ClaimsList = viewModel.claims,
                IsClaimsPresent = true,
                SaveConsent = viewModel.SaveConsent
            };


            var response = await _scopeService.AddProfileAsync(scope);
            if (response == null || !response.Success)
            {
                var Claimslist = await GetClaimList(true, viewModel.claims);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.ClaimLists = Claimslist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to create Scopes Configuration");
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("New", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Create new Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Created New Scopes Configuration with name " + viewModel.DisplayName + " Successfully");
                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> Update(ScopesEditViewModel viewModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var Claimslist = await GetClaimList(viewModel.isClaimPresent,viewModel.claims);
        //        if (Claimslist == null)
        //        {
        //            return NotFound();
        //        }
        //        viewModel.ClaimLists = Claimslist;
        //        return View("Edit", viewModel);
        //    }



        //    var scopeInDb = await _scopeService.GetScopeAsync(viewModel.Id);
        //    if (scopeInDb == null)
        //    {
        //        var Claimslist = await GetClaimList(viewModel.isClaimPresent, viewModel.claims);
        //        if (Claimslist == null)
        //        {
        //            return NotFound();
        //        }
        //        viewModel.ClaimLists = Claimslist;
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
        //        ModelState.AddModelError(string.Empty, "Scopes Configuration not found");

        //        return View("Edit", viewModel);
        //    }


        //    scopeInDb.Id = viewModel.Id;
        //    scopeInDb.Name = viewModel.Name;
        //    scopeInDb.DisplayName = viewModel.DisplayName;
        //    scopeInDb.Description = viewModel.Description;
        //    scopeInDb.UserConsent = viewModel.UserConsent;
        //    scopeInDb.DefaultScope = viewModel.DefaultScope;
        //    scopeInDb.MetadataPublish = viewModel.Metadata;
        //    scopeInDb.UpdatedBy = UUID;
        //    scopeInDb.ClaimsList = viewModel.claims;
        //    scopeInDb.IsClaimsPresent = true;
        //    scopeInDb.SaveConsent = viewModel.SaveConsent;
        //    scopeInDb.Version= Guid.NewGuid().ToString();

        //    //var CliamList = viewModel.claims.Split(",")
        //    //      .Select(x => new { claimId = int.Parse(x.Split(":")[0]), isSelected = x.Split(":")[1].Equals("true") })
        //    //      .ToDictionary(x => x.claimId, x => x.isSelected);

        //    //foreach (var item in scopeInDb.ScopeClaims)
        //    //{
        //    //    if (CliamList.ContainsKey(item.ClaimId))

        //    //    {
        //    //        CliamList.TryGetValue(item.ClaimId, out var val);

        //    //        item.IsEnabled = val;
        //    //    }
        //    //    else
        //    //    {
        //    //        item.IsEnabled = false;
        //    //    }
        //    //}

        //    var response = await _scopeService.UpdateScopeAsync(scopeInDb);
        //    if (response == null || !response.Success)
        //    {
        //        var Claimslist = await GetClaimList(viewModel.isClaimPresent, viewModel.claims);
        //        if (Claimslist == null)
        //        {
        //            return NotFound();
        //        }
        //        viewModel.ClaimLists = Claimslist;
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to update Scopes Configuration details of  name " + viewModel.DisplayName);
        //        Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
        //        TempData["Alert"] = JsonConvert.SerializeObject(alert);
        //        return View("Edit", viewModel);
        //    }
        //    else
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Scopes Configuration details of  name " + viewModel.DisplayName + " successfully" : "Request for Update Scopes Configuration details of application name " + viewModel.DisplayName + " has send for approval "));

        //        Alert alert = new Alert { IsSuccess = true, Message = response.Message };
        //        TempData["Alert"] = JsonConvert.SerializeObject(alert);

        //        return RedirectToAction("List");
        //    }
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ScopesEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var Claimslist = await GetClaimList(viewModel.isClaimPresent, viewModel.claims);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.ClaimLists = Claimslist;
                return View("Edit", viewModel);
            }



            var scopeData = await _scopeService.GetProfileByIdAsync(viewModel.Id);
            if (scopeData == null)
            {
                var Claimslist = await GetClaimList(viewModel.isClaimPresent, viewModel.claims);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.ClaimLists = Claimslist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to get Scopes Configuration details");
                ModelState.AddModelError(string.Empty, "Scopes Configuration not found");

                return View("Edit", viewModel);
            }


            scopeData.Id = viewModel.Id;
            scopeData.Name = viewModel.Name;
            scopeData.DisplayName = viewModel.DisplayName;
            scopeData.DisplayNameArabic = viewModel.DisplayNameArabic;
            scopeData.Description = viewModel.Description;
            scopeData.UserConsent = viewModel.UserConsent;
            scopeData.DefaultScope = viewModel.DefaultScope;
            scopeData.MetadataPublish = viewModel.Metadata;
            scopeData.UpdatedBy = UUID;
            scopeData.ClaimsList = viewModel.claims;
            scopeData.IsClaimsPresent = true;
            scopeData.SaveConsent = viewModel.SaveConsent;
            scopeData.Version = Guid.NewGuid().ToString();



            //var response = await _scopeService.UpdateScopeAsync(scopeData);
            var response = await _scopeService.UpdateProfileAsync(scopeData);

            if (response == null || !response.Success)
            {
                var Claimslist = await GetClaimList(viewModel.isClaimPresent, viewModel.claims);
                if (Claimslist == null)
                {
                    return NotFound();
                }
                viewModel.ClaimLists = Claimslist;
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to update Scopes Configuration details of  name " + viewModel.DisplayName);
                Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View("Edit", viewModel);
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Update Scopes Configuration", LogMessageType.SUCCESS.ToString(), (response.Message != "Your request sent for approval" ? "Updated Scopes Configuration details of  name " + viewModel.DisplayName + " successfully" : "Request for Update Scopes Configuration details of application name " + viewModel.DisplayName + " has send for approval "));

                Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);

                return RedirectToAction("List");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        var response = await _scopeService.DeleteScopeAsync(id, UUID);
        //        if (response != null || response.Success)
        //        {

        //            Alert alert = new Alert { IsSuccess = true, Message = response.Message };
        //            TempData["Alert"] = JsonConvert.SerializeObject(alert);
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Delete Scopes Configuration successfully");
        //            return new JsonResult(true);
        //        }
        //        else
        //        {
        //            Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
        //            TempData["Alert"] = JsonConvert.SerializeObject(alert);
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration");
        //            return new JsonResult(true);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration " + e.Message);
        //        return StatusCode(500, e);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "id need to be a positive number" });
            }

            try
            {
                //var response = await _scopeService.DeleteScopeAsync(id, UUID);
                var response = await _scopeService.DeleteProfileAsync(id, UUID);

               
                if (response != null || response.Success)
                {
                    Alert alert = new Alert { IsSuccess = true, Message = response.Message };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.SUCCESS.ToString(), "Delete Scopes Configuration successfully");
                    return new JsonResult(true);
                }
                else
                {
                    Alert alert = new Alert { Message = (response == null ? "Internal error please contact to admin" : response.Message) };
                    TempData["Alert"] = JsonConvert.SerializeObject(alert);
                    SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration");
                    return new JsonResult(true);
                }
            }
            catch (Exception e)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Delete Scopes Configuration", LogMessageType.FAILURE.ToString(), "Fail to delete Scopes Configuration " + e.Message);
                return StatusCode(500, e);
            }
        }


        public async Task<List<ClaimListItem>> GetClaimList(bool isClaimPresent ,string ScopeClaims = null)
        {
            if (!ModelState.IsValid)
                return null;

            var UserClaimList = await _userClaimService.GetAttributeListAsync();
            if (UserClaimList == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.ScopesConfiguration, "Get all user clamis list", LogMessageType.FAILURE.ToString(), "Fail to get all user clamis list");
                throw new Exception("Fail to get user clamis");
            }
            var nodes = new List<ClaimListItem>();

            if (isClaimPresent && ScopeClaims != null)
            {
                var ScopeClaimslist = ScopeClaims.Split(" ");
                foreach (var claim in UserClaimList)
                {
                    nodes.Add(new ClaimListItem()
                    {
                        name = claim.Name,
                        Display = claim.DisplayName ,
                        IsSelected = ScopeClaimslist.Any(x => x == claim.Name)
                    });
                }
            }
            else
            {
                foreach (var claim in UserClaimList)
                {

                    nodes.Add(new ClaimListItem()
                    {
                        name = claim.Name,
                        Display = claim.DisplayName,
                        IsSelected = false
                    });
                }
            }

            return nodes;
        }
        [HttpGet]
        public async Task<string[]> GetScopesNamesList(string value)
        {
            //var scopesList = await _scopeService.GetScopesNamesAsync(value);
            var scopesList = await _scopeService.GetProfileNamesAsync(value);
            return scopesList;
        }
    }
}
