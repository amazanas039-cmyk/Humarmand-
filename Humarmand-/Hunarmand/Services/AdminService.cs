using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserRepository _userRepo;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserRepository userRepo, ILogger<AdminService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(string? search)
        {
            return await _userRepo.GetAllForAdmin(search);
        }

        public async Task SuspendUserAsync(int userId, bool suspend)
        {
            await _userRepo.SuspendUser(userId, suspend);
            _logger.LogInformation("User {UID} {Action}", userId, suspend ? "suspended" : "unsuspended");
        }
    }
}
