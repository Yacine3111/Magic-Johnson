using DTOs.Responses;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Services;

public class ClientService : IClientService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ClientService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<ClientResponse>> ListAsync()
    {
        var users = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Client.Value);

        var result = new List<ClientResponse>();

        foreach (var user in users)
        {
            result.Add(
                new ClientResponse()
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsAccountLocked = await _userManager.IsLockedOutAsync(user),
                    Role = ApplicationRoles.Client.Value.ToString(),
                }
            );
        }

        result = result.OrderByDescending(r => !r.IsAccountLocked).ToList();

        return result;
    }

    public async Task<List<ClientResponse>> SearchByEmailAsync(string emailQuery, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(emailQuery))
            return new List<ClientResponse>();

        var users = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Client.Value);

        var emailQueryLower = emailQuery.ToLower();

        var exactMatch = users.FirstOrDefault(u =>
            u.Email != null && u.Email.ToLower() == emailQueryLower
        );
        var matchingUsers = new List<ApplicationUser>();

        if (exactMatch != null)
        {
            matchingUsers.Add(exactMatch);
        }
        else
        {
            matchingUsers = users
                .Where(u => u.Email != null && u.Email.ToLower().Contains(emailQueryLower))
                .Take(limit)
                .ToList();
        }

        var result = new List<ClientResponse>();

        foreach (var user in matchingUsers)
        {
            result.Add(
                new ClientResponse()
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsAccountLocked = await _userManager.IsLockedOutAsync(user),
                    Role = ApplicationRoles.Client.Value.ToString(),
                }
            );
        }

        return result;
    }

    public async Task<bool> LockAccountAsync(string clientId)
    {
        var user = await _userManager.FindByIdAsync(clientId);

        if (user == null)
        {
            return false;
        }
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return result.Succeeded;
    }

    public async Task<bool> UnlockAccountAsync(string clientId)
    {
        var user = await _userManager.FindByIdAsync(clientId);

        if (user == null)
        {
            return false;
        }
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
        var IsAccountLocked = await _userManager.IsLockedOutAsync(user);

        return result.Succeeded;
    }

    public async Task<ClientResponse?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return null;
        }
        else
        {
            return new ClientResponse()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                IsAccountLocked = await _userManager.IsLockedOutAsync(user),
                Role = ApplicationRoles.Client.Value.ToString(),
            };
        }
    }

    public async Task<bool> UpdateAsync(ClientUpdateRequest request)
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
