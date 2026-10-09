using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Hunarmand.Data;
using Hunarmand.Models;
using Hunarmand.Patterns;

namespace Hunarmand.Repositories
{
    public class JobRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(DatabaseConnection db, ILogger<JobRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> Create(JobRequest req)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_CreateJobRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CustomerID", req.CustomerID);
                cmd.Parameters.AddWithValue("@LabourerID", req.LabourerID);
                cmd.Parameters.AddWithValue("@JobDescription", req.JobDescription);
                cmd.Parameters.AddWithValue("@PreferredDate", req.PreferredDate);
                cmd.Parameters.AddWithValue("@PreferredTime", req.PreferredTime);
                cmd.Parameters.AddWithValue("@Location", req.Location);
                cmd.Parameters.AddWithValue("@Latitude", (object)req.Latitude ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", (object)req.Longitude ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EstimatedHours", req.EstimatedHours);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_CreateJobRequest failed for customer ID {CID}", req.CustomerID);
                throw;
            }
        }

        public async Task<JobRequest?> GetById(int requestId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetJobRequestById", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestID", requestId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return JobRequestFactory.FromReader(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetJobRequestById failed for request ID {ID}", requestId);
                throw;
            }
        }

        public async Task UpdateStatus(int requestId, string status, string? reason)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_UpdateJobStatus", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RequestID", requestId);
                cmd.Parameters.AddWithValue("@NewStatus", status);
                cmd.Parameters.AddWithValue("@RejectionReason", (object)reason ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_UpdateJobStatus failed for request ID {ID}", requestId);
                throw;
            }
        }

        public async Task<IEnumerable<JobRequest>> GetAll()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetAllJobRequests", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var list = new List<JobRequest>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(JobRequestFactory.FromReader(reader));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetAllJobRequests failed");
                throw;
            }
        }

        public async Task<IEnumerable<JobRequest>> GetForCustomer(int customerId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetJobsForCustomer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@CustomerID", customerId);

                var list = new List<JobRequest>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(JobRequestFactory.FromReader(reader));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetJobsForCustomer failed for customer ID {ID}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<JobRequest>> GetForLabourer(int labourerId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetJobsForLabourer", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);

                var list = new List<JobRequest>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(JobRequestFactory.FromReader(reader));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetJobsForLabourer failed for laborer ID {ID}", labourerId);
                throw;
            }
        }

        public async Task<bool> CheckAvailability(int labourerId, DateTime date, TimeSpan time)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_CheckLabourerAvailability", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Time", time);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToBoolean(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_CheckLabourerAvailability failed for laborer ID {ID}", labourerId);
                throw;
            }
        }
    }
}
