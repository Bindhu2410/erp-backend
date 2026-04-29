# Email System Documentation

## Overview

The Email System provides comprehensive email functionality for the ERP system, including real-time email sending using Gmail API with OAuth 2.0 authentication, email templates, campaigns, tracking, and integration with other ERP modules.

## Features

### 🔐 OAuth 2.0 Authentication
- Secure Gmail account connection using OAuth 2.0
- Automatic token refresh handling
- Support for multiple email accounts per user

### 📧 Email Management
- Send individual emails
- Bulk email sending
- Template-based emails
- Email scheduling
- Attachment support
- HTML and plain text content

### 📊 Email Tracking
- Open tracking (pixel tracking)
- Click tracking
- Delivery status monitoring
- Bounce handling
- Email statistics and analytics

### 📋 Templates & Campaigns
- Reusable email templates
- Template variables and personalization
- Email campaign management
- Campaign analytics and reporting

### 🔄 Queue System
- Background email processing
- Retry mechanism for failed emails
- Priority-based queue processing
- Scalable email delivery

### 🔗 ERP Integration
- Lead follow-up emails
- Quotation emails with PDF attachments
- Invoice emails with PDF attachments
- Order confirmation emails
- Payment reminder emails
- Related entity tracking

## Database Schema

### Core Tables

#### `email_accounts`
Stores OAuth credentials and account details for connected Gmail accounts.

#### `email_messages`
Stores all email messages with content, recipients, and metadata.

#### `email_templates`
Stores reusable email templates with variables support.

#### `email_campaigns`
Manages email campaigns with analytics.

#### `email_recipients`
Tracks individual recipients and their interactions.

#### `email_tracking_events`
Detailed tracking events (opens, clicks, bounces).

#### `email_queue`
Queue system for background email processing.

#### `email_attachments`
Email attachment metadata and file references.

## API Endpoints

### OAuth & Account Management

```
GET    /api/email/oauth/url                    - Get OAuth authorization URL
GET    /api/email/oauth/callback               - Handle OAuth callback
GET    /api/email/accounts                     - Get user's email accounts
GET    /api/email/accounts/{id}                - Get specific email account
DELETE /api/email/accounts/{id}                - Remove email account
PUT    /api/email/accounts/{id}/primary        - Set primary account
```

### Email Operations

```
POST   /api/email/send                         - Send single email
POST   /api/email/send/bulk                    - Send bulk emails
POST   /api/email/send/template/{templateId}   - Send template email
GET    /api/email                              - Get emails with search/pagination
GET    /api/email/{messageId}                  - Get specific email
DELETE /api/email/{messageId}                  - Delete email
PUT    /api/email/{messageId}/read             - Mark email as read
GET    /api/email/{messageId}/stats            - Get email statistics
```

### Templates

```
GET    /api/email/templates                    - Get all templates
GET    /api/email/templates/{id}               - Get specific template
POST   /api/email/templates                    - Create template
PUT    /api/email/templates/{id}               - Update template
DELETE /api/email/templates/{id}               - Delete template
```

### Campaigns

```
GET    /api/email/campaigns                    - Get all campaigns
GET    /api/email/campaigns/{id}               - Get specific campaign
POST   /api/email/campaigns                    - Create campaign
POST   /api/email/campaigns/{id}/start         - Start campaign
POST   /api/email/campaigns/{id}/pause         - Pause campaign
POST   /api/email/campaigns/{id}/stop          - Stop campaign
```

### Tracking

```
GET    /api/email/track/open/{trackingId}      - Track email open (returns 1x1 pixel)
GET    /api/email/track/click/{trackingId}     - Track email click and redirect
```

### Sync Operations

```
POST   /api/email/sync/{accountId}             - Sync emails from Gmail
POST   /api/email/queue/process                - Process email queue manually
```

## Setup Instructions

### 1. Google Cloud Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Gmail API:
   - Go to APIs & Services > Library
   - Search for "Gmail API" and enable it

### 2. OAuth 2.0 Credentials

1. Go to APIs & Services > Credentials
2. Click "Create Credentials" > "OAuth 2.0 Client IDs"
3. Configure OAuth consent screen:
   - App name: ERP System
   - Add required scopes:
     - `https://www.googleapis.com/auth/gmail.send`
     - `https://www.googleapis.com/auth/gmail.readonly`
     - `https://www.googleapis.com/auth/gmail.modify`
4. Create Web Application credentials:
   - Authorized redirect URIs:
     - `http://localhost:5104/api/email/oauth/callback`
     - `https://localhost:5104/api/email/oauth/callback`

### 3. Configuration

Update `appsettings.json`:

```json
{
  "Gmail": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "http://localhost:5104/api/email/oauth/callback",
    "Scopes": [
      "https://www.googleapis.com/auth/gmail.send",
      "https://www.googleapis.com/auth/gmail.readonly",
      "https://www.googleapis.com/auth/gmail.modify"
    ]
  },
  "Email": {
    "DefaultSenderName": "ERP System",
    "MaxRetries": 3,
    "QueueProcessingInterval": 30,
    "EnableTracking": true,
    "TrackingDomain": "http://localhost:5104"
  }
}
```

### 4. Database Setup

Run the SQL scripts:

```bash
# Navigate to the project directory
cd "d:\MagnusVista\Brindha\Project ERP & CRM\Project CRM\Sales\salesAIQ_Api\Sales_jbs\JBS4IR_v2\Backend\ERP-1\ERP.API"

# Run the setup script
psql -h localhost -p 5433 -U postgres -d postgres -f "Sqlscript\setup_email_system.sql"
```

Or run individual scripts:

```bash
psql -h localhost -p 5433 -U postgres -d postgres -f "Sqlscript\OAuthStatesTable.sql"
psql -h localhost -p 5433 -U postgres -d postgres -f "Sqlscript\EmailSystemTables.sql"
```

### 5. Build and Run

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### 6. Quick Setup Script

Run the PowerShell setup script for guided setup:

```powershell
.\setup_gmail_oauth.ps1
```

## Usage Examples

### Sending a Simple Email

```csharp
var emailRequest = new SendEmailRequest
{
    To = "customer@example.com",
    Subject = "Test Email",
    BodyHtml = "<h1>Hello!</h1><p>This is a test email.</p>",
    BodyText = "Hello!\n\nThis is a test email."
};

var response = await gmailService.SendEmailAsync(emailRequest, userId);
```

### Using Email Templates

```csharp
var templateRequest = new SendEmailRequest
{
    To = "customer@example.com",
    TemplateId = 1, // Lead follow-up template
    TemplateVariables = new Dictionary<string, string>
    {
        { "customer_name", "John Doe" },
        { "product_name", "ERP System" },
        { "sender_name", "Sales Team" }
    },
    RelatedEntityType = "lead",
    RelatedEntityId = 123
};

var response = await gmailService.SendTemplateEmailAsync(1, templateRequest, userId);
```

### Integration with ERP Modules

```csharp
// Send quotation email with PDF attachment
var quotationResponse = await emailHelper.SendQuotationEmailAsync(
    quotationId: 456,
    customerEmail: "customer@example.com",
    customerName: "John Doe",
    quotationNumber: "Q-2025-001",
    amount: 5000.00m,
    validityDate: DateTime.Now.AddDays(30),
    userId: currentUserId,
    quotationPdf: pdfBytes
);

// Send payment reminder
var reminderResponse = await emailHelper.SendPaymentReminderAsync(
    invoiceId: 789,
    customerEmail: "customer@example.com",
    customerName: "John Doe",
    invoiceNumber: "INV-2025-001",
    amount: 5000.00m,
    dueDate: DateTime.Now.AddDays(-5), // 5 days overdue
    userId: currentUserId
);
```

## Security Considerations

### 🔒 Credential Security
- Never commit OAuth credentials to version control
- Use environment variables in production
- Consider Azure Key Vault or similar for credential storage
- Regularly rotate OAuth credentials

### 🛡️ API Security
- All endpoints require authentication except tracking and OAuth callback
- Rate limiting recommended for email sending endpoints
- Input validation for all email content
- Sanitize HTML content to prevent XSS

### 📊 Monitoring
- Monitor API usage and quotas
- Track failed emails and retry patterns
- Set up alerts for OAuth token expiration
- Monitor email delivery rates and bounces

## Performance Optimization

### 📈 Queue Processing
- Background service processes email queue every 30 seconds (configurable)
- Failed emails are automatically retried with exponential backoff
- Priority-based queue processing for urgent emails

### 🚀 Scaling Considerations
- Consider Redis for distributed queue management
- Implement email rate limiting per Gmail API quotas
- Use connection pooling for database operations
- Consider separate service for heavy email processing

## Troubleshooting

### Common Issues

1. **OAuth Setup Issues**
   - Verify redirect URIs match exactly
   - Check OAuth consent screen configuration
   - Ensure Gmail API is enabled

2. **Email Sending Failures**
   - Check Gmail API quotas and limits
   - Verify email account tokens are valid
   - Check email content for spam triggers

3. **Database Connection Issues**
   - Verify connection string in appsettings.json
   - Ensure database tables are created
   - Check PostgreSQL service is running

### Logging

The system provides comprehensive logging:
- OAuth flow events
- Email sending results
- Queue processing status
- Error details with stack traces

Check logs in the console output or configure file logging as needed.

## Future Enhancements

- [ ] Support for additional email providers (Outlook, SendGrid)
- [ ] Advanced email analytics dashboard
- [ ] A/B testing for email campaigns
- [ ] Email automation workflows
- [ ] Integration with calendar for meeting scheduling
- [ ] Email signature management
- [ ] Unsubscribe management
- [ ] Email archiving and compliance features

## Support

For issues and questions:
1. Check the troubleshooting section
2. Review the logs for error details
3. Verify configuration settings
4. Test with a simple email send first

## License

This email system is part of the ERP project and follows the same licensing terms.
