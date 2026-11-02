using DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task UpdateUserAsync(ApplicationUser user, UpdateUserRequest request)
    {
        user.UserName = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> UpdateUserPassword(
        ApplicationUser user,
        UpdatePasswordRequest request
    )
    {
        return await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );
    }
}
