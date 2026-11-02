using DTOs.Requests;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;

namespace Magik_Johnson.Services.Interfaces;

public interface IAddressService
{
    Task AddAsync(string userId, AddressRegistrationRequest request);

    Task UpdateAsync(Guid addressId, string userId, AddressRegistrationRequest request);
    Task UpdateForClientAsync(Guid addressId, AddressRegistrationRequest request);

    Task DeleteAsync(Guid addressId, string userId);

    Country? GetCountryByName(string name);
    Task<List<AddressResponse>> GetAddressByUserIdAsync(string userId);
}
