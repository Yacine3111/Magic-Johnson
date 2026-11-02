using DTOs.Requests;
using FluentValidation;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class ApplicationUserController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUserRequest> _updateClientRequestValidator;
    private readonly IValidator<UpdatePasswordRequest> _updatePasswordRequestValidator;
    private readonly IUserService _userService;

    public ApplicationUserController(
        UserManager<ApplicationUser> userManager,
        IValidator<UpdateUserRequest> updateClientRequestValidator,
        IValidator<UpdatePasswordRequest> updatePasswordRequestValidator,
        IUserService userService
    )
    {
        _userManager = userManager;
        _updateClientRequestValidator = updateClientRequestValidator;
        _updatePasswordRequestValidator = updatePasswordRequestValidator;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUser()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null)
            return NoContent();

        return Ok(user);
    }

    [HttpGet("role")]
    public async Task<IActionResult> GetUserRole()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        return Ok(await _userManager.GetRolesAsync(user));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _updateClientRequestValidator.ValidateAndThrowAsync(request);

        await _userService.UpdateUserAsync(user, request);

        return Ok();
    }

    [HttpPut("changePassword")]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UpdatePasswordRequest request)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _updatePasswordRequestValidator.ValidateAndThrowAsync(request);

        IdentityResult result = await _userService.UpdateUserPassword(user, request);

        if (!result.Succeeded)
            return BadRequest();

        return Ok();
    }
}
