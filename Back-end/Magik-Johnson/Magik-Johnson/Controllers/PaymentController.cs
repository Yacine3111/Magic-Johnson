using FluentValidation;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Magik_Johnson.Controllers;

[ApiController]
[Authorize]
public class PaymentController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IRefundRequestService _refundRequestService;
    private readonly IPendingPaymentService _pendingPaymentService;
    private readonly IValidator<IEnumerable<PaymentRequest>> _paymentRequestValidator;
    private readonly IValidator<CreateCheckoutRequest> _createcheckoutRequestvalidation;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        IRefundRequestService refundRequestService,
        IPendingPaymentService pendingPaymentService,
        IValidator<IEnumerable<PaymentRequest>> paymentRequestValidator,
        IValidator<CreateCheckoutRequest> createcheckoutRequestvalidation,
        UserManager<ApplicationUser> userManager
    )
    {
        _orderService = orderService;
        _refundRequestService = refundRequestService;
        _paymentService = paymentService;
        _pendingPaymentService = pendingPaymentService;
        _paymentRequestValidator = paymentRequestValidator;
        _createcheckoutRequestvalidation = createcheckoutRequestvalidation;
        _userManager = userManager;
    }

    [HttpPost("card")]
    [Authorize(Roles = "client,employee")]
    public async Task<IActionResult> CreateCheckoutCardSession(
        [FromBody] CreateCheckoutRequest request
    )
    {
        await _createcheckoutRequestvalidation.ValidateAndThrowAsync(request);
        await _paymentRequestValidator.ValidateAndThrowAsync(request.ItemsRequest);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("client"))
        {
            if (request.Email != user.Email)
                return BadRequest();
            request.FirstName = user.FirstName;
            request.LastName = user.LastName;
        }

        var session = await _paymentService.CreateCheckoutSessionAsync(
            request.ItemsRequest,
            request.DeliveryMode,
            request.ShippingCost,
            request.ShippingAddress,
            request.Email,
            HttpContext
        );

        var totalAmount = (session.AmountTotal ?? 0) / 100m;

        await _pendingPaymentService.CreatePendingPaymentAsync(
            new CreatePendingPaymentDto
            {
                Email = request.Email,
                UserId = user.Id,
                PaymentIntentId = session.PaymentIntentId ?? "",
                SessionId = session.Id,
                TotalAmount = totalAmount,
                ShippingAddress = request.ShippingAddress,
                DeliveryMode = request.DeliveryMode,
                Items = request.ItemsRequest,
            }
        );

        return Ok(session.Url);
    }

    [HttpPost("cash")]
    [Authorize(Roles = "employee")]
    public async Task<IActionResult> CreateCheckoutCashSession(
        [FromBody] CreateCheckoutRequest request
    )
    {
        await _createcheckoutRequestvalidation.ValidateAndThrowAsync(request);
        await _paymentRequestValidator.ValidateAndThrowAsync(request.ItemsRequest);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        await _orderService.CreateOrderFromCashCheckoutAsync(request, user.Id);

        return Ok();
    }

    [HttpPost("order")]
    [Authorize(Roles = "client,employee")]
    public async Task<IActionResult> CreateOrder([FromBody] string email)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Forbid();

        var roles = await _userManager.GetRolesAsync(user);

        Order? result;

        if (roles.Contains("client"))
        {
            result = await _orderService.CreateOrderFromPendingAsync(user.Id);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest();

            result = await _orderService.CreateOrderFromPendingForClientByEmployeeAsync(
                email,
                user.Id
            );
        }

        if (result is null)
        {
            return Conflict();
        }

        if (result.Email is not null)
        {
            await _paymentService.SendReceiptAsync(result.Email);
        }

        return Ok(result);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund(RefundRequest request)
    {
        await _refundRequestService.RefundOrder(request);
        return Ok();
    }
}
