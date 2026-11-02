using System.Text;
using DTOs.Requests;
using Magik_Johnson.CustomException;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Magik_Johnson.Services;

public class ResetPasswordService : IResetPasswordService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailSender _emailSender;

    public ResetPasswordService(UserManager<ApplicationUser> userManager, EmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task SendResetCode(ClientForgotPasswordRequest request)
    {
        ApplicationUser user =
            await _userManager.FindByEmailAsync(request.Email)
            ?? throw new RecordNotFoundException("l'utilisateur n'a pas été trouvé");

        if (await _userManager.IsLockedOutAsync(user))
        {
            return;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

        await _emailSender.SendPasswordResetCodeAsync(user, user.Email!, encodedToken);
    }

    public async Task<bool> ResetPassword(ClientResetPasswordRequest request)
    {
        ApplicationUser user =
            await _userManager.FindByEmailAsync(request.Email)
            ?? throw new RecordNotFoundException("l'utilisateur n'a pas été trouvé");

        if (await _userManager.IsLockedOutAsync(user))
        {
            return false;
        }

        var bytes = WebEncoders.Base64UrlDecode(request.ResetCode);

        string resetToken = Encoding.UTF8.GetString(bytes);

        bool isValidToken = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
            resetToken
        );

        if (!isValidToken)
        {
            return false;
        }

        await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        return true;
    }
}
