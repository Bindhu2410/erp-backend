# Email System - Quick Request Body Reference

## 🚀 Quick Start - Most Common Operations

### 1. Send Simple Email
```json
POST /api/email/send
{
  "to": ["recipient@example.com"],
  "subject": "Hello from ERP",
  "body": "Your message here",
  "isHtml": false
}
```

### 2. Send HTML Email with Tracking
```json
POST /api/email/send
{
  "to": ["client@company.com"],
  "cc": ["manager@company.com"],
  "subject": "Invoice #12345",
  "body": "<h2>Invoice</h2><p>Please find your invoice attached.</p>",
  "isHtml": true,
  "trackOpens": true,
  "trackClicks": true,
  "attachments": [
    {
      "fileName": "invoice.pdf",
      "mimeType": "application/pdf",
      "content": "base64-content-here"
    }
  ]
}
```

### 3. Create Email Template
```json
POST /api/email/templates
{
  "name": "Welcome Email",
  "subject": "Welcome {{customerName}}!",
  "body": "<h1>Hello {{customerName}}!</h1><p>Welcome to {{companyName}}.</p>",
  "isHtml": true,
  "variables": ["customerName", "companyName"]
}
```

### 4. Send Template Email
```json
POST /api/email/send/template/{templateId}
{
  "to": ["customer@example.com"],
  "variables": {
    "customerName": "John Doe",
    "companyName": "ABC Corp"
  }
}
```

### 5. Send Bulk Emails
```json
POST /api/email/send/bulk
{
  "recipients": [
    {
      "email": "user1@example.com",
      "name": "User 1",
      "customData": {"orderId": "001"}
    },
    {
      "email": "user2@example.com", 
      "name": "User 2",
      "customData": {"orderId": "002"}
    }
  ],
  "subject": "Order {{orderId}} Confirmed",
  "body": "Hello {{name}}, your order {{orderId}} is confirmed!",
  "isHtml": false
}
```

### 6. Create Campaign
```json
POST /api/email/campaigns
{
  "name": "Summer Sale",
  "subject": "🌞 Summer Sale - 50% Off!",
  "templateId": "tpl_123",
  "scheduleTime": "2025-08-15T09:00:00Z",
  "settings": {
    "trackOpens": true,
    "trackClicks": true
  }
}
```

## 📋 Common Headers
```http
Authorization: Bearer your-jwt-token
Content-Type: application/json
X-User-ID: user123
```

## 🔗 API Endpoints Summary
- `GET /api/email/oauth/authorize/{userId}` - Get OAuth URL
- `POST /api/email/send` - Send email
- `POST /api/email/send/bulk` - Bulk send
- `POST /api/email/templates` - Create template
- `POST /api/email/send/template/{id}` - Send template
- `GET /api/email/messages` - Get email list
- `GET /api/email/tracking/{messageId}` - Get tracking

## ⚡ Quick Test
```bash
# Test email send
curl -X POST "https://your-domain.com/api/email/send" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"to":["test@example.com"],"subject":"Test","body":"Hello"}'
```
