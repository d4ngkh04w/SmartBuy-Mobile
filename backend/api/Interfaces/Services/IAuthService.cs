using api.DTOs.Auth;

namespace api.Interfaces.Services
{
    public interface IAuthService
    {
        public Task<TokenResponseDTO> Register(RegisterDTO registerDto, string role);
        public Task<TokenResponseDTO> Login(LoginDTO loginDto, string role);
        public Task<TokenResponseDTO> LoginWithGoogleAsync(GoogleLoginDTO dto, string role);
        public Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto);
        public Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);
        public Task ChangePasswordAsync(ChangePasswordDTO changePasswordDto, Guid userId);
        public Task SendEmailVerificationAsync();
        public Task VerifyEmailAsync(VerifyEmailDTO verifyEmailDto);
    }
}