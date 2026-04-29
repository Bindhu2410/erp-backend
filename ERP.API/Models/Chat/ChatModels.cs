using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.Chat
{
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("chat_name")]
        [MaxLength(255)]
        public string? ChatName { get; set; }

        [Column("chat_type")]
        [Required]
        [MaxLength(20)]
        public string ChatType { get; set; } = "private";

        [Column("chat_image_url")]
        public string? ChatImageUrl { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }

    public class ChatMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("chat_id")]
        [Required]
        public int ChatId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("role")]
        [MaxLength(20)]
        public string Role { get; set; } = "member";

        [Column("joined_at")]
        public DateTime? JoinedAt { get; set; } = DateTime.UtcNow;

        [Column("left_at")]
        public DateTime? LeftAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_muted")]
        public bool IsMuted { get; set; } = false;
    }

    public class ChatMessageV2
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("chat_id")]
        [Required]
        public int ChatId { get; set; }

        [Column("sender_id")]
        [Required]
        public int SenderId { get; set; }

        [Column("message_text")]
        public string? MessageText { get; set; }

        [Column("message_type")]
        [MaxLength(20)]
        public string MessageType { get; set; } = "text";

        [Column("file_url")]
        public string? FileUrl { get; set; }

        [Column("file_name")]
        [MaxLength(500)]
        public string? FileName { get; set; }

        [Column("file_size")]
        public long? FileSize { get; set; }

        [Column("reply_to_id")]
        public int? ReplyToId { get; set; }

        [Column("is_edited")]
        public bool IsEdited { get; set; } = false;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }

    public class MessageStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("message_id")]
        [Required]
        public int MessageId { get; set; }

        [Column("user_id")]
        [Required]
        public int UserId { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "sent";

        [Column("status_at")]
        public DateTime StatusAt { get; set; } = DateTime.UtcNow;
    }

    public class UserPresence
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("is_online")]
        public bool IsOnline { get; set; } = false;

        [Column("last_seen")]
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        [Column("connection_id")]
        [MaxLength(255)]
        public string? ConnectionId { get; set; }
    }
}
