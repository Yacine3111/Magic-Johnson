using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;
    private readonly IValidator<AddToCartRequest> _addToCartRequestValidator;
    private readonly IValidator<DeleteCardFromCartRequest> _deleteCardFromCartRequestValidator;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ICartService cartService,
        IValidator<AddToCartRequest> addToCartRequestValidator,
        IValidator<DeleteCardFromCartRequest> deleteCardFromCartRequestValidator,
        UserManager<ApplicationUser> userManager
    )
    {
        _cartService = cartService;
        _addToCartRequestValidator = addToCartRequestValidator;
        _deleteCardFromCartRequestValidator = deleteCardFromCartRequestValidator;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Forbid();

        if (request.Items.IsNullOrEmpty())
            return NoContent();

        await _addToCartRequestValidator.ValidateAndThrowAsync(request);

        await _cartService.AddToCartAsync(user.Id, request);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Forbid();

        return Ok(await _cartService.GetCartAsync(user.Id));
    }

    [HttpDelete("{cardId}")]
    public async Task<IActionResult> DeleteCardFromCart(
        [FromRoute] Guid cardId,
        [FromBody] DeleteCardFromCartRequest request
    )
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Forbid();

        await _deleteCardFromCartRequestValidator.ValidateAndThrowAsync(request);

        await _cartService.DeleteCardFromCartAsync(user.Id, cardId, request);

        return NoContent();
    }
}
