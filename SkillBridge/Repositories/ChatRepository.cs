using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SkillBridge.Data;
using SkillBridge.Models;

namespace SkillBridge.Repositories
{
    public class ChatRepository
    {
        private readonly DatabaseConnection _db;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(DatabaseConnection db, ILogger<ChatRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<DisputeChatMessage?> AddMessage(int disputeId, int senderUserId, string senderRole, string text)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_AddChatMessage", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@DisputeID", disputeId);
                cmd.Parameters.AddWithValue("@SenderUserID", senderUserId);
                cmd.Parameters.AddWithValue("@SenderRole", senderRole);
                cmd.Parameters.AddWithValue("@MessageText", text);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new DisputeChatMessage
                    {
                        MessageID = reader.GetInt32(reader.GetOrdinal("MessageID")),
                        SenderUserID = senderUserId,
                        SenderRole = senderRole,
                        MessageText = text,
                        SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt"))
                    };
                }
                return null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_AddChatMessage failed for dispute ID {DID}", disputeId);
                throw;
            }
        }

        public async Task<IEnumerable<DisputeChatMessage>> GetMessages(int disputeId)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_GetChatMessages", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@DisputeID", disputeId);

                var list = new List<DisputeChatMessage>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DisputeChatMessage
                    {
                        MessageID = reader.GetInt32(reader.GetOrdinal("MessageID")),
                        SenderUserID = reader.GetInt32(reader.GetOrdinal("SenderUserID")),
                        SenderName = reader.GetString(reader.GetOrdinal("SenderName")),
                        SenderRole = reader.GetString(reader.GetOrdinal("SenderRole")),
                        MessageText = reader.GetString(reader.GetOrdinal("MessageText")),
                        SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt"))
                    });
                }
                return list;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "sp_GetChatMessages failed for dispute ID {DID}", disputeId);
                throw;
            }
        }
    }
}
