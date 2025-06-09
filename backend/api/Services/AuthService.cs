using api.DTOs.Auth;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Models;
using Google.Apis.Auth;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<TokenResponseDTO> Register(RegisterDTO registerDto, string role)
        {
            if (await _userRepository.UserExistsByPhoneNumberAsync(registerDto.PhoneNumber))
                throw new AlreadyExistsException("Phone number already exists");

            if (await _userRepository.UserExistsByEmailAsync(registerDto.Email))
                throw new AlreadyExistsException("Email already exists");

            var user = new User
            {
                PhoneNumber = registerDto.PhoneNumber,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password,
                    BCrypt.Net.BCrypt.GenerateSalt(11, 'b')),
                Role = role,
                LastLogin = DateTime.Now,
                EmailConfirmed = false
            };

            await _userRepository.CreateUserAsync(user);

            var refreshToken = await _tokenService.GenerateRefreshToken(user);
            var token = _tokenService.CreateToken(user, role);

            return new TokenResponseDTO
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDTO> Login(LoginDTO loginDto, string role)
        {
            var user = await _userRepository.GetUserByPhoneNumberAsync(loginDto.PhoneNumber);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new UnauthorizedException("Invalid phone number or password");
            }

            // Kiểm tra trạng thái khóa tài khoản
            if (user.IsLocked)
            {
                string lockMessage = user.LockedBy == "user"
                    ? "Your account has been temporarily locked by yourself.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it"
                    : $"Your account has been restricted by administrator.\nReason: {user.LockReason}.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it";

                throw new UnauthorizedException(lockMessage);
            }

            if (user.Role != role)
            {
                throw new ForbiddenException($"Your account does not have access as a '{role}'");
            }

            // Update last login
            user.LastLogin = DateTime.Now;
            await _userRepository.UpdateUserAsync(user);

            // Tạo access token và refresh token
            var refreshToken = await _tokenService.GenerateRefreshToken(user);
            var token = _tokenService.CreateToken(user, role);

            return new TokenResponseDTO
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDTO> LoginWithGoogleAsync(GoogleLoginDTO dto, string role)
        {
            // Xác thực token Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { ConfigHelper.GoogleClientId }
            }) ?? throw new UnauthorizedException("Invalid Google token");

            // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
            var user = await _userRepository.GetUserByEmailAsync(payload.Email);
            if (user == null)
            {
                // Tạo người dùng mới nếu chưa tồn tại
                user = new User
                {
                    PhoneNumber = string.Empty,
                    Email = payload.Email,
                    Name = payload.Name ?? string.Empty,
                    Password = string.Empty,
                    Role = role,
                    EmailConfirmed = true
                };

                await _userRepository.CreateUserAsync(user);
            }
            else if (user.Role != role)
            {
                throw new ForbiddenException($"Your account does not have access as a '{role}'");
            }
            else if (user.IsLocked)
            {
                string lockMessage = user.LockedBy == "user"
                    ? "Your account has been temporarily locked by yourself.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it"
                    : $"Your account has been restricted by administrator.\nReason: {user.LockReason}.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it";

                throw new UnauthorizedException(lockMessage);
            }

            // Tạo token xác thực
            var token = _tokenService.CreateToken(user, role);
            var refreshToken = await _tokenService.GenerateRefreshToken(user);

            user.LastLogin = DateTime.Now;
            await _userRepository.UpdateUserAsync(user);

            return new TokenResponseDTO
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                Console.WriteLine($"[INF] User not found for email: {forgotPasswordDto.Email}");
                return;
            }

            // Tạo token đặt lại mật khẩu
            var resetToken = await _tokenService.GeneratePasswordResetToken(user);

            // Gửi email reset mật khẩu với token
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            // Xác thực token và lấy thông tin người dùng
            var user = await _tokenService.ValidatePasswordResetToken(resetPasswordDto.Email, resetPasswordDto.Token);
            if (user == null)
            {
                return;
            }

            // Cập nhật mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.Password);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task ChangePasswordAsync(ChangePasswordDTO changePasswordDto, Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId) ?? throw new NotFoundException("User not found");

            // Kiểm tra mật khẩu cũ
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.Password))
            {
                throw new UnauthorizedException("Old password is incorrect");
            }
            if (string.Equals(changePasswordDto.OldPassword, changePasswordDto.NewPassword))
            {
                throw new BadRequestException("New password must be different from old password");
            }

            // Cập nhật mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword,
                BCrypt.Net.BCrypt.GenerateSalt(11, 'b'));
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task SendEmailVerificationAsync()
        {
            var user = await _userRepository.GetUserByIdAsync(HttpContextHelper.CurrentUserId) ?? throw new NotFoundException("User not found");

            if (user.EmailConfirmed)
            {
                return;
            }

            // Tạo token xác thực email
            var verificationToken = await _tokenService.GenerateEmailVerificationToken(user);

            // Gửi email xác thực với token
            await _emailService.SendEmailVerificationAsync(user.Email, verificationToken);
        }

        public async Task VerifyEmailAsync(VerifyEmailDTO verifyEmailDto)
        {
            // Xác thực token và lấy thông tin người dùng
            var user = await _tokenService.ValidateEmailVerificationToken(verifyEmailDto.Email, verifyEmailDto.Token);
            if (user == null)
            {
                return;
            }
            // Xác nhận email
            user.EmailConfirmed = true;
            await _userRepository.UpdateUserAsync(user);
        }
    }
}