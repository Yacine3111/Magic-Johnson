using DTOs.Requests;
using Magik_Johnson.Models;
using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Services.Interfaces;

public interface IUserService
{
    Task UpdateUserAsync(ApplicationUser user, UpdateUserRequest request);

    Task<IdentityResult> UpdateUserPassword(ApplicationUser user, UpdatePasswordRequest request);
}
