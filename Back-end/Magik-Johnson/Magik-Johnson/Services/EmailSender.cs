namespace Magik_Johnson.Services;

using System.Threading.Tasks;
using Magik_Johnson.Configs;
using Magik_Johnson.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly EmailConfiguration _option;
    private readonly IConfiguration _config;

    public EmailSender(EmailConfiguration option, IConfiguration config)
    {
        _option = option;
        _config = config;
    }

    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink
    )
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(
            email,
            "Réinitialisation de mot de passe",
            CreatePasswordResetEmail(user, resetLink)
        );

    public async Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode
    )
    {
        var baseUrl = _config["FrontUrl"];

        var resetLink = $"{baseUrl}/resetCodeLoading?email={user.Email}&resetCode={resetCode}";

        var htmlMessage = CreatePasswordResetEmail(user, resetLink);

        await SendEmailAsync(email, "Réinitialisation de mot de passe", htmlMessage);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        await Execute(toEmail, subject, message);
    }

    public async Task Execute(string to, string subject, string htmlMessage)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_option.From));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = htmlMessage };

        try
        {
            using var smtp = new SmtpClient();

            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // Connexion au serveur
            await smtp.ConnectAsync(_option.SmtpServer, _option.Port, SecureSocketOptions.StartTls);

            // Authentification
            await smtp.AuthenticateAsync(_option.UserName, _option.Password);

            // Envoi du message
            await smtp.SendAsync(email);

            // Déconnexion après l'envoi
            await smtp.DisconnectAsync(true);
        }
        catch (SslHandshakeException sslEx)
        {
            throw new Exception(
                "Erreur SSL lors de la connexion au serveur SMTP : " + sslEx.Message,
                sslEx
            );
        }
        catch (SmtpCommandException smtpEx)
        {
            throw new Exception(
                $"Erreur SMTP : {smtpEx.Message} (code : {smtpEx.StatusCode})",
                smtpEx
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Échec de l'envoi de l'e-mail : " + ex.Message, ex);
        }
    }

    private string CreatePasswordResetEmail(ApplicationUser user, string resetLink)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='fr'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Réinitialisation de mot de passe</title>
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
            <div class='container'>
                <div class='header'>
                    <h1>Réinitialisation de mot de passe</h1>
                </div>
                <div class='content'>
                    <p>Bonjour {user.FirstName},</p>
                    <p>Nous avons reçu une demande de réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour en choisir un nouveau.</p>
                    <div style='text-align: center;'>
                        <a href='{resetLink}' class='button' style='color: #ffffff !important;'>Réinitialiser mon mot de passe</a>
                    </div>
                    <p style='color: #6c757d; font-size: 14px;'>Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer cet e-mail.</p>
                </div>
                <div class='footer'>
                    <p>&copy; {DateTime.UtcNow.Year} L'Arène des Stratèges. Tous droits réservés.</p>
                </div>
            </div>
        </body>
        </html>";
    }
}
