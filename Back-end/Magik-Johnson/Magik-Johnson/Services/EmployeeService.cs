using DTOs.Responses;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Services;

public class EmployeeService : IEmployeeService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> AddAsync(EmployeeRegistrationRequest request)
    {
        ApplicationUser user = new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserName = request.Email,
            EmailConfirmed = true,
            HiringDate = request.HiringDate,
        };

        var succeeded = false;
        var response = await _userManager.CreateAsync(user, request.Password);

        //Si creation réussi, ajouter role employé
        if (response.Succeeded)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(
                user,
                ApplicationRoles.Employee.Value
            );
            succeeded = addRoleResult.Succeeded;
        }

        if (succeeded && request.IsAccountEnabled == false)
        {
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            succeeded = result.Succeeded;
        }

        return succeeded;
    }

    public async Task<EmployeeResponse?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return null;
        }
        else
        {
            return new EmployeeResponse()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                IsAccountLocked = await _userManager.IsLockedOutAsync(user),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "",
                HiringDate = user.HiringDate,
            };
        }
    }

    public async Task<List<EmployeeResponse>> ListAsync()
    {
        var users = await _userManager.GetUsersInRoleAsync(
            ApplicationRoles.Employee.Value.ToString()
        );
        var result = new List<EmployeeResponse>();

        foreach (var user in users)
        {
            result.Add(
                new EmployeeResponse()
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsAccountLocked = await _userManager.IsLockedOutAsync(user),
                    Role = ApplicationRoles.Employee.Value.ToString(),
                    HiringDate = user.HiringDate,
                }
            );
        }

        result = result.OrderByDescending(r => !r.IsAccountLocked).ToList();

        return result;
    }

    public async Task<bool> LockAccountAsync(string accountId)
    {
        var user = await _userManager.FindByIdAsync(accountId);

        if (user == null)
        {
            return false;
        }
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return result.Succeeded;
    }

    public async Task<bool> UnlockAccountAsync(string accountId)
    {
        var user = await _userManager.FindByIdAsync(accountId);

        if (user == null)
        {
            return false;
        }
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
        var IsAccountLocked = await _userManager.IsLockedOutAsync(user);

        return result.Succeeded;
    }

    public async Task<bool> UpdateAsync(EmployeeUpdateRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id);

        if (user == null)
        {
            return false;
        }

        user.UserName = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        var response = await _userManager.UpdateAsync(user);

        return response.Succeeded;
    }
}
