using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Hunarmand.Data;
using Hunarmand.Models;

namespace Hunarmand.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(DatabaseConnection db, ILogger<UserRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        private User MapUser(SqlDataReader reader)
        {
            string role = reader.GetString(reader.GetOrdinal("Role"));
            User user = role switch
            {
                "Customer" => new Customer(),
                "Labourer" => new Labourer(),
                "Admin" => new Admin(),
                _ => throw new InvalidOperationException($"Unknown role: {role}")
            };

            user.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            user.FullName = reader.GetString(reader.GetOrdinal("FullName"));
            user.Email = reader.GetString(reader.GetOrdinal("Email"));
            user.PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            user.Role = role;
            user.Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"));
            user.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
            user.IsSuspended = reader.GetBoolean(reader.GetOrdinal("IsSuspended"));
            user.DisputeStrikes = reader.GetInt32(reader.GetOrdinal("DisputeStrikes"));
            user.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));

            // If customer/labor details are included in future joins, map them here.
            return user;
        }

        public async Task<User?> GetByEmail(string email)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetUserByEmail", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUser(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetUserByEmail failed for email: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetById(int id)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetUserById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapUser(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetUserById failed for id: {Id}", id);
                throw;
            }
        }

        public async Task<int> CreateCustomer(Customer c, string passwordHash)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RegisterCustomer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@FullName", c.FullName);
                cmd.Parameters.AddWithValue("@Email", c.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@Phone", (object)c.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)c.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", (object)c.Latitude ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", (object)c.Longitude ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_RegisterCustomer failed for email: {Email}", c.Email);
                throw;
            }
        }

        public async Task<int> CreateLabourer(Labourer l, string passwordHash)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RegisterLabourer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@FullName", l.FullName);
                cmd.Parameters.AddWithValue("@Email", l.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@Phone", (object)l.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CategoryID", l.CategoryID);
                cmd.Parameters.AddWithValue("@ExperienceYears", l.ExperienceYears);
                cmd.Parameters.AddWithValue("@HourlyRate", l.HourlyRate);
                cmd.Parameters.AddWithValue("@Bio", (object)l.Bio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AvailabilitySchedule", (object)l.AvailabilitySchedule ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)l.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", (object)l.Latitude ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", (object)l.Longitude ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_RegisterLabourer failed for email: {Email}", l.Email);
                throw;
            }
        }

        public async Task<bool> UpdateProfile(int userId, string fullName, string phone, string address, decimal? lat, decimal? lng)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateUserProfile", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", (object)lat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", (object)lng ?? DBNull.Value);

                var affected = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(affected) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_UpdateUserProfile failed for userId: {UserId}", userId);
                throw;
            }
        }

        public async Task SuspendUser(int userId, bool suspend)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_SuspendUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@IsSuspended", suspend);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_SuspendUser failed for userId: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllForAdmin(string? search)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetAllUsersForAdmin", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Search", (object)search ?? DBNull.Value);

                var users = new List<User>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(MapUser(reader));
                }
                return users;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetAllUsersForAdmin failed with search term: {Search}", search);
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var user = await GetByEmail(email);
            return user != null;
        }

        public async Task<int?> GetCustomerIdByUserId(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT CustomerID FROM Customers WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to get CustomerID for UserID: {UserID}", userId);
                throw;
            }
        }

        public async Task<int?> GetLabourerIdByUserId(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT LabourerID FROM Labourers WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to get LabourerID for UserID: {UserID}", userId);
                throw;
            }
        }

        public async Task<int?> GetAdminIdByUserId(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT AdminID FROM Admins WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to get AdminID for UserID: {UserID}", userId);
                throw;
            }
        }
    }
}
