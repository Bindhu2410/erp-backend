using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class SendEmailRequest
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public string Priority { get; set; } = "normal"; // low, normal, high
        public DateTime? ScheduledAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public List<EmailAttachmentDto> Attachments { get; set; } = new();
        public int? TemplateId { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; } = new();
    }

    public class EmailAttachmentDto
    {
        public string Filename { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public bool IsInline { get; set; } = false;
        public string? ContentId { get; set; }
    }

    public class BulkEmailRequest
    {
        [Required]
        public List<string> Recipients { get; set; } = new();
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public int? TemplateId { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; } = new();
        public DateTime? ScheduledAt { get; set; }
        public int? CampaignId { get; set; }
    }

    public class EmailTemplateRequest
    {
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        [Required]
        public string Subject { get; set; } = string.Empty;
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public string? TemplateType { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EmailAccountRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsPrimary { get; set; } = false;
    }

    public class EmailCampaignRequest
    {
        [Required]
        public string CampaignName { get; set; } = string.Empty;
        public string? CampaignDescription { get; set; }
        public int? TemplateId { get; set; }
        public List<string> Recipients { get; set; } = new();
        public DateTime? ScheduledAt { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; } = new();
    }

    public class EmailResponse
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? GmailMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? SentAt { get; set; }
    }

    public class EmailListResponse
    {
        public List<EmailMessageDto> Messages { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class EmailMessageDto
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public List<string> ToEmails { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EmailAttachmentDto> Attachments { get; set; } = new();
        public EmailStatsDto Stats { get; set; } = new();
    }

    public class EmailStatsDto
    {
        public int RecipientCount { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
        public int BounceCount { get; set; }
        public DateTime? LastOpenedAt { get; set; }
        public DateTime? LastClickedAt { get; set; }
    }

    public class EmailAccountDto
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmailTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? BodyHtml { get; set; }
        public string? BodyText { get; set; }
        public string? TemplateType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EmailCampaignDto
    {
        public int Id { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string? CampaignDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public EmailCampaignStatsDto Stats { get; set; } = new();
        public EmailTemplateDto? Template { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmailCampaignStatsDto
    {
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int OpenedCount { get; set; }
        public int ClickedCount { get; set; }
        public int BouncedCount { get; set; }
        public decimal DeliveryRate => TotalRecipients > 0 ? (decimal)DeliveredCount / TotalRecipients * 100 : 0;
        public decimal OpenRate => DeliveredCount > 0 ? (decimal)OpenedCount / DeliveredCount * 100 : 0;
        public decimal ClickRate => DeliveredCount > 0 ? (decimal)ClickedCount / DeliveredCount * 100 : 0;
        public decimal BounceRate => TotalRecipients > 0 ? (decimal)BouncedCount / TotalRecipients * 100 : 0;
    }

    public class OAuthUrlResponse
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class OAuthCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class EmailSearchRequest
    {
        public string? Query { get; set; }
        public string? Status { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "DESC";
    }
}
