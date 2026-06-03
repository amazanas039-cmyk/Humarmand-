using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillBridge.Models;
using SkillBridge.Repositories;

namespace SkillBridge.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatRepository _chatRepo;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ChatRepository chatRepo, ILogger<ChatService> logger)
        {
            _chatRepo = chatRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<DisputeChatMessage>> GetMessagesAsync(int disputeId)
        {
            return await _chatRepo.GetMessages(disputeId);
        }

        public async Task<DisputeChatMessage?> SaveMessageAsync(int disputeId, int senderUserId, string senderRole, string text)
        {
            _logger.LogInformation("Saving dispute chat message for dispute {DID} from user {UID} ({Role})",
                disputeId, senderUserId, senderRole);
            return await _chatRepo.AddMessage(disputeId, senderUserId, senderRole, text);
        }
    }
}
