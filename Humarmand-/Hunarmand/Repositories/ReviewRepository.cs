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
    public class ReviewRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(DatabaseConnection db, ILogger<ReviewRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Create(int requestId, int reviewerUserId, int targetLabourerId, int rating, string comment)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_CreateReview", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestID", requestId);
                cmd.Parameters.AddWithValue("@ReviewerUserID", reviewerUserId);
                cmd.Parameters.AddWithValue("@TargetLabourerID", targetLabourerId);
                cmd.Parameters.AddWithValue("@Rating", rating);
                cmd.Parameters.AddWithValue("@Comment", (object)comment ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_CreateReview failed for request ID {RID}", requestId);
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetForLabourer(int labourerId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetReviewsForLabourer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);

                var list = new List<Review>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Review
                    {
                        ReviewID = reader.GetInt32(reader.GetOrdinal("ReviewID")),
                        RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                        StarRating = reader.GetInt32(reader.GetOrdinal("StarRating")),
                        ReviewText = reader.IsDBNull(reader.GetOrdinal("ReviewText")) ? null : reader.GetString(reader.GetOrdinal("ReviewText")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        ReviewerName = reader.GetString(reader.GetOrdinal("ReviewerName"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetReviewsForLabourer failed for laborer ID {ID}", labourerId);
                throw;
            }
        }

        public async Task<bool> HasReviewed(int requestId, int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_HasReviewed", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestID", requestId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToBoolean(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_HasReviewed failed for request ID {RID} and user ID {UID}", requestId, userId);
                throw;
            }
        }
    }
}
