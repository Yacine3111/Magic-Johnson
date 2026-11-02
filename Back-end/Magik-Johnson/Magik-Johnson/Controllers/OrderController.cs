using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class OrderController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IOrderService _orderService;

    public OrderController(UserManager<ApplicationUser> userManager, IOrderService orderService)
    {
        _userManager = userManager;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        return Ok(await _orderService.GetOrdersAsync(user.Id));
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(Guid orderId, bool onlyNonRefunded = false)
    {
        return Ok(await _orderService.GetOrderByIdAsync(orderId, onlyNonRefunded));
    }

    [HttpGet("all")]
    [Authorize(Roles = "manager,employee")]
    public async Task<IActionResult> GetAllOrdersAsync()
    {
        var items = await _orderService.GetAllOrdersAsync();
        return Ok(items);
    }

    [HttpGet("{orderId}/items")]
    public async Task<IActionResult> GetOrderItemsById(Guid orderId)
    {
        var items = await _orderService.GetOrderItemsByIdAsync(orderId);
        return Ok(items);
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFilteredOrdersAsync(
        [FromQuery] string[] status,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] string? startTime,
        [FromQuery] string? endTime,
        [FromQuery] bool employeeOnly = false
    )
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);
        var isEmployee = roles.Contains("employee") || roles.Contains("admin");

        if (!isEmployee)
        {
            employeeOnly = false;
        }

        var response = await _orderService.GetFilteredOrdersAsync(
            status,
            user.Id,
            startDate,
            endDate,
            startTime,
            endTime,
            employeeOnly,
            restrictToUser: !isEmployee
        );

        return Ok(response);
    }

    [HttpPut("changeStatus")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatus updates)
    {
        var result = await _orderService.UpdateOrderStatus(updates);

        if (!result)
        {
            return BadRequest();
        }

        return Ok();
    }
}
