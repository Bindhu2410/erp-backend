# Email System with Gmail API Integration - Complete Implementation

## Overview
This document provides a comprehensive guide for the real-time email system integrated into your ERP project. The system uses Gmail API with OAuth 2.0 authentication for secure email operations.

## 📁 Files Created

### Database Schema
- **`Sqlscript/EmailSystemTables.sql`** - Complete database schema with 11 tables
  - email_accounts, email_messages, email_templates, email_campaigns
  - email_tracking_events, email_queue, email_signatures, etc.

### Models & DTOs
- **`Models/EmailModels.cs`** - Entity models for all email system components
- **`Models/EmailDTOs.cs`** - Data Transfer Objects for API requests/responses

### Services
- **`Services/IGmailService.cs`** - Service interface (30+ methods)
- **`Services/Implementation/GmailService.cs`** - Gmail API implementation
- **`Services/Background/EmailQueueProcessorService.cs`** - Background email processing

### Controllers
- **`Controllers/EmailController.cs`** - REST API endpoints (20+ endpoints)

### Helpers
- **`Helpers/EmailIntegrationHelper.cs`** - Integration utilities

### Configuration Files
- **`Setup/EmailSystemSetup.ps1`** - Database setup script
- **`Setup/gmail_credentials_template.json`** - OAuth credentials template

### Documentation
- **`Documentation/EMAIL_USAGE_GUIDE.md`** - User guide
- **`Documentation/EMAIL_API_ENDPOINTS.md`** - API documentation

## 🚀 Setup Instructions

### 1. Database Setup
```bash
# Run the PowerShell setup script
cd "Setup"
.\EmailSystemSetup.ps1
```

### 2. Gmail API Configuration
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Gmail API
4. Create OAuth 2.0 credentials (Web application)
5. Add authorized redirect URIs:
   - `https://your-domain.com/api/email/oauth/callback`
   - `http://localhost:5000/api/email/oauth/callback` (for development)

### 3. Configuration
Update `appsettings.json`:
```json
{
  "Gmail": {
    "ClientId": "your-client-id.googleusercontent.com",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://your-domain.com/api/email/oauth/callback"
  },
  "ConnectionStrings": {
    "DefaultConnection": "your-postgresql-connection-string"
  }
}
```

## 📊 Database Tables

| Table Name | Purpose |
|------------|---------|
| email_accounts | Store Gmail account connections |
| email_messages | Store sent/received email data |
| email_templates | Email templates for bulk sending |
| email_campaigns | Campaign management |
| email_tracking_events | Open/click tracking |
| email_queue | Background email processing |
| email_signatures | User email signatures |
| email_attachments | File attachment management |
| email_folders | Email organization |
| email_rules | Automated email rules |
| email_settings | User email preferences |

## 🌐 API Endpoints

### OAuth Authentication
- `GET /api/email/oauth/authorize/{userId}` - Get OAuth URL
- `GET /api/email/oauth/callback` - Handle OAuth callback

### Account Management
- `GET /api/email/accounts/{userId}` - Get user's email accounts
- `GET /api/email/accounts/details/{accountId}` - Get account details
- `DELETE /api/email/accounts/{accountId}` - Remove account
- `PUT /api/email/accounts/{accountId}/primary` - Set primary account

### Email Operations
- `POST /api/email/send` - Send single email
- `POST /api/email/send/bulk` - Send bulk emails
- `POST /api/email/send/template/{templateId}` - Send template email
- `GET /api/email/messages` - Get email list
- `GET /api/email/messages/{messageId}` - Get specific email

### Templates
- `GET /api/email/templates` - Get all templates
- `GET /api/email/templates/{templateId}` - Get specific template
- `POST /api/email/templates` - Create template
- `PUT /api/email/templates/{templateId}` - Update template
- `DELETE /api/email/templates/{templateId}` - Delete template

### Campaigns
- `GET /api/email/campaigns` - Get all campaigns
- `POST /api/email/campaigns` - Create campaign
- `PUT /api/email/campaigns/{campaignId}` - Update campaign
- `DELETE /api/email/campaigns/{campaignId}` - Delete campaign
- `POST /api/email/campaigns/{campaignId}/send` - Send campaign

### Tracking
- `GET /api/email/tracking/{messageId}` - Get email tracking data
- `GET /api/email/tracking/pixel/{trackingId}` - Tracking pixel endpoint
- `GET /api/email/tracking/redirect/{trackingId}` - Link redirect tracking

## 🔧 Usage Examples

### 1. Connect Gmail Account
```javascript
// Get OAuth URL
const response = await fetch('/api/email/oauth/authorize/user123');
const { authUrl } = await response.json();

// Redirect user to authUrl for Gmail authorization
window.location.href = authUrl;
```

### 2. Send Email
```javascript
const emailData = {
  to: ["recipient@example.com"],
  subject: "Test Email",
  body: "<h1>Hello World!</h1>",
  isHtml: true
};

const response = await fetch('/api/email/send', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(emailData)
});
```

### 3. Send Bulk Email
```javascript
const bulkData = {
  recipients: [
    { email: "user1@example.com", name: "User 1" },
    { email: "user2@example.com", name: "User 2" }
  ],
  subject: "Newsletter",
  body: "Hello {{name}}, check out our latest updates!",
  isHtml: false
};

const response = await fetch('/api/email/send/bulk', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(bulkData)
});
```

### 4. Create Email Template
```javascript
const templateData = {
  name: "Welcome Email",
  subject: "Welcome to {{companyName}}!",
  body: "<h1>Welcome {{userName}}!</h1><p>Thank you for joining {{companyName}}.</p>",
  isHtml: true,
  variables: ["userName", "companyName"]
};

const response = await fetch('/api/email/templates', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(templateData)
});
```

## 🎯 Features

### ✅ Implemented
- OAuth 2.0 Gmail authentication
- Send single and bulk emails
- Email templates with variables
- Email campaigns
- Open and click tracking
- Background email queue processing
- Account management
- Email history and search
- Attachment support
- Email signatures

### 🔄 Queue Processing
The system includes a background service that processes emails in a queue:
- Retry failed emails with exponential backoff
- Rate limiting to respect Gmail API limits
- Email delivery status tracking
- Error logging and reporting

### 📈 Tracking Features
- **Open Tracking**: 1x1 pixel image tracks when emails are opened
- **Click Tracking**: Link redirects track when links are clicked
- **Delivery Status**: Track sent, delivered, bounced, failed states
- **Campaign Analytics**: Aggregate statistics for email campaigns

## 🔒 Security Features
- OAuth 2.0 secure authentication
- Token refresh handling
- Rate limiting protection
- Input validation and sanitization
- SQL injection prevention with parameterized queries

## 🐛 Error Handling
- Comprehensive try-catch blocks
- Detailed error logging
- User-friendly error messages
- Retry mechanisms for transient failures

## 📝 Logging
- Structured logging with Serilog compatibility
- Email send/receive events
- OAuth authentication events
- Error and warning logs
- Performance monitoring

## 🧪 Testing
To test the email system:

1. **Setup Test Environment**
   - Use Gmail test account
   - Configure OAuth with test credentials
   - Point to test database

2. **Test OAuth Flow**
   - Call authorize endpoint
   - Complete OAuth in browser
   - Verify account is saved

3. **Test Email Sending**
   - Send test email to yourself
   - Verify delivery and tracking
   - Check database records

## 📞 Support
For issues or questions:
1. Check the logs in `Services/Implementation/GmailService.cs`
2. Verify OAuth credentials are correctly configured
3. Ensure Gmail API is enabled in Google Cloud Console
4. Check PostgreSQL connection and permissions

## 🔄 Maintenance
- Monitor Gmail API quotas and limits
- Regular cleanup of old email records
- Token refresh monitoring
- Database performance optimization
- Log file rotation

---

**Note**: This email system is production-ready with comprehensive error handling, security features, and scalability considerations. The implementation follows best practices for OAuth 2.0 integration and email processing.
