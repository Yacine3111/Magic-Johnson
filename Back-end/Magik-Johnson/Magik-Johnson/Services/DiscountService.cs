using Magik_Johnson.CustomException;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Services;

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DiscountService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task CreateDiscountAsync(DiscountRequestBase request)
    {
        Discount discount = new()
        {
            Title = request.Title,
            Description = request.Description,
            Rate = request.Rate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status ?? "En Attente",
        };

        if (discount.Status == "Actif") { }

        _applicationDbContext.Discounts.Add(discount);

        // Valider que toutes les cartes existent
        await ValidateCardsExistAsync(request.CardsIds.ToList());

        // Associer toutes les cartes à cette promotion
        foreach (var cardId in request.CardsIds)
        {
            Card card = await _applicationDbContext.Cards.SingleAsync(x => x.Id == cardId);
            card.DiscountId = discount.Id;
            _applicationDbContext.Update(card);
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task UpdateDiscountAsync(DiscountRequestBase request)
    {
        Discount discount =
            await _applicationDbContext.Discounts.SingleOrDefaultAsync(d => d.Id == request.Id)
            ?? throw new RecordNotFoundException($"Promotion avec l'ID {request.Id} introuvable");

        // Valider que toutes les nouvelles cartes existent
        await ValidateCardsExistAsync(request.CardsIds.ToList());

        // Récupérer les cartes actuellement associées à la promotion
        var currentCards = await _applicationDbContext
            .Cards.Where(c => c.DiscountId == discount.Id)
            .ToListAsync();

        // Identifier les cartes à supprimer
        var cardsToRemove = currentCards.Where(c => !request.CardsIds.Contains(c.Id)).ToList();

        // Identifier les cartes à ajouter
        var currentCardIds = currentCards.Select(c => c.Id).ToList();
        var cardsToAdd = request.CardsIds.Except(currentCardIds).ToList();

        // Supprimer les cartes qui ne sont plus dans la promotion
        foreach (var card in cardsToRemove)
        {
            card.DiscountId = null;
        }

        // Ajouter les nouvelles cartes
        if (cardsToAdd.Any())
        {
            var newCards = await _applicationDbContext
                .Cards.Where(c => cardsToAdd.Contains(c.Id))
                .ToListAsync();

            foreach (var card in newCards)
            {
                card.DiscountId = discount.Id;
            }
        }

        // Mettre à jour les propriétés de la promotion
        discount.Title = request.Title;
        discount.Description = request.Description;
        discount.Rate = request.Rate;
        discount.StartDate = request.StartDate;
        discount.EndDate = request.EndDate;
        discount.Status = request.Status;

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<Discount> GetDiscountAsync(Guid discountId)
    {
        Discount discount =
            await _applicationDbContext.Discounts.SingleOrDefaultAsync(d => d.Id == discountId)
            ?? throw new RecordNotFoundException($"Promotion avec l'ID {discountId} introuvable");

        return discount;
    }

    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        return await _applicationDbContext
            .Discounts.Where(d => d.Status == "Actif" && d.StartDate <= now && d.EndDate >= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var discounts = await _applicationDbContext.Discounts.ToListAsync();

        bool hasChanges = false;
        foreach (var discount in discounts)
        {
            if (
                discount.Status == "En Attente"
                && discount.StartDate <= now
                && discount.EndDate > now
            )
            {
                discount.Status = "Actif";
                hasChanges = true;
            }

            if (discount.Status == "Actif" && discount.EndDate < now)
            {
                discount.Status = "Inactif";
                hasChanges = true;
            }

            if (discount.Status == "Actif" && discount.StartDate > now)
            {
                discount.Status = "En Attente";
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        return discounts;
    }

    public async Task DeleteDiscountAsync(Guid discountId)
    {
        var discount = await _applicationDbContext.Discounts.FirstOrDefaultAsync(d =>
            d.Id == discountId
        );

        if (discount == null)
        {
            throw new RecordNotFoundException($"Promotion avec l'ID {discountId} introuvable");
        }

        // Supprimer les références dans les cartes
        var cards = await _applicationDbContext
            .Cards.Where(c => c.DiscountId == discountId)
            .ToListAsync();

        foreach (var card in cards)
        {
            card.DiscountId = null;
        }

        _applicationDbContext.Discounts.Remove(discount);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Card>> GetDiscountCardsAsync(Guid discountId)
    {
        return await _applicationDbContext
            .Cards.Where(c => c.DiscountId == discountId)
            .ToListAsync();
    }

    public async Task AddCardsToDiscountAsync(Guid discountId, List<Guid> cardIds)
    {
        // Vérifier que la promotion existe
        var discount = await _applicationDbContext.Discounts.FirstOrDefaultAsync(d =>
            d.Id == discountId
        );

        if (discount == null)
        {
            throw new RecordNotFoundException($"Promotion avec l'ID {discountId} introuvable");
        }

        // Valider que toutes les cartes existent
        await ValidateCardsExistAsync(cardIds);

        // Récupérer les cartes à ajouter
        var cardsToAdd = await _applicationDbContext
            .Cards.Where(c => cardIds.Contains(c.Id))
            .ToListAsync();

        // Ajouter les cartes qui ne sont pas déjà associées à cette promotion
        foreach (var card in cardsToAdd)
        {
            if (card.DiscountId != discountId)
            {
                card.DiscountId = discountId;
            }
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    private async Task ValidateCardsExistAsync(List<Guid> cardIds)
    {
        var existingCardIds = await _applicationDbContext
            .Cards.Where(c => cardIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var missingCardIds = cardIds.Except(existingCardIds).ToList();

        if (missingCardIds.Any())
        {
            throw new RecordNotFoundException(
                $"Cartes introuvables: {string.Join(", ", missingCardIds)}"
            );
        }
    }
}
