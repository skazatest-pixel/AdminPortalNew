using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DTPortal.Core.Services
{
    public class UserClaimService : IUserClaimService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IMCValidationService _mcValidationService;
        private readonly ICacheClient _cacheClient;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        public UserClaimService(ILogger<ClientService> logger,
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

        //public async Task<UserClaimResponse> CreateUserClaimAsync(UserClaim userClaim,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogDebug("--->CreateUserClaimAsync");

        //    var isExists = await _unitOfWork.UserClaims.IsUserClaimExistsWithNameAsync(
        //        userClaim.Name);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("UserClaim already exists with given name");
        //        return new UserClaimResponse("Attribute already exists with given Name");
        //    }

        //    // Check whether maker checker enabled for this module
        //    bool isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.UserClaimsActivityId);

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        // Check whether maker checker enabled for this operation
        //        // If enabled store request object in maker checker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired
        //            (ActivityIdConstants.UserClaimsActivityId, "CREATE", userClaim.CreatedBy,
        //            JsonConvert.SerializeObject(userClaim));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new UserClaimResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new UserClaimResponse(userClaim, "Your request sent for approval");
        //        }
        //    }

        //    userClaim.CreatedDate = DateTime.Now;
        //    userClaim.ModifiedDate = DateTime.Now;
        //    userClaim.Status = StatusConstants.ACTIVE;
        //    try
        //    {
        //        await _unitOfWork.UserClaims.AddAsync(userClaim);
        //        await _unitOfWork.SaveAsync();
        //        return new UserClaimResponse(userClaim, "Attribute created successfully");
        //    }
        //    catch
        //    {
        //        _logger.LogError("UserClaim AddAsync failed");
        //        return new UserClaimResponse("An error occurred while creating the Attribute." +
        //            " Please contact the admin.");
        //    }
        //}

        //public async Task<UserClaim> GetUserClaimAsync(int id)
        //{
        //    _logger.LogDebug("--->GetUserClaimAsync");

        //    var userClaim = await _unitOfWork.UserClaims.GetByIdAsync(id);
        //    if (null == userClaim)
        //    {
        //        _logger.LogError("UserClaims GetByIdAsync() Failed");
        //        return null;
        //    }

        //    return userClaim;
        //}

        //public async Task<UserClaimResponse> UpdateUserClaimAsync(UserClaim userClaim,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogDebug("--->UpdateUserClaimAsync");

        //    // Check whether the userClaim exists or not
        //    var userClaimInDb = _unitOfWork.UserClaims.GetById(userClaim.Id);
        //    if (null == userClaimInDb)
        //    {
        //        _logger.LogError("userClaim not found");
        //        return new UserClaimResponse("Attribute not found");
        //    }

        //    if (userClaimInDb.Status == "DELETED")
        //    {
        //        _logger.LogError("UserClaim is already deleted");
        //        return new UserClaimResponse("Attribute is already deleted");
        //    }

        //    // Check wheter the userClaim already exists other than the given userClaim
        //    var allUserClaims = await _unitOfWork.UserClaims.GetAllAsync();
        //    foreach (var item in allUserClaims)
        //    {
        //        if (item.Id != userClaim.Id)
        //        {
        //            if (item.Name == userClaim.Name)
        //            {
        //                _logger.LogError("UserClaim already exists with given Name");
        //                return new UserClaimResponse("Attribute already exists with given Name");
        //            }
        //        }
        //    }

        //    // Check whether maker checker enabled for this module
        //    bool isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.UserClaimsActivityId);

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        _unitOfWork.DisableDetectChanges();

        //        // Check whether maker checker enabled for this operation
        //        // If enabled store request object in maker checker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired
        //            (ActivityIdConstants.UserClaimsActivityId, "UPDATE", userClaim.UpdatedBy,
        //            JsonConvert.SerializeObject(userClaim));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new UserClaimResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new UserClaimResponse(userClaim, "Your request sent for approval");
        //        }
        //    }

        //    userClaimInDb.Name = userClaim.Name;
        //    userClaimInDb.DisplayName = userClaim.DisplayName;
        //    userClaimInDb.Description = userClaim.Description;
        //    userClaimInDb.DefaultClaim = userClaim.DefaultClaim;
        //    userClaimInDb.UserConsent = userClaim.UserConsent;
        //    userClaimInDb.MetadataPublish = userClaim.MetadataPublish;
        //    userClaimInDb.UpdatedBy = userClaim.UpdatedBy;
        //    userClaimInDb.ModifiedDate = DateTime.Now;

        //    try
        //    {
        //        _unitOfWork.UserClaims.Update(userClaimInDb);
        //        await _unitOfWork.SaveAsync();
        //        return new UserClaimResponse(userClaimInDb, "Attribute updated successfully");
        //    }
        //    catch
        //    {
        //        _logger.LogError("UserClaim Update failed");
        //        return new UserClaimResponse("An error occurred while updating the Attribute." +
        //            " Please contact the admin.");
        //    }
        //}
        //public async Task<IEnumerable<UserClaim>> ListUserClaimAsync()
        //{
        //    return await _unitOfWork.UserClaims.ListAllUserClaimAsync();
        //}

        //public async Task<UserClaimResponse> DeleteUserClaimAsync(int id, string updatedBy,
        //        bool makerCheckerFlag = false)
        //{
        //    var userClaimInDb = await _unitOfWork.UserClaims.GetByIdAsync(id);
        //    if (null == userClaimInDb)
        //    {
        //        return new UserClaimResponse("Attribute not found");
        //    }

        //    try
        //    {
        //        _unitOfWork.UserClaims.Remove(userClaimInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new UserClaimResponse(userClaimInDb,
        //            "Attribute deleted successfully");
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("DeleteUserClaimAsync failed : {0}", error.Message);
        //        return new UserClaimResponse(
        //            "An error occurred,Please contact the admin.");
        //    }
        //}

        public async Task<Dictionary<string, string>> GetAttributes()
        {
            var attributesPair = new Dictionary<string, string>();

            //var claimsList = await _unitOfWork.UserClaims.ListAllUserClaimAsync();
            var claimsList = await GetAttributeListAsync();

            foreach (var claim in claimsList)
            {
                attributesPair[claim.Name] = claim.Id.ToString();
            }

            return attributesPair;
        }

        public async Task<Dictionary<string, string>> GetAttributeNameDisplayNameAsync()
        {
            var attributesPair = new Dictionary<string, string>();

            //var claimsList = await _unitOfWork.UserClaims.ListAllUserClaimAsync();
            var claimsList = await GetAttributeListAsync();

            foreach (var claim in claimsList)
            {
                attributesPair[claim.Name] = claim.DisplayName;
            }

            return attributesPair;
        }

        public async Task<Dictionary<string, bool>> GetAttributeNameMandatoryAsync()
        {
            var attributesPair = new Dictionary<string, bool>();

            var claimsList = await GetAttributeListAsync();

            foreach (var claim in claimsList)
            {
                attributesPair[claim.Name] = claim.DefaultClaim;
            }

            return attributesPair;
        }

        ///api implementation

        public async Task<UserClaimResponse> AddAttributeAsync(UserClaimListDTO userClaim,
    bool makerCheckerFlag = false)
        {
 
            try
            {

                var jsonContent = new StringContent(JsonConvert.SerializeObject(userClaim),
                            Encoding.UTF8, "application/json");


                HttpResponseMessage response = await _client.PostAsync("api/Attribute/add-attribute", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var userClaimData = JsonConvert.DeserializeObject<UserClaimListDTO>(apiResponse.Result.ToString());
                        return new UserClaimResponse(userClaimData, "Attribute created successfully");
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new UserClaimResponse(apiResponse.Message);

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
                return new UserClaimResponse("An error occurred while creating the Attribute." +
                    " Please contact the admin.");
            }

            return null;
        }

        
        public async Task<IEnumerable<UserClaimListDTO>> GetAttributeListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/Attribute/get-attribute-list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var attributes = JsonConvert.DeserializeObject<List<UserClaimListDTO>>(apiResponse.Result.ToString());
                        return attributes;
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
         
        public async Task<IEnumerable<UserClaimListDTO>> GetAllAttributeListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/Attribute/get-all-attribute-list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var attributes = JsonConvert.DeserializeObject<IEnumerable<UserClaimListDTO>>(apiResponse.Result.ToString());
                        return attributes;
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


        public async Task<UserClaimListDTO> GetAttributeByIdAsync(int id)
        {
             
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/Attribute/get-attribute-by-id?id={id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var attribute = JsonConvert.DeserializeObject<UserClaimListDTO>(apiResponse.Result.ToString());

                        return attribute;
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

        public async Task<APIResponse> UpdateAttributeAsync(UserClaimListDTO userClaimdata)
        {
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userClaimdata),
                        Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/Attribute/update-attribute", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new APIResponse(apiResponse.Message);
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

       
        public async Task<UserClaimResponse> DeleteAttributeAsync(int id, string updatedBy, bool makerCheckerFlag = false)
        {
            var userClaimData = await GetAttributeByIdAsync(id);
            if (null == userClaimData)
            {
                return new UserClaimResponse("Attribute not found");
            }

            try
            {
                HttpResponseMessage response = await _client.DeleteAsync($"api/Attribute/delete-attribute?id={id}&UUID");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var  userClaimRes = JsonConvert.DeserializeObject<UserClaim>(apiResponse.Result.ToString());
                        //return new UserClaimResponse(userClaimRes , apiResponse.Message);
                        return new UserClaimResponse(null, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new UserClaimResponse(apiResponse.Message);
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
                return new UserClaimResponse(
                    "An error occurred,Please contact the admin.");
            }
            return null;

        }

    }
}
