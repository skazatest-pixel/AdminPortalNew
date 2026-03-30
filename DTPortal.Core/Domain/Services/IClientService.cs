using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Services.Communication;
using Microsoft.AspNetCore.Mvc.Rendering;
using DTPortal.Core.DTOs;

namespace DTPortal.Core.Domain.Services
{
    public interface IClientService
    {
        //Task<ClientResponse> CreateClientAsync(Client client, bool makerCheckerFlag = false);
        //Task<Client> GetClientAsync(int id);
       //Task<Client> GetClientByAppNameAsync(string appName);
        //Task<Client> GetClientByClientIdAsync(string clientId);
        //Task<ClientResponse> UpdateClientAsync(Client client,
        //    ClientsSaml2 clientsSaml2,
        //    bool makerCheckerFlag = false);
        //Task<ClientResponse> DeleteClientAsync(int id, string updatedBy,
        //    bool makerCheckerFlag = false);
        //Task<IEnumerable<Client>> ListClientAsync();
       //Task<ClientResponse> UpdateClientState(int id, bool isApproved, string reason = null);
        //Task<ClientResponse> DeActivateClientAsync(int id);
        //Task<ClientResponse> ActivateClientAsync(int id);

        //Task<IEnumerable<Client>> ListClientByOrganizationIdAsync(string orgID);
        //Task<IEnumerable<Client>> ListKycClientByOrgUidAsync(string OrgUid);
        //Task<IEnumerable<ClientsAllDTO>> ListSaml2ClientAsync();
        //Task<IEnumerable<ClientsAllDTO>> ListOAuth2ClientAsync();
        //Task<string> GetSaml2Config(string clientId);
        //Task<string[]> GetAllowedOrigins(string origin);
        //Task<string[]> GetAllClientAppNames(string request);
        //Task<ClientsCount> GetAllClientsCount();
        //Task<Dictionary<string, string>> EnumClientIds();
        //Task<IEnumerable<ClientsAllDTO>> ListClientByOrgUidAsync(string OrgUid);
        //Task<Dictionary<string, string>> GetClientsByName(string value);
        //Task<List<SelectListItem>> GetApplicationsList();
        //Task<List<SelectListItem>> GetApplicationsListByOuid(string Ouid);
        //Task<List<string>> GetApplicationsListByOrgId(string orgId);
        //Task<Client> GetClientProfilesAndPurposesAsync(string clientId);
        //Task<string[]> GetClientNamesAsync(string Value);
        //Task<Dictionary<string, string>> GetApplicationsDictionary();
        //Task<ClientResponse> GetClientByApplicationName
        //    (string ApplicationName, string orgId);

       // Task<ClientResponse> DeleteClientByClientId(string clientId);
       //Task<Dictionary<string, ApplicationInfo>> GetClientOrgApplicationMap();


       //------api implemantation -----------------
        Task<IEnumerable<ClientListDTO>> GetClientDataListAsync();
        Task<bool> IsClientExistsAsync(string id);
        Task<ClientResponseDTO> GetClientDataByIdAsync(int id);
         
        Task<ClientResponse> CreateClientDataAsync(ClientDTO client, bool makerCheckerFlag = false);
        Task<ClientResponse> UpdateClientDataAsync(ClientDTO client, ClientsSaml2 clientsSaml2, bool makerCheckerFlag = false);
        Task<IEnumerable<AuthSchemeDTO>> GetAuthSchemeList();
        Task<IEnumerable<ClientListDTO>> GetClientDataListByOrgIdAsync(string orgId);
        Task<IEnumerable<ClientListDTO>> GetKycClientDataListByOrgIdAsync(string orgId);
        Task<ClientResponseDTO> GetClientDataByNameAsync(string ApplicationName);
        Task<ClientResponseDTO> GetClientDataByClientIdAsync(string clientId);
        Task<ClientProfilesPurposesDTO> GetClientProfilesAndPurposesDataAsync(string clientId);
        Task<ClientResponse> DeleteClientDataAsync(int id, string updatedBy, bool makerCheckerFlag = false);
        Task<ClientResponse> DeleteClientDataByClientIdAsync(string clientId);
        Task<ClientResponse> ActivateClientDataAsync(int id);
        Task<ClientResponse> DeactivateClientDataAsync(int id);
        Task<string[]> GetClientDataAppNameAsync(string request);
        Task<ClientsCount> GetAllClientsDataCountAsync();
        Task<Dictionary<string, string>> GetEnumClientDataIdsAsync();
        Task<Dictionary<string, string>> GetEnumClientsDataAsync();

        Task<List<SelectListItem>> GetApplicationsDataListAsync();
        Task<string[]> GetClientDataNamesAsync(string value);

        Task<Dictionary<string, string>> GetApplicationDictionaryDataAsync();

        Task<List<SelectListItem>> GetApplicationsDataListByOrgIdAsync(string orgId);

    }
}
