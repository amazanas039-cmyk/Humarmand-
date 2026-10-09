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
    public class NotificationRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(DatabaseConnection db, ILogger<NotificationRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> Create(int userId, string message, string type)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_CreateNotification", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@NotificationType", (object)type ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_CreateNotification failed for user ID {UID}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetUnread(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetUnreadNotifications", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);

                var list = new List<Notification>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Notification
                    {
                        NotificationID = reader.GetInt32(reader.GetOrdinal("NotificationID")),
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        NotificationType = reader.IsDBNull(reader.GetOrdinal("NotificationType")) ? null : reader.GetString(reader.GetOrdinal("NotificationType")),
                        IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetUnreadNotifications failed for user ID {UID}", userId);
                throw;
            }
        }

        public async Task MarkRead(int notificationId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_MarkNotificationRead", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@NotificationID", notificationId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_MarkNotificationRead failed for ID {ID}", notificationId);
                throw;
            }
        }

        public async Task MarkAllRead(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_MarkAllNotificationsRead", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_MarkAllNotificationsRead failed for user ID {UID}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetNotificationCount", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", userId);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetNotificationCount failed for user ID {UID}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<(Notification Notification, string ActorName)>> GetRecentNotifications(int count)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand(@"
                    SELECT TOP (@Count) n.NotificationID, n.UserID, n.Message, n.NotificationType, n.IsRead, n.CreatedAt, u.FullName
                    FROM Notifications n
                    JOIN Users u ON n.UserID = u.UserID
                    ORDER BY n.CreatedAt DESC", conn);
                cmd.Parameters.AddWithValue("@Count", count);

                var list = new List<(Notification, string)>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var n = new Notification
                    {
                        NotificationID = reader.GetInt32(reader.GetOrdinal("NotificationID")),
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        NotificationType = reader.IsDBNull(reader.GetOrdinal("NotificationType")) ? null : reader.GetString(reader.GetOrdinal("NotificationType")),
                        IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    };
                    var actor = reader.GetString(reader.GetOrdinal("FullName"));
                    list.Add((n, actor));
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "GetRecentNotifications failed");
                throw;
            }
        }
    }
}
