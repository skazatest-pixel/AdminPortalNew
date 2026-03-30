using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Services;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DTPortal.Core.Services
{
    public class ScopeService : IScopeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IMCValidationService _mcValidationService;
        private readonly ICacheClient _cacheClient;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        public ScopeService(ILogger<ClientService> logger,
            IConfigurationService configurationService,
            IUnitOfWork unitOfWork, ILogClient logClient,
            IMCValidationService mcValidationService,
            ICacheClient cacheClient,
            IConfiguration configuration,
            HttpClient httpClient)
        {

            _configuration = configuration;
            httpClient.BaseAddress = new Uri(_configuration["APIServiceLocations:IDPConfigurationBaseAddress"]);

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");


            _logger = logger;
            _unitOfWork = unitOfWork;
            _configurationService = configurationService;
            _LogClient = logClient;
            _mcValidationService = mcValidationService;
            _cacheClient = cacheClient;
            _client = httpClient;
        }


        //public async Task<ScopeResponse> CreateScopeAsync(Scope scope,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogDebug("--->CreateScopeAsync");

        //    var isExists = await _unitOfWork.Scopes.IsScopeExistsWithNameAsync(
        //        scope.Name);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("Profile already exists with given name");
        //        return new ScopeResponse("Profile already exists with given Name");
        //    }

        //    // Check whether maker checker enabled for this module
        //    bool isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ScopesActivityId);

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        // Check whether maker checker enabled for this operation
        //        // If enabled store request object in maker checker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired
        //            (ActivityIdConstants.ScopesActivityId, "CREATE", scope.CreatedBy,
        //            JsonConvert.SerializeObject(scope));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new ScopeResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new ScopeResponse(scope, "Your request sent for approval");
        //        }
        //    }

        //    scope.CreatedDate = DateTime.Now;
        //    scope.ModifiedDate = DateTime.Now;
        //    scope.Status = StatusConstants.ACTIVE;
        //    try
        //    {
        //        await _unitOfWork.Scopes.AddAsync(scope);
        //        await _unitOfWork.SaveAsync();
        //        return new ScopeResponse(scope, "Profile created successfully");
        //    }
        //    catch
        //    {
        //        _logger.LogError("Profile AddAsync failed");
        //        return new ScopeResponse("An error occurred while creating the Profile." +
        //            " Please contact the admin.");
        //    }
        //}

        //public async Task<Scope> GetScopeAsync(int id)
        //{
        //    _logger.LogDebug("--->GetScopeAsync");

        //    var scope = await _unitOfWork.Scopes.GetScopeByIdWithClaims(id);
        //    if (null == scope)
        //    {
        //        _logger.LogError("Profile GetByIdAsync() Failed");
        //        return null;
        //    }

        //    return scope;
        //}

        ///get-profile-by-name
       
        //public async Task<int> GetScopeIdByNameAsync(string name)
        //{
        //    _logger.LogDebug("--->GetScopeAsync");

        //    var scope = await _unitOfWork.Scopes.GetScopeByNameAsync(name);
        //    if (null == scope)
        //    {
        //        _logger.LogError("Profile GetByIdAsync() Failed");
        //        return -1;
        //    }

        //    return scope.Id;
        //}

        //public async Task<ScopeResponse> UpdateScopeAsync(Scope scope,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogDebug("--->UpdateScopeAsync");

        //    // Check whether the scope exists or not
        //    var scopeInDb = _unitOfWork.Scopes.GetById(scope.Id);
        //    if (null == scopeInDb)
        //    {
        //        _logger.LogError("Profile not found");
        //        return new ScopeResponse("Profile not found");
        //    }

        //    if (scopeInDb.Status == "DELETED")
        //    {
        //        _logger.LogError("Profile is already deleted");
        //        return new ScopeResponse("Profile is already deleted");
        //    }

        //    // Check wheter the scope already exists other than the given scope
        //    var allScopes = await _unitOfWork.Scopes.GetAllAsync();
        //    foreach (var item in allScopes)
        //    {
        //        if (item.Id != scope.Id)
        //        {
        //            if (item.Name == scope.Name)
        //            {
        //                _logger.LogError("Profile already exists with given Name");
        //                return new ScopeResponse("Profile already exists with given Name");
        //            }
        //        }
        //    }

        //    // Check whether maker checker enabled for this module
        //    bool isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ScopesActivityId);

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        _unitOfWork.DisableDetectChanges();

        //        // Check whether maker checker enabled for this operation
        //        // If enabled store request object in maker checker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired
        //            (ActivityIdConstants.ScopesActivityId, "UPDATE", scope.UpdatedBy,
        //            JsonConvert.SerializeObject(scope));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new ScopeResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new ScopeResponse(scope, "Your request sent for approval");
        //        }
        //    }

        //    scopeInDb.Name = scope.Name;
        //    scopeInDb.DisplayName = scope.DisplayName;
        //    scopeInDb.Description = scope.Description;
        //    scopeInDb.DefaultScope = scope.DefaultScope;
        //    scopeInDb.UserConsent = scope.UserConsent;
        //    scopeInDb.MetadataPublish = scope.MetadataPublish;
        //    scopeInDb.UpdatedBy = scope.UpdatedBy;
        //    scopeInDb.ModifiedDate = DateTime.Now;
        //    scopeInDb.ClaimsList = scope.ClaimsList;
        //    scopeInDb.IsClaimsPresent = scope.IsClaimsPresent;

        //    try
        //    {
        //        _unitOfWork.Scopes.Update(scopeInDb);
        //        await _unitOfWork.SaveAsync();
        //        return new ScopeResponse(scopeInDb, "Profile updated successfully");
        //    }
        //    catch
        //    {
        //        _logger.LogError("Profile Update failed");
        //        return new ScopeResponse("An error occurred while updating the Profile." +
        //            " Please contact the admin.");
        //    }
        //}



        public async Task<IEnumerable<ScopeAllListDTO>> ListScopeAsync()
        {
            //return await _unitOfWork.Scopes.ListAllScopeAsync();
            return await GetProfileListAsync();
        }

        public async Task<IList<string>> GetScopesListAsync()
        {
            // var list = await _unitOfWork.Scopes.ListAllScopeAsync();

            var list = await GetProfileListAsync();
            var scopeList = new List<string>();
            foreach (var scope in list)
            {
                scopeList.Add(scope.Name);
            }
            return scopeList;
        }

        //public async Task<IList<UserClaimDto>> ListAttributeDisplayNames(string fieldsString)
        //{
        //    var attributes = await _unitOfWork.UserClaims
        //        .GetUserClaimByNameAsync(fieldsString);

        //    return attributes;
        //}

        /// <summary>
        /// it is being call in authentication service which is used by idp
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public async Task<bool> isScopehaveSaveConsent(int scopeId)
        {
            var scope = await GetProfileByIdAsync(scopeId);
            if (scope == null)
            {
                return false;
            }
            return scope.SaveConsent;
        }

        //public async Task<bool> isScopehaveSaveConsentByName(string scopename)
        //{
        //    var scope = await _unitOfWork.Scopes.GetScopeByNameAsync(scopename);
        //    if (scope == null)
        //    {
        //        return false;
        //    }
        //    return scope.SaveConsent;
        //}

        //public async Task<ScopeResponse> DeleteScopeAsync(int id, string updatedBy,
        //        bool makerCheckerFlag = false)
        //{
        //    var scopeInDb = await _unitOfWork.Scopes.GetByIdAsync(id);
        //    if (null == scopeInDb)
        //    {
        //        return new ScopeResponse("Profile not found");
        //    }

        //    try
        //    {
        //        _unitOfWork.Scopes.Remove(scopeInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new ScopeResponse(scopeInDb, "Profile deleted successfully");
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("DeleteScopeAsync failed : {0}", error.Message);
        //        return new ScopeResponse(
        //            "An error occurred,Please contact the admin.");
        //    }
        //}

        //get-profile-names 
        //public async Task<string[]> GetScopesNamesAsync(string Value)
        //{
        //    var scopesNames = await _unitOfWork.Scopes.GetScopesNamesAsync(Value);

        //    return scopesNames;
        //}

        //public async Task<Dictionary<string, string>> GetScopeNameDisplayNameAsync()
        //{
        //    var scopesPair = new Dictionary<string, string>();

        //    //var scopesList = await _unitOfWork.Scopes.ListAllScopeAsync();

        //    var scopesList = await GetProfileListAsync();

        //    foreach (var scope in scopesList)
        //    {
        //        scopesPair[scope.Name] = scope.DisplayName;
        //    }

        //    return scopesPair;
        //}

        ///api implementations functions

        //ListAllScopeAsync
        public async Task<IEnumerable<ScopeAllListDTO>> GetProfileListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/Profile/get-profile-list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopes = JsonConvert.DeserializeObject<List<ScopeAllListDTO>>(apiResponse.Result.ToString());
                        return scopes;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }
        // GetScopeAsync
        public async Task<ScopeAllListDTO> GetProfileByIdAsync(int id)
        {

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/Profile/get-profile-by-id?id={id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scope = JsonConvert.DeserializeObject<ScopeAllListDTO>(apiResponse.Result.ToString());

                        return scope;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

        // CreateScopeAsync
        public async Task<ScopeResponse> AddProfileAsync(ScopeAllListDTO scope,
           bool makerCheckerFlag = false)
        {
            try
            {

                var jsonContent = new StringContent(JsonConvert.SerializeObject(scope),
                            Encoding.UTF8, "application/json");
                

              

                HttpResponseMessage response = await _client.PostAsync("api/Profile/add-profile", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopeData = JsonConvert.DeserializeObject<ScopeAllListDTO>(apiResponse.Result.ToString());
                        return new ScopeResponse(scopeData, "Attribute created successfully");
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ScopeResponse(apiResponse.Message);

                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ScopeResponse("An error occurred while creating the Attribute." +
                    " Please contact the admin.");
            }

            return null;
        }

        // UpdateScopeAsync
        public async Task<ScopeResponse> UpdateProfileAsync(ScopeAllListDTO scope,
           bool makerCheckerFlag = false)
        {

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(scope),
                                    Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PutAsync("api/Profile/update-profile", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopeRes = JsonConvert.DeserializeObject<ScopeAllListDTO>(apiResponse.Result.ToString());
                        return new ScopeResponse(scopeRes, "Profile updated successfully");
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        //return new ScopeResponse("An error occurred while updating the Profile." +
                        //    " Please contact the admin.");
                        return new ScopeResponse(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }
        // DeleteScopeAsync
        public async Task<ScopeResponse> DeleteProfileAsync(int id, string updatedBy,
              bool makerCheckerFlag = false)
        {
            var scopeData = await GetProfileByIdAsync(id);
            if (null == scopeData)
            {
                return new ScopeResponse("Profile not found");
            }



            try
            {
                HttpResponseMessage response = await _client.DeleteAsync($"api/Profile/delete-profile?id={id}&UUID={updatedBy}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var  userClaimRes = JsonConvert.DeserializeObject<UserClaim>(apiResponse.Result.ToString());
                        //return new UserClaimResponse(userClaimRes , apiResponse.Message);
                        return new ScopeResponse(null, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ScopeResponse(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteUserClaimAsync failed : {0}", ex.Message);
                return new ScopeResponse(
                    "An error occurred,Please contact the admin.");
            }
            return null;
        }
        // GetScopeIdByNameAsync 
        public async Task<int> GetProfileIdByNameAsync(string name)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/Profile/get-profile-by-name?name={name}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopeId= JsonConvert.DeserializeObject<int>(apiResponse.Result.ToString());
                        return scopeId;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return -1;
        }
        //GetScopeIdByNameAsync
        public async Task<ScopeAllListDTO> GetProfileByNameAsync(string name)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/Profile/get-scope-by-name?name={name}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopedb = JsonConvert.DeserializeObject<ScopeAllListDTO>(apiResponse.Result.ToString());
                        return scopedb;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }
        //GetScopesNamesAsync
        public async Task<string[]> GetProfileNamesAsync(string value)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/Profile/get-profile-names?value={value}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var scopesNames = JsonConvert.DeserializeObject<string[]>(apiResponse.Result.ToString());
                        return scopesNames;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }

    }
}

