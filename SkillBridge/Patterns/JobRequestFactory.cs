using System;
using Microsoft.Data.SqlClient;
using SkillBridge.Models;
using SkillBridge.Models.JobRequests;

namespace SkillBridge.Patterns
{
    public static class JobRequestFactory
    {
        public static JobRequest Create(string status)
        {
            return status switch
            {
                "Pending"    => new PendingRequest(),
                "Accepted"   => new AcceptedRequest(),
                "InProgress" => new InProgressRequest(),
                "Completed"  => new CompletedRequest(),
                "Rejected"   => new RejectedRequest(),
                _ => throw new ArgumentException($"Unknown status: {status}")
            };
        }

        public static JobRequest FromReader(SqlDataReader reader)
        {
            string status = reader.GetString(reader.GetOrdinal("Status"));
            var req = Create(status);

            req.RequestID = reader.GetInt32(reader.GetOrdinal("RequestID"));
            req.CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID"));
            req.LabourerID = reader.GetInt32(reader.GetOrdinal("LabourerID"));
            req.JobDescription = reader.GetString(reader.GetOrdinal("JobDescription"));
            req.PreferredDate = reader.GetDateTime(reader.GetOrdinal("PreferredDate"));
            req.PreferredTime = reader.GetTimeSpan(reader.GetOrdinal("PreferredTime"));
            req.Location = reader.GetString(reader.GetOrdinal("Location"));
            req.Latitude = reader.IsDBNull(reader.GetOrdinal("Latitude")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Latitude"));
            req.Longitude = reader.IsDBNull(reader.GetOrdinal("Longitude")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Longitude"));
            req.RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason")) ? null : reader.GetString(reader.GetOrdinal("RejectionReason"));
            req.EstimatedHours = reader.GetDecimal(reader.GetOrdinal("EstimatedHours"));
            req.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
            req.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"));

            // Optional mappings (if query contains joins)
            if (HasColumn(reader, "CustomerName"))
            {
                req.CustomerName = reader.GetString(reader.GetOrdinal("CustomerName"));
            }
            if (HasColumn(reader, "LabourerName"))
            {
                req.LabourerName = reader.GetString(reader.GetOrdinal("LabourerName"));
            }
            if (HasColumn(reader, "CategoryName"))
            {
                req.CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
            }
            if (HasColumn(reader, "CustomerUserId"))
            {
                req.CustomerUserId = reader.GetInt32(reader.GetOrdinal("CustomerUserId"));
            }
            if (HasColumn(reader, "LabourerUserId"))
            {
                req.LabourerUserId = reader.GetInt32(reader.GetOrdinal("LabourerUserId"));
            }

            return req;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
