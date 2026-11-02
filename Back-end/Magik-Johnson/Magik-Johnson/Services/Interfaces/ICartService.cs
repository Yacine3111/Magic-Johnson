using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;

namespace Magik_Johnson.Services.Interfaces;

public interface ICartService
{
    Task CreateCarteAsync(string userId);

    Task AddToCartAsync(string userId, AddToCartRequest request);

    Task<CartResponse> GetCartAsync(string userId);

    Task DeleteCardFromCartAsync(string userId, Guid cardId, DeleteCardFromCartRequest request);
}
