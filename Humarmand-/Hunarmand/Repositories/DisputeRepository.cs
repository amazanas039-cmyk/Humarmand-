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
    public class DisputeRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<DisputeRepository> _logger;

        public DisputeRepository(DatabaseConnection db, ILogger<DisputeRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        private Dispute MapDispute(SqlDataReader reader)
        {
            var d = new Dispute
            {
                DisputeID = reader.GetInt32(reader.GetOrdinal("DisputeID")),
                RequestID = reader.GetInt32(reader.GetOrdinal("RequestID")),
                RaisedByUserID = reader.GetInt32(reader.GetOrdinal("RaisedByUserID")),
                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Resolution = reader.IsDBNull(reader.GetOrdinal("Resolution")) ? null : reader.GetString(reader.GetOrdinal("Resolution")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                ResolvedAt = reader.IsDBNull(reader.GetOrdinal("ResolvedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ResolvedAt"))
            };

            // Optional Joins
            if (HasColumn(reader, "RaisedByName"))
            {
                d.RaisedByName = reader.GetString(reader.GetOrdinal("RaisedByName"));
            }
            if (HasColumn(reader, "CustomerUserID"))
            {
                d.CustomerUserID = reader.GetInt32(reader.GetOrdinal("CustomerUserID"));
            }
            if (HasColumn(reader, "CustomerName"))
            {
                d.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
            }
            if (HasColumn(reader, "LabourerUserID"))
            {
                d.LabourerUserID = reader.GetInt32(reader.GetOrdinal("LabourerUserID"));
            }
            if (HasColumn(reader, "LabourerName"))
            {
                d.LabourerName = reader.GetString(reader.GetOrdinal("LabourerName"));
            }
            if (HasColumn(reader, "JobDescription"))
            {
                d.JobDescription = reader.GetString(reader.GetOrdinal("JobDescription"));
            }
            if (HasColumn(reader, "RaisedByRole"))
            {
                d.RaisedByRole = reader.GetString(reader.GetOrdinal("RaisedByRole"));
            }

            return d;
        }

        private bool HasColumn(SqlDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<int> Raise(int requestId, int raisedByUserId, string reason)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_RaiseDispute", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestID", requestId);
                cmd.Parameters.AddWithValue("@RaisedByUserID", raisedByUserId);
                cmd.Parameters.AddWithValue("@Reason", reason);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_RaiseDispute failed for request ID {RID}", requestId);
                throw;
            }
        }

        public async Task<Dispute?> GetById(int disputeId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetDisputeById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@DisputeID", disputeId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapDispute(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetDisputeById failed for ID {ID}", disputeId);
                throw;
            }
        }

        public async Task<IEnumerable<Dispute>> GetOpen()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetOpenDisputes", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var list = new List<Dispute>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapDispute(reader));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetOpenDisputes failed.");
                throw;
            }
        }

        public async Task<IEnumerable<Dispute>> GetForUser(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetDisputesForUser", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);

                var list = new List<Dispute>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapDispute(reader));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetDisputesForUser failed for user ID {UID}", userId);
                throw;
            }
        }

        public async Task Resolve(int disputeId, string resolutionType, string resolutionText)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ResolveDispute", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@DisputeID", disputeId);
                cmd.Parameters.AddWithValue("@ResolutionType", resolutionType);
                cmd.Parameters.AddWithValue("@ResolutionText", resolutionText);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_ResolveDispute failed for ID {ID}", disputeId);
                throw;
            }
        }
    }
}
