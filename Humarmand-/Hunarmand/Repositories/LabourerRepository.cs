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
    public class LabourerRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<LabourerRepository> _logger;

        public UserRepository UserRepo { get; }

        public LabourerRepository(DatabaseConnection db, ILogger<LabourerRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        private Labourer MapLabourer(SqlDataReader reader)
        {
            var l = new Labourer
            {
                LabourerID = reader.GetInt32(reader.GetOrdinal("LabourerID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                ExperienceYears = reader.GetInt32(reader.GetOrdinal("ExperienceYears")),
                HourlyRate = reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString(reader.GetOrdinal("Bio")),
                AvailabilitySchedule = reader.IsDBNull(reader.GetOrdinal("AvailabilitySchedule")) ? null : reader.GetString(reader.GetOrdinal("AvailabilitySchedule")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Latitude = reader.IsDBNull(reader.GetOrdinal("Latitude")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Latitude")),
                Longitude = reader.IsDBNull(reader.GetOrdinal("Longitude")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Longitude")),
                VerificationStatus = reader.GetString(reader.GetOrdinal("VerificationStatus")),
                IsProfileLive = reader.GetBoolean(reader.GetOrdinal("IsProfileLive"))
            };
            
            l.RecalculateReputation(reader.GetDecimal(reader.GetOrdinal("ReputationScore")));
            l.CompletionRate = reader.GetDecimal(reader.GetOrdinal("CompletionRate"));
            
            return l;
        }

        public async Task<IEnumerable<Labourer>> GetPendingVerification()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetPendingLabourers", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var list = new List<Hunarmand.Models.Labourer>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Labourer
                    {
                        LabourerID = reader.GetInt32(reader.GetOrdinal("LabourerID")),
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        ExperienceYears = reader.GetInt32(reader.GetOrdinal("ExperienceYears")),
                        HourlyRate = reader.GetDecimal(reader.GetOrdinal("HourlyRate")),
                        Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString(reader.GetOrdinal("Bio")),
                        VerificationStatus = reader.GetString(reader.GetOrdinal("VerificationStatus")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetPendingLabourers failed.");
                throw;
            }
        }

        public async Task Approve(int labourerId, string note)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ApproveLabourer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@Note", note);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_ApproveLabourer failed for ID: {ID}", labourerId);
                throw;
            }
        }

        public async Task Reject(int labourerId, string note)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RejectLabourer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@Note", note);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_RejectLabourer failed for ID: {ID}", labourerId);
                throw;
            }
        }

        public async Task<IEnumerable<Labourer>> SearchNearby(double lat, double lng, double radiusKm, int? categoryId, decimal? minRating, int? minExp)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_SearchLabourersNearby", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CustomerLat", (decimal)lat);
                cmd.Parameters.AddWithValue("@CustomerLng", (decimal)lng);
                cmd.Parameters.AddWithValue("@RadiusKm", (decimal)radiusKm);
                cmd.Parameters.AddWithValue("@CategoryID", (object)categoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinRating", (object)minRating ?? 0.0m);
                cmd.Parameters.AddWithValue("@MinExperience", (object)minExp ?? 0);

                var list = new List<Hunarmand.Models.Labourer>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var l = MapLabourer(reader);
                    l.DistanceKm = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("DistanceKm")));
                    list.Add(l);
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_SearchLabourersNearby failed.");
                throw;
            }
        }

        public async Task<(Labourer Profile, List<Review> Reviews)?> GetProfile(int labourerId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetLabourerProfile", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                var profile = MapLabourer(reader);
                var reviews = new List<Review>();

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reviews.Add(new Review
                        {
                            ReviewID = reader.GetInt32(reader.GetOrdinal("ReviewID")),
                            StarRating = reader.GetInt32(reader.GetOrdinal("StarRating")),
                            ReviewText = reader.IsDBNull(reader.GetOrdinal("ReviewText")) ? null : reader.GetString(reader.GetOrdinal("ReviewText")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            ReviewerName = reader.GetString(reader.GetOrdinal("ReviewerName"))
                        });
                    }
                }

                return (profile, reviews);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetLabourerProfile failed for ID: {ID}", labourerId);
                throw;
            }
        }

        public async Task UpdateAvailability(int labourerId, string scheduleJson)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateLabourerAvailability", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@AvailabilitySchedule", scheduleJson);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_UpdateLabourerAvailability failed for ID: {ID}", labourerId);
                throw;
            }
        }

        public async Task UpdateProfile(Labourer l)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateLabourerProfile", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", l.LabourerID);
                cmd.Parameters.AddWithValue("@Bio", (object)l.Bio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HourlyRate", l.HourlyRate);
                cmd.Parameters.AddWithValue("@Address", (object)l.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", (object)l.Latitude ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", (object)l.Longitude ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_UpdateLabourerProfile failed for ID: {ID}", l.LabourerID);
                throw;
            }
        }

        public async Task<IEnumerable<SkillCategory>> GetCategories()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT CategoryID, Name, Description, IsActive, IconClass FROM SkillCategories", conn);
                var list = new List<SkillCategory>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new SkillCategory
                    {
                        CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        IconClass = reader.IsDBNull(reader.GetOrdinal("IconClass")) ? "" : reader.GetString(reader.GetOrdinal("IconClass"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to select categories.");
                throw;
            }
        }
    }
}
