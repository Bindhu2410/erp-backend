using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    // ─────────────────────────────────────────────
    //  Entity Models (mapped to DB tables)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Represents a user's registered WhatsApp Business phone number.
    /// One row per user per phone number.
    /// </summary>
    [Table("whatsapp_accounts")]
    public class WhatsAppAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>Meta phone_number_id for this number (used in API calls).</summary>
        [Column("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;

        /// <summary>E.164 format e.g. +919876543210</summary>
        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>Per-number or system-user access token.</summary>
        [Column("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Meta WhatsApp Business Account ID.</summary>
        [Column("waba_id")]
        public string WabaId { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<WhatsAppConversation> Conversations { get; set; } = new();
        public List<WhatsAppTemplate> Templates { get; set; } = new();
    }

    /// <summary>
    /// A WhatsApp conversation thread between the CRM user and a contact.
    /// </summary>
    [Table("whatsapp_conversations")]
    public class WhatsAppConversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("account_id")]
        public int AccountId { get; set; }

        /// <summary>Contact's WhatsApp number in E.164 format.</summary>
        [Column("contact_phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [Column("contact_name")]
        public string? ContactName { get; set; }

        /// <summary>Linked SalesLead (nullable FK).</summary>
        [Column("lead_id")]
        public int? LeadId { get; set; }

        /// <summary>Linked SalesContact (nullable FK).</summary>
        [Column("contact_id")]
        public int? ContactId { get; set; }

        [Column("last_message_at")]
        public DateTime? LastMessageAt { get; set; }

        [Column("unread_count")]
        public int UnreadCount { get; set; } = 0;

        /// <summary>Meta's conversation ID returned in webhook for billing context.</summary>
        [Column("wa_conversation_id")]
        public string? WaConversationId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public WhatsAppAccount? Account { get; set; }
        public List<WhatsAppMessage> Messages { get; set; } = new();
    }

    /// <summary>
    /// An individual WhatsApp message (inbound or outbound).
    /// </summary>
    [Table("whatsapp_messages")]
    public class WhatsAppMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("conversation_id")]
        public int ConversationId { get; set; }

        /// <summary>Meta's unique message ID — used for deduplication and status updates.</summary>
        [Column("wa_message_id")]
        public string? WaMessageId { get; set; }

        /// <summary>inbound | outbound</summary>
        [Column("direction")]
        public string Direction { get; set; } = "outbound";

        /// <summary>text | template | image | document | audio | video | interactive</summary>
        [Column("message_type")]
        public string MessageType { get; set; } = "text";

        [Column("content")]
        public string? Content { get; set; }

        [Column("template_name")]
        public string? TemplateName { get; set; }

        /// <summary>sent | delivered | read | failed | pending</summary>
        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public WhatsAppConversation? Conversation { get; set; }
    }

    /// <summary>
    /// Cached copy of approved WhatsApp message templates from Meta.
    /// </summary>
    [Table("whatsapp_templates")]
    public class WhatsAppTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("account_id")]
        public int AccountId { get; set; }

        [Column("template_name")]
        public string TemplateName { get; set; } = string.Empty;

        [Column("language")]
        public string Language { get; set; } = "en_US";

        /// <summary>MARKETING | UTILITY | AUTHENTICATION</summary>
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>JSON array of component objects as returned by Meta.</summary>
        [Column("components_json")]
        public string? ComponentsJson { get; set; }

        /// <summary>APPROVED | PENDING | REJECTED</summary>
        [Column("approval_status")]
        public string ApprovalStatus { get; set; } = "PENDING";

        [Column("synced_at")]
        public DateTime? SyncedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public WhatsAppAccount? Account { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Request / Response DTOs
    // ─────────────────────────────────────────────

    public class RegisterWhatsAppAccountRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string PhoneNumberId { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string WabaId { get; set; } = string.Empty;

        public string? DisplayName { get; set; }
    }

    public class SendWhatsAppTextRequest
    {
        [Required]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? ContactName { get; set; }
    }

    public class SendWhatsAppTemplateRequest
    {
        [Required]
        public string To { get; set; } = string.Empty;

        [Required]
        public string TemplateName { get; set; } = string.Empty;

        public string Language { get; set; } = "en_US";

        /// <summary>
        /// Template component parameters.
        /// Example: [{ "type": "body", "parameters": [{ "type": "text", "text": "John" }] }]
        /// </summary>
        public List<TemplateComponent>? Components { get; set; }

        public string? ContactName { get; set; }
    }

    public class TemplateComponent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public List<TemplateParameter>? Parameters { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonPropertyName("index")]
        public int? Index { get; set; }
    }

    public class TemplateParameter
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class LinkLeadRequest
    {
        [Required]
        public int LeadId { get; set; }
    }

    public class LinkContactRequest
    {
        [Required]
        public int ContactId { get; set; }
    }

    public class WhatsAppMessageResponse
    {
        public bool Success { get; set; }
        public string? WaMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public int? SavedMessageId { get; set; }
    }

    public class WhatsAppAccountDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PhoneNumberId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string WabaId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WhatsAppConversationDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string ContactPhone { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public int? LeadId { get; set; }
        public int? ContactId { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessagePreview { get; set; }
    }

    public class WhatsAppMessageDto
    {
        public int Id { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? TemplateName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Meta Webhook Payload Models
    //  Aligned with the Meta Graph API v19.0 webhook schema
    // ─────────────────────────────────────────────

    public class WhatsAppWebhookPayload
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<WhatsAppWebhookEntry>? Entry { get; set; }
    }

    public class WhatsAppWebhookEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<WhatsAppWebhookChange>? Changes { get; set; }
    }

    public class WhatsAppWebhookChange
    {
        [JsonPropertyName("value")]
        public WhatsAppWebhookValue? Value { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    public class WhatsAppWebhookValue
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public WhatsAppWebhookMetadata? Metadata { get; set; }

        [JsonPropertyName("contacts")]
        public List<WhatsAppWebhookContact>? Contacts { get; set; }

        [JsonPropertyName("messages")]
        public List<WhatsAppWebhookMessage>? Messages { get; set; }

        [JsonPropertyName("statuses")]
        public List<WhatsAppWebhookStatus>? Statuses { get; set; }
    }

    public class WhatsAppWebhookMetadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class WhatsAppWebhookContact
    {
        [JsonPropertyName("profile")]
        public WhatsAppContactProfile? Profile { get; set; }

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class WhatsAppContactProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class WhatsAppWebhookMessage
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public WhatsAppTextContent? Text { get; set; }
    }

    public class WhatsAppTextContent
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }

    public class WhatsAppWebhookStatus
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("recipient_id")]
        public string RecipientId { get; set; } = string.Empty;
    }
}
