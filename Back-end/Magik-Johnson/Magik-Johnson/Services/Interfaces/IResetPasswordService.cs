using DTOs.Requests;
using Magik_Johnson.DTOs.Requests;

namespace Magik_Johnson.Services.Interfaces;

public interface IResetPasswordService
{
    Task SendResetCode(ClientForgotPasswordRequest request);
    Task<bool> ResetPassword(ClientResetPasswordRequest request);
}
