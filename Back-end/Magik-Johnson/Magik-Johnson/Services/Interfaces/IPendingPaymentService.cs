using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Enums;
using Magik_Johnson.Models;

namespace Magik_Johnson.Services.Interfaces;

public interface IPendingPaymentService
{
    Task<PendingPayment> CreatePendingPaymentAsync(CreatePendingPaymentDto dto);
    Task DeleteLastByEmailAsync(string email);
    Task<PendingPayment?> GetLastByEmailAsync(string email);
    Task<PendingPayment?> GetLastWhenCompleteByEmailAsync(string email);
}
