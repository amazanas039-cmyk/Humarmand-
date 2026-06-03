using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillBridge.Models;
using SkillBridge.Repositories;

namespace SkillBridge.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(UserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string email, string password, string expectedRole)
        {
            var user = await _userRepo.GetByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for {Email}", email);
                return null;
            }

            if (user.Role != expectedRole)
            {
                _logger.LogWarning("Login failed: role mismatch for {Email}. Expected={Expected}, Actual={Actual}", email, expectedRole, user.Role);
                return null;
            }

            if (user.IsSuspended)
            {
                _logger.LogWarning("Login failed: account suspended for {Email}", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: invalid password for {Email}", email);
                return null;
            }

            _logger.LogInformation("User {Email} logged in successfully as {Role}", email, user.Role);
            return user;
        }

        public async Task<bool> RegisterCustomerAsync(Customer c, string password)
        {
            if (await _userRepo.EmailExistsAsync(c.Email))
            {
                _logger.LogWarning("Registration failed: email already exists {Email}", c.Email);
                return false;
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            var userId = await _userRepo.CreateCustomer(c, hash);
            _logger.LogInformation("Customer registered with UserID {UID}", userId);
            return true;
        }

        public async Task<bool> RegisterLabourerAsync(Labourer l, string password)
        {
            if (await _userRepo.EmailExistsAsync(l.Email))
            {
                _logger.LogWarning("Registration failed: email already exists {Email}", l.Email);
                return false;
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            var userId = await _userRepo.CreateLabourer(l, hash);
            _logger.LogInformation("Labourer registered with UserID {UID}", userId);
            return true;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _userRepo.GetById(userId);
        }

        public async Task<int?> GetCustomerIdByUserIdAsync(int userId)
        {
            return await _userRepo.GetCustomerIdByUserId(userId);
        }

        public async Task<int?> GetLabourerIdByUserIdAsync(int userId)
        {
            return await _userRepo.GetLabourerIdByUserId(userId);
        }

        public async Task<int?> GetAdminIdByUserIdAsync(int userId)
        {
            return await _userRepo.GetAdminIdByUserId(userId);
        }
    }
}
