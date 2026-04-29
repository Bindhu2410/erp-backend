using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.Chat
{
    // ===== Request DTOs =====

    public class CreatePrivateChatRequest
    {
        [Required]
        public int OtherUserId { get; set; }
    }

    public class CreateGroupChatRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<int> MemberIds { get; set; } = new();

        public string? ImageUrl { get; set; }
    }

    public class SendMessageRequest
    {
        [Required]
        public int ChatId { get; set; }

        public string? MessageText { get; set; }

        public string MessageType { get; set; } = "text";

        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        public int? ReplyToId { get; set; }
    }

    public class EditMessageRequest
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        public string MessageText { get; set; } = string.Empty;
    }

    public class ChatHistoryRequest
    {
        public int ChatId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class SearchMessagesRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string SearchTerm { get; set; } = string.Empty;

        public int? ChatId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AddGroupMemberRequest
    {
        [Required]
        public int ChatId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class RemoveGroupMemberRequest
    {
        [Required]
        public int ChatId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class UpdateGroupRequest
    {
        [Required]
        public int ChatId { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? ImageUrl { get; set; }
    }

    // ===== Response DTOs =====

    public class ChatListItemDto
    {
        public int ChatId { get; set; }
        public string? ChatName { get; set; }
        public string ChatType { get; set; } = string.Empty;
        public string? ChatImageUrl { get; set; }
        public string? LastMessage { get; set; }
        public string? LastMessageType { get; set; }
        public string? LastMessageSender { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public int MemberCount { get; set; }
        public bool IsMuted { get; set; }
        // For private chats
        public int? OtherUserId { get; set; }
        public string? OtherUserName { get; set; }
        public string? OtherUserAvatar { get; set; }
        public bool OtherUserOnline { get; set; }
    }

    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string? MessageText { get; set; }
        public string MessageType { get; set; } = "text";
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public int? ReplyToId { get; set; }
        public string? ReplyText { get; set; }
        public string? ReplySenderName { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DateCreated { get; set; }
        public int ReadByCount { get; set; }
        public int TotalRecipients { get; set; }
    }

    public class ChatMemberDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string Role { get; set; } = "member";
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? JoinedAt { get; set; }
    }

    public class SearchMessageResultDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public string? ChatName { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? MessageText { get; set; }
        public string? MessageType { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ChatUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public bool IsOnline { get; set; }
    }

    // ===== SignalR DTOs =====

    public class SignalRMessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string? MessageText { get; set; }
        public string MessageType { get; set; } = "text";
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public int? ReplyToId { get; set; }
        public string? ReplyText { get; set; }
        public string? ReplySenderName { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class TypingIndicatorDto
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }

    public class PresenceDto
    {
        public int UserId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
    }

    public class ReadReceiptDto
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int MessageId { get; set; }
    }
}
