using Magik_Johnson.DTOs.Responses;

namespace Magik_Johnson.Services.Interfaces;

public interface IOrderStatusService
{
    Task<OrderStatusResponse> GetOrderStatusNameAsync();
}
