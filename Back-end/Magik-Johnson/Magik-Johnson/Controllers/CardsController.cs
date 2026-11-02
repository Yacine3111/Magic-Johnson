using Magik_Johnson.DTOs.Cards;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
public class CardsController(ICardService cardService) : BaseController
{
    [HttpGet("catalogue")]
    public async Task<IActionResult> GetCards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 18,
        [FromQuery] string? search = null,
        [FromQuery] string? searchByDescription = null,
        [FromQuery] string[]? series = null,
        [FromQuery] string[]? rarities = null,
        [FromQuery] string[]? types = null,
        [FromQuery] string[]? subtypes = null,
        [FromQuery] string[]? colors = null,
        [FromQuery] string[]? languages = null,
        [FromQuery] string[]? availabilities = null,
        [FromQuery] int[]? discounts = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] string[]? finition = null
    )
    {
        var result = await cardService.GetCatalogCardsAsync(
            page,
            pageSize,
            search,
            searchByDescription,
            series,
            rarities,
            types,
            subtypes,
            colors,
            languages,
            availabilities,
            discounts,
            sortBy,
            priceMin,
            priceMax,
            finition
        );

        return Ok(result);
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result = await cardService.GetFilterOptionsAsync();
        return Ok(result);
    }

    [HttpGet("subtypes")]
    public async Task<IActionResult> GetSubtypes([FromQuery] string[] types)
    {
        var result = await cardService.GetSubtypesAsync(types);
        return Ok(result);
    }

    [HttpGet("trendyCards")]
    public async Task<ActionResult<IEnumerable<CardResponse>>> GetTrendyCardsAsync()
    {
        var cardsDtos = await cardService.GetTrendyCardsAsync();
        return Ok(cardsDtos);
    }

    [HttpGet("mostSold")]
    public async Task<ActionResult<IEnumerable<CardResponse>>> GetMostSoldCards(
        [FromQuery] int count = 4,
        [FromQuery] int days = 30
    )
    {
        var cardsDtos = await cardService.GetMostSoldCardsAsync(count, days);
        return Ok(cardsDtos);
    }

    [HttpPut("{id}/clickCount")]
    public async Task<ActionResult<object>> UpdateCardClickCountAsync(Guid id)
    {
        var result = await cardService.UpdateCardClickCountAsync(id);

        if (!result)
            return NotFound(new { message = "Carte introuvable" });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CardResponse>> GetCardById(Guid id)
    {
        var cardDto = await cardService.GetCardByIdAsync(id);
        return Ok(cardDto);
    }

    [HttpGet("{id}/variants")]
    public async Task<ActionResult<List<CardVariantResponse>>> GetCardVariants(Guid id)
    {
        var variants = await cardService.GetCardScryfallVariantsAsync(id);
        return Ok(variants);
    }

    [HttpGet("by-name/{cardName}")]
    public async Task<ActionResult<List<CardSummaryResponse>>> GetCardsByName(string cardName)
    {
        var cards = await cardService.GetCardsByNameAsync(cardName);
        return Ok(cards);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryCards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 18,
        [FromQuery] string? search = null,
        [FromQuery] string[]? series = null,
        [FromQuery] string[]? rarities = null,
        [FromQuery] string[]? types = null,
        [FromQuery] string[]? subtypes = null,
        [FromQuery] string[]? colors = null,
        [FromQuery] string[]? languages = null,
        [FromQuery] string[]? availabilities = null,
        [FromQuery] int[]? discounts = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] string[]? finition = null
    )
    {
        var result = await cardService.GetInventoryCardsAsync(
            page,
            pageSize,
            search,
            series,
            rarities,
            types,
            subtypes,
            colors,
            languages,
            availabilities,
            discounts,
            sortBy,
            priceMin,
            priceMax,
            finition
        );

        return Ok(result);
    }

    [HttpPut("{cardId}/language/{languageId}/quantity")]
    [Authorize(Roles = "manager,employee")]
    public async Task<IActionResult> UpdateCardLanguageQuantity(
        Guid cardId,
        Guid languageId,
        [FromBody] UpdateQuantitiesDto request
    )
    {
        var result = await cardService.UpdateCardLanguageQuantityAsync(
            cardId,
            languageId,
            request.FoilQuantity,
            request.NonFoilQuantity
        );

        if (!result)
            return NotFound();

        return Ok();
    }
}
