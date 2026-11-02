using DTOs.Responses;
using Magik_Johnson.DTOs.Requests;

namespace Magik_Johnson.Services.Interfaces;

public interface IEmployeeService
{
    Task<bool> AddAsync(EmployeeRegistrationRequest request);
    Task<EmployeeResponse?> GetByIdAsync(string id);
    Task<List <EmployeeResponse>> ListAsync();
    Task<bool> LockAccountAsync(string accountId);
    Task<bool> UnlockAccountAsync(string accountId);
    Task<bool> UpdateAsync(EmployeeUpdateRequest request);
}
