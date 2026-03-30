using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class OrganizationCategoriesService : IOrganizationCategoriesService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrganizationCategoriesService> _logger;

        public OrganizationCategoriesService(HttpClient httpClient, IConfiguration configuration, ILogger<OrganizationCategoriesService> logger)
        {
            httpClient.BaseAddress = new Uri(configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);
            _client = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResult> GetAllCategories()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("get/all/categories");
                _logger.LogInformation("Get all categories list api call end");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API failed with status {response.StatusCode}");
                    return new ServiceResult(false, "Internal Error");
                }

                var json = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<APIResponse>(json);

                if (apiResponse == null || !apiResponse.Success)
                {
                    _logger.LogError(apiResponse?.Message ?? "API response null");
                    return new ServiceResult(false, apiResponse?.Message ?? "API Error");
                }

                var result = apiResponse.Result != null
                    ? JsonConvert.DeserializeObject<List<SelfServiceCategoryDTO>>(apiResponse.Result.ToString())
                    : new List<SelfServiceCategoryDTO>();

                return new ServiceResult(true, apiResponse.Message, result ?? new List<SelfServiceCategoryDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAllCategories");
                return new ServiceResult(false, ex.Message, new List<SelfServiceCategoryDTO>());
            }
        }
        public async Task<ServiceResult> GetCategoryById(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("get/all/categories");

                if (response.IsSuccessStatusCode)
                {
                    APIResponse apiResponse =
                        JsonConvert.DeserializeObject<APIResponse>(
                            await response.Content.ReadAsStringAsync());

                    if (apiResponse.Success)
                    {
                        var categories = JsonConvert.DeserializeObject<List<CategoryDTOOrg>>(
                            apiResponse.Result.ToString());

                        var category = categories.FirstOrDefault(x => x.Id == id);

                        if (category != null)
                        {
                            return new ServiceResult(true, "Success", category);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new ServiceResult(false, "Category not found", null);
        }

        public async Task<ServiceResult> GetCategoryFieldNameById(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"get/category-fields/by/id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var details = JsonConvert.DeserializeObject<OrgCategoryFieldDetailsDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, details);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, null);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                    return new ServiceResult(false, "Internal Error", null);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, "Internal Error", null);

            }
        }

        public async Task<ServiceResult> GetAllCategoryFields()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/get/all/category-fields");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var details = JsonConvert.DeserializeObject<List<OrganizationFieldDTO>>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message, details);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message, null);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                    return new ServiceResult(false, "Internal Error", null);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex.ToString());
                return new ServiceResult(false, "Internal Error", null);

            }
        }

        public async Task<ServiceResult> UpdateCatogeryFields(int categoryId, string displayName)
        {
            try
            {
                 
                var oldBaseAddress = _client.BaseAddress;

               
                _client.BaseAddress = new Uri(
                    _configuration["APIServiceLocations:SelfServiceOnboardingBaseAddress"]);

                // Encode display name safely
                var encodedLabel = Uri.EscapeDataString(displayName);

                string url = $"update/category/label?id={categoryId}&labelName={encodedLabel}";

                _logger.LogInformation(    "Calling API {BaseAddress} with categoryId {CategoryId}", _client.BaseAddress,   categoryId
     );

                HttpResponseMessage response = await _client.PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(responseContent);

                    APIResponse apiResponse =
                        JsonConvert.DeserializeObject<APIResponse>(responseContent);

                    if (apiResponse.Success)
                    {
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        return new ServiceResult(false, apiResponse.Message);
                    }
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Status: {response.StatusCode}, Response: {error}");
                }

                 
                _client.BaseAddress = oldBaseAddress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new ServiceResult(false, "Failed to update category label.");
        }



        public async Task<ServiceResult> SaveCatogeryFields(OrganizationCategoryAddRequestDTO fieldsDto)
        {
            try
            {
                _logger.LogInformation(fieldsDto.ToString());
                string json = JsonConvert.SerializeObject(fieldsDto,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInformation(json);
                HttpResponseMessage response = await _client.PostAsync("api/post/add-categories", content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ServiceResult(false, apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                           $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new ServiceResult(false, "An error occurred while updating Category fields. Please try later.");
        }

        public async Task<ServiceResult> DeleteCategoryAsync(int id)
        {
            try
            {
                //string json = JsonConvert.SerializeObject(id,
                //        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"api/post/delete-org-categories/{id}",null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        // return JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());  
                        // var status = JsonConvert.DeserializeObject<AgentListDTO>(apiResponse.Result.ToString());
                        return new ServiceResult(true, apiResponse.Message);
                    }
                    else
                    {
                        return new ServiceResult(false, apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with URI={response.RequestMessage.RequestUri} failed " +
                               $"with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError("GetTemplateDetailsAsync Exception :  {0}", ex.Message);
            }

            return null;
        }

    }
}
