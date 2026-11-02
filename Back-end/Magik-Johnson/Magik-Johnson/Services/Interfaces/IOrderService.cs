using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;

namespace Magik_Johnson.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> GetOrdersAsync(string userId);
    Task<List<OrderSummaryResponse>> GetFilteredOrdersAsync(
        string[]? status,
        string userId,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? startTime,
        string? endTime,
        bool employeeOnly,
        bool restrictToUser
    );
    Task<OrderResponse> GetAllOrdersAsync();
    Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, bool onlyNonRefunded);
    Task<OrderItemsResponse> GetOrderItemsByIdAsync(Guid orderId);
    Task<bool> UpdateOrderStatus(UpdateOrderStatus update);
    Task<Order?> CreateOrderFromCashCheckoutAsync(
        CreateCheckoutRequest request,
        string? employeeId = null
    );
    Task<Order?> CreateOrderFromPendingAsync(string userId);
    Task<Order?> CreateOrderFromPendingForClientByEmployeeAsync(string email, string employeeId);
}
