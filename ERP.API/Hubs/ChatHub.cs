using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ERP.API.Models.Chat;
using ERP.API.Services;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace ERP.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        // Track userId -> connectionId mapping
        private static readonly ConcurrentDictionary<int, string> _userConnections = new();

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var claim = Context.User?.Claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "userid" ||
                c.Type == "sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        private string GetUserName()
        {
            var firstName = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value ?? "";
            var lastName = Context.User?.FindFirst(ClaimTypes.Surname)?.Value ?? "";
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                return $"{firstName} {lastName}".Trim();
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                _userConnections[userId] = Context.ConnectionId;
                await _chatService.UpdateUserPresenceAsync(userId, true, Context.ConnectionId);

                // Join all user's chat groups
                var chats = await _chatService.GetUserChatsAsync(userId);
                foreach (var chat in chats)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chat.ChatId}");
                }

                // Notify others that user is online
                await Clients.Others.SendAsync("UserOnline", new PresenceDto
                {
                    UserId = userId,
                    IsOnline = true,
                    LastSeen = DateTime.UtcNow
                });

                _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                _userConnections.TryRemove(userId, out _);
                await _chatService.UpdateUserPresenceAsync(userId, false, null);

                // Notify others that user is offline
                await Clients.Others.SendAsync("UserOffline", new PresenceDto
                {
                    UserId = userId,
                    IsOnline = false,
                    LastSeen = DateTime.UtcNow
                });

                _logger.LogInformation("User {UserId} disconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific chat group
        /// </summary>
        public async Task JoinChat(int chatId)
        {
            var userId = GetUserId();
            if (await _chatService.IsUserInChatAsync(chatId, userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            }
        }

        /// <summary>
        /// Leave a specific chat group
        /// </summary>
        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        /// <summary>
        /// Send a message in real-time
        /// </summary>
        public async Task SendMessage(SendMessageRequest request)
        {
            var userId = GetUserId();
            if (userId == 0 || !await _chatService.IsUserInChatAsync(request.ChatId, userId))
                return;

            var message = await _chatService.SendMessageAsync(userId, request);
            if (message != null)
            {
                var signalRMsg = new SignalRMessageDto
                {
                    MessageId = message.MessageId,
                    ChatId = message.ChatId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    SenderAvatar = message.SenderAvatar,
                    MessageText = message.MessageText,
                    MessageType = message.MessageType,
                    FileUrl = message.FileUrl,
                    FileName = message.FileName,
                    FileSize = message.FileSize,
                    ReplyToId = message.ReplyToId,
                    ReplyText = message.ReplyText,
                    ReplySenderName = message.ReplySenderName,
                    DateCreated = message.DateCreated
                };

                await Clients.Group($"chat_{request.ChatId}").SendAsync("ReceiveMessage", signalRMsg);
            }
        }

        /// <summary>
        /// Typing indicator
        /// </summary>
        public async Task Typing(int chatId, bool isTyping)
        {
            var userId = GetUserId();
            var userName = GetUserName();

            await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserTyping", new TypingIndicatorDto
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName,
                IsTyping = isTyping
            });
        }

        /// <summary>
        /// Mark messages as read and notify sender
        /// </summary>
        public async Task MarkAsRead(int chatId)
        {
            var userId = GetUserId();
            var userName = GetUserName();
            await _chatService.MarkMessagesAsReadAsync(chatId, userId);

            await Clients.OthersInGroup($"chat_{chatId}").SendAsync("MessagesRead", new ReadReceiptDto
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName
            });
        }

        /// <summary>
        /// Get connection ID for a specific user (for direct messages)
        /// </summary>
        public static string? GetConnectionId(int userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        // Legacy methods for backward compatibility
        public async Task SendMessageLegacy(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoined", Context.User?.Identity?.Name ?? "Anonymous");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", Context.User?.Identity?.Name ?? "Anonymous");
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }
    }
}