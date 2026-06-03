using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SkillBridge.Services;

namespace SkillBridge.Hubs
{
    public class DisputeChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IDisputeService _disputeService;
        private readonly ILogger<DisputeChatHub> _logger;

        public DisputeChatHub(IChatService chatService, IDisputeService disputeService, ILogger<DisputeChatHub> logger)
        {
            _chatService = chatService;
            _disputeService = disputeService;
            _logger = logger;
        }

        public async Task JoinDispute(int disputeId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                _logger.LogWarning("JoinDispute failed: Unauthenticated connection.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"dispute-{disputeId}");
            _logger.LogInformation("Connection {ConnId} joined dispute-{DID} group for User {UID}",
                Context.ConnectionId, disputeId, userId);

            // Send message history
            var msgs = await _chatService.GetMessagesAsync(disputeId);
            await Clients.Caller.SendAsync("LoadHistory", msgs);
        }

        public async Task SendMessage(int disputeId, string text)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var name = GetUserName();

            if (userId == null || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(name))
            {
                _logger.LogWarning("SendMessage failed: Unauthenticated or incomplete session credentials.");
                return;
            }

            var msg = await _chatService.SaveMessageAsync(disputeId, userId.Value, role, text);
            if (msg != null)
            {
                await Clients.Group($"dispute-{disputeId}")
                    .SendAsync("ReceiveMessage", name, role, text, msg.SentAt);
            }
        }

        public async Task ResolveDispute(int disputeId, string resolutionType, string resolutionText)
        {
            var role = GetUserRole();
            var userId = GetUserId();

            if (role != "Admin" || userId == null)
            {
                _logger.LogWarning("ResolveDispute rejected: Unauthorized role '{Role}' or invalid UserID '{UID}'", role, userId);
                return;
            }

            await _disputeService.ResolveAsync(disputeId, resolutionType, resolutionText, userId.Value);
            await Clients.Group($"dispute-{disputeId}").SendAsync("DisputeResolved", disputeId);
        }

        private int? GetUserId()
        {
            var session = Context.GetHttpContext()?.Session;
            if (session == null) return null;
            var val = session.GetString("UserId");
            return int.TryParse(val, out var id) ? id : null;
        }

        private string? GetUserRole()
        {
            return Context.GetHttpContext()?.Session?.GetString("UserRole");
        }

        private string? GetUserName()
        {
            return Context.GetHttpContext()?.Session?.GetString("UserFullName");
        }
    }
}
