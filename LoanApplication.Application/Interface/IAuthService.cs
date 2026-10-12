using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IAuthService
    {
       
        Task<bool> RegisterUserAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthDto dto);
        Task<AuthResponseDto> VerifyTwoFactorAsync(Verify2FaDto dto);//verify 2fa
        Task<Setup2FaDto> EnableTwoFactorAsync(int userId);//enable 2fa

        Task<bool> CompleteProfileAsync(int userId,CompleteProfileDto dto);
    }
}
