using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace DTPortal.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogClient _LogClient;
        private readonly ILogger<ClientService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IMCValidationService _mcValidationService;
        private readonly ICacheClient _cacheClient;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        public ClientService(ILogger<ClientService> logger,
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

            _client = httpClient;
            _logger = logger;
            _configurationService = configurationService;
            _LogClient = logClient;
            _mcValidationService = mcValidationService;
            _cacheClient = cacheClient;
        }

        //public async Task<ClientResponse> CreateClientAsync(Client client,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogInformation("--->CreateClientAsync");
        //    var isEnabled = false;
        //    int actvityId = 0;

        //    // Get Start Time
        //    var startTime = DateTime.Now.ToString("s");

        //    var isExists = await _unitOfWork.Client.IsClientExistsWithNameAsync(
        //        client);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("Client already exists with given client id");
        //        return new ClientResponse("Client already exists with given" +
        //            " Client Id");
        //    }

        //    isExists = await _unitOfWork.Client.IsClientExistsWithAppNameAsync(
        //        client);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("Client already exists with given application name");
        //        return new ClientResponse("Client already exists with given" +
        //            " Name");
        //    }

        //    isExists = await _unitOfWork.Client.IsClientExistsWithRedirecturlAsync(
        //        client);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("Client already exists with given redirect url");
        //        return new ClientResponse("Client already exists with given" +
        //            " Redirect url");
        //    }

        //    isExists = await _unitOfWork.Client.IsClientExistsWithAppUrlAsync(
        //        client);
        //    if (true == isExists)
        //    {
        //        _logger.LogError("Client already exists with given application url");
        //        return new ClientResponse("Client already exists with given" +
        //            " Application url");
        //    }

        //    // Check isMCEnabled
        //    if (client.Type == "SAML2")
        //    {
        //        isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ClientSaml2ActivityId);
        //        actvityId = ActivityIdConstants.ClientSaml2ActivityId;
        //    }
        //    if (client.Type == "OAUTH2")
        //    {
        //        isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ClientActivityId);
        //        actvityId = ActivityIdConstants.ClientActivityId;
        //    }

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        // Validation in makerchecker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired
        //            (actvityId,
        //            "CREATE", client.CreatedBy,
        //            JsonConvert.SerializeObject(client));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new ClientResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new ClientResponse(client, "Your request sent for approval");
        //        }
        //    }

        //    try
        //    {
        //        client.CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        //        client.ModifiedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        //        client.Status = "ACTIVE";
        //        client.Hash = "na";
        //        client.IsKycApplication = false;
        //        client.PublicKeyCert = Convert.ToBase64String(Encoding.UTF8.GetBytes(client.PublicKeyCert));
        //        if (client.GrantTypes.Contains("authorization_code_with_pkce"))
        //        {
        //            client.WithPkce = true;
        //        }
        //        else
        //        {
        //            client.WithPkce = false;
        //        }

        //        await _unitOfWork.Client.AddAsync(client);
        //        await _unitOfWork.SaveAsync();

        //        // Test for central log
        //        LogMessage logMessage = new LogMessage();
        //        logMessage.identifier = client.ClientId;
        //        logMessage.transactionID = Guid.NewGuid().ToString();
        //        logMessage.serviceName = LogClientServices.SPOnboarded;
        //        logMessage.startTime = startTime;
        //        logMessage.endTime = DateTime.Now.ToString("s");
        //        logMessage.logMessage = "Service Provider Onboarded";
        //        logMessage.logMessageType = "SUCCESS";
        //        logMessage.transactionType = "BUSINESS";
        //        logMessage.correlationID = logMessage.transactionID;
        //        logMessage.serviceProviderName = client.ClientId;
        //        logMessage.serviceProviderAppName = client.ApplicationName;

        //        var checkSum = PKIMethods.Instance.AddChecksum(JsonConvert.SerializeObject(
        //            logMessage));
        //        logMessage.checksum = checkSum;

        //        // Send log message to central log server
        //        int result = await _LogClient.SendCentralLogMessage(logMessage);
        //        if (0 != result)
        //        {
        //            _logger.LogError("Failed to send log message to central log server");
        //            return new ClientResponse("Failed to send log message to central " +
        //                "log server");
        //        }

        //        result = await _LogClient.SendServiceLogMessage(logMessage);
        //        if (0 != result)
        //        {
        //            _logger.LogError("Failed to send log message to service log server");
        //            return new ClientResponse("Failed to send log message to service " +
        //                "log server");
        //        }

        //        _logger.LogInformation("<---CreateClientAsync");

        //        return new ClientResponse(client, "Client created successfully");
        //    }
        //    catch
        //    {
        //        _logger.LogError("client AddAsync failed");
        //        _logger.LogInformation("<---CreateClientAsync");
        //        return new ClientResponse("An error occurred while creating the client." +
        //            " Please contact the admin.");
        //    }
        //}

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        //public async Task<Client> GetClientAsync(int id)
        //{
        //    _logger.LogInformation("--->GetClientAsync");
        //    var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }

        //    if (null != clientInDb.PublicKeyCert)
        //    {
        //        try
        //        {
        //            clientInDb.PublicKeyCert = Encoding.UTF8.GetString(Convert.FromBase64String(
        //                clientInDb.PublicKeyCert));
        //        }
        //        catch (Exception error)
        //        {
        //            // do nothing
        //            _logger.LogError("GetClientAsync Failed: {0}", error.Message);
        //        }
        //    }

        //    if (clientInDb.Type == "SAML2")
        //    {
        //        return await _unitOfWork.Client.GetClientByIdWithSaml2Async(id);
        //    }

        //    return clientInDb;
        //}
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //public async Task<Client> GetClientByAppNameAsync(string appName)
        //{
        //    _logger.LogInformation("--->GetClientByAppNameAsync");
        //    var clientInDb = await _unitOfWork.Client.GetClientByAppNameAsync(appName);
        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }

        //    if (clientInDb.Type == "SAML2")
        //    {
        //        return await _unitOfWork.Client.GetClientByIdWithSaml2Async(clientInDb.Id);
        //    }

        //    return clientInDb;
        //}
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //public async Task<Client> GetClientByClientIdAsync(string clientId)
        //{
        //    _logger.LogDebug("--->GetClientByClientIdAsync");
        //    var clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(clientId);
        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }

        //    if (clientInDb.Type == "SAML2")
        //    {
        //        return await _unitOfWork.Client.GetClientByIdWithSaml2Async(clientInDb.Id);
        //    }

        //    return clientInDb;
        //}

        //public async Task<Client> GetClientProfilesAndPurposesAsync(string clientId)
        //{
        //    _logger.LogDebug("--->GetClientByClientIdAsync");
        //    var clientInDb = await _unitOfWork.Client.GetClientProfilesAndPurposesAsync(clientId);

        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }

        //    return clientInDb;
        //}

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //public async Task<ClientResponse> UpdateClientMCOffAsync(Client client,
        //    ClientsSaml2 clientsSaml2)
        //{

        //    var isExists = await _unitOfWork.Client.IsClientExistsAsync(client.ClientId);
        //    if (false == isExists)
        //    {
        //        _logger.LogError("Client not found");
        //        return new ClientResponse("Client not found");
        //    }
        //    _unitOfWork.Client.Reload(client);
        //    var clientInDb = _unitOfWork.Client.GetById(client.Id);
        //    if (null == clientInDb)
        //    {
        //        _logger.LogError("Client not found");
        //        return new ClientResponse("Client not found");
        //    }

        //    _unitOfWork.Client.Reload(clientInDb);


        //    if (clientInDb.RedirectUri != client.RedirectUri)
        //    {
        //        isExists = await _unitOfWork.Client.IsClientExistsWithRedirecturlAsync(
        //            client);
        //        if (true == isExists)
        //        {
        //            _logger.LogError("Another client with redirect_url found");
        //            return new ClientResponse("Another client with redirect_url found");
        //        }
        //    }

        //    if (clientInDb.ApplicationName != client.ApplicationName)
        //    {
        //        isExists = await _unitOfWork.Client.IsClientExistsWithAppNameAsync(
        //            client);
        //        if (true == isExists)
        //        {
        //            _logger.LogError("Another client with application name found");
        //            return new ClientResponse("Another client with application name found");
        //        }
        //    }

        //    if (clientInDb.ApplicationUrl != client.ApplicationUrl)
        //    {
        //        isExists = await _unitOfWork.Client.IsClientExistsWithAppUrlAsync(
        //            client);
        //        if (true == isExists)
        //        {
        //            _logger.LogError("Another client with application url");
        //            return new ClientResponse("Another client with application url");
        //        }
        //    }

        //    if (clientInDb.Status == "DELETED")
        //    {
        //        _logger.LogError("Client is already deleted");
        //        return new ClientResponse("Client is already deleted");
        //    }
        //    try
        //    {

        //        //clientInDb.Id = client.Id;
        //        clientInDb.Scopes = client.Scopes;
        //        clientInDb.RedirectUri = client.RedirectUri;
        //        clientInDb.ClientSecret = client.ClientSecret;
        //        clientInDb.ClientId = client.ClientId;
        //        clientInDb.ResponseTypes = client.ResponseTypes;
        //        clientInDb.LogoutUri = client.LogoutUri;
        //        clientInDb.ModifiedDate =
        //        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        //        clientInDb.EncryptionCert = client.EncryptionCert;
        //        if (null != clientInDb.PublicKeyCert)
        //            clientInDb.PublicKeyCert = Convert.ToBase64String(
        //                Encoding.UTF8.GetBytes(client.PublicKeyCert));
        //        clientInDb.GrantTypes = client.GrantTypes;
        //        clientInDb.ApplicationName = client.ApplicationName;
        //        clientInDb.ApplicationType = client.ApplicationType;
        //        clientInDb.ApplicationUrl = client.ApplicationUrl;
        //        clientInDb.OrganizationUid = client.OrganizationUid;

        //        if (client.Type == "SAML2")
        //        {
        //            clientInDb.ClientsSaml2s = new List<ClientsSaml2>()
        //                { clientsSaml2};
        //        }

        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        _logger.LogInformation("<---UpdateClient");
        //        return new ClientResponse(client, "Client updated successfully");
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("Client Update failed : {0}", error.Message);
        //        return new ClientResponse("An error occurred while updating the client." +
        //            " Please contact the admin.");
        //    }

        //}

        //public async Task<ClientResponse> UpdateClientAsync(Client ipClient,
        //    ClientsSaml2 clientsSaml2,
        //    bool makerCheckerFlag = false)
        //{
        //    _logger.LogInformation("--->UpdateClientAsync");

        //    var isEnabled = false;
        //    int activityId = 0;

        //    Client client = ipClient;

        //    //if (false == makerCheckerFlag && false == isEnabled)
        //    //{
        //    //    var clientResp = await UpdateClientMCOffAsync(ipClient, clientsSaml2);

        //    //    return clientResp;
        //    //}

        //    var isExists = await _unitOfWork.Client.IsClientExistsAsync(client.ClientId);
        //    if (false == isExists)
        //    {
        //        _logger.LogError("Client not found");
        //        return new ClientResponse("Client not found");
        //    }
        //    var allClients = await _unitOfWork.Client.GetAllAsync();

        //    foreach (var item in allClients)
        //    {
        //        if (item.ClientId != client.ClientId)
        //        {
        //            if (item.RedirectUri == client.RedirectUri)
        //            {
        //                _logger.LogError("Client already exists with given redirect uri");
        //                return new ClientResponse("Client already exists with given redirect uri");
        //            }
        //            if (item.ApplicationName == client.ApplicationName)
        //            {
        //                _logger.LogError("Client already exists with given application name");
        //                return new ClientResponse("Client already exists with given application name");
        //            }
        //            if (item.ApplicationUrl == client.ApplicationUrl)
        //            {
        //                _logger.LogError("Client already exists with given application url");
        //                return new ClientResponse("Client already exists with given application url");
        //            }
        //        }
        //    }





        //    var clientInDb = _unitOfWork.Client.GetById(client.Id);
        //    if (null == clientInDb)
        //    {
        //        _logger.LogError("Client not found");
        //        return new ClientResponse("Client not found");
        //    }


        //    if (clientInDb.Status == "DELETED")
        //    {
        //        _logger.LogError("Client is already deleted");
        //        return new ClientResponse("Client is already deleted");
        //    }

        //    // Check isMCEnabled
        //    if (client.Type == "SAML2")
        //    {
        //        isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ClientSaml2ActivityId);
        //        activityId = ActivityIdConstants.ClientSaml2ActivityId;
        //    }
        //    if (client.Type == "OAUTH2")
        //    {
        //        isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ClientActivityId);
        //        activityId = ActivityIdConstants.ClientActivityId;
        //    }

        //    ClientRequest clientRequest = new ClientRequest()
        //    {
        //        client = client,
        //        ClientSaml2 = clientsSaml2
        //    };


        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        _unitOfWork.DisableDetectChanges();

        //        // Validation in makerchecker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired(
        //            activityId,
        //            "UPDATE",
        //            client.UpdatedBy,
        //            JsonConvert.SerializeObject(clientRequest));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new ClientResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new ClientResponse(client, "Your request sent for approval");
        //        }
        //    }
        //    try
        //    {
        //        clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(client.ClientId);
        //        _logger.LogError("Hi1");
        //        _logger.LogError(clientInDb.ToString());
        //        if (null == clientInDb)
        //        {
        //            _logger.LogError("Client not found");
        //            return new ClientResponse("Client not found");
        //        }
        //        _logger.LogError("Client ID: " + clientInDb.ClientId);
        //        _logger.LogError("Scopes: " + client.Scopes);
        //        _logger.LogError("RedirectUri: " + client.RedirectUri);
        //        _logger.LogError("ClientSecret: " + client.ClientSecret);
        //        _logger.LogError("ResponseTypes: " + client.ResponseTypes);
        //        _logger.LogError("LogoutUri: " + client.LogoutUri);
        //        _logger.LogError("EncryptionCert: " + client.EncryptionCert);
        //        _logger.LogError("PublicKeyCert: " + client.PublicKeyCert);
        //        _logger.LogError("GrantTypes: " + client.GrantTypes);
        //        _logger.LogError("ApplicationName: " + client.ApplicationName);
        //        _logger.LogError("ApplicationType: " + client.ApplicationType);
        //        _logger.LogError("ApplicationUrl: " + client.ApplicationUrl);
        //        _logger.LogError("OrganizationUid: " + client.OrganizationUid);
        //        _logger.LogError("Hi2");
        //        //clientInDb.Id = client.Id;
        //        clientInDb.Scopes = client.Scopes;
        //        clientInDb.RedirectUri = client.RedirectUri;
        //        clientInDb.ClientSecret = client.ClientSecret;
        //        clientInDb.ClientId = client.ClientId;
        //        clientInDb.ResponseTypes = client.ResponseTypes;
        //        clientInDb.LogoutUri = client.LogoutUri;
        //        clientInDb.ModifiedDate =
        //        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        //        clientInDb.EncryptionCert = client.EncryptionCert;
        //        if (null != clientInDb.PublicKeyCert)
        //            clientInDb.PublicKeyCert = Convert.ToBase64String(
        //                Encoding.UTF8.GetBytes(client.PublicKeyCert));
        //        clientInDb.GrantTypes = client.GrantTypes;
        //        clientInDb.ApplicationName = client.ApplicationName;
        //        clientInDb.ApplicationType = client.ApplicationType;
        //        clientInDb.ApplicationUrl = client.ApplicationUrl;
        //        clientInDb.OrganizationUid = client.OrganizationUid;

        //        if (client.Type == "SAML2")
        //        {
        //            clientInDb.ClientsSaml2s = new List<ClientsSaml2>()
        //                { clientsSaml2};
        //        }
        //        _logger.LogInformation("Updating Client Info in DB-->" + clientInDb.ToString());
        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        _logger.LogInformation("<---UpdateClient");
        //        return new ClientResponse(client, "Client updated successfully");
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("Client Update failed : " + error.Message);
        //        _logger.LogError("Client Update failed : " + error.InnerException.ToString());
        //        _logger.LogError("Client Update failed : " + error.InnerException.Message.ToString());
        //        return new ClientResponse("An error occurred while updating the client." +
        //            " Please contact the admin.");
        //    }
        //}
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //public async Task<ClientResponse> DeleteClientAsync(int id, string updatedBy,
        //    bool makerCheckerFlag = false)
        //{
        //    var clientInDb = new Client();

        //    clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
        //    if (null == clientInDb)
        //    {
        //        return new ClientResponse("Client not found");
        //    }

        //    // Check isMCEnabled
        //    var isEnabled = await _mcValidationService.IsMCEnabled(
        //        ActivityIdConstants.ClientActivityId);

        //    if (false == makerCheckerFlag && true == isEnabled)
        //    {
        //        // Validation in makerchecker table
        //        var response = await _mcValidationService.IsCheckerApprovalRequired(
        //            ActivityIdConstants.ClientActivityId,
        //            "DELETE",
        //            updatedBy,
        //            JsonConvert.SerializeObject(new { Id = id, UpdatedBy = updatedBy }));
        //        if (!response.Success)
        //        {
        //            _logger.LogError("CheckApprovalRequired Failed");
        //            return new ClientResponse(response.Message);
        //        }
        //        if (response.Result)
        //        {
        //            return new ClientResponse(clientInDb, "Your request sent for approval");
        //        }
        //    }
        //    try
        //    {

        //        clientInDb.ModifiedDate =
        //        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        //        clientInDb.UpdatedBy = updatedBy;
        //        clientInDb.Status = "DELETED";

        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new ClientResponse(clientInDb, "Client deleted successfully");
        //    }
        //    catch
        //    {
        //        return new ClientResponse("An error occurred while deleting the client." +
        //            " Please contact the admin.");
        //    }

        //}
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //public async Task<ClientResponse> DeActivateClientAsync(int id)
        //{
        //    var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
        //    if (null == clientInDb)
        //    {
        //        return new ClientResponse("Client not found");
        //    }

        //    clientInDb.Status = "DEACTIVATED";

        //    try
        //    {
        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new ClientResponse(clientInDb);
        //    }
        //    catch
        //    {
        //        return new ClientResponse("An error occurred while deleting the client." +
        //            " Please contact the admin.");
        //    }

        //}

        //public async Task<ClientResponse> ActivateClientAsync(int id)
        //{
        //    var clientInDb = await _unitOfWork.Client.GetByIdAsync(id);
        //    if (null == clientInDb)
        //    {
        //        return new ClientResponse("Client not found");
        //    }

        //    clientInDb.Status = "ACTIVE";

        //    try
        //    {
        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new ClientResponse(clientInDb);
        //    }
        //    catch
        //    {
        //        return new ClientResponse("An error occurred while deleting the client." +
        //            " Please contact the admin.");
        //    }

        //}
        //public async Task<IEnumerable<Client>> ListClientAsync()
        //{
        //    return await _unitOfWork.Client.ListAllClient();
        //}



        //public async Task<IEnumerable<Client>> ListClientByOrganizationIdAsync(string orgID)
        //{
        //    return await _unitOfWork.Client.ListClientByOrganizationIdAsync(orgID);
        //}

        //SAML2 controller not being used
        //public async Task<IEnumerable<ClientsAllDTO>> ListSaml2ClientAsync()
        //{
        //    return await _unitOfWork.Client.ListSaml2ClientAsync();
        //}

        //this is getting called in clients api controller from web
        //public async Task<IEnumerable<ClientsAllDTO>> ListClientByOrgUidAsync(string OrgUid)
        //{
        //    return await _unitOfWork.Client.ListClientByOrgUidAsync(OrgUid);
        //}


        //public async Task<IEnumerable<Client>> ListKycClientByOrgUidAsync(string OrgUid)
        //{
        //    return await _unitOfWork.Client.ListKycClientByOrgUidAsync(OrgUid);
        //}

        //this is getting called in clients api controller from web
        //public async Task<IEnumerable<ClientsAllDTO>> ListOAuth2ClientAsync()
        //{
        //    return await _unitOfWork.Client.ListOAuth2ClientAsync();
        //}
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //public async Task<ClientResponse> UpdateClientState(int id,
        //    bool isApproved,
        //    string reason = null)
        //{
        //    var clientInDB = await _unitOfWork.Client.GetByIdAsync(id);
        //    if (clientInDB == null)
        //    {
        //        return new ClientResponse("Role not found");
        //    }

        //    if (isApproved)
        //    {
        //        clientInDB.Status = "ACTIVE";
        //    }
        //    else
        //    {
        //        clientInDB.Status = "BLOCKED";
        //        //.BlockedReason = reason;
        //    }
        //    try
        //    {
        //        _unitOfWork.Client.Update(clientInDB);
        //        await _unitOfWork.SaveAsync();

        //        return new ClientResponse(clientInDB);
        //    }
        //    catch (Exception)
        //    {
        //        // Do some logging stuff
        //        return new ClientResponse($"An error occurred while changing Status" +
        //            $" of the client. Please contact the admin.");
        //    }
        //}
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //string GetConfig(Client clientObj, string idp, string idpCert)
        //{
        //    ClientsSaml2 Saml2Cilent = clientObj.ClientsSaml2s.FirstOrDefault();
        //    /*parse spconfig and get url*/
        //    dynamic spconfig = JsonConvert.DeserializeObject(Saml2Cilent.Config);
        //    dynamic idpConfig = JsonConvert.DeserializeObject(idp);
        //    dynamic idpCertficate = JsonConvert.DeserializeObject(idpCert);

        //    spconfig.entityID = Saml2Cilent.EntityId;
        //    spconfig.signingCert = clientObj.PublicKeyCert;
        //    spconfig.encryptCert = clientObj.EncryptionCert;
        //    spconfig.assertionConsumerService = clientObj.RedirectUri;
        //    spconfig.singleLogoutService = clientObj.LogoutUri;
        //    spconfig.IDPEntityID = idpConfig["entityID"].ToString();
        //    spconfig.IDPsigningCert = idpCertficate["signCertificate"].ToString();
        //    spconfig.IDPencryptCert = idpCertficate["encryptionCertificate"].ToString();
        //    spconfig.IDPsingleSignOnService = idpConfig["singleSignOnService"][0]["Location"].ToString();
        //    spconfig.IDPsingleLogoutService = idpConfig["singleLogoutService"][0]["Location"].ToString();
        //    spconfig.IDPmessageSigningOrder = idpConfig["messageSigningOrder"].ToString();


        //    return JsonConvert.SerializeObject(spconfig, Formatting.Indented).Replace("\r\n", "");
        //}

        //public async Task<string> GetSaml2Config(string clientId)
        //{
        //    try
        //    {
        //        dynamic Result = new JObject();


        //        var configInDB = _configurationService.GetConfiguration<idp_configuration>("IDP_Configuration");

        //        if (configInDB == null)
        //        {
        //            return null;
        //        }
        //        var IDP = configInDB.saml2.ToString().Replace("\r\n", "");
        //        var IDP_cert = configInDB.common.ToString().Replace("\r\n", "");
        //        IDP = IDP.Replace("<client_ID>", clientId);

        //        var response = await _unitOfWork.Client.GetClientByClientIdWithSaml2Async(clientId);
        //        if (response == null)
        //        {
        //            return null;
        //        }

        //        var Config = GetConfig(response, IDP, IDP_cert);
        //        var a = Config.Length;
        //        Result.Success = true;
        //        Result.Config = Config;

        //        return Config;
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("GetSaml2Config Failed: {0}",
        //            error.Message);

        //        dynamic Result = new JObject();
        //        Result.Success = false;
        //        Result.Config = null;
        //        return null;
        //    }
        //}

        //this func is called in idp
        //public async Task<string[]> GetAllowedOrigins(string origin)
        //{
        //    // Get allowed origin list from cache
        //    var cacheAllowedOriginList = await _cacheClient.Get<string[]>("Allowed",
        //        "Origins");
        //    if (null != cacheAllowedOriginList)
        //    {
        //        // Check the origin is in cache list
        //        if (cacheAllowedOriginList.Contains(origin))
        //        {
        //            _logger.LogDebug("Origin Found in Allowed Origin List Cache");
        //            return cacheAllowedOriginList;
        //        }

        //        _logger.LogInformation("Origin not Found in Allowed Origin List Cache");
        //    }
        //    else
        //    {
        //        _logger.LogInformation("Allowed Origin List in Cache is Empty");
        //    }

        //    // Get Allowed Origin List from Database
        //    var allowedOriginList = await _unitOfWork.Client.GetAllowedOrigins();
        //    if (allowedOriginList.Contains(origin))
        //    {
        //        // Add/Update allowed origin list in cache
        //        await _cacheClient.Add("Allowed", "Origins", allowedOriginList);
        //    }

        //    return allowedOriginList;
        //}

        //public async Task<string[]> GetAllClientAppNames(string request)
        //{
        //    return await _unitOfWork.Client.GetAllClientAppNames(request);
        //}

        //public async Task<ClientsCount> GetAllClientsCount()
        //{
        //    var activeClientsCount = await _unitOfWork.Client.GetActiveClientsCount();
        //    var inactiveClientsCount = await _unitOfWork.Client.GetInActiveClientsCount();

        //    ClientsCount clientsCount = new ClientsCount();
        //    clientsCount.Active = activeClientsCount;
        //    clientsCount.InActive = inactiveClientsCount;
        //    return clientsCount;
        //}

        //public async Task<Dictionary<string, string>> EnumClientIds()
        //{
        //    var response = new Dictionary<string, string>();
        //    var clientInDb = await _unitOfWork.Client.ListOAuth2ClientAsync();
        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }

        //    foreach (var item in clientInDb)
        //    {
        //        response.Add(item.ClientId, item.ApplicationName);
        //    }

        //    return response;
        //}

        


        /// <summary>
        /// this is being called in signing credits controller 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public async Task<Dictionary<string, string>> GetClientsByName(string value)
        //{
        //    var response = new Dictionary<string, string>();

        //    var clientInDb = await _unitOfWork.Client.GetClientsList();

        //    if (null == clientInDb)
        //    {
        //        return null;
        //    }
        //    foreach (var item in clientInDb)
        //    {
        //        response.Add(item.ClientId, item.ApplicationName);
        //    }
        //    return response;
        //}

        //public async Task<List<SelectListItem>> GetApplicationsList()
        //{
        //    var result = await _unitOfWork.Client.GetClientsList();
        //    var list = new List<SelectListItem>();
        //    if (result == null)
        //    {
        //        return list;
        //    }
        //    else
        //    {
        //        foreach (var client in result)
        //        {
        //            list.Add(new SelectListItem { Text = client.ApplicationName, Value = client.ClientId });
        //        }

        //        return list;
        //    }
        //}
        //public async Task<List<SelectListItem>> GetApplicationsListByOuid(string Ouid)
        //{
        //    var result = await _unitOfWork.Client.GetClientsList();
        //    var list = new List<SelectListItem>();
        //    if (result == null)
        //    {
        //        return list;
        //    }
        //    else
        //    {
        //        foreach (var client in result)
        //        {
        //            if (client.OrganizationUid == Ouid)
        //            {
        //                list.Add(new SelectListItem { Text = client.ApplicationName, Value = client.ClientId });
        //            }
        //        }
        //        return list;
        //    }
        //}

        /// <summary>
        /// this is being called in signing credits controller 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public async Task<List<string>> GetApplicationsListByOrgId(string orgId)
        //{
        //    var result = await _unitOfWork.Client.ListClientByOrgUidAsync(orgId);
        //    var list = new List<string>();
        //    if (result == null)
        //    {
        //        return list;
        //    }
        //    else
        //    {
        //        foreach (var client in result)
        //        {
        //            list.Add(client.ApplicationName + "," + client.ClientId);
        //        }

        //        return list;
        //    }
        //}
        //public async Task<string[]> GetClientNamesAsync(string Value)
        //{
        //    var result = await _unitOfWork.Client.GetClientNamesAsync(Value);

        //    return result;
        //}

        //public async Task<Dictionary<string, string>> GetApplicationsDictionary()
        //{
        //    try
        //    {
        //        var result = await _unitOfWork.Client.GetClientsList();
        //        var dict = new Dictionary<string, string>();
        //        if (result == null)
        //        {
        //            return dict;
        //        }
        //        else
        //        {
        //            foreach (var client in result)
        //            {
        //                dict.Add(client.ClientId, client.ApplicationName);
        //            }
        //            return dict;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("GetApplicationsDictionary Failed: {0}", ex.Message);
        //        return null;
        //    }
        //}

        //public async Task<ClientResponse> GetClientByApplicationName
        //    (string ApplicationName, string orgId)
        //{
        //    var client = await _unitOfWork.Client.ListClientByApplicationNameAsync
        //        (orgId, ApplicationName);
        //    if (client == null)
        //    {
        //        return new ClientResponse("Client not found");
        //    }
        //    return new ClientResponse(client);
        //}


        //public async Task<ClientResponse> DeleteClientByClientId(string clientId)
        //{
        //    var clientInDb = new Client();

        //    clientInDb = await _unitOfWork.Client.GetClientByClientIdAsync(clientId);
        //    if (null == clientInDb)
        //    {
        //        return new ClientResponse("Client not found");
        //    }

        //    try
        //    {

        //        clientInDb.ModifiedDate =
        //        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        //        clientInDb.Status = "DELETED";

        //        _unitOfWork.Client.Update(clientInDb);
        //        await _unitOfWork.SaveAsync();

        //        return new ClientResponse(clientInDb, "Client Deactivated successfully");
        //    }
        //    catch
        //    {
        //        return new ClientResponse("An error occurred while deleting the client." +
        //            " Please contact the admin.");
        //    }

        //}

        //public async Task<Dictionary<string, ApplicationInfo>> GetClientOrgApplicationMap()
        //{
        //    try
        //    {
        //        var result = await _unitOfWork.Client.GetClientsList();
        //        var dict = new Dictionary<string, ApplicationInfo>();

        //        if (result == null)
        //            return dict;

        //        foreach (var client in result)
        //        {
        //            dict.Add(client.ClientId, new ApplicationInfo
        //            {
        //                ApplicationName = client.ApplicationName,
        //                OrganizationUid = client.OrganizationUid
        //            });
        //        }

        //        return dict;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("GetApplicationsDictionary Failed: {0}", ex.Message);
        //        return null;
        //    }
        //}


        ///api implementation functions
        //ListClientAsync
        public async Task<IEnumerable<ClientListDTO>> GetClientDataListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/clients/get-client-list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clients = JsonConvert.DeserializeObject<List<ClientListDTO>>(apiResponse.Result.ToString());
                        return clients;
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
        


        public async Task<bool> IsClientExistsAsync(string id)
        {
            var client  = await GetClientDataByClientIdAsync(id);
            if (client == null)
            {
                return false;
            }
            return true;
        }

        //GetClientAsync
        public async Task<ClientResponseDTO> GetClientDataByIdAsync(int id)
        {
           

            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-by-id/{id}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var client = JsonConvert.DeserializeObject<ClientResponseDTO>(apiResponse.Result.ToString());

                        return client;
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
        public async Task<IEnumerable<AuthSchemeDTO>> GetAuthSchemeList()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync("api/clients/get-auth-schemas-list");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(content);

                    if (apiResponse.Success)
                    {
                        var authSchemas = JsonConvert.DeserializeObject<IEnumerable<AuthSchemeDTO>>(apiResponse.Result.ToString());

                        return authSchemas;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed with status code={response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return Enumerable.Empty<AuthSchemeDTO>();
        }
        //CreateClientAsync
        public async Task<ClientResponse> CreateClientDataAsync(ClientDTO client, bool makerCheckerFlag = false)
        {
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(client),
                            Encoding.UTF8, "application/json");
               
                HttpResponseMessage response = await _client.PostAsync("api/clients/create-client", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientData = JsonConvert.DeserializeObject<ClientsAllDTO>(apiResponse.Result.ToString());
                        return new ClientResponse(clientData, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ClientResponse(apiResponse.Message);

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
                return new ClientResponse("An error occurred while creating the Attribute." +
                    " Please contact the admin.");
            }

            return null;

        }
        //UpdateClientAsync
        public async Task<ClientResponse> UpdateClientDataAsync(ClientDTO client, ClientsSaml2 clientsSaml2, bool makerCheckerFlag = false)
        {
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(client),
                            Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/clients/update-client", jsonContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientData = JsonConvert.DeserializeObject<ClientsAllDTO>(apiResponse.Result.ToString());
                        return new ClientResponse(clientData, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ClientResponse(apiResponse.Message);

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
                return new ClientResponse("An error occurred while creating the Attribute." +
                    " Please contact the admin.");
            }

            return null;

        }



        //ListClientByOrganizationIdAsync
        public async Task<IEnumerable<ClientListDTO>> GetClientDataListByOrgIdAsync(string orgId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-list_by_orgId?orgId={orgId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clients = JsonConvert.DeserializeObject<IEnumerable<ClientListDTO>>(apiResponse.Result.ToString());
                        return clients;
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

        //ListKycClientByOrgUidAsync
        public async Task<IEnumerable<ClientListDTO>> GetKycClientDataListByOrgIdAsync(string orgId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-kycClient-list_by_orgId?orgId={orgId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientList = JsonConvert.DeserializeObject<IEnumerable<ClientListDTO>>(apiResponse.Result.ToString());
                        return clientList;
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

        
        //GetClientByApplicationName
        public async Task<ClientResponseDTO> GetClientDataByNameAsync(string ApplicationName)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-by-name?appName={ApplicationName}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var client = JsonConvert.DeserializeObject<ClientResponseDTO>(apiResponse.Result.ToString());
                        //return new ClientResponse(client,apiResponse.Message );
                        return client;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        //return new ClientResponse(apiResponse.Message);

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

        //GetClientByClientIdAsync
        public async Task<ClientResponseDTO> GetClientDataByClientIdAsync(string clientId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-by-clientId?clientId={clientId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var client = JsonConvert.DeserializeObject<ClientResponseDTO>(apiResponse.Result.ToString());
                        return client;
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



        //GetClientProfilesAndPurposesAsync
        public async Task<ClientProfilesPurposesDTO> GetClientProfilesAndPurposesDataAsync(string clientId)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-profilesAndPurposes?clientId={clientId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientProfilesPurpose = JsonConvert.DeserializeObject<ClientProfilesPurposesDTO>(apiResponse.Result.ToString());
                        return clientProfilesPurpose;
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

        //DeleteClientAsync
        public async Task<ClientResponse> DeleteClientDataAsync(int id, string updatedBy,bool makerCheckerFlag = false)
        {
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync($"api/clients/delete-client?id={id}&updatedBy={updatedBy}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var client = JsonConvert.DeserializeObject<Client>(apiResponse.Result.ToString());
                        return new ClientResponse(null,apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ClientResponse(apiResponse.Message);

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

        //DeleteClientByClientId
        public async Task<ClientResponse> DeleteClientDataByClientIdAsync(string clientId)
        {
        try
        {
        HttpResponseMessage response = await _client.DeleteAsync($"api/clients/delete-client-by-clientId?clientId={clientId}");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
            if (apiResponse.Success)
            {
                //var client = JsonConvert.DeserializeObject<Client>(apiResponse.Result.ToString());
                return new ClientResponse(null, apiResponse.Message);
            }
            else
            {
                _logger.LogError(apiResponse.Message);
                return new ClientResponse(apiResponse.Message);

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

        //ActivateClientAsync
        public async Task<ClientResponse> ActivateClientDataAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.PutAsync($"api/clients/activate-client?id={id}",null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var client = JsonConvert.DeserializeObject<Client>(apiResponse.Result.ToString());
                        return new ClientResponse(null, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ClientResponse(apiResponse.Message);

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

        //DeActivateClientAsync
        public async Task<ClientResponse> DeactivateClientDataAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.PutAsync($"api/clients/deactivate-client?id={id}", null);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        //var client = JsonConvert.DeserializeObject<Client>(apiResponse.Result.ToString());
                        return new ClientResponse(null, apiResponse.Message);
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return new ClientResponse(apiResponse.Message);

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

        //GetAllClientAppNames
        public async Task<string[]> GetClientDataAppNameAsync(string request)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-clientApp-name?request={request}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var client = JsonConvert.DeserializeObject<string[]>(apiResponse.Result.ToString());
                        return client;
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

        //GetAllClientsCount
        public async Task<ClientsCount> GetAllClientsDataCountAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-all-clients-count");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientsCount = JsonConvert.DeserializeObject<ClientsCount>(apiResponse.Result.ToString());
                        return clientsCount;
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

        //EnumClientIds
        public async Task<Dictionary<string, string>> GetEnumClientDataIdsAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-enum-clientIds");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var enumClientsIds = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse.Result.ToString());
                        return enumClientsIds;
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
        //
        public async Task<Dictionary<string, string>> GetEnumClientsDataAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-enum-clients");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var enumClients = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse.Result.ToString());
                        return enumClients;
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

        //GetApplicationsList
        public async Task<List<SelectListItem>> GetApplicationsDataListAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-applications-list");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var appList= JsonConvert.DeserializeObject<List<SelectListItem>>(apiResponse.Result.ToString());
                        return appList;
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


        //GetClientNamesAsync
        public async Task<string[]> GetClientDataNamesAsync(string value)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-client-names");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var clientNames = JsonConvert.DeserializeObject<string[]>(apiResponse.Result.ToString());
                        return clientNames;
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

        //GetApplicationsDictionary
        public async Task<Dictionary<string, string>> GetApplicationDictionaryDataAsync()
        {
          try
         {
        HttpResponseMessage response = await _client.GetAsync($"api/clients/get-application-dictionary");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
            if (apiResponse.Success)
            {
                var appDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse.Result.ToString());
                return appDictionary;
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

        
        //GetApplicationsListByOuid
        public async Task<List<SelectListItem>> GetApplicationsDataListByOrgIdAsync(string ouid)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"api/clients/get-applications-list-by-orgId");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        var appList = JsonConvert.DeserializeObject<List<SelectListItem>>(apiResponse.Result.ToString());

                        return appList;
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
