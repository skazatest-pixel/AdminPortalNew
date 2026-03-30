using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Services
{  
    public class ConfigurationService : IConfigurationService
    {
        private IUnitOfWork _unitOfWork;
        // Initialize logger.
        private readonly ILogger<ConfigurationService> _logger;
        private readonly IMCValidationService _mcValidationService;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        public ConfigurationService(IUnitOfWork unitOfWork,
            ILogger<ConfigurationService> logger,
            IMCValidationService mcValidationService,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;
            httpClient.BaseAddress = new Uri(_configuration["APIServiceLocations:IDPConfigurationBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");


            _unitOfWork = unitOfWork;
            _logger = logger;
            _mcValidationService = mcValidationService;
            _client = httpClient;
        }

        public async Task<IList<string>> GetAllScopes()
        {
            _logger.LogDebug("-->GetAllScopes");

            // Get Configuration
            var configObject = await GetConfigurationAsync<JObject>
                ("IDP_Configuration");

            if (null == configObject)
            {
                _logger.LogError("GetConfigurationAsync Failed");
                return null;
            }

            // Get supported scopes list
            var openidconnect = configObject.SelectToken("openidconnect");
            if (null == openidconnect)
            {
                _logger.LogError("Get scopes_supported Failed");
                return null;
            }


            // Get supported scopes list
            var scopes_supported = openidconnect.SelectToken("scopes_supported")
                .Values<string>().ToList();
            if (null == scopes_supported)
            {
                _logger.LogError("Get scopes_supported Failed");
                return null;
            }

            return scopes_supported;
        }

        public async Task<IList<string>> GetAllGrantTypes()
        {
            _logger.LogDebug("-->GetAllGrantTypes");

            // Get Configuration
            var configObject = await GetConfigurationAsync<JObject>
                ("IDP_Configuration");
            if (null == configObject)
            {
                _logger.LogError("GetConfigurationAsync Failed");
                return null;
            }

            // Get supported scopes list
            var openidconnect = configObject.SelectToken("openidconnect");
            if (null == openidconnect)
            {
                _logger.LogError("Get scopes_supported Failed");
                return null;
            }

            // Get supported grant types
            var grantTypesSupported = openidconnect.SelectToken("grant_types_supported")
                .Values<string>().ToList();
            if (null == grantTypesSupported)
            {
                _logger.LogError("Get scopes_supported Failed");
                return null;
            }

            _logger.LogDebug("<--GetAllGrantTypes");
            return grantTypesSupported;
        }
        public T GetPlainConfiguration<T>(string configName) where T : class
        {
            _logger.LogDebug("-->GetConfiguration");

            // Get Configuration Record
            var configRecord = _unitOfWork.Configuration.
                GetConfigurationByName(configName);
            if (null == configRecord || null == configRecord.Value)
            {
                _logger.LogError("Get Configuration Record Failed in GetPlainConfiguration");
                return default;
            }


            // Convert Plain data string to object
            T config = JsonConvert.DeserializeObject<T>(configRecord.Value);
            //if (null == config)
            //{
            //    _logger.LogError("Convert Plain data string to object Failed");
            //
            //  return default;
            //}
            if (EqualityComparer<T>.Default.Equals(config, default(T)))
            {
                _logger.LogError("Convert Plain data string to object Failed");

                return default;
            }

            _logger.LogDebug("<--GetConfiguration");
            return config;
        }
        
        public T GetConfiguration<T>(string configName) where T : class
        {
            _logger.LogDebug("-->GetConfiguration");

            // Get Configuration Record
            var configRecord = _unitOfWork.Configuration.
                GetConfigurationByName(configName);
            if (null == configRecord || null == configRecord.Value)
            {
                _logger.LogError("ConfigName - " + configName.ToString());
                _logger.LogError("Get Configuration Record Failed in GetConfiguration");
                return default;
            }

            // Get Plain data from secured data
            var plainData = PKIMethods.Instance.
                PKIDecryptSecureWireData(configRecord.Value);
            if (null == plainData)
            {
                _logger.LogError("PKIDecryptSecureWireData Failed");
                return default;
            }

            // Convert Plain data string to object
            T config = JsonConvert.DeserializeObject<T>(plainData);
            //if (null == config)
            //{
            //    _logger.LogError("Convert Plain data string to object Failed");
            //    return default;
            //}
            if (EqualityComparer<T>.Default.Equals(config, default(T)))
            {
             
                    _logger.LogError("Convert Plain data string to object Failed");
                    return default;
                
            }

            _logger.LogDebug("<--GetConfiguration");
            return config;
        }

      
        public async Task<T> GetConfigurationAsync<T>(string configName)
        {
            _logger.LogDebug("-->GetConfiguration");

            // Get Configuration Record
            var configRecord = await _unitOfWork.Configuration.
                GetConfigurationByNameAsync(configName);
            if (null == configRecord || null == configRecord.Value)
            {
                _logger.LogError("Get Configuration Record Failed in GetConfigurationAsync");
                return default;
            }

            // Get Plain data from secured data
            var plainData = PKIMethods.Instance.
                PKIDecryptSecureWireData(configRecord.Value);
            if (null == plainData)
            {
                _logger.LogError("PKIDecryptSecureWireData Failed");
                return default;
            }

            // Convert Plain data string to object
            T config = JsonConvert.DeserializeObject<T>(plainData);
            //if (null == config)
            //{
            //    _logger.LogError("Convert Plain data string to object Failed");
            //    return default;
            //}
            if (EqualityComparer<T>.Default.Equals(config, default(T)))
            {

                _logger.LogError("Convert Plain data string to object Failed");
                return default;

            }

            _logger.LogDebug("<--GetConfiguration");
            return config;
        }

        public ConfigurationResponse SetConfiguration(
                string configName, object config)
        {
            _logger.LogDebug("-->SetConfiguration");

            // Get Configuration Record
            var configRecord = _unitOfWork.Configuration.
                GetConfigurationByName(configName);
            if (null == configRecord || null == configRecord.Value)
            {
                _logger.LogError("Get Configuration Record Failed in set configuration");
                return null;
            }

            // Convert Configuration Object to string
            var serializedConfig = JsonConvert.SerializeObject(config);
            if (null == serializedConfig)
            {
                _logger.LogError("Convert Configuration Object to string Failed");
                return null;
            }

            // Create Secure data from plain data
            var secureData = PKIMethods.Instance.
                PKICreateSecureWireData(serializedConfig);
            if (null == secureData)
            {
                _logger.LogError("PKICreateSecureWireData Failed");
                return null;
            }

            // Keep the updated data
            configRecord.Value = secureData;

            try
            {
                _unitOfWork.Configuration.Update(configRecord);
                _unitOfWork.Save();
                return new ConfigurationResponse(configRecord);
            }
            catch
            {
                return null;
            }
        }

      
        public async Task<ConfigurationResponse> SetConfigurationAsync(
                string configName, object config, string updatedBy,
                bool makerCheckerFlag = false)
        {
            _logger.LogDebug("-->SetConfiguration");
            var secureData = string.Empty;

            // Get Configuration Record
            var configRecord = await _unitOfWork.Configuration.
                GetConfigurationByNameAsync(configName);
            if (null == configRecord || null == configRecord.Value)
            {
                _logger.LogError("Get Configuration Record Failed in set config async");
                return null;
            }

            // Check isMCEnabled
            var isEnabled = await _mcValidationService.IsMCEnabled(15);

            if (false == makerCheckerFlag && true == isEnabled)
            {

                configurationMCRequest mCRequest = new configurationMCRequest()
                {
                    configName = configName,
                    requestData = config
                };

                // Validation in makerchecker table
                var response = await _mcValidationService.IsCheckerApprovalRequired(
                    ActivityIdConstants.ConfigurationActivityId,
                    "UPDATE", updatedBy,
                    JsonConvert.SerializeObject(mCRequest));
                if (!response.Success)
                {
                    _logger.LogError("CheckApprovalRequired Failed");
                    return new ConfigurationResponse(response.Message);
                }
                if (response.Result)
                {
                    return new ConfigurationResponse(configRecord, "Your request sent for approval");
                }
            }

            try
            {
                // Convert Configuration Object to string
                var serializedConfig = JsonConvert.SerializeObject(config);
                if (null == serializedConfig)
                {
                    _logger.LogError("Convert Configuration Object to string Failed");
                    return null;
                }

                // Create Secure data from plain data
                secureData = PKIMethods.Instance.
                    PKICreateSecureWireData(serializedConfig);
                if (null == secureData)
                {
                    _logger.LogError("PKICreateSecureWireData Failed");
                    return null;
                }

                // Keep the updated data
                configRecord.Value = secureData;
                configRecord.UpdatedBy = updatedBy;
                configRecord.ModifiedDate = DateTime.Now;

                _unitOfWork.Configuration.Update(configRecord);
                await _unitOfWork.SaveAsync();
                return new ConfigurationResponse(configRecord, "Configuration updated successfully");
            }
            catch
            {
                return null;
            }
        }

        public FaceThreshold GetThreshold()
        {
            var Threshold = _unitOfWork.Threshold.GetThreshold();
            return Threshold;
        }

        //public async Task<string> GetActiveAuthenticationId()
        //{
        //    var res = await _unitOfWork.Configuration.GetConfigurationByNameAsync("DEFAULT_AUTH_SCHEME");
        //    if (res == null)
        //    {
        //        return "";
        //    }
        //    return res.Value;
        //}
        
        //public async Task<ConfigurationResponse> UpdateDefaultAuthScheme(string Id)
        //{
        //    try
        //    {
        //        var configuration = await _unitOfWork.Configuration.GetConfigurationByNameAsync("DEFAULT_AUTH_SCHEME");

        //        configuration.Value = Id;

        //        _unitOfWork.Configuration.Update(configuration);

        //        _unitOfWork.Save();

        //        return new ConfigurationResponse(configuration, "Configuration updated successfully");
        //    }
        //    catch (Exception)
        //    {
        //        _logger.LogError("Failed to update configuration");
        //        return new ConfigurationResponse("Failed to update Configuration");
        //    }
        //}

        //Api Implementations functions
        public async Task<DefaultAuthenticationResponseDTO> GetDefaultAuthenticationSchemeAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/Configuration/get-default-authentication-scheme");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<DefaultAuthenticationResponseDTO>(apiResponse.Result.ToString());
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
        
        public async Task<ConfigurationResponse> UpdateDefaultAuthenticationSchemeAsync(string Id)
        {
            try
            {
                HttpResponseMessage response = await _client.PutAsync($"api/Configuration/update-default-authentication-scheme/{Id}", null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse>(json);
                    
                    if (apiResponse.Success)
                    {
                        var config = ((JObject)apiResponse.Result)
                                         .ToObject<Configuration>();
                        return new ConfigurationResponse(config, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ConfigurationResponse(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");
                }
            }
            catch (Exception)
            {
                _logger.LogError("Failed to update configuration");
                return new ConfigurationResponse("Failed to update Configuration");
            }
            return null;
        }


        public async Task<ApplicationConfigurationDTO> GetApplicationConfigurationAsync()
        {

            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/Configuration/get-application-configuration");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return JsonConvert.DeserializeObject<ApplicationConfigurationDTO>(apiResponse.Result.ToString());
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

        public async Task<APIResponse> UpdateApplicationConfiguration(ApplicationConfigurationDTO appConfig , string updatedBy , bool makerCheckerFlag = false)
        {

            try
            {
                appConfig.UUID = updatedBy;

                var jsonContent = new StringContent(JsonConvert.SerializeObject(appConfig),
                            Encoding.UTF8, "application/json");
            

                HttpResponseMessage response = await _client.PostAsync("api/Configuration/update-application-configuration", jsonContent);
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
    }
}
