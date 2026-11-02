using Magik_Johnson.CustomException;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Cards;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CartService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task CreateCarteAsync(string userId)
    {
        _applicationDbContext.Carts.Add(new Cart { IdUser = userId });

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task AddToCartAsync(string userId, AddToCartRequest request)
    {
        Cart cart = await _applicationDbContext.Carts.SingleAsync(c => c.IdUser == userId);

        foreach (var item in request.Items)
        {
            Language? cardLanguage = await _applicationDbContext.Languages.SingleOrDefaultAsync(l =>
                l.CardId == item.CardId && l.Name == item.Language
            );

            Guid? languageId = cardLanguage?.Id;

            CardCart? existingItem = await _applicationDbContext.CardsCarts.FirstOrDefaultAsync(
                cc =>
                    cc.CardId == item.CardId
                    && cc.CartId == cart.Id
                    && cc.LanguageId == languageId
                    && cc.IsFoil == item.IsFoil
            );

            if (existingItem == null)
            {
                _applicationDbContext.CardsCarts.Add(
                    new CardCart
                    {
                        CardId = item.CardId,
                        CartId = cart.Id,
                        LanguageId = languageId,
                        Quantity = item.Quantity,
                        IsFoil = item.IsFoil,
                    }
                );
            }
            else if (item.Quantity != existingItem.Quantity)
            {
                existingItem.Quantity = item.Quantity;
                _applicationDbContext.CardsCarts.Update(existingItem);
            }
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<CartResponse> GetCartAsync(string userId)
    {
        Cart cart = await _applicationDbContext
            .Carts.Include(c => c.CardCarts)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.Quantities)
            .Include(cart => cart.CardCarts)
            .ThenInclude(cardCart => cardCart.Card)
            .ThenInclude(card => card.CardTypes)
            .ThenInclude(cardType => cardType.Type)
            .Include(cart => cart.CardCarts)
            .ThenInclude(cardCart => cardCart.Card)
            .ThenInclude(card => card.CardSubTypes)
            .ThenInclude(cardSubType => cardSubType.SubType)
            .SingleAsync(c => c.IdUser == userId);

        var setIds = cart.CardCarts.Select(ct => ct.Card!.SetId).Distinct().ToList();
        var sets = await GetSetNamesDictionaryAsync(setIds);

        var discountIds = cart
            .CardCarts.Where(ct => ct.Card!.DiscountId.HasValue)
            .Select(ct => ct.Card!.DiscountId!.Value)
            .Distinct()
            .ToList();
        var discountsDict = await _applicationDbContext
            .Discounts.Where(d => discountIds.Contains(d.Id) && d.Status == "Actif")
            .ToDictionaryAsync(d => d.Id);

        List<CartItemResponse> items = [];
        foreach (var cardCart in cart.CardCarts)
        {
            Language? language = await _applicationDbContext.Languages.FirstOrDefaultAsync(l =>
                l.Id == cardCart.LanguageId
            );

            var card = cardCart.Card;
            var discount = 0;
            if (
                card!.DiscountId.HasValue
                && discountsDict.TryGetValue(card.DiscountId.Value, out var value)
            )
            {
                discount = value.Rate;
            }

            var cardResponse = new CardResponse
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
                Price = card.Price,
                FoilPrice = card.FoilPrice,
                Discount = discount,
                TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
                Types = card.CardTypes.Select(ct => ct.Type.Name).ToList(),
                SubTypes = card.CardSubTypes.Select(cst => cst.SubType.Name).ToList(),
            };

            items.Add(
                new CartItemResponse
                {
                    Card = cardResponse,
                    Quantity = cardCart.Quantity,
                    Language = language?.Name ?? "English",
                    IsFoil = cardCart.IsFoil,
                }
            );
        }

        return new CartResponse { Items = items };
    }

    public async Task DeleteCardFromCartAsync(
        string userId,
        Guid cardId,
        DeleteCardFromCartRequest request
    )
    {
        Cart userCart = await _applicationDbContext.Carts.SingleAsync(c => c.IdUser == userId);

        Language? cardLanguage = await _applicationDbContext.Languages.SingleOrDefaultAsync(ct =>
            ct.CardId == cardId && ct.Name == request.LanguageName
        );

        Guid? languageId = cardLanguage?.Id;

        CardCart rowToDelete =
            await _applicationDbContext.CardsCarts.SingleOrDefaultAsync(cc =>
                cc.CardId == cardId
                && cc.CartId == userCart.Id
                && cc.LanguageId == languageId
                && cc.IsFoil == request.IsFoil
            ) ?? throw new RecordNotFoundException($"l'item n'existe pas");

        _applicationDbContext.Remove(rowToDelete);
        await _applicationDbContext.SaveChangesAsync();
    }

    private async Task<Dictionary<Guid, string>> GetSetNamesDictionaryAsync(List<Guid> setIds)
    {
        return await _applicationDbContext
            .Sets.Where(s => setIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);
    }

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

        return colors.Count == 0 ? "Incolore" : string.Join(", ", colors);
    }
}
