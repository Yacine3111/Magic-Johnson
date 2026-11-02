using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize(Roles = "manager")]
public class DiscountController : BaseController
{
    private readonly IDiscountService _discountService;
    private readonly IValidator<DiscountRequestBase> _discountRequestValidator;

    public DiscountController(
        IDiscountService discountService,
        IValidator<DiscountRequestBase> discountRequestValidator
    )
    {
        _discountService = discountService;
        _discountRequestValidator = discountRequestValidator;
    }

    [HttpGet("{discountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscount(Guid discountId)
    {
        return Ok(await _discountService.GetDiscountAsync(discountId));
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDiscounts()
    {
        return Ok(await _discountService.GetAllDiscountsAsync());
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveDiscounts()
    {
        return Ok(await _discountService.GetActiveDiscountsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscount([FromBody] DiscountRequestBase request)
    {
        await _discountRequestValidator.ValidateAndThrowAsync(request);

        await _discountService.CreateDiscountAsync(request);

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDiscount([FromBody] DiscountRequestBase request)
    {
        await _discountRequestValidator.ValidateAndThrowAsync(request);

        await _discountService.UpdateDiscountAsync(request);

        return Ok();
    }

    [HttpDelete("{discountId}")]
    public async Task<IActionResult> DeleteDiscount(Guid discountId)
    {
        await _discountService.DeleteDiscountAsync(discountId);

        return Ok();
    }

    [HttpGet("{discountId}/cards")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscountCards(Guid discountId)
    {
        return Ok(await _discountService.GetDiscountCardsAsync(discountId));
    }

    [HttpPost("{discountId}/cards")]
    public async Task<IActionResult> AddCardsToDiscount(
        Guid discountId,
        [FromBody] List<Guid> cardIds
    )
    {
        await _discountService.AddCardsToDiscountAsync(discountId, cardIds);
        return Ok();
    }
}
