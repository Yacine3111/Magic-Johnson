using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class RefundRequestController : BaseController
{
    private readonly IRefundRequestService _refundRequestService;

    public RefundRequestController(IRefundRequestService refundRequestService)
    {
        _refundRequestService = refundRequestService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRefundRequest([FromBody] RefundRequestRequest request)
    {
        var response = await _refundRequestService.AddRefundRequestAsync(request);

        if (!response)
        {
            return BadRequest();
        }

        return Ok();
    }

    [Authorize(Roles = "manager,employee")]
    [HttpGet("getRefundRequests")]
    public async Task<IActionResult> GetRefundRequest()
    {
        var response = await _refundRequestService.GetRefundRequests();
        return Ok(response);
    }

    [Authorize(Roles = "manager,employee")]
    [HttpPut("updateStatus")]
    public async Task<IActionResult> UpdateRefundStatus(
        [FromBody] UpdateRefundStatusRequest request
    )
    {
        var response = await _refundRequestService.UpdateRefundStatus(request);

        if (!response)
        {
            return NoContent();
        }

        return Ok();
    }

    [HttpGet("taxPercentage")]
    public async Task<decimal> GetOrderTaxPercentage([FromQuery] string orderId)
    {
        decimal taxPercentage = await _refundRequestService.GetOrderTaxPercentage(orderId);
        return taxPercentage;
    }
}
