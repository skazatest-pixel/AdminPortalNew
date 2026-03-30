using DTPortal.Core.Domain.Models;
using DTPortal.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Domain.Repositories
{
    public interface IScopeRepository : IGenericRepository<ScopeAllListDTO>
    {
        //public Task<bool> IsScopeExistsWithNameAsync(string name);

        //public Task<IEnumerable<ScopeAllListDTO>> ListAllScopeAsync();

        //public Task<ScopeAllListDTO> GetScopeByIdWithClaims(int id);
        //public Task<ScopeAllListDTO> GetScopeByNameAsync(string name);
        //public Task<string[]> GetScopesNamesAsync(string Value);
    }
}
