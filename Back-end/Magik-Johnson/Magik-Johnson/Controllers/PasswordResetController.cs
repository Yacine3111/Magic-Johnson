using DTOs.Requests;
using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

public class PasswordResetController : BaseController
{
    private readonly IResetPasswordService _resetPasswordService;
    private readonly IValidator<ClientForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ClientResetPasswordRequest> _resetPasswordValidator;

    public PasswordResetController(
        IResetPasswordService resetPasswordService,
        IValidator<ClientForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ClientResetPasswordRequest> resetPasswordValidator
    )
    {
        _resetPasswordService = resetPasswordService;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    [HttpPost("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ClientForgotPasswordRequest request)
    {
        await _forgotPasswordValidator.ValidateAndThrowAsync(request);

        await _resetPasswordService.SendResetCode(request);

        return Ok();
    }

    [HttpPost("reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ClientResetPasswordRequest request)
    {
        await _resetPasswordValidator.ValidateAndThrowAsync(request);

        bool result = await _resetPasswordService.ResetPassword(request);

        if (!result)
            BadRequest();

        return Ok();
    }
}
