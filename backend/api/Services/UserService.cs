using api.DTOs.User;
using api.Exceptions;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Utils;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _env;

        public UserService(IUserRepository userRepository, IWebHostEnvironment webEnvironment)
        {
            _env = webEnvironment;
            _userRepository = userRepository;
        }

        public async Task DeleteUserAsync(Guid id, string password)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new NotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new UnauthorizedException("Password is incorrect");
            }

            if (!string.IsNullOrEmpty(user.Avatar))
            {
                await ImageUtils.DeleteImageAsync(_env.WebRootPath + user.Avatar);
            }

            await _userRepository.DeleteUserAsync(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                throw new NotFoundException("Not found any users");
            }

            var userDTOs = users.Select(u => u.ToDTO()).ToList();
            return userDTOs;
        }

        public async Task<UserDTO> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new NotFoundException("User not found");
            return user.ToDTO();
        }

        public async Task LockUserAsync(Guid id, LockUserDTO lockUserDTO, string lockedBy)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new NotFoundException("User not found");

            user.IsLocked = true;
            user.LockReason = lockUserDTO.Reason?.Trim() ?? user.LockReason;
            user.LockedBy = lockedBy;
            user.LockedAt = DateTime.Now;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task UnlockUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new NotFoundException("User not found");
            user.IsLocked = false;
            user.LockReason = null;
            user.LockedBy = null;
            user.LockedAt = null;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new NotFoundException("User not found");

            // Xác thực tính duy nhất của email
            if (!string.IsNullOrEmpty(userDTO.Email) && userDTO.Email != user.Email)
            {
                var emailExists = await _userRepository.UserExistsByEmailAsync(userDTO.Email);
                if (emailExists)
                {
                    throw new AlreadyExistsException("Email already exists");
                }
            }

            // Xác thực tính duy nhất của số điện thoại
            if (!string.IsNullOrEmpty(userDTO.PhoneNumber) && userDTO.PhoneNumber != user.PhoneNumber)
            {
                var phoneExists = await _userRepository.UserExistsByPhoneNumberAsync(userDTO.PhoneNumber);
                if (phoneExists)
                {
                    throw new AlreadyExistsException("Phone number already exists");
                }
            }

            // Cập nhật thông tin người dùng
            user.Name = userDTO.Name?.Trim() ?? user.Name;
            if (!string.IsNullOrEmpty(userDTO.Email))
            {
                user.Email = userDTO.Email.Trim().ToLower();
                user.EmailConfirmed = false;
            }
            if (!string.IsNullOrEmpty(userDTO.PhoneNumber))
            {
                user.PhoneNumber = userDTO.PhoneNumber.Trim();
                user.PhoneNumberConfirmed = false;
            }
            user.Address = userDTO.Address?.Trim() ?? user.Address;
            user.Gender = userDTO.Gender?.Trim() ?? user.Gender;
            user.DateOfBirth = userDTO.DateOfBirth ?? user.DateOfBirth;

            if (userDTO.Avatar != null)
            {
                // Xóa ảnh đại diện cũ nếu tồn tại
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    await ImageUtils.DeleteImageAsync(_env.WebRootPath + user.Avatar);
                }

                // Lưu ảnh đại diện mới
                var filePath = await ImageUtils.SaveImageAsync(userDTO.Avatar, _env.WebRootPath, "users", 15 * 1024 * 1024);
                user.Avatar = filePath;
            }

            user.UpdatedAt = DateTime.Now;

            var res = await _userRepository.UpdateUserAsync(user);
            return res.ToDTO();
        }
    }
}