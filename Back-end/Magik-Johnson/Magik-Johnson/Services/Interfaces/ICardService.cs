using DTOs.Responses;
using Magik_Johnson.DTOs.Cards;
using Magik_Johnson.DTOs.Responses;

namespace Magik_Johnson.Services.Interfaces;

public interface ICardService
{
    Task<PaginationResponse<CatalogResponse>> GetCatalogCardsAsync(
        int page,
        int pageSize,
        string? search,
        string? searchByDescription,
        string[]? series,
        string[]? rarities,
        string[]? types,
        string[]? subtypes,
        string[]? colors,
        string[]? languages,
        string[]? availabilities,
        int[]? discounts,
        string? sortBy,
        decimal? priceMin,
        decimal? priceMax,
        string[]? finition = null
    );

    Task<FilterOptionResponse> GetFilterOptionsAsync();

    Task<SubTypesResponse> GetSubtypesAsync(string[] types);

    Task<CardResponse?> GetCardByIdAsync(Guid id);

    Task<List<CardVariantResponse>> GetCardScryfallVariantsAsync(Guid cardId);

    Task<List<CardSummaryResponse>> GetCardsByNameAsync(string cardName);
    Task<IEnumerable<CardResponse>> GetTrendyCardsAsync();
    Task<IEnumerable<CardResponse>> GetMostSoldCardsAsync(int count = 4, int days = 30);

    Task<PaginationResponse<InventoryCardResponse>> GetInventoryCardsAsync(
        int page,
        int pageSize,
        string? search,
        string[]? series,
        string[]? rarities,
        string[]? types,
        string[]? subtypes,
        string[]? colors,
        string[]? languages,
        string[]? availabilities,
        int[]? discounts,
        string? sortBy,
        decimal? priceMin,
        decimal? priceMax,
        string[]? finition = null
    );

    Task<bool> UpdateCardClickCountAsync(Guid id);

    Task<int> ResetAllClickCountsAsync();

    Task<bool> UpdateCardLanguageQuantityAsync(
        Guid cardId,
        Guid languageId,
        int foilQuantity,
        int nonFoilQuantity
    );
}
