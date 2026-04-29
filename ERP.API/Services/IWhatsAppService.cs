using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IWhatsAppService
    {
        // ── Account Management ────────────────────────────────────────────────
        Task<WhatsAppAccountDto> RegisterAccountAsync(RegisterWhatsAppAccountRequest request);
        Task<List<WhatsAppAccountDto>> GetAccountsAsync(int userId);
        Task<bool> DeregisterAccountAsync(int accountId, int userId);

        // ── Messaging ─────────────────────────────────────────────────────────
        Task<WhatsAppMessageResponse> SendTextMessageAsync(int accountId, SendWhatsAppTextRequest request);
        Task<WhatsAppMessageResponse> SendTemplateMessageAsync(int accountId, SendWhatsAppTemplateRequest request);

        // ── Conversations ─────────────────────────────────────────────────────
        Task<List<WhatsAppConversationDto>> GetConversationsAsync(int userId);
        Task<List<WhatsAppMessageDto>> GetMessagesAsync(int conversationId, int userId);
        Task<bool> LinkToLeadAsync(int conversationId, int leadId, int userId);
        Task<bool> LinkToContactAsync(int conversationId, int contactId, int userId);
        Task MarkConversationReadAsync(int conversationId, int userId);

        // ── Templates ─────────────────────────────────────────────────────────
        Task<List<WhatsAppTemplate>> GetTemplatesAsync(int accountId);
        Task<int> SyncTemplatesAsync(int accountId);

        // ── Webhook ───────────────────────────────────────────────────────────
        Task ProcessWebhookAsync(WhatsAppWebhookPayload payload);
        bool VerifyWebhookSignature(string rawBody, string signatureHeader, string appSecret);
    }
}
