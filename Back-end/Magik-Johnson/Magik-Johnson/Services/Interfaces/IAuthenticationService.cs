using DTOs.Requests;
using Magik_Johnson.DTOs.Requests;
using Magik_Johnson.Models;

namespace Magik_Johnson.Services.Interfaces;

public interface IAuthenticationService
{
    Task<bool> RegisterAsync(ClientRegistrationRequest request);
    Task<bool> CheckResetCodeAsync(ApplicationUser user, ClientResetCodeCheckRequest request);
    Task SendConfirmationEmail(ApplicationUser user);
}
