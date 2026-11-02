using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;

namespace Magik_Johnson.Services.Interfaces;

public interface IDiscountService
{
    Task CreateDiscountAsync(DiscountRequestBase request);

    Task UpdateDiscountAsync(DiscountRequestBase request);

    Task DeleteDiscountAsync(Guid discountId);

    Task<Discount> GetDiscountAsync(Guid discountId);

    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();

    Task<IEnumerable<Discount>> GetAllDiscountsAsync();

    Task<IEnumerable<Card>> GetDiscountCardsAsync(Guid discountId);

    Task AddCardsToDiscountAsync(Guid discountId, List<Guid> cardIds);
}
