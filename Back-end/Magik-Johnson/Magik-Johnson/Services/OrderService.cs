using System.Text;
using System.Text.Json;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Cards;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Enums;
using Magik_Johnson.Extensions;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Stripe.Tax;

namespace Magik_Johnson.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly EmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderService(
        ApplicationDbContext applicationDbContext,
        EmailSender emailSender,
        UserManager<ApplicationUser> userManager
    )
    {
        _applicationDbContext = applicationDbContext;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<OrderResponse> GetOrdersAsync(string userId)
    {
        var orders = await _applicationDbContext
            .Orders.Include(o => o.OrderStatus)
            .Include(o => o.CardOrders)
            .Where(o => o.UserId == userId)
            .ToListAsync();

        if (orders.Count == 0)
        {
            return new OrderResponse { Items = [] };
        }

        var employeeIds = orders
            .Where(o => !string.IsNullOrEmpty(o.EmployeeId))
            .Select(o => o.EmployeeId!)
            .Distinct()
            .ToList();

        Dictionary<string, (string FirstName, string LastName)> employees =
            employeeIds.Count > 0
                ? await _userManager
                    .Users.Where(u => employeeIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => (u.FirstName, u.LastName))
                : new Dictionary<string, (string FirstName, string LastName)>();

        var orderSummaries = orders
            .Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                EmployeeId = o.EmployeeId,
                EmployeeFirstName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].FirstName
                        : null,
                EmployeeLastName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].LastName
                        : null,
                Email = o.Email,
                OrderStatusId = o.OrderStatusId,
                OrderStatus = o.OrderStatus,
                CheckoutDate = o.CheckoutDate,
                DeliveryDate = o.DeliveryDate,
                SubTotal = o.SubTotal,
                TaxeAmount = o.TaxeAmount,
                DeliveryMode = o.DeliveryMode,
                TotalQuantity = o.CardOrders.Sum(co => co.Quantity),
            })
            .ToList();

        return new OrderResponse { Items = orderSummaries };
    }

    public async Task<OrderResponse> GetAllOrdersAsync()
    {
        var orders = await _applicationDbContext
            .Orders.Include(o => o.OrderStatus)
            .Include(o => o.CardOrders)
            .ToListAsync();

        if (orders == null || orders.Count == 0)
        {
            return new OrderResponse { Items = new List<OrderSummaryResponse>() };
        }

        var employeeIds = orders
            .Where(o => !string.IsNullOrEmpty(o.EmployeeId))
            .Select(o => o.EmployeeId!)
            .Distinct()
            .ToList();

        Dictionary<string, (string FirstName, string LastName)> employees =
            employeeIds.Count > 0
                ? await _userManager
                    .Users.Where(u => employeeIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => (u.FirstName, u.LastName))
                : new Dictionary<string, (string FirstName, string LastName)>();

        var orderSummaries = orders
            .Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                EmployeeId = o.EmployeeId,
                EmployeeFirstName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].FirstName
                        : null,
                EmployeeLastName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].LastName
                        : null,
                Email = o.Email,
                OrderStatusId = o.OrderStatusId,
                OrderStatus = o.OrderStatus,
                CheckoutDate = o.CheckoutDate,
                DeliveryDate = o.DeliveryDate,
                SubTotal = o.SubTotal,
                TaxeAmount = o.TaxeAmount,
                DeliveryMode = o.DeliveryMode,
                TotalQuantity = o.CardOrders.Sum(co => co.Quantity),
            })
            .ToList();

        return new OrderResponse { Items = orderSummaries };
    }

    public async Task<List<OrderSummaryResponse>> GetFilteredOrdersAsync(
        string[]? status,
        string userId,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        string? startTime,
        string? endTime,
        bool employeeOnly,
        bool restrictToUser
    )
    {
        if (startDate > endDate)
        {
            throw new ArgumentException(
                "La date de début ne peut pas être postérieure à la date de fin."
            );
        }

        var query = _applicationDbContext
            .Orders.Include(q => q.OrderStatus)
            .Include(q => q.CardOrders)
            .AsQueryable();

        if (employeeOnly)
        {
            query = query.Where(o => o.EmployeeId == userId);
        }
        if (startDate.HasValue)
        {
            query = query.Where(o => o.CheckoutDate >= startDate);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            var endOfDayWithOffset = new DateTimeOffset(endOfDay, endDate.Value.Offset);
            query = query.Where(o => o.CheckoutDate <= endOfDayWithOffset);
        }
        else if (restrictToUser)
        {
            query = query.Where(o => o.UserId == userId);
        }

        if (status is { Length: > 0 })
        {
            query = query.Where(o => status.Contains(o.OrderStatus.Name));
        }

        var noStatus = status == null || status.Length == 0;
        var noDateRange = startDate == null || endDate == null;
        var noTimeRange = string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime);

        if (noStatus && noDateRange && noTimeRange && !employeeOnly)
        {
            return new List<OrderSummaryResponse>();
        }

        var orders = await query.ToListAsync();

        if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
        {
            if (
                TimeSpan.TryParse(startTime, out var startTs)
                && TimeSpan.TryParse(endTime, out var endTs)
            )
            {
                var offset = DateTimeOffset.Now.Offset;
                if (startTs <= endTs)
                {
                    orders = orders
                        .Where(o =>
                            (
                                o.CheckoutDate.TimeOfDay >= startTs
                                && o.CheckoutDate.TimeOfDay <= endTs
                            )
                            || (
                                o.CheckoutDate.ToOffset(offset).TimeOfDay >= startTs
                                && o.CheckoutDate.ToOffset(offset).TimeOfDay <= endTs
                            )
                        )
                        .ToList();
                }
                else
                {
                    orders = orders
                        .Where(o =>
                            (
                                o.CheckoutDate.TimeOfDay >= startTs
                                || o.CheckoutDate.TimeOfDay <= endTs
                            )
                            || (
                                o.CheckoutDate.ToOffset(offset).TimeOfDay >= startTs
                                || o.CheckoutDate.ToOffset(offset).TimeOfDay <= endTs
                            )
                        )
                        .ToList();
                }
            }
        }

        if (orders.Count == 0)
        {
            return new List<OrderSummaryResponse>();
        }

        var employeeIds = orders
            .Where(o => !string.IsNullOrEmpty(o.EmployeeId))
            .Select(o => o.EmployeeId!)
            .Distinct()
            .ToList();

        Dictionary<string, (string FirstName, string LastName)> employees =
            employeeIds.Count > 0
                ? await _userManager
                    .Users.Where(u => employeeIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => (u.FirstName, u.LastName))
                : new Dictionary<string, (string FirstName, string LastName)>();

        List<OrderSummaryResponse> response = orders
            .Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                EmployeeId = o.EmployeeId,
                EmployeeFirstName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].FirstName
                        : null,
                EmployeeLastName =
                    o.EmployeeId != null && employees.ContainsKey(o.EmployeeId)
                        ? employees[o.EmployeeId].LastName
                        : null,
                Email = o.Email,
                OrderStatusId = o.OrderStatusId,
                OrderStatus = o.OrderStatus,
                CheckoutDate = o.CheckoutDate.ToOffset(DateTimeOffset.Now.Offset),
                DeliveryDate = o.DeliveryDate,
                SubTotal = o.SubTotal,
                TaxeAmount = o.TaxeAmount,
                DeliveryMode = o.DeliveryMode,
                TotalQuantity = o.CardOrders.Sum(co => co.Quantity),
            })
            .ToList();

        return response;
    }

    private async Task<Dictionary<Guid, string>> GetSetNamesDictionaryAsync(List<Guid> setIds)
    {
        return await _applicationDbContext
            .Sets.Where(s => setIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);
    }

    private static string GetCardColors(string manaCost)
    {
        var colors = new List<string>();
        if (manaCost.Contains("W"))
            colors.Add("Blanc");
        if (manaCost.Contains("U"))
            colors.Add("Bleu");
        if (manaCost.Contains("B"))
            colors.Add("Noir");
        if (manaCost.Contains("R"))
            colors.Add("Rouge");
        if (manaCost.Contains("G"))
            colors.Add("Vert");

        return colors.Count == 0 ? "Incolore" : string.Join(", ", colors);
    }

    public async Task<OrderItemsResponse> GetOrderItemsByIdAsync(Guid orderId)
    {
        Order? order = await _applicationDbContext
            .Orders.Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.CardTypes)
            .ThenInclude(ct => ct.Type)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.CardSubTypes)
            .ThenInclude(cst => cst.SubType)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.Quantities)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Language)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new OrderItemsResponse { Items = new List<OrderItemResponse>() };
        }

        return await BuildOrderItemsResponse(order);
    }

    public async Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, bool onlyNonRefunded)
    {
        Order? order = await _applicationDbContext
            .Orders.Include(o => o.OrderStatus)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.CardTypes)
            .ThenInclude(ct => ct.Type)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.CardSubTypes)
            .ThenInclude(cst => cst.SubType)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Card)
            .ThenInclude(c => c!.Quantities)
            .Include(c => c.CardOrders)
            .ThenInclude(ct => ct.Language)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return null;
        }

        ApplicationUser? client = null;
        if (!string.IsNullOrEmpty(order.UserId))
        {
            client = await _userManager.FindByIdAsync(order.UserId);
        }

        var itemsResponse = await BuildOrderItemsResponse(order, onlyNonRefunded);
        decimal totalPrice = itemsResponse.Items.Sum(item => item.TotalPrice);
        int totalQuantity = itemsResponse.Items.Sum(item => item.Quantity);

        return new OrderDetailResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            ClientFirstName = client?.FirstName,
            ClientLastName = client?.LastName,
            ClientEmail = client?.Email,
            OrderStatusId = order.OrderStatusId,
            OrderStatusName = order.OrderStatus?.Name ?? string.Empty,
            CheckoutDate = order.CheckoutDate.ToOffset(DateTimeOffset.Now.Offset),
            DeliveryDate = order.DeliveryDate.ToOffset(DateTimeOffset.Now.Offset),
            Items = itemsResponse.Items,
            TotalPrice = totalPrice,
            TotalQuantity = totalQuantity,
            SubTotal = order.SubTotal,
            TaxeAmount = order.TaxeAmount,
            DeliveryMode = order.DeliveryMode,
        };
    }

    private async Task<OrderItemsResponse> BuildOrderItemsResponse(
        Order order,
        bool onlyNonRefunded = false
    )
    {
        var cardOrders = order.CardOrders.Where(c => !onlyNonRefunded || !c.IsRefunded).ToList();

        var setIds = cardOrders.Select(ct => ct.Card!.SetId).Distinct().ToList();
        var sets = await GetSetNamesDictionaryAsync(setIds);

        var discountIds = cardOrders
            .Where(ct => ct.Card!.DiscountId.HasValue)
            .Select(ct => ct.Card!.DiscountId!.Value)
            .Distinct()
            .ToList();

        var discountsDict = await _applicationDbContext
            .Discounts.Where(d => discountIds.Contains(d.Id) && d.Status == "Actif")
            .ToDictionaryAsync(d => d.Id);

        var items = new List<OrderItemResponse>();

        foreach (var co in cardOrders)
        {
            var card = co.Card!;
            var discount =
                card.DiscountId.HasValue && discountsDict.ContainsKey(card.DiscountId.Value)
                    ? discountsDict[card.DiscountId.Value].Rate
                    : 0;

            var cardResponse = BuildCardResponse(card, sets, discount);

            var unitPrice = await GetFinalUnitPriceAsync(card, co.IsFoil);

            items.Add(
                new OrderItemResponse
                {
                    Card = cardResponse,
                    Quantity = co.Quantity,
                    IsFoil = co.IsFoil,
                    LanguageName = co.Language?.Name ?? "English",
                    UnitPrice = unitPrice,
                }
            );
        }

        return new OrderItemsResponse { Items = items };
    }

    private CardResponse BuildCardResponse(
        Models.Card card,
        Dictionary<Guid, string> sets,
        int discount
    )
    {
        return new CardResponse
        {
            Id = card.Id,
            SetId = card.SetId,
            SetName = sets.GetValueOrDefault(card.SetId, "Inconnu"),
            ScryfallId = card.ScryfallId,
            Name = card.Name,
            ManaCost = card.ManaCost,
            Colors = GetCardColors(card.ManaCost ?? ""),
            Text = card.Text,
            Power = card.Power,
            Toughness = card.Toughness,
            Rarity = card.Rarity,
            Number = card.Number,
            HasFoil = card.HasFoil,
            HasNonFoil = card.HasNonFoil,
            Price = card.Price,
            FoilPrice = card.FoilPrice,
            Discount = discount,
            TotalQuantity = card.Quantities.Sum(q => q.FoilQuantity + q.NonFoilQuantity),
            Types = card.CardTypes.Select(ct => ct.Type.Name).ToList(),
            SubTypes = card.CardSubTypes.Select(cst => cst.SubType.Name).ToList(),
        };
    }

    private async Task<decimal> GetFinalUnitPriceAsync(Models.Card card, bool isFoil)
    {
        if (card == null)
            throw new ArgumentNullException(nameof(card));

        // Prix de base selon foil ou non
        decimal basePrice = isFoil ? card.FoilPrice : card.Price;

        // Récupérer le taux de discount actif
        decimal discountRate = 0;
        if (card.DiscountId.HasValue)
        {
            discountRate = await _applicationDbContext
                .Discounts.Where(d => d.Id == card.DiscountId.Value && d.Status == "Actif")
                .Select(d => d.Rate)
                .FirstOrDefaultAsync();
        }

        // Appliquer le discount
        return basePrice * (1 - (discountRate / 100m));
    }

    public async Task<bool> UpdateOrderStatus(UpdateOrderStatus update)
    {
        if (
            !Guid.TryParse(update.OrderId, out var orderId)
            || !Guid.TryParse(update.NewStatus, out var statusId)
        )
        {
            return false;
        }

        var order = await _applicationDbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return false;
        }

        order.OrderStatusId = statusId;

        await _applicationDbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Order?> CreateOrderFromPendingForClientByEmployeeAsync(
        string email,
        string employeeId
    )
    {
        var pending = await _applicationDbContext
            .PendingPayments.Where(p => p.Email == email && !p.IsCompleted)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (pending is null)
            return null;

        var items = JsonSerializer.Deserialize<List<PaymentRequest>>(pending.ItemsJson) ?? [];

        SessionService sessionService = new();
        Session session = await sessionService.GetAsync(pending.SessionId);

        var order = await CreateOrderAsync(
            pending.UserId,
            pending.Email,
            [.. items.Select(i => new OrderItemInput(i.CardId, i.Quantity, i.IsFoil, i.Language))],
            session.PaymentIntentId,
            (decimal)session.AmountSubtotal!,
            session.TotalDetails.AmountTax,
            pending.DeliveryMode,
            employeeId
        );

        // Marquer le pending comme terminé
        pending.IsCompleted = true;

        await _applicationDbContext.SaveChangesAsync();

        return order;
    }

    public async Task<Order?> CreateOrderFromPendingAsync(string userId)
    {
        var lastPending = await _applicationDbContext
            .PendingPayments.Where(p => p.UserId == userId && !p.IsCompleted)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastPending is null)
        {
            return null;
        }

        var items = JsonSerializer.Deserialize<List<PaymentRequest>>(lastPending.ItemsJson);
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Aucun article trouvé dans le paiement.");

        SessionService sessionService = new();
        Session session = await sessionService.GetAsync(lastPending.SessionId);

        var order = await CreateOrderAsync(
            userId,
            lastPending.Email,
            [.. items.Select(i => new OrderItemInput(i.CardId, i.Quantity, i.IsFoil, i.Language))],
            session.PaymentIntentId,
            (decimal)session.AmountSubtotal!,
            session.TotalDetails.AmountTax,
            lastPending.DeliveryMode
        );

        await _applicationDbContext.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> CreateOrderFromCashCheckoutAsync(
        CreateCheckoutRequest request,
        string? employeeId = null
    )
    {
        var calculationLineItem = new List<CalculationLineItemOptions>();

        foreach (var item in request.ItemsRequest)
        {
            var amount = item.Price * 100 * item.Quantity;

            calculationLineItem.Add(
                new CalculationLineItemOptions
                {
                    Amount = (long)amount,
                    Quantity = item.Quantity,
                    Reference = item.CardId.ToString(),
                    TaxCode = "txcd_10000000",
                }
            );
        }

        var taxOptions = new CalculationCreateOptions
        {
            Currency = "cad",
            LineItems = calculationLineItem,
            CustomerDetails = new CalculationCustomerDetailsOptions
            {
                Address = new AddressOptions
                {
                    Line1 = "4430 Rue Wellington",
                    City = "Verdun",
                    State = "QC",
                    PostalCode = "H4G 1W5",
                    Country = "CA",
                },
                AddressSource = "billing",
            },
        };

        var calculationService = new CalculationService();
        Calculation calculation = await calculationService.CreateAsync(taxOptions);

        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = calculation.AmountTotal,
            Currency = "cad",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never",
            },
            PaymentMethod = "pm_card_visa",
            Confirm = true,
        };

        var paymentIntentService = new PaymentIntentService();
        PaymentIntent paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

        var user = await _userManager.FindByEmailAsync(request.Email);

        var order = await CreateOrderAsync(
            user?.Id,
            request.Email,
            [
                .. request.ItemsRequest.Select(i => new OrderItemInput(
                    i.CardId,
                    i.Quantity,
                    i.IsFoil,
                    i.Language
                )),
            ],
            paymentIntent.Id,
            calculation.AmountTotal - calculation.TaxAmountExclusive,
            calculation.TaxAmountExclusive,
            request.DeliveryMode,
            employeeId
        );

        await SendReceiptAsync(
            request.Email,
            request.ShippingCost,
            request.DeliveryMode,
            request.ItemsRequest
        );

        return order;
    }

    private async Task<Order> CreateOrderAsync(
        string? userId,
        string email,
        List<OrderItemInput> items,
        string paymentIntentId,
        decimal subTotal,
        decimal taxAmount,
        DeliveryMode deliveryMode,
        string? employeeId = null
    )
    {
        var random = new Random();
        double days = random.NextDouble() * 3;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            OrderStatusId = await GetDefaultOrderStatusIdAsync(),
            PaymentIntentId = paymentIntentId,
            CheckoutDate = DateTimeOffset.Now,
            DeliveryDate = DateTimeOffset.Now.AddDays(days),
            SubTotal = subTotal / 100,
            TaxeAmount = taxAmount / 100,
            CardOrders = new List<CardOrder>(),
            DeliveryMode = deliveryMode,
            EmployeeId = employeeId,
        };

        foreach (var item in items)
        {
            await UpdateStockAfterOrderAsync(order, item);
        }

        await _applicationDbContext.Orders.AddAsync(order);
        await _applicationDbContext.SaveChangesAsync();

        return order;
    }

    private async Task UpdateStockAfterOrderAsync(Order order, OrderItemInput item)
    {
        var card =
            await _applicationDbContext
                .Cards.Include(c => c.Quantities)
                .FirstOrDefaultAsync(c => c.Id == item.CardId)
            ?? throw new InvalidOperationException($"Carte introuvable (ID: {item.CardId}).");

        var langId = await GetLanguageIdByNameAsync(item.Language, item.CardId);
        var quantity =
            card.Quantities.FirstOrDefault(q => q.LanguageId == langId && q.CardId == item.CardId)
            ?? throw new InvalidOperationException(
                $"Aucun stock pour la langue '{item.Language}'."
            );

        if (item.IsFoil)
        {
            if (quantity.FoilQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{card.Name}' (foil, {item.Language}). "
                        + $"Disponible : {quantity.FoilQuantity}, demandé : {item.Quantity}."
                );
            quantity.FoilQuantity -= item.Quantity;
        }
        else
        {
            if (quantity.NonFoilQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{card.Name}' (non-foil, {item.Language}). "
                        + $"Disponible : {quantity.NonFoilQuantity}, demandé : {item.Quantity}."
                );
            quantity.NonFoilQuantity -= item.Quantity;
        }

        decimal unitPricePaid = await GetFinalUnitPriceAsync(card, item.IsFoil);

        order.CardOrders.Add(
            new CardOrder
            {
                CardId = item.CardId,
                Quantity = item.Quantity,
                IsFoil = item.IsFoil,
                LanguageId = langId,
                UnitPricePaid = (double)unitPricePaid,
            }
        );
    }

    private async Task<Guid> GetDefaultOrderStatusIdAsync()
    {
        var status = await _applicationDbContext.OrderStatus.FirstOrDefaultAsync(s =>
            s.Name == "En préparation"
        );
        if (status == null)
            throw new InvalidOperationException("Statut de commande 'En préparation' introuvable.");
        return status.Id;
    }

    private async Task<Guid?> GetLanguageIdByNameAsync(string? languageName, Guid cardId)
    {
        if (string.IsNullOrWhiteSpace(languageName))
            return null;
        var lang = await _applicationDbContext.Languages.FirstOrDefaultAsync(l =>
            l.Name.ToLower() == LanguageHelper.ToEnglish(languageName).ToLower()
            && l.CardId == cardId
        );
        return lang?.Id;
    }

    private async Task SendReceiptAsync(
        string userEmail,
        decimal shippingCost,
        DeliveryMode deliveryMode,
        IEnumerable<PaymentRequest> items
    )
    {
        StringBuilder itemsHtml = new();

        decimal totalWithNoTax = shippingCost;

        decimal tpsRate = 0.05m;
        decimal tvqRate = 0.09975m;

        if (deliveryMode is DeliveryMode.HomeDelivery)
        {
            itemsHtml.Append(
                $@"
                            <tr style='border-bottom:1px solid #ddd;'>
                                <td style='padding:10px 0; font-size:10px; color:#333; word-break:break-word;'>Livraison</td>
                                <td style='text-align:right; font-size:15px; color:#111; font-weight:bold;'>{shippingCost:C}</td>
                            </tr>"
            );
        }

        foreach (var item in items)
        {
            // Montants Stripe sont en centimes
            decimal unitPrice = item.Price;

            int quantity = item.Quantity;

            decimal subtotal = unitPrice * quantity;
            totalWithNoTax += subtotal;

            itemsHtml.Append(
                $@"
                        <tr style='border-bottom:1px solid #ddd;'>
                            <td style='padding:10px 0; font-size:10px; color:#333; word-break:break-word;'>{item.CardName}</td>
                            <td style='text-align:center; font-size:15px; color:#555;'>{item.Quantity}</td>
                            <td style='text-align:right; font-size:15px; color:#555;'>{unitPrice:C}</td>
                            <td style='text-align:right; font-size:15px; color:#111; font-weight:bold;'>{subtotal:C}</td>
                        </tr>"
            );
        }
        decimal tps = totalWithNoTax * tpsRate;
        decimal tvq = (totalWithNoTax + tps) * tvqRate;
        decimal totalTax = tps + tvq;
        decimal total = totalWithNoTax + totalTax;

        string html =
            $@"
                <html lang='fr'>
                  <head>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                  </head>
                  <body style='font-family:Arial, sans-serif; background-color:#f4f6f8; padding:15px; margin:0;'>
                    <div style='max-width:600px; margin:auto; background:#ffffff; padding:20px; border-radius:8px; box-shadow:0 0 10px rgba(0,0,0,0.05); box-sizing:border-box;'>
                      <h2 style='text-align:center; color:#0d6efd;'>Reçu de votre paiement</h2>
                      <p style='font-size:16px; color:#333;'>Bonjour,</p>
                      <p style='font-size:15px; color:#555;'>Merci pour votre achat. Voici le récapitulatif de votre commande :</p>

                      <div style='width:100%; overflow-x:auto;'>
                        <table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse; min-width:100%; max-width:100%; table-layout:fixed; margin-top:20px;'>
                          <thead>
                            <tr style='background-color:#f0f2f5; text-align:left;'>
                              <th style='padding:10px; font-size:12px; color:#333; text-align:left;'>Article</th>
                              <th style='padding:8px; font-size:12px; color:#333; text-align:center; white-space:nowrap;'>Quantité</th>
                              <th style='padding:10px 0 10px 10px; font-size:12px; color:#333; text-align:right; white-space:nowrap; min-width:80px;'>Prix unitaire</th>
                              <th style='padding:10px 10px 10px 15px; font-size:12px; color:#333; text-align:right; min-width:90px;'>Sous-total</th>
                            </tr>
                          </thead>
                          <tbody>
                            {itemsHtml}
                            <tr>
                              <td colspan='3' style='padding:10px; text-align:right; font-size:13px; color:#333;'>Taxes</td>
                              <td style='padding:10px; text-align:right; font-size:13px; color:#333;'>{totalTax:C}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>

                      <hr style='margin:30px 0; border:none; border-top:1px solid #ddd;' />

                      <p style='font-size:16px; text-align:right; color:#333;'>
                        <strong>Total :</strong> 
                        <span style='font-size:18px; color:#0d6efd;'>{total:C}</span>
                      </p>

                      <p style='font-size:12px; color:#999; text-align:center; margin-top:30px;'>
                        &copy; {DateTime.UtcNow.Year} L'Arène des Stratèges — Tous droits réservés.
                      </p>
                    </div>
                  </body>
                </html>";

        await _emailSender.SendEmailAsync(userEmail, "Votre reçu d'achat", html);
    }

    private record OrderItemInput(Guid CardId, int Quantity, bool IsFoil, string Language);
}
