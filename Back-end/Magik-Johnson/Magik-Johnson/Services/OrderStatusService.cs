using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Magik_Johnson.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public OrderStatusService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<OrderStatusResponse> GetOrderStatusNameAsync()
    {
        List<OrderStatus> orderStatus = await _applicationDbContext.OrderStatus.ToListAsync();

        List<string> orderStatusName = [];

        foreach (var orderStat in orderStatus)
        {
            orderStatusName.Add(orderStat.Name);
        }

        return new OrderStatusResponse { Status = orderStatusName };
    }
}
