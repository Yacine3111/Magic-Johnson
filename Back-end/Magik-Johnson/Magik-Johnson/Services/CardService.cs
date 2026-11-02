using DTOs.Responses;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Cards;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Services;

public class CardService(ApplicationDbContext context) : ICardService
{
    private bool _frenchSearch;

    public async Task<CardResponse?> GetCardByIdAsync(Guid id)
    {
        var card = await context
            .Cards.Include(c => c.ArtistsCards)
            .ThenInclude(ac => ac.Artist)
            .Include(c => c.Languages)
            .Include(c => c.CardTypes)
            .ThenInclude(ct => ct.Type)
            .Include(c => c.CardSubTypes)
            .ThenInclude(cst => cst.SubType)
            .Include(c => c.Quantities)
            .ThenInclude(q => q.Language)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null)
            return null;

        var sets = await GetSetNamesDictionaryAsync([card.SetId]);

        var discount = 0;
        if (card.DiscountId.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            var discountEntity = await context
                .Discounts.Where(d =>
                    d.Id == card.DiscountId.Value
                    && d.Status == "Actif"
                    && d.StartDate <= now
                    && d.EndDate >= now
                )
                .FirstOrDefaultAsync();

            if (discountEntity != null)
            {
                discount = discountEntity.Rate;
            }
        }

        return new CardResponse
        {
            Id = card.Id,
            SetId = card.SetId,
            SetName = sets.GetValueOrDefault(card.SetId, "Inconnu"),
            ScryfallId = card.ScryfallId,
            Name = _frenchSearch && card.FrenchName != null ? card.FrenchName : card.Name, // LA LA
            ManaCost = card.ManaCost,
            Colors = GetCardColors(card.ManaCost ?? ""),
            Text = card.Text,
            Power = card.Power,
            Toughness = card.Toughness,
            Rarity = card.Rarity,
            Number = card.Number,
            HasFoil = card.HasFoil,
            HasNonFoil = card.HasNonFoil,
            OnlineOnly = card.OnlineOnly,
            Price = card.Price,
            FoilPrice = card.FoilPrice,
            Discount = discount,
            TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
            Types = [.. card.CardTypes.Select(ct => ct.Type.Name)],
            SubTypes = [.. card.CardSubTypes.Select(cst => cst.SubType.Name)],
            Languages = [.. card.Languages.Select(l => l.Name)],
            ArtistsNames =
                card.ArtistsCards?.Select(ac => ac.Artist?.Name ?? "Inconnu").ToList() ?? [],
            LanguageQuantities = card
                .Quantities.Select(q => new CardLanguageQuantity
                {
                    LanguageId = q.LanguageId ?? Guid.Empty,
                    LanguageName =
                        q.LanguageId == null ? "English" : (q.Language?.Name ?? "Inconnue"),
                    FoilQuantity = card.OnlineOnly ? "N/A" : q.FoilQuantity.ToString(),
                    NonFoilQuantity = card.OnlineOnly ? "N/A" : q.NonFoilQuantity.ToString(),
                })
                .ToList(),
        };
    }

    public async Task<List<CardSummaryResponse>> GetCardsByNameAsync(string cardName)
    {
        var cards = await context.Cards.Where(c => c.Name == cardName).ToListAsync();
        var setIds = cards.Select(c => c.SetId).Distinct().ToList();
        var sets = await GetSetNamesDictionaryAsync(setIds);

        return cards
            .Select(c => new CardSummaryResponse
            {
                Id = c.Id,
                Name = c.Name,
                SetName = sets.GetValueOrDefault(c.SetId, "Inconnu"),
                ScryfallId = c.ScryfallId,
            })
            .ToList();
    }

    public async Task<List<CardVariantResponse>> GetCardScryfallVariantsAsync(Guid cardId)
    {
        return await context
            .Languages.Where(l => l.CardId == cardId)
            .Select(l => new CardVariantResponse { ScryfallId = l.ScryfallId, Language = l.Name })
            .ToListAsync();
    }

    public async Task<PaginationResponse<CatalogResponse>> GetCatalogCardsAsync(
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
    )
    {
        IQueryable<Card> query = context.Cards.Include(c => c.Quantities);

        query = await ApplyFilters(
            query,
            search,
            searchByDescription,
            series,
            rarities,
            types,
            subtypes,
            colors,
            languages,
            availabilities,
            priceMin,
            priceMax,
            finition
        );

        query = ApplySortingToQuery(query, sortBy);

        int totalCards = await query.CountAsync();

        List<Card> paginatedCards = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await BuildCardResponse(paginatedCards, page, pageSize, totalCards);
    }

    public async Task<FilterOptionResponse> GetFilterOptionsAsync()
    {
        var sets = await context.Sets.Select(s => s.Name).Distinct().ToListAsync();
        var rarities = await context
            .Cards.Select(c => MapRarity(c.Rarity))
            .Distinct()
            .ToListAsync();
        var types = await context.Types.Select(t => t.Name).Distinct().ToListAsync();
        var languages = await context.Languages.Select(l => l.Name).Distinct().ToListAsync();

        return new FilterOptionResponse
        {
            Series = sets,
            Rarities = rarities,
            Types = types,
            Languages = languages,
            Colors = ["Blanc", "Bleu", "Noir", "Rouge", "Vert", "Incolore"],
            Availabilities = ["En stock", "En rupture de stock"],
        };
    }

    public async Task<SubTypesResponse> GetSubtypesAsync(string[] types)
    {
        if (types.Length == 0)
            return new SubTypesResponse { SubTypes = [] };

        var typeIds = await context
            .Types.Where(t => types.Contains(t.Name))
            .Select(t => t.Id)
            .ToListAsync();

        var cardIdsWithSelectedTypes = await context
            .CardTypes.Where(ct => typeIds.Contains(ct.TypeId))
            .Select(ct => ct.CardId)
            .Distinct()
            .ToListAsync();

        var subtypes = await context
            .CardSubTypes.Where(cs => cardIdsWithSelectedTypes.Contains(cs.CardId))
            .Join(context.SubTypes, cs => cs.SubTypeId, st => st.Id, (cs, st) => st.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();

        return new SubTypesResponse { SubTypes = subtypes };
    }

    private async Task<IQueryable<Card>> ApplyFilters(
        IQueryable<Card> query,
        string? search,
        string? searchByDescription,
        string[]? series,
        string[]? rarities,
        string[]? types,
        string[]? subtypes,
        string[]? colors,
        string[]? languages,
        string[]? availabilities,
        decimal? priceMin,
        decimal? priceMax,
        string[]? finition = null
    )
    {
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Name.ToLower().Contains(search.ToLower())
                || (c.FrenchName != null && c.FrenchName.ToLower().Contains(search.ToLower()))
            );

            _frenchSearch = await query.AnyAsync(c =>
                c.FrenchName != null && c.FrenchName.ToLower().Contains(search.ToLower())
            );
        }

        if (!string.IsNullOrEmpty(searchByDescription))
        {
            query = query.Where(c =>
                c.Text != null && c.Text.ToLower().Contains(searchByDescription.ToLower())
            );
        }

        if (rarities?.Length > 0)
        {
            var mappedRarities = rarities.Select(MapRarityReverse);
            query = query.Where(c => mappedRarities.Contains(c.Rarity));
        }

        if (series?.Length > 0)
        {
            var seriesSetIds = context.Sets.Where(s => series.Contains(s.Name)).Select(s => s.Id);
            query = query.Where(c => seriesSetIds.Contains(c.SetId));
        }

        if (types?.Length > 0)
        {
            var typeIds = context.Types.Where(t => types.Contains(t.Name)).Select(t => t.Id);

            query = query.Where(c => typeIds.All(t => c.CardTypes.Any(ct => ct.TypeId == t)));
        }

        if (subtypes?.Length > 0)
        {
            var subtypeIds = context
                .SubTypes.Where(st => subtypes.Contains(st.Name))
                .Select(st => st.Id);
            var cardIdsWithSubtypes = context
                .CardSubTypes.Where(cs => subtypeIds.Contains(cs.SubTypeId))
                .Select(cs => cs.CardId);
            query = query.Where(c => cardIdsWithSubtypes.Contains(c.Id));
        }

        if (colors?.Length > 0)
            query = ApplyColorFilter(query, colors);

        if (languages?.Length > 0)
        {
            var cardIdsWithLanguages = context
                .Languages.Where(l => languages.Contains(l.Name))
                .Select(l => l.CardId)
                .Distinct();
            query = query.Where(c => cardIdsWithLanguages.Contains(c.Id));
        }

        query = ApplyPriceFilter(query, priceMin, priceMax);

        if (availabilities?.Length > 0)
        {
            if (availabilities.Contains("En stock"))
            {
                IQueryable<Guid> cardIdsInStock = context
                    .Quantities.GroupBy(q => q.CardId)
                    .Where(g => g.Sum(q => q.FoilQuantity + q.NonFoilQuantity) > 0)
                    .Select(g => g.Key);
                query = query.Where(c =>
                    cardIdsInStock.Contains(c.Id) && (c.Price + c.FoilPrice) > 0 && !c.OnlineOnly
                );
            }
            if (availabilities.Contains("En rupture de stock"))
            {
                var cardIdsInStock = context
                    .Quantities.GroupBy(q => q.CardId)
                    .Where(g => g.Sum(q => q.FoilQuantity + q.NonFoilQuantity) > 0)
                    .Select(g => g.Key);
                query = query.Where(c =>
                    !cardIdsInStock.Contains(c.Id) && (c.Price + c.FoilPrice) > 0 && !c.OnlineOnly
                );
            }
        }

        if (finition?.Length > 0)
        {
            if (finition.Contains("Foil") && !finition.Contains("Non-Foil"))
            {
                query = query.Where(c => c.HasFoil);
            }
            else if (finition.Contains("Non-Foil") && !finition.Contains("Foil"))
            {
                query = query.Where(c => c.HasNonFoil);
            }
        }

        return query;
    }

    private static IQueryable<Card> ApplyColorFilter(IQueryable<Card> query, string[] colors)
    {
        IQueryable<Card> colorQuery = query.Where(c => false);

        foreach (var color in colors)
        {
            IQueryable<Card> singleColorQuery = color.ToLower() switch
            {
                "blanc" => query.Where(c => c.ManaCost != null && c.ManaCost.Contains("W")),
                "bleu" => query.Where(c => c.ManaCost != null && c.ManaCost.Contains("U")),
                "noir" => query.Where(c => c.ManaCost != null && c.ManaCost.Contains("B")),
                "rouge" => query.Where(c => c.ManaCost != null && c.ManaCost.Contains("R")),
                "vert" => query.Where(c => c.ManaCost != null && c.ManaCost.Contains("G")),
                "incolore" => query.Where(c =>
                    c.ManaCost == null
                    || (
                        !c.ManaCost.Contains("W")
                        && !c.ManaCost.Contains("U")
                        && !c.ManaCost.Contains("B")
                        && !c.ManaCost.Contains("R")
                        && !c.ManaCost.Contains("G")
                    )
                ),
                _ => query.Where(c => false),
            };
            colorQuery = colorQuery.Union(singleColorQuery);
        }

        return colorQuery;
    }

    private static IQueryable<Card> ApplyPriceFilter(
        IQueryable<Card> query,
        decimal? priceMin,
        decimal? priceMax
    )
    {
        if (!priceMin.HasValue && !priceMax.HasValue)
            return query;

        if (priceMin.HasValue && priceMax.HasValue)
        {
            return query.Where(c =>
                (c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)) >= priceMin.Value
                && (c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)) <= priceMax.Value
            );
        }

        if (priceMin.HasValue)
        {
            return query.Where(c =>
                (c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)) >= priceMin.Value
            );
        }

        return query.Where(c =>
            (c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)) <= priceMax!.Value
        );
    }

    private static IQueryable<Card> ApplySortingToQuery(IQueryable<Card> query, string? sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "price-asc" => query
                .Where(c => (c.Price + c.FoilPrice) > 0)
                .OrderBy(c => c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)),
            "price-desc" => query
                .Where(c => (c.Price + c.FoilPrice) > 0)
                .OrderByDescending(c =>
                    c.Price > 0 ? c.Price : (c.FoilPrice > 0 ? c.FoilPrice : 0)
                ),
            "name-asc" => query.OrderBy(c => c.SortableName),
            "name-desc" => query.OrderByDescending(c => c.SortableName),
            _ => query,
        };
    }

    private async Task<PaginationResponse<CatalogResponse>> BuildCardResponse(
        List<Card> cards,
        int page,
        int pageSize,
        int totalCards
    )
    {
        var cardSetIds = cards.Select(c => c.SetId).Distinct();
        var sets = await context
            .Sets.Where(s => cardSetIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var discountIds = cards
            .Where(c => c.DiscountId.HasValue)
            .Select(c => c.DiscountId!.Value)
            .Distinct();

        var discountsDict = await context
            .Discounts.Where(d =>
                discountIds.Contains(d.Id)
                && d.Status == "Actif"
                && d.StartDate <= DateTimeOffset.UtcNow
                && d.EndDate >= DateTimeOffset.UtcNow
            )
            .ToDictionaryAsync(d => d.Id);

        var cardList = cards.Select(card => new CatalogResponse
        {
            Id = card.Id,
            ScryfallId = card.ScryfallId,
            Name = _frenchSearch && card.FrenchName != null ? card.FrenchName : card.Name,
            SetName = sets.GetValueOrDefault(card.SetId, "Série inconnue"),
            Rarity = MapRarity(card.Rarity),
            Number = card.Number,
            Price = card.Price,
            FoilPrice = card.FoilPrice,
            Discount =
                card.DiscountId.HasValue
                && discountsDict.TryGetValue(card.DiscountId.Value, out Discount? value)
                    ? value.Rate
                    : 0,
            HasFoil = card.HasFoil,
            HasNonFoil = card.HasNonFoil,
            OnlineOnly = card.OnlineOnly,
            StockFoil = card.Quantities.Sum(q => q.FoilQuantity),
            StockNonFoil = card.Quantities.Sum(q => q.NonFoilQuantity),
            TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
        });

        var response = new PaginationResponse<CatalogResponse>
        {
            Cards = [.. cardList],
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCards = totalCards,
                TotalPages = (int)Math.Ceiling((double)totalCards / pageSize),
            },
        };

        return response;
    }

    private async Task<Dictionary<Guid, string>> GetSetNamesDictionaryAsync(List<Guid> setIds)
    {
        return await context
            .Sets.Where(s => setIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);
    }

    private static string MapRarity(string rarity) =>
        rarity.ToLower() switch
        {
            "common" => "Commune",
            "uncommon" => "Peu commune",
            "rare" => "Rare",
            "mythic" => "Mythique",
            _ => rarity,
        };

    private static string MapRarityReverse(string rarity) =>
        rarity.ToLower() switch
        {
            "commune" => "common",
            "peu commune" => "uncommon",
            "rare" => "rare",
            "mythique" => "mythic",
            _ => rarity.ToLower(),
        };

    private static string GetCardColors(string manaCost)
    {
        var colors = new List<string>();
        if (manaCost.Contains("W"))
            colors.Add("Blanc");
        if (manaCost.Contains("U"))
            colors.Add("Bleu");
        if (manaCost.Contains("B"))
            colors.Add("Noir");
        if (manaCost.Contains("R"))
            colors.Add("Rouge");
        if (manaCost.Contains("G"))
            colors.Add("Vert");
        return colors.Count > 0 ? string.Join(", ", colors) : "Incolore";
    }

    public async Task<PaginationResponse<InventoryCardResponse>> GetInventoryCardsAsync(
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
    )
    {
        var baseQuery = context.Cards.Include(c => c.Quantities).ThenInclude(q => q.Language);

        var filteredQuery = await ApplyFilters(
            baseQuery.AsQueryable(),
            search,
            null,
            series,
            rarities,
            types,
            subtypes,
            colors,
            languages,
            availabilities,
            priceMin,
            priceMax,
            finition
        );

        var totalCards = await filteredQuery.CountAsync();
        var sortedQuery = ApplySortingToQuery(filteredQuery, sortBy);

        var paginatedCards = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return await BuildInventoryCardResponse(paginatedCards, page, pageSize, totalCards);
    }

    public async Task<bool> UpdateCardLanguageQuantityAsync(
        Guid cardId,
        Guid languageId,
        int foilQuantity,
        int nonFoilQuantity
    )
    {
        Guid? actualLanguageId = languageId == Guid.Empty ? null : languageId;

        var quantity = await context.Quantities.FirstOrDefaultAsync(q =>
            q.CardId == cardId && q.LanguageId == actualLanguageId
        );

        if (quantity == null)
        {
            quantity = new Quantity
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                LanguageId = actualLanguageId,
                FoilQuantity = foilQuantity,
                NonFoilQuantity = nonFoilQuantity,
            };
            context.Quantities.Add(quantity);
        }
        else
        {
            quantity.FoilQuantity = foilQuantity;
            quantity.NonFoilQuantity = nonFoilQuantity;
        }

        await context.SaveChangesAsync();
        return true;
    }

    private async Task<PaginationResponse<InventoryCardResponse>> BuildInventoryCardResponse(
        List<Card> cards,
        int page,
        int pageSize,
        int totalCards
    )
    {
        var cardIds = cards.Select(c => c.Id).ToList();
        var cardSetIds = cards.Select(c => c.SetId).Distinct().ToList();

        var sets = await context
            .Sets.Where(s => cardSetIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var cardTypesDict = await context
            .CardTypes.Where(ct => cardIds.Contains(ct.CardId))
            .Join(context.Types, ct => ct.TypeId, t => t.Id, (ct, t) => new { ct.CardId, t.Name })
            .GroupBy(x => x.CardId)
            .ToDictionaryAsync(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)));

        var allLanguagesForCards = await context
            .Languages.Where(l => cardIds.Contains(l.CardId))
            .GroupBy(l => l.CardId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        var inventoryCards = cards
            .Select(card =>
            {
                var cardLanguages = allLanguagesForCards.ContainsKey(card.Id)
                    ? allLanguagesForCards[card.Id]
                    : new List<Language>();

                var quantitiesByLanguageId = card
                    .Quantities.Where(q => q.LanguageId.HasValue)
                    .ToDictionary(q => q.LanguageId!.Value, q => q);

                var languageQuantities = cardLanguages
                    .Select(lang => new CardLanguageQuantity
                    {
                        LanguageId = lang.Id,
                        LanguageName = lang.Name,
                        FoilQuantity =
                            card.OnlineOnly ? "N/A"
                            : quantitiesByLanguageId.ContainsKey(lang.Id)
                                ? quantitiesByLanguageId[lang.Id].FoilQuantity.ToString()
                            : "0",
                        NonFoilQuantity =
                            card.OnlineOnly ? "N/A"
                            : quantitiesByLanguageId.ContainsKey(lang.Id)
                                ? quantitiesByLanguageId[lang.Id].NonFoilQuantity.ToString()
                            : "0",
                    })
                    .ToList();
                var quantitiesWithoutLanguage = card
                    .Quantities.Where(q => q.LanguageId == null)
                    .ToList();
                if (quantitiesWithoutLanguage.Any())
                {
                    var englishQuantity = quantitiesWithoutLanguage.FirstOrDefault();
                    if (englishQuantity != null)
                    {
                        var hasEnglish = languageQuantities.Any(lq =>
                            lq.LanguageName.ToLower() == "english"
                        );

                        if (!hasEnglish)
                        {
                            languageQuantities.Add(
                                new CardLanguageQuantity
                                {
                                    LanguageId = Guid.Empty,
                                    LanguageName = "English",
                                    FoilQuantity = card.OnlineOnly
                                        ? "N/A"
                                        : englishQuantity.FoilQuantity.ToString(),
                                    NonFoilQuantity = card.OnlineOnly
                                        ? "N/A"
                                        : englishQuantity.NonFoilQuantity.ToString(),
                                }
                            );
                        }
                    }
                }

                return new InventoryCardResponse
                {
                    Id = card.Id,
                    ScryfallId = card.ScryfallId,
                    Name = card.Name,
                    SetName = sets.GetValueOrDefault(card.SetId, "Série inconnue"),
                    Rarity = MapRarity(card.Rarity),
                    Number = card.Number,
                    Price = card.Price,
                    FoilPrice = card.FoilPrice,
                    Couleur = GetCardColors(card.ManaCost ?? ""),
                    Type = cardTypesDict.GetValueOrDefault(card.Id, "Inconnue"),
                    ManaCost = card.ManaCost ?? "",
                    Text = card.Text ?? "",
                    Power = card.Power ?? "",
                    Toughness = card.Toughness ?? "",
                    HasFoil = card.HasFoil,
                    HasNonFoil = card.HasNonFoil,
                    OnlineOnly = card.OnlineOnly,
                    LanguageQuantities = languageQuantities,
                };
            })
            .ToList();

        return new PaginationResponse<InventoryCardResponse>
        {
            Cards = [.. inventoryCards],
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCards = totalCards,
                TotalPages = (int)Math.Ceiling((double)totalCards / pageSize),
            },
        };
    }

    public async Task<IEnumerable<CardResponse>> GetTrendyCardsAsync()
    {
        var cards = await context
            .Cards.Include(c => c.CardTypes)
            .ThenInclude(ct => ct.Type)
            .Include(c => c.CardSubTypes)
            .ThenInclude(cst => cst.SubType)
            .Include(c => c.Quantities)
            .OrderByDescending(c => c.ClickCount)
            .Take(4)
            .ToListAsync();

        var setIds = cards.Select(c => c.SetId).Distinct().ToList();
        var sets = await GetSetNamesDictionaryAsync(setIds);

        var now = DateTimeOffset.UtcNow;
        var discountIds = cards
            .Where(c => c.DiscountId.HasValue)
            .Select(c => c.DiscountId!.Value)
            .Distinct()
            .ToList();
        var discountsDict = await context
            .Discounts.Where(d =>
                discountIds.Contains(d.Id)
                && d.Status == "Actif"
                && d.StartDate <= now
                && d.EndDate >= now
            )
            .ToDictionaryAsync(d => d.Id);

        return cards
            .Select(card => new CardResponse
            {
                Id = card.Id,
                SetId = card.SetId,
                SetName = sets.GetValueOrDefault(card.SetId, "Inconnu"),
                ScryfallId = card.ScryfallId,
                Name = card.Name,
                ManaCost = card.ManaCost,
                Colors = GetCardColors(card.ManaCost ?? ""),
                Text = card.Text,
                Power = card.Power,
                Toughness = card.Toughness,
                Rarity = card.Rarity,
                Number = card.Number,
                HasFoil = card.HasFoil,
                HasNonFoil = card.HasNonFoil,
                OnlineOnly = card.OnlineOnly,
                Price = card.Price,
                FoilPrice = card.FoilPrice,
                Discount =
                    card.DiscountId.HasValue && discountsDict.ContainsKey(card.DiscountId.Value)
                        ? discountsDict[card.DiscountId.Value].Rate
                        : 0,
                TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
                StockFoil = card.Quantities.Sum(q => q.FoilQuantity),
                StockNonFoil = card.Quantities.Sum(q => q.NonFoilQuantity),
                ClickCount = card.ClickCount,
                Types = card.CardTypes.Select(ct => ct.Type.Name).ToList(),
                SubTypes = card.CardSubTypes.Select(cst => cst.SubType.Name).ToList(),
            })
            .ToList();
    }

    public async Task<IEnumerable<CardResponse>> GetMostSoldCardsAsync(int count = 4, int days = 30)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        var cardSales = await context
            .CardOrders.Include(co => co.Order)
            .Where(co => co.Order != null && co.Order.CheckoutDate >= startDate)
            .GroupBy(co => co.CardId)
            .Select(g => new { CardId = g.Key, TotalSold = g.Sum(co => co.Quantity) })
            .OrderByDescending(x => x.TotalSold)
            .Take(count)
            .ToListAsync();

        if (!cardSales.Any())
        {
            return new List<CardResponse>();
        }

        var cardIds = cardSales.Select(cs => cs.CardId).ToList();

        var cards = await context
            .Cards.Include(c => c.CardTypes)
            .ThenInclude(ct => ct.Type)
            .Include(c => c.CardSubTypes)
            .ThenInclude(cst => cst.SubType)
            .Include(c => c.Quantities)
            .Where(c => cardIds.Contains(c.Id))
            .ToListAsync();

        var orderedCards = cardIds
            .Select(id => cards.FirstOrDefault(c => c.Id == id))
            .Where(c => c != null)
            .ToList();

        var setIds = orderedCards.Select(c => c!.SetId).Distinct().ToList();
        var sets = await GetSetNamesDictionaryAsync(setIds);

        var now = DateTimeOffset.UtcNow;
        var discountIds = orderedCards
            .Where(c => c!.DiscountId.HasValue)
            .Select(c => c!.DiscountId!.Value)
            .Distinct()
            .ToList();
        var discountsDict = await context
            .Discounts.Where(d =>
                discountIds.Contains(d.Id)
                && d.Status == "Actif"
                && d.StartDate <= now
                && d.EndDate >= now
            )
            .ToDictionaryAsync(d => d.Id);

        return orderedCards
            .Select(card => new CardResponse
            {
                Id = card!.Id,
                SetId = card.SetId,
                SetName = sets.GetValueOrDefault(card.SetId, "Inconnu"),
                ScryfallId = card.ScryfallId,
                Name = card.Name,
                ManaCost = card.ManaCost,
                Colors = GetCardColors(card.ManaCost ?? ""),
                Text = card.Text,
                Power = card.Power,
                Toughness = card.Toughness,
                Rarity = card.Rarity,
                Number = card.Number,
                HasFoil = card.HasFoil,
                HasNonFoil = card.HasNonFoil,
                OnlineOnly = card.OnlineOnly,
                Price = card.Price,
                FoilPrice = card.FoilPrice,
                Discount =
                    card.DiscountId.HasValue && discountsDict.ContainsKey(card.DiscountId.Value)
                        ? discountsDict[card.DiscountId.Value].Rate
                        : 0,
                TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
                StockFoil = card.Quantities.Sum(q => q.FoilQuantity),
                StockNonFoil = card.Quantities.Sum(q => q.NonFoilQuantity),
                ClickCount = card.ClickCount,
                Types = card.CardTypes.Select(ct => ct.Type.Name).ToList(),
                SubTypes = card.CardSubTypes.Select(cst => cst.SubType.Name).ToList(),
            })
            .ToList();
    }

    public async Task<bool> UpdateCardClickCountAsync(Guid id)
    {
        var card = await context.Cards.FirstOrDefaultAsync(x => x.Id == id);

        if (card == null)
            return false;

        card.ClickCount += 1;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int> ResetAllClickCountsAsync()
    {
        var rowsAffected = await context
            .Cards.Where(c => c.ClickCount > 0)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.ClickCount, 0));

        return rowsAffected;
    }
}
