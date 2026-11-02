using System.Text;
using Magik_Johnson.Data;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Enums;
using Magik_Johnson.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace Magik_Johnson.Services;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly EmailSender _emailSender;
    private readonly IPendingPaymentService _pendingPaymentService;

    public PaymentService(
        ApplicationDbContext applicationDbContext,
        IConfiguration config,
        EmailSender emailSender,
        IPendingPaymentService pendingPaymentService
    )
    {
        _config = config;
        _emailSender = emailSender;
        _pendingPaymentService = pendingPaymentService;
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        IEnumerable<PaymentRequest> requests,
        DeliveryMode deliveryMode,
        decimal shippingCost,
        string? ShippingAddress,
        string userEmail,
        HttpContext httpContext
    )
    {
        var baseUrl = _config["FrontUrl"];

        var lineItems = new List<SessionLineItemOptions>();

        if (deliveryMode == DeliveryMode.HomeDelivery && shippingCost > 0)
        {
            lineItems.Add(
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "cad",
                        UnitAmountDecimal = Math.Round(shippingCost, 2) * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Frais de livraison",
                            Images =
                            [
                                "https://www.svgrepo.com/show/314607/shipping-fast-solid.svg",
                            ],
                            Description = $"Livraison à : {ShippingAddress}",
                        },
                    },
                    Quantity = 1,
                }
            );
        }

        foreach (var item in requests)
        {
            lineItems.Add(
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "cad",
                        UnitAmountDecimal = Math.Round(item.Price, 2) * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Metadata = new() { { nameof(item.CardId), item.CardId.ToString() } },
                            Name = item.CardName,
                            Images = [item.ImageURL],
                        },
                    },
                    Quantity = item.Quantity,
                }
            );
        }

        var options = new SessionCreateOptions
        {
            CustomerEmail = userEmail,
            BillingAddressCollection = "required",
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = baseUrl + "/PaymentSuccess",
            CancelUrl = baseUrl + "/Commander",
        };

        if (deliveryMode is DeliveryMode.InStorePickup)
        {
            options.CustomText = new SessionCustomTextOptions
            {
                AfterSubmit = new SessionCustomTextAfterSubmitOptions
                {
                    Message = "Récupérez votre commande en magasin (aucun frais d'expédition).",
                },
            };
        }

        var service = new SessionService();
        Session session = await service.CreateAsync(options);

        return session;
    }

    public async Task SendReceiptAsync(string email)
    {
        var pending = await _pendingPaymentService.GetLastByEmailAsync(email);

        if (pending is null)
            return;

        SessionService sessionService = new();
        Session session = await sessionService.GetAsync(pending.SessionId);

        SessionLineItemService sessionLineItemService = new();

        StripeList<LineItem> lineItems = await sessionLineItemService.ListAsync(session.Id);

        StringBuilder itemsHtml = new();

        var totalAmountInCent = session.AmountTotal;

        decimal total = totalAmountInCent.HasValue ? (decimal)totalAmountInCent / 100 : 0;
        decimal totalWithNoTax = 0;
        foreach (var item in lineItems)
        {
            // Montants Stripe sont en centimes
            decimal unitPrice = item.Price.UnitAmountDecimal.HasValue
                ? item.Price.UnitAmountDecimal.Value / 100
                : 0;

            int quantity = (int)(item.Quantity ?? 0);

            decimal subtotal = unitPrice * quantity;
            totalWithNoTax += subtotal;

            itemsHtml.Append(
                $@"
                    <tr style='border-bottom:1px solid #ddd;'>
                        <td style='padding:10px 0; font-size:10px; color:#333; word-break:break-word;'>{item.Description}</td>
                        <td style='text-align:center; font-size:15px; color:#555;'>{item.Quantity}</td>
                        <td style='text-align:right; font-size:15px; color:#555;'>{unitPrice:C}</td>
                        <td style='text-align:right; font-size:15px; color:#111; font-weight:bold;'>{subtotal:C}</td>
                    </tr>"
            );
        }

        decimal tax = total - totalWithNoTax;
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
                              <td style='padding:10px; text-align:right; font-size:13px; color:#333;'>{tax:C}</td>
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

        await _emailSender.SendEmailAsync(session!.CustomerEmail, "Votre reçu d'achat", html);

        await _pendingPaymentService.DeleteLastByEmailAsync(email);
    }
}
