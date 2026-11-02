using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;

namespace Magik_Johnson.Services.Interfaces;

public interface IRefundRequestService
{
    Task<bool> AddRefundRequestAsync(RefundRequestRequest request);

    Task<RefundResponse> GetRefundRequests();
    Task<bool> UpdateRefundStatus(UpdateRefundStatusRequest request);

    Task RefundOrder(RefundRequest request);
    Task<decimal> GetOrderTaxPercentage(string orderId);
}
