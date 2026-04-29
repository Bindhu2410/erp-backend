using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using ERP.API.Hubs;
using ERP.API.Models.Chat;
using ERP.API.Services;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _hubContext = hubContext;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "userid" ||
                c.Type == "sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Get all chats for the current user
        /// </summary>
        [HttpGet("chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(chats);
        }

        /// <summary>
        /// Create a private (1-to-1) chat
        /// </summary>
        [HttpPost("chats/private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var chatId = await _chatService.CreatePrivateChatAsync(userId, request.OtherUserId);

            // Add both users to the SignalR group
            var connId1 = ChatHub.GetConnectionId(userId);
            var connId2 = ChatHub.GetConnectionId(request.OtherUserId);
            if (connId1 != null)
                await _hubContext.Groups.AddToGroupAsync(connId1, $"chat_{chatId}");
            if (connId2 != null)
                await _hubContext.Groups.AddToGroupAsync(connId2, $"chat_{chatId}");

            return Ok(new { chatId });
        }

        /// <summary>
        /// Create a group chat
        /// </summary>
        [HttpPost("chats/group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var chatId = await _chatService.CreateGroupChatAsync(request.Name, userId, request.MemberIds, request.ImageUrl);

            // Add all members to SignalR group
            foreach (var memberId in request.MemberIds)
            {
                var connId = ChatHub.GetConnectionId(memberId);
                if (connId != null)
                    await _hubContext.Groups.AddToGroupAsync(connId, $"chat_{chatId}");
            }
            var creatorConnId = ChatHub.GetConnectionId(userId);
            if (creatorConnId != null)
                await _hubContext.Groups.AddToGroupAsync(creatorConnId, $"chat_{chatId}");

            // Notify all members
            await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ChatCreated", new { chatId, name = request.Name, chatType = "group" });

            return CreatedAtAction(nameof(GetChatHistory), new { chatId }, new { chatId });
        }

        /// <summary>
        /// Send a message via REST (alternative to SignalR)
        /// </summary>
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!await _chatService.IsUserInChatAsync(request.ChatId, userId))
                return Forbid();

            var message = await _chatService.SendMessageAsync(userId, request);
            if (message == null) return BadRequest("Failed to send message");

            // Broadcast via SignalR
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
            await _hubContext.Clients.Group($"chat_{request.ChatId}").SendAsync("ReceiveMessage", signalRMsg);

            return Ok(message);
        }

        /// <summary>
        /// Get chat history with pagination
        /// </summary>
        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetChatHistory(int chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!await _chatService.IsUserInChatAsync(chatId, userId))
                return Forbid();

            var messages = await _chatService.GetChatHistoryAsync(chatId, userId, page, pageSize);
            return Ok(messages);
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPost("messages/read/{chatId}")]
        public async Task<IActionResult> MarkAsRead(int chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var count = await _chatService.MarkMessagesAsReadAsync(chatId, userId);

            // Notify others via SignalR
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("MessagesRead", new ReadReceiptDto
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName
            });

            return Ok(new { markedCount = count });
        }

        /// <summary>
        /// Edit a message
        /// </summary>
        [HttpPut("messages")]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var message = await _chatService.EditMessageAsync(request.MessageId, userId, request.MessageText);
            if (message == null) return NotFound("Message not found or you don't have permission");

            // Notify via SignalR
            await _hubContext.Clients.Group($"chat_{message.ChatId}").SendAsync("MessageEdited", message);

            return Ok(message);
        }

        /// <summary>
        /// Delete a message (soft delete)
        /// </summary>
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var deleted = await _chatService.DeleteMessageAsync(messageId, userId);
            if (!deleted) return NotFound("Message not found or you don't have permission");

            return Ok(new { success = true });
        }

        /// <summary>
        /// Get chat members
        /// </summary>
        [HttpGet("chats/{chatId}/members")]
        public async Task<IActionResult> GetChatMembers(int chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (!await _chatService.IsUserInChatAsync(chatId, userId))
                return Forbid();

            var members = await _chatService.GetChatMembersAsync(chatId);
            return Ok(members);
        }

        /// <summary>
        /// Add member to group
        /// </summary>
        [HttpPost("chats/members/add")]
        public async Task<IActionResult> AddGroupMember([FromBody] AddGroupMemberRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _chatService.AddGroupMemberAsync(request.ChatId, request.UserId, userId);
            if (!result) return BadRequest("Could not add member");

            // Add new member to SignalR group
            var connId = ChatHub.GetConnectionId(request.UserId);
            if (connId != null)
                await _hubContext.Groups.AddToGroupAsync(connId, $"chat_{request.ChatId}");

            // Notify chat group
            await _hubContext.Clients.Group($"chat_{request.ChatId}").SendAsync("MemberAdded", new { chatId = request.ChatId, userId = request.UserId });

            return Ok(new { success = true });
        }

        /// <summary>
        /// Remove member from group
        /// </summary>
        [HttpPost("chats/members/remove")]
        public async Task<IActionResult> RemoveGroupMember([FromBody] RemoveGroupMemberRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _chatService.RemoveGroupMemberAsync(request.ChatId, request.UserId, userId);
            if (!result) return BadRequest("Could not remove member");

            // Remove from SignalR group
            var connId = ChatHub.GetConnectionId(request.UserId);
            if (connId != null)
                await _hubContext.Groups.RemoveFromGroupAsync(connId, $"chat_{request.ChatId}");

            // Notify chat group
            await _hubContext.Clients.Group($"chat_{request.ChatId}").SendAsync("MemberRemoved", new { chatId = request.ChatId, userId = request.UserId });

            return Ok(new { success = true });
        }

        /// <summary>
        /// Update group info
        /// </summary>
        [HttpPut("chats/group")]
        public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await _chatService.UpdateGroupAsync(request.ChatId, request.Name, request.ImageUrl);
            await _hubContext.Clients.Group($"chat_{request.ChatId}").SendAsync("GroupUpdated", new { chatId = request.ChatId, name = request.Name, imageUrl = request.ImageUrl });

            return Ok(new { success = true });
        }

        /// <summary>
        /// Search messages
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchMessages([FromQuery] string q, [FromQuery] int? chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search term required");

            var results = await _chatService.SearchMessagesAsync(userId, q, chatId, page, pageSize);
            return Ok(results);
        }

        /// <summary>
        /// Get available users to chat with
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAvailableUsers()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var users = await _chatService.GetAvailableUsersAsync(userId);
            return Ok(users);
        }

        /// <summary>
        /// Toggle mute for a chat
        /// </summary>
        [HttpPost("chats/{chatId}/mute")]
        public async Task<IActionResult> ToggleMute(int chatId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await _chatService.ToggleMuteAsync(chatId, userId);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Upload media file for chat
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".mp4", ".mp3", ".zip" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedTypes.Contains(ext))
                return BadRequest("File type not allowed");

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "chat-uploads");
            Directory.CreateDirectory(uploadsPath);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/chat-uploads/{uniqueName}";
            var messageType = ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
                ".mp4" => "video",
                ".mp3" => "audio",
                _ => "file"
            };

            return Ok(new
            {
                fileUrl,
                fileName = file.FileName,
                fileSize = file.Length,
                messageType
            });
        }

        // ===== Legacy endpoints for backward compatibility =====

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessageLegacy([FromBody] Models.ChatMessage message)
        {
            var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Message);
            return Ok(new { success = true, messageId = message.Id });
        }

        [HttpPost("send-to-group")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessageToGroupLegacy([FromBody] Models.ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.GroupName))
                return BadRequest("Group name is required");

            var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            await _hubContext.Clients.Group(message.GroupName).SendAsync("ReceiveMessage", message.User, message.Message);
            return Ok(new { success = true, messageId = message.Id });
        }

        [HttpGet("messages-legacy")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMessagesLegacy(string? groupName = null)
        {
            var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var query = context.ChatMessages.AsQueryable();
            if (!string.IsNullOrEmpty(groupName))
                query = query.Where(m => m.GroupName == groupName);
            var messages = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderBy(m => m.Timestamp));
            return Ok(messages);
        }
    }
}