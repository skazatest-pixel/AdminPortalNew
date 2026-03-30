using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IClientRepository : IGenericRepository<ClientsAllDTO>
    {
        //Task<bool> IsClientExistsAsync(string clientName);
        //Task<bool> IsClientExistsWithNameAsync(ClientsAllDTO client);
        //Task<bool> IsClientExistsWithRedirecturlAsync(ClientsAllDTO client);
        //Task<bool> IsClientExistsWithAppUrlAsync(ClientsAllDTO client);
        //Task<bool> IsClientExistsWithAppNameAsync(ClientsAllDTO client);
        ////Task<ClientsAllDTO> GetClientByClientIdAsync(string clientId);
        ////Task<ClientsAllDTO> GetClientByAppNameAsync(string appName);
        ////Task<ClientsAllDTO> GetClientByClientIdWithSaml2Async(string clientId);
        ////Task<ClientsAllDTO> GetClientByIdWithSaml2Async(int id);

        ////Task<IEnumerable<ClientsAllDTO>> ListClientByOrganizationIdAsync(string orgID);
        //Task<IEnumerable<ClientsAllDTO>> ListSaml2ClientAsync();
        ////Task<IEnumerable<ClientsAllDTO>> ListOAuth2ClientAsync();
        ////Task<IEnumerable<ClientsAllDTO>> ListAllClient();
        //Task<string[]> GetAllowedOrigins();
        //Task<string[]> GetAllClientAppNames(string request);
        //Task<int> GetActiveClientsCount();
        //Task<int> GetInActiveClientsCount();
        //Task<IEnumerable<ClientsAllDTO>> ListClientByOrgUidAsync(string OrgUid);
        //Task<IEnumerable<ClientsAllDTO>> GetClientsList();
        ////Task<ClientsAllDTO> GetClientProfilesAndPurposesAsync(string clientId);
        //Task<string[]> GetClientNamesAsync(string Value);
        //Task<IEnumerable<ClientsAllDTO>> GetKycClientsList();
        //Task<IEnumerable<ClientsAllDTO>> ListKycClientByOrgUidAsync(string OrgUid);
        //Task<ClientsAllDTO> ListClientByApplicationNameAsync(
        //    string OrgUid, string applicationName);
    }
}
