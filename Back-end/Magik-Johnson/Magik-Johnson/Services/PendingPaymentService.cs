namespace Magik_Johnson.Services;

using System.Text.Json;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PendingPaymentService : IPendingPaymentService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PendingPaymentService(ApplicationDbContext context)
    {
        _applicationDbContext = context;
    }

    public async Task<PendingPayment> CreatePendingPaymentAsync(CreatePendingPaymentDto dto)
    {
        var payment = new PendingPayment
        {
            PaymentIntentId = dto.PaymentIntentId,
            TotalAmount = dto.TotalAmount,
            ShippingAddress = dto.ShippingAddress,
            ItemsJson = JsonSerializer.Serialize(dto.Items),
            CreatedAt = DateTime.UtcNow,
            DeliveryMode = dto.DeliveryMode,
            UserId = dto.UserId ?? "",
            SessionId = dto.SessionId,
            Email = dto.Email,
        };

        _applicationDbContext.PendingPayments.Add(payment);

        await _applicationDbContext.SaveChangesAsync();

        return payment;
    }

    public async Task<PendingPayment?> GetLastByEmailAsync(string email)
    {
        return await _applicationDbContext
            .PendingPayments.Where(p => p.Email == email && !p.IsCompleted)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<PendingPayment?> GetLastWhenCompleteByEmailAsync(string email)
    {
        return await _applicationDbContext
            .PendingPayments.Where(p => p.Email == email && p.IsCompleted)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteLastByEmailAsync(string email)
    {
        var lastPending = await GetLastByEmailAsync(email);

        if (lastPending != null)
        {
            _applicationDbContext.PendingPayments.Remove(lastPending);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
