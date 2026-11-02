using DTOs.Requests;
using FluentValidation;
using Magik_Johnson.DTOs.Requests.Validations;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class AddressController : BaseController
{
    private readonly IAddressService _addressService;
    private readonly IValidator<AddressRegistrationRequest> _addressRegistrationRequestValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public AddressController(
        IAddressService addressService,
        UserManager<ApplicationUser> userManager
    )
    {
        _addressService = addressService;
        _addressRegistrationRequestValidator = new AddressRegistrationRequestValidator();
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAddress()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        return Ok(await _addressService.GetAddressByUserIdAsync(user.Id));
    }

    [HttpGet("canada")]
    public IActionResult GetCanada()
    {
        var canada = _addressService.GetCountryByName("canada");

        return Ok(canada);
    }

    [HttpPost]
    public async Task<IActionResult> AddClientAddress([FromBody] AddressRegistrationRequest request)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        await _addressRegistrationRequestValidator.ValidateAndThrowAsync(request);

        await _addressService.AddAsync(user.Id, request);

        return Ok();
    }

    [HttpPut("update/{addressId}")]
    public async Task<IActionResult> UpdateAddress(
        Guid addressId,
        [FromBody] AddressRegistrationRequest request
    )
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        await _addressRegistrationRequestValidator.ValidateAndThrowAsync(request);

        await _addressService.UpdateAsync(addressId, user.Id, request);

        return Ok();
    }

    [HttpDelete("delete/{addressId}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        await _addressService.DeleteAsync(addressId, user.Id);

        return Ok();
    }

    [HttpPut("client/update/{addressId}")]
    [Authorize(Roles = "employee,manager")]
    public async Task<IActionResult> UpdateClientAddress(
        Guid addressId,
        [FromBody] AddressRegistrationRequest request
    )
    {
        await _addressRegistrationRequestValidator.ValidateAndThrowAsync(request);

        await _addressService.UpdateForClientAsync(addressId, request);

        return Ok();
    }

    [HttpGet("client/{userId}")]
    [Authorize(Roles = "employee,manager")]
    public async Task<IActionResult> GetClientAddresses(string userId)
    {
        var addresses = await _addressService.GetAddressByUserIdAsync(userId);
        return Ok(addresses);
    }

    [HttpPost("client/{userId}")]
    [Authorize(Roles = "employee,manager")]
    public async Task<IActionResult> AddAddressForClient(
        string userId,
        [FromBody] AddressRegistrationRequest request
    )
    {
        await _addressRegistrationRequestValidator.ValidateAndThrowAsync(request);

        await _addressService.AddAsync(userId, request);

        return Ok();
    }
}
