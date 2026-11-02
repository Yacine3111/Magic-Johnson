using DTOs.Requests;
using FluentValidation;
using Magik_Johnson.Configs;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.DTOs.Requests.Validations;
using Magik_Johnson.Models;
using Magik_Johnson.Services;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Magik_Johnson.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        //Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDbDataInsertion, DbDataInsertion>();
        services.AddScoped<IResetClickCountJob, ResetClickCountJob>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();

        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPendingPaymentService, PendingPaymentService>();
        services.AddScoped<IRefundRequestService, RefundRequestService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();

        //Validators
        services.AddScoped<
            IValidator<ClientRegistrationRequest>,
            ClientRegistrationRequestValidator
        >();
        services.AddScoped<
            IValidator<ClientResetCodeCheckRequest>,
            ClientResetCodeCheckRequestValidator
        >();
        services.AddScoped<
            IValidator<ClientForgotPasswordRequest>,
            ClientForgotPasswordRequestValidator
        >();
        services.AddScoped<
            IValidator<ClientResetPasswordRequest>,
            ClientResetPasswordRequestValidator
        >();
        services.AddScoped<
            IValidator<EmployeeRegistrationRequest>,
            EmployeeRegistrationRequestValidator
        >();
        services.AddScoped<IValidator<CreateCheckoutRequest>, CreateCheckoutRequestValidator>();

        services.AddScoped<IValidator<EmployeeUpdateRequest>, EmployeeUpdateRequestValidator>();

        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
        services.AddScoped<IValidator<UpdatePasswordRequest>, UpdatePasswordRequestValidator>();
        services.AddScoped<
            IValidator<AddressRegistrationRequest>,
            AddressRegistrationRequestValidator
        >();

        services.AddScoped<IValidator<AddToCartRequest>, AddToCartRequestValidator>();

        services.AddScoped<
            IValidator<DeleteCardFromCartRequest>,
            DeleteCardFromCartRequestValidator
        >();

        services.AddScoped<IValidator<DiscountRequestBase>, DiscountRequestValidator>();

        services.AddScoped<
            IValidator<IEnumerable<PaymentRequest>>,
            CreatePaymentRequestValidator
        >();

        //Code de rénitialisation  bon  pour 30 minutes
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromMinutes(30)
        );

        var emailConfig = config.GetRequiredSection("EmailConfiguration").Get<EmailConfiguration>();

        services.AddSingleton(emailConfig!);

        services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();

        //Ajouter pour pouvoir bénéficier de la fonction SendEmailAsync
        services.AddSingleton<EmailSender>();

        return services;
    }
}
