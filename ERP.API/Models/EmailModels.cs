using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class EmailAccount : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPrimary { get; set; } = false;
    }

    public class EmailTemplate : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public string? TemplateType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmailMessage : BaseEntity
    {
        public int Id { get; set; }
        public string? GmailMessageId { get; set; }
        public string? GmailThreadId { get; set; }
        public int? SenderEmailAccountId { get; set; }
        public int? CampaignId { get; set; }
        public string MessageType { get; set; } = "outbound";
        public string? Subject { get; set; }
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        [Required]
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; }
        [Required]
        public string ToEmails { get; set; } = string.Empty; // JSON array
        public string? CcEmails { get; set; } // JSON array
        public string? BccEmails { get; set; } // JSON array
        public string? ReplyTo { get; set; }
        public string Status { get; set; } = "draft";
        public string Priority { get; set; } = "normal";
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }

        // Navigation properties
        public EmailAccount? SenderEmailAccount { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new();
        public List<EmailRecipient> Recipients { get; set; } = new();
    }

    public class EmailAttachment
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        [Required]
        public string Filename { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public string? MimeType { get; set; }
        public string? FilePath { get; set; }
        public string? GmailAttachmentId { get; set; }
        public bool IsInline { get; set; } = false;
        public string? ContentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public EmailMessage Message { get; set; } = null!;
    }

    public class EmailRecipient
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int? CampaignId { get; set; }
        [Required]
        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string RecipientType { get; set; } = "to"; // to, cc, bcc
        public string Status { get; set; } = "pending";
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? LastOpenedAt { get; set; }
        public int OpenCount { get; set; } = 0;
        public DateTime? ClickedAt { get; set; }
        public DateTime? LastClickedAt { get; set; }
        public int ClickCount { get; set; } = 0;
        public DateTime? BouncedAt { get; set; }
        public string? BounceReason { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public EmailMessage Message { get; set; } = null!;
    }

    public class EmailCampaign : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string CampaignName { get; set; } = string.Empty;
        public string? CampaignDescription { get; set; }
        public int? TemplateId { get; set; }
        public int? SenderEmailAccountId { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecipients { get; set; } = 0;
        public int SentCount { get; set; } = 0;
        public int DeliveredCount { get; set; } = 0;
        public int OpenedCount { get; set; } = 0;
        public int ClickedCount { get; set; } = 0;
        public int BouncedCount { get; set; } = 0;

        // Navigation properties
        public EmailTemplate? Template { get; set; }
        public EmailAccount? SenderEmailAccount { get; set; }
    }

    public class EmailSignature : BaseEntity
    {
        public int Id { get; set; }
        public int EmailAccountId { get; set; }
        [Required]
        public string SignatureName { get; set; } = string.Empty;
        public string? SignatureHtml { get; set; }
        public string? SignatureText { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public EmailAccount EmailAccount { get; set; } = null!;
    }
}
