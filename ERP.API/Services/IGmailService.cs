using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface IGmailService
    {
        string GetAuthorizationUrl(int userId, string state);
        Task<bool> AuthenticateAsync(int userId, string code);
        Task<EmailResponse> SendEmailAsync(int userId, SendEmailRequest request);
        Task<EmailAccountDto?> GetConnectedAccountAsync(int userId);
        Task<bool> DisconnectAccountAsync(int userId);
        Task<EmailListResponse> ListMessagesAsync(int userId, EmailSearchRequest request);
        Task<EmailMessageDto?> GetMessageAsync(int userId, string messageId);
    }
}
