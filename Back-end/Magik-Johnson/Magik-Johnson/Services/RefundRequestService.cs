using Magik_Johnson.CustomException;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Responses;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Magik_Johnson.Services;

public class RefundRequestService : IRefundRequestService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public RefundRequestService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<bool> AddRefundRequestAsync(RefundRequestRequest request)
    {
        var orderId = Guid.Parse(request.OrderId);

        //Aller chercher la commande concernée par la demande de remboursement
        var associatedOrder = await _applicationDbContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == orderId
        );

        if (associatedOrder == null)
        {
            throw new Exception("Aucune commande d'associée a cette demande de remboursement");
        }

        var refundPaymentIntentId = associatedOrder.PaymentIntentId;

        var newRefundRequest = new Models.Refund
        {
            OrderId = Guid.Parse(request.OrderId),
            PaymentIntentId = refundPaymentIntentId,
            TotalAmount = request.TotalAmount,
            Status = "en attente",
            IsPartialRefund = request.IsPartial,
            PreviousOrderStatusId = Guid.Parse(request.PreviousOrderStatusId),
            RefundJustification = request.RefundJustification,
            Comments = request.Comments ?? null,
        };

        if (request.Items != null && request.Items.Any())
        {
            var cardIds = request.Items.Select(x => x.CardId).ToList();

            var cards = await _applicationDbContext
                .Cards.Where(c => cardIds.Contains(c.Id))
                .ToListAsync();

            var cardOrder = await _applicationDbContext
                .CardOrders.Where(c =>
                    cardIds.Contains(c.CardId) && Guid.Parse(request.OrderId) == c.OrderId
                )
                .ToListAsync();

            newRefundRequest.AmountToRefund = CalculateAmountToRefund(cardOrder);
            newRefundRequest.Cards = cards;
        }
        else
        {
            if (request.IsStillPreparing == true)
            {
                newRefundRequest.AmountToRefund = request.TotalAmount;
            }
            else
            {
                var cardOrder = await _applicationDbContext
                    .CardOrders.Where(c => Guid.Parse(request.OrderId) == c.OrderId)
                    .ToListAsync();

                newRefundRequest.AmountToRefund = CalculateAmountToRefund(cardOrder);
            }
            newRefundRequest.TotalAmount = request.TotalAmount;
        }

        //Ajout de la demande de la nouvelle demande de remboursement
        _applicationDbContext.Refunds.Add(newRefundRequest);
        var response = await _applicationDbContext.SaveChangesAsync();

        //Vérification que la demande a été créée avant de changer le statut de la commande vers "En attente"

        if (await ChangeOrderStatusToPending(response, associatedOrder))
        {
            return true;
        }

        return false;
    }

    private static double CalculateAmountToRefund(List<CardOrder> cardOrder)
    {
        double amountToRefund = 0;

        foreach (var card in cardOrder)
        {
            amountToRefund += card.UnitPricePaid * card.Quantity * 1.14975;
        }

        return amountToRefund;
    }

    private async Task<bool> ChangeOrderStatusToPending(int response, Order associatedOrder)
    {
        if (response > 0)
        {
            associatedOrder.OrderStatusId = Guid.Parse("d8c70d58-a557-4ea2-9301-5bd6626eb585");
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<RefundResponse> GetRefundRequests()
    {
        var response = await _applicationDbContext.Refunds.Include(r => r.Order).ToListAsync();

        if (response == null)
        {
            return new RefundResponse();
        }

        return new RefundResponse { Refunds = response };
    }

    public async Task<bool> UpdateRefundStatus(UpdateRefundStatusRequest request)
    {
        Guid refundId = Guid.Parse(request.RefundRequestId);

        var refund = await _applicationDbContext.Refunds.FirstOrDefaultAsync(r => r.Id == refundId);

        if (refund == null)
            return false;

        var order = await _applicationDbContext.Orders.FirstOrDefaultAsync(o =>
            o.Id == refund.OrderId
        );

        if (order == null)
            return false;

        refund.Status = request.NewRefundRequestStatus;

        var rejectedStatus = Guid.Parse("02792325-3b41-41e8-a1e8-a8f679b0d124");

        if (request.NewRefundRequestStatus == "refusée")
            order.OrderStatusId = rejectedStatus;

        await _applicationDbContext.SaveChangesAsync();

        return refund.IsPartialRefund || request.NewRefundRequestStatus == "acceptée";
    }

    private async Task<bool> ReturnCardsToInventory(Guid orderId, List<CardOrder>? cards)
    {
        if (cards == null || cards.Any())
        {
            var allCardsInOrder = _applicationDbContext
                .CardOrders.Where(co => co.OrderId == orderId)
                .ToList();

            foreach (var card in allCardsInOrder)
            {
                Guid? actualLanguageId = card.LanguageId == Guid.Empty ? null : card.LanguageId;

                var cardQuantity = await _applicationDbContext.Quantities.FirstOrDefaultAsync(cq =>
                    cq.CardId == card.CardId && cq.LanguageId == actualLanguageId
                );

                if (cardQuantity != null)
                {
                    if (card.IsFoil)
                    {
                        cardQuantity.FoilQuantity = cardQuantity.FoilQuantity + card.Quantity;
                    }
                    else
                    {
                        cardQuantity.NonFoilQuantity = cardQuantity.NonFoilQuantity + card.Quantity;
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
            }
            return true;
        }
        else
        {
            foreach (var card in cards)
            {
                Guid? actualLanguageId = card.LanguageId == Guid.Empty ? null : card.LanguageId;

                var cardQuantity = await _applicationDbContext.Quantities.FirstOrDefaultAsync(cq =>
                    cq.CardId == card.CardId && cq.LanguageId == actualLanguageId
                );

                if (cardQuantity != null)
                {
                    if (card.IsFoil)
                    {
                        cardQuantity.FoilQuantity = cardQuantity.FoilQuantity + card.Quantity;
                    }
                    else
                    {
                        cardQuantity.NonFoilQuantity = cardQuantity.NonFoilQuantity + card.Quantity;
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
            }
            return true;
        }
    }

    public async Task RefundOrder(RefundRequest request)
    {
        RefundService refundService = new();

        var refundRequest = await GetRefundRequest(request.RefundRequestId);
        var order = await GetOrder(request.OrderId);
        var allCardsInOrder = await GetCardsInOrder(order.Id);

        if (!request.IsAccepted)
        {
            return;
        }

        if (!request.IsPartial)
        {
            await HandleFullRefund(request, refundRequest, order, allCardsInOrder, refundService);
            await ReturnCardsToInventory(request.OrderId, null);
        }
        else
        {
            await HandlePartialRefund(
                request,
                refundRequest,
                order,
                allCardsInOrder,
                refundService
            );
        }
    }

    private async Task<Models.Refund> GetRefundRequest(Guid id)
    {
        return await _applicationDbContext
                .Refunds.Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new RecordNotFoundException("La demande de remboursement n'existe pas");
    }

    private async Task<Order> GetOrder(Guid id)
    {
        return await _applicationDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new RecordNotFoundException("L'order n'existe pas");
    }

    private async Task<List<CardOrder>> GetCardsInOrder(Guid orderId)
    {
        return await _applicationDbContext
            .CardOrders.Where(co => co.OrderId == orderId)
            .ToListAsync();
    }

    private async Task HandleFullRefund(
        RefundRequest request,
        Models.Refund refundRequest,
        Order order,
        List<CardOrder> allCards,
        RefundService refundService
    )
    {
        bool isStillPreparing =
            refundRequest.PreviousOrderStatusId
            == Guid.Parse("EE4BB5B8-94B0-4BFE-8C59-34A4C35EE052");

        var options = new RefundCreateOptions
        {
            PaymentIntent = request.PaymentIntentId,
            Amount = isStillPreparing ? null : await CalculateRefundAmount(order, allCards),
        };

        await refundService.CreateAsync(options);

        Guid refundedStatusId = Guid.Parse("6F6A895D-67F0-4DA5-A873-63588D7A1C73");
        order.OrderStatusId = refundedStatusId;
        await _applicationDbContext.SaveChangesAsync();
    }

    private async Task HandlePartialRefund(
        RefundRequest request,
        Models.Refund refundRequest,
        Order order,
        List<CardOrder> allCards,
        RefundService refundService
    )
    {
        var selectedCardIds = refundRequest.Cards.Select(c => c.Id).ToList();

        var selectedCards = allCards.Where(c => selectedCardIds.Contains(c.CardId)).ToList();

        var amount = await CalculateRefundAmount(order, selectedCards);

        var options = new RefundCreateOptions
        {
            PaymentIntent = request.PaymentIntentId,
            Amount = amount,
        };

        await refundService.CreateAsync(options);

        await ReturnCardsToInventory(request.OrderId, selectedCards);

        foreach (var card in selectedCards)
        {
            card.IsRefunded = true;
        }

        if (request.IsPartial)
        {
            var cardOrders = order.CardOrders.Where(c => !c.IsRefunded).ToList();

            bool noMoreCardsToRefund = cardOrders == null || !cardOrders.Any();

            Guid refundedStatusId = Guid.Parse("6F6A895D-67F0-4DA5-A873-63588D7A1C73");
            Guid partiallyRefundedStatusId = Guid.Parse("353FC37F-8DDC-49F1-80E8-338028896C4B");

            order.OrderStatusId = noMoreCardsToRefund
                ? refundedStatusId
                : partiallyRefundedStatusId;

            await _applicationDbContext.SaveChangesAsync();
        }

        _applicationDbContext.CardOrders.UpdateRange(selectedCards);

        await _applicationDbContext.SaveChangesAsync();
    }

    private Task<long> CalculateRefundAmount(Order order, List<CardOrder> cards)
    {
        decimal taxFactor = (order.TaxeAmount / order.SubTotal) + 1;
        decimal subTotal = cards.Sum(c => c.Quantity * (decimal)c.UnitPricePaid);

        decimal total = subTotal * taxFactor;

        return Task.FromResult((long)(total * 100));
    }

    public async Task<decimal> GetOrderTaxPercentage(string orderId)
    {
        Order order =
            await _applicationDbContext.Orders.FirstOrDefaultAsync(o => o.Id == Guid.Parse(orderId))
            ?? throw new RecordNotFoundException("l'order n'existe pas");

        return (order.TaxeAmount / order.SubTotal);
    }
}
