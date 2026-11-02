using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class OrderStatusController : BaseController
{
    private readonly IOrderStatusService _orderStatusService;

    public OrderStatusController(IOrderStatusService orderStatusService)
    {
        _orderStatusService = orderStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderStatusNameAsync()
    {
        return Ok(await _orderStatusService.GetOrderStatusNameAsync());
    }
}
