using DTOs.Requests;
using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
public class AuthenticationController : BaseController
{
    private readonly IValidator<ClientRegistrationRequest> _clientRegistrationRequestValidator;
    private readonly IValidator<ClientResetCodeCheckRequest> _clientResetCodeCheckRequestValidator;
    private readonly IAuthenticationService _authenticationService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartService _cartService;

    public AuthenticationController(
        IValidator<ClientRegistrationRequest> clientRegistrationRequestValidator,
        IValidator<ClientResetCodeCheckRequest> clientResetCodeCheckRequestValidator,
        IAuthenticationService authenticationService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICartService cartService
    )
    {
        _clientRegistrationRequestValidator = clientRegistrationRequestValidator;
        _clientResetCodeCheckRequestValidator = clientResetCodeCheckRequestValidator;
        _authenticationService = authenticationService;
        _signInManager = signInManager;
        _userManager = userManager;
        _cartService = cartService;
    }

    [HttpPost("registration")]
    [AllowAnonymous]
    public async Task<IActionResult> ClientRegistration(
        [FromBody] ClientRegistrationRequest request
    )
    {
        await _clientRegistrationRequestValidator.ValidateAndThrowAsync(request);

        bool result = await _authenticationService.RegisterAsync(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null && result)
        {
            await _userManager.AddToRoleAsync(user, ApplicationRoles.Client.Value);
            await _cartService.CreateCarteAsync(user.Id);
            await _authenticationService.SendConfirmationEmail(user);
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> UserLogout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost("checkResetCode")]
    public async Task<IActionResult> UserCheckResetCode(ClientResetCodeCheckRequest request)
    {
        await _clientResetCodeCheckRequestValidator.ValidateAsync(request);

        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return NotFound();

        var result = await _authenticationService.CheckResetCodeAsync(user, request);

        if (result)
            return Ok();

        return BadRequest();
    }
}
