using DTOs.Responses;
using Magik_Johnson.DTOs.Requests;

namespace Magik_Johnson.Services.Interfaces;

public interface IClientService
{

    Task<List<ClientResponse>> ListAsync();
    Task<bool> LockAccountAsync(string accountId);
    Task<bool> UnlockAccountAsync(string accountId);
    Task<List<ClientResponse>> SearchByEmailAsync(string emailQuery, int limit = 10);
    Task<ClientResponse?> GetByIdAsync(string id);
    Task<bool> UpdateAsync(ClientUpdateRequest request);
}
