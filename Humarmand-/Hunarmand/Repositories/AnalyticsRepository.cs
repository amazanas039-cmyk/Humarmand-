using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Hunarmand.Data;
using Hunarmand.Models;
using Hunarmand.Models.DTOs;

namespace Hunarmand.Repositories
{
    public class AnalyticsRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<AnalyticsRepository> _logger;

        public AnalyticsRepository(DatabaseConnection db, ILogger<AnalyticsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PlatformKPIs> GetPlatformKPIs()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetPlatformKPIs", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var kpis = new PlatformKPIs
                    {
                        TotalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsers")),
                        ActiveLabourers = reader.GetInt32(reader.GetOrdinal("ActiveLabourers")),
                        JobsToday = reader.GetInt32(reader.GetOrdinal("JobsToday")),
                        NewUsersThisWeek = reader.GetInt32(reader.GetOrdinal("NewUsersThisWeek")),
                        AvgRating = Convert.ToDouble(reader.GetDouble(reader.GetOrdinal("AvgRating"))),
                        CompletionRate = Convert.ToDouble(reader.GetDouble(reader.GetOrdinal("CompletionRate"))),
                        PendingVerifications = reader.GetInt32(reader.GetOrdinal("PendingVerifications")),
                        OpenDisputes = reader.GetInt32(reader.GetOrdinal("OpenDisputes")),
                        ActiveJobs = reader.GetInt32(reader.GetOrdinal("ActiveJobs")),
                        TotalJobsCompleted = reader.GetInt32(reader.GetOrdinal("TotalJobsCompleted")),
                        TotalDisputes = reader.GetInt32(reader.GetOrdinal("TotalDisputes"))
                    };

                    // Compute WoW Deltas
                    int newUsersLastWeek = reader.GetInt32(reader.GetOrdinal("NewUsersLastWeek"));
                    int jobsThisWeek = reader.GetInt32(reader.GetOrdinal("JobsThisWeek"));
                    int jobsLastWeek = reader.GetInt32(reader.GetOrdinal("JobsLastWeek"));

                    // User growth WoW change %
                    if (newUsersLastWeek > 0)
                    {
                        kpis.UserGrowthDelta = Math.Round(((double)(kpis.NewUsersThisWeek - newUsersLastWeek) / newUsersLastWeek) * 100, 2);
                    }
                    else
                    {
                        kpis.UserGrowthDelta = kpis.NewUsersThisWeek > 0 ? 100 : 0;
                    }

                    // Jobs completed WoW change %
                    if (jobsLastWeek > 0)
                    {
                        kpis.JobsDelta = Math.Round(((double)(jobsThisWeek - jobsLastWeek) / jobsLastWeek) * 100, 2);
                    }
                    else
                    {
                        kpis.JobsDelta = jobsThisWeek > 0 ? 100 : 0;
                    }

                    return kpis;
                }
                return new PlatformKPIs();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetPlatformKPIs failed.");
                throw;
            }
        }

        public async Task<IEnumerable<DataPoint>> GetUserGrowth(int days)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetUserGrowthTrend", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Days", days);

                var list = new List<DataPoint>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DataPoint
                    {
                        Date = reader.GetString(reader.GetOrdinal("Date")),
                        Value = Convert.ToDouble(reader.GetInt32(reader.GetOrdinal("NewUsers")))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetUserGrowthTrend failed.");
                throw;
            }
        }

        public async Task<IEnumerable<DataPoint>> GetDailyDeals(int days)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetDailyCompletedJobs", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Days", days);

                var list = new List<DataPoint>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DataPoint
                    {
                        Date = reader.GetString(reader.GetOrdinal("Date")),
                        Value = Convert.ToDouble(reader.GetInt32(reader.GetOrdinal("CompletedJobs")))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetDailyCompletedJobs failed.");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryStat>> GetCategoryDistribution()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetCategoryDistribution", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var list = new List<CategoryStat>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new CategoryStat
                    {
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Count = reader.GetInt32(reader.GetOrdinal("LabourerCount"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetCategoryDistribution failed.");
                throw;
            }
        }

        public async Task<IEnumerable<Labourer>> GetTopRatedLabourers(int topN)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetTopRatedLabourers", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@TopN", topN);

                var list = new List<Hunarmand.Models.Labourer>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var l = new Labourer
                    {
                        LabourerID = reader.GetInt32(reader.GetOrdinal("LabourerID")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        CompletionRate = reader.GetDecimal(reader.GetOrdinal("CompletionRate"))
                    };
                    l.RecalculateReputation(reader.GetDecimal(reader.GetOrdinal("ReputationScore")));
                    list.Add(l);
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetTopRatedLabourers failed.");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryStat>> GetUnmetDemand()
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetUnmetDemand", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var list = new List<CategoryStat>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new CategoryStat
                    {
                        Name = reader.GetString(reader.GetOrdinal("CategoryName")),
                        Count = reader.GetInt32(reader.GetOrdinal("DemandGap"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetUnmetDemand failed.");
                throw;
            }
        }

        public async Task<IEnumerable<DataPoint>> GetLabourerEarnings(int labourerId, int days)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetLabourerEarningsTrend", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@Days", days);

                var list = new List<DataPoint>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DataPoint
                    {
                        Date = reader.GetString(reader.GetOrdinal("Date")),
                        Value = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("EstimatedEarnings")))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetLabourerEarningsTrend failed.");
                throw;
            }
        }

        public async Task<IEnumerable<DataPoint>> GetLabourerRatingTrend(int labourerId, int days)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetLabourerRatingTrend", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);
                cmd.Parameters.AddWithValue("@Days", days);

                var list = new List<DataPoint>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DataPoint
                    {
                        Date = reader.GetString(reader.GetOrdinal("Date")),
                        Value = Convert.ToDouble(reader.GetDouble(reader.GetOrdinal("AvgRating")))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetLabourerRatingTrend failed.");
                throw;
            }
        }

        public async Task<JobFunnel> GetLabourerJobFunnel(int labourerId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetLabourerJobFunnel", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LabourerID", labourerId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new JobFunnel
                    {
                        TotalRequests = reader.GetInt32(reader.GetOrdinal("TotalRequests")),
                        Accepted = reader.GetInt32(reader.GetOrdinal("Accepted")),
                        Completed = reader.GetInt32(reader.GetOrdinal("Completed")),
                        Rejected = reader.GetInt32(reader.GetOrdinal("Rejected"))
                    };
                }
                return new JobFunnel();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetLabourerJobFunnel failed.");
                throw;
            }
        }
    }
}
