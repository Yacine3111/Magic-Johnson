using System.Text;
using DTOs.Requests;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Magik_Johnson.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailSender _emailSender;
    private readonly IConfiguration _config;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        EmailSender emailSender,
        IConfiguration config
    )
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<bool> RegisterAsync(ClientRegistrationRequest request)
    {
        ApplicationUser user = new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserName = request.Email,
            EmailConfirmed = true,
        };

        var response = await _userManager.CreateAsync(user, request.Password);

        return response.Succeeded;
    }

    public async Task<bool> CheckResetCodeAsync(
        ApplicationUser user,
        ClientResetCodeCheckRequest request
    )
    {
        byte[] bytes = WebEncoders.Base64UrlDecode(request.ResetCode);
        string resetCode = Encoding.UTF8.GetString(bytes);

        return await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
            resetCode
        );
    }

    public async Task SendConfirmationEmail(ApplicationUser user)
    {
        var siteUrl = _config["FrontUrl"];

        string html =
            $@"
        <!DOCTYPE html>
        <html lang=""fr"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Bienvenue chez L'Arène des Stratèges</title>

            <style>
                body {{
                    font-family: Arial, Helvetica, sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: #f0eada;
                    line-height: 1.6;
                }}

                .container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: #ffffff;
                    border-radius: 8px;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                    overflow: hidden;
                }}

                .header {{
                    background-color: #a89c8d;
                    color: white;
                    padding: 20px;
                    text-align: center;
                }}

                .content {{
                    padding: 30px;
                    background-color: #ffffff;
                }}

                .button {{
                    display: inline-block;
                    background-color: #2d3c49 !important;
                    color: #ffffff !important;
                    padding: 12px 24px;
                    border-radius: 8px;
                    text-decoration: none;
                    font-weight: 500;
                    margin: 20px 0;
                    border: none;
                    transition: background-color 0.3s ease, color 0.3s ease;
                    font-size: 0.95em;
                }}

                .button:hover {{
                    background-color: #ba9461 !important;
                    color: #ffffff !important;
                }}

                .footer {{
                    background-color: #ddd7c7;
                    padding: 20px;
                    text-align: center;
                    font-size: 12px;
                    color: #1f2937;
                }}

                @media only screen and (max-width: 600px) {{
                    .container {{
                        width: 100% !important;
                        margin: 0 !important;
                        border-radius: 0 !important;
                    }}

                    .content {{
                        padding: 20px !important;
                    }}

                    .button {{
                        display: block !important;
                        width: 90% !important;
                        margin: 20px auto !important;
                        text-align: center !important;
                        padding: 15px 0 !important;
                        font-size: 16px !important;
                    }}

                    .header h1 {{
                        font-size: 22px !important;
                    }}
                }}
            </style>
        </head>
        <body>

            <div class=""container"">
                <div class=""header"">
                    <h1>Bienvenue chez L'Arène des Stratèges !</h1>
                </div>

                <div class=""content"">
                    <p>Bonjour {user.FirstName},</p>
                    <p>Nous sommes ravis de vous accueillir parmi nos clients ! Votre compte a été créé avec succès.</p>

                    <p>Vous pouvez dès maintenant :</p>

                    <ul style=""color: #1f2937; line-height: 1.8;"">
                        <li>Parcourir notre catalogue de cartes</li>
                        <li>Ajouter des articles à votre panier</li>
                        <li>Passer des commandes en ligne</li>
                        <li>Suivre l'historique de vos commandes</li>
                    </ul>

                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{siteUrl}"" class=""button"" style=""color: #ffffff !important;"">Accéder au site</a>
                    </div>

                    <p style=""color: #6c757d; font-size: 14px;"">
                        Si vous avez des questions, n'hésitez pas à nous contacter. Nous sommes là pour vous aider !
                    </p>
                </div>

                <div class=""footer"">
                    <p>&copy; {DateTime.UtcNow.Year.ToString()} L'Arène des Stratèges. Tous droits réservés.</p>
                </div>
            </div>

        </body>
        </html>
        ";

        if (user.Email != null)
        {
            await _emailSender.SendEmailAsync(
                user.Email,
                "Bienvenue dans l'Arène des Stratèges",
                html
            );
        }
    }
}
