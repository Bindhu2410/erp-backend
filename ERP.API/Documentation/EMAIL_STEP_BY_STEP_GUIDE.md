# Email System - Step-by-Step Usage Guide with Request Bodies

## 🚀 Getting Started

This guide provides complete step-by-step instructions with actual request bodies for using the email system in your ERP project.

## Prerequisites

1. ✅ Email system is installed and configured
2. ✅ Gmail OAuth credentials are set up
3. ✅ Database tables are created
4. ✅ API is running

---

## Step 1: Connect Gmail Account (OAuth Setup)

### 1.1 Get OAuth Authorization URL

**Endpoint:** `GET /api/email/oauth/authorize/{userId}`

**Example Request:**
```http
GET /api/email/oauth/authorize/user123
Content-Type: application/json
```

**Response:**
```json
{
  "success": true,
  "data": {
    "authUrl": "https://accounts.google.com/oauth/authorize?client_id=xxx&redirect_uri=xxx&scope=https://www.googleapis.com/auth/gmail.send&response_type=code&state=user123"
  },
  "message": "OAuth URL generated successfully"
}
```

### 1.2 Handle OAuth Callback (Automatic)

**Endpoint:** `GET /api/email/oauth/callback`

This endpoint is called automatically by Google after user authorization.

**Example URL:**
```
https://your-domain.com/api/email/oauth/callback?code=4/0AX4XfWj...&state=user123
```

**Response:**
```json
{
  "success": true,
  "message": "Gmail account connected successfully"
}
```

---

## Step 2: Send Your First Email

### 2.1 Send Simple Email

**Endpoint:** `POST /api/email/send`

**Request Body:**
```json
{
  "to": ["recipient@example.com"],
  "subject": "Hello from ERP System",
  "body": "This is a test email from our ERP system.",
  "isHtml": false,
  "accountId": null
}
```

**cURL Example:**
```bash
curl -X POST "https://your-domain.com/api/email/send" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-jwt-token" \
  -d '{
    "to": ["john.doe@example.com"],
    "subject": "Welcome to Our ERP System",
    "body": "Hello John, welcome to our system!",
    "isHtml": false
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "messageId": "msg_001234567890",
    "status": "sent",
    "timestamp": "2025-08-14T10:30:00Z",
    "recipients": ["john.doe@example.com"]
  },
  "message": "Email sent successfully"
}
```

### 2.2 Send HTML Email with Attachments

**Request Body:**
```json
{
  "to": ["client@company.com"],
  "cc": ["manager@company.com"],
  "bcc": ["archive@company.com"],
  "subject": "Invoice #12345 - Payment Due",
  "body": "<html><body><h2>Invoice Details</h2><p>Dear Client,</p><p>Please find your invoice attached.</p><table border='1'><tr><th>Item</th><th>Amount</th></tr><tr><td>Service</td><td>$500.00</td></tr></table><p>Best regards,<br/>Accounting Team</p></body></html>",
  "isHtml": true,
  "attachments": [
    {
      "fileName": "invoice_12345.pdf",
      "mimeType": "application/pdf",
      "content": "base64-encoded-pdf-content-here"
    }
  ],
  "priority": "high",
  "trackOpens": true,
  "trackClicks": true
}
```

---

## Step 3: Create and Use Email Templates

### 3.1 Create Email Template

**Endpoint:** `POST /api/email/templates`

**Request Body:**
```json
{
  "name": "Customer Welcome Email",
  "subject": "Welcome to {{companyName}}, {{customerName}}!",
  "body": "<html><body><h1>Welcome {{customerName}}!</h1><p>Thank you for choosing {{companyName}}. Your account has been created successfully.</p><p><strong>Account Details:</strong></p><ul><li>Customer ID: {{customerId}}</li><li>Email: {{customerEmail}}</li><li>Registration Date: {{registrationDate}}</li></ul><p>If you have any questions, contact us at {{supportEmail}}.</p><p>Best regards,<br/>{{companyName}} Team</p></body></html>",
  "isHtml": true,
  "variables": [
    "customerName",
    "companyName", 
    "customerId",
    "customerEmail",
    "registrationDate",
    "supportEmail"
  ],
  "category": "customer_onboarding",
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "tpl_001234567890",
    "name": "Customer Welcome Email",
    "createdAt": "2025-08-14T10:30:00Z"
  },
  "message": "Template created successfully"
}
```

### 3.2 Send Email Using Template

**Endpoint:** `POST /api/email/send/template/{templateId}`

**Request Body:**
```json
{
  "to": ["newcustomer@example.com"],
  "variables": {
    "customerName": "John Smith",
    "companyName": "ABC Corporation",
    "customerId": "CUST001234",
    "customerEmail": "newcustomer@example.com",
    "registrationDate": "August 14, 2025",
    "supportEmail": "support@abccorp.com"
  },
  "trackOpens": true,
  "trackClicks": true
}
```

---

## Step 4: Bulk Email Operations

### 4.1 Send Bulk Emails

**Endpoint:** `POST /api/email/send/bulk`

**Request Body:**
```json
{
  "recipients": [
    {
      "email": "customer1@example.com",
      "name": "Alice Johnson",
      "customData": {
        "orderId": "ORD001",
        "amount": "250.00"
      }
    },
    {
      "email": "customer2@example.com", 
      "name": "Bob Wilson",
      "customData": {
        "orderId": "ORD002",
        "amount": "175.50"
      }
    },
    {
      "email": "customer3@example.com",
      "name": "Carol Davis",
      "customData": {
        "orderId": "ORD003",
        "amount": "320.75"
      }
    }
  ],
  "subject": "Order Confirmation - {{orderId}}",
  "body": "Dear {{name}},\n\nYour order {{orderId}} has been confirmed.\nTotal Amount: ${{amount}}\n\nThank you for your business!\n\nBest regards,\nSales Team",
  "isHtml": false,
  "scheduleTime": null,
  "trackOpens": true,
  "trackClicks": false
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "batchId": "batch_001234567890",
    "totalRecipients": 3,
    "emailsSent": 3,
    "emailsFailed": 0,
    "results": [
      {
        "email": "customer1@example.com",
        "messageId": "msg_001234567891",
        "status": "sent"
      },
      {
        "email": "customer2@example.com",
        "messageId": "msg_001234567892", 
        "status": "sent"
      },
      {
        "email": "customer3@example.com",
        "messageId": "msg_001234567893",
        "status": "sent"
      }
    ]
  },
  "message": "Bulk emails sent successfully"
}
```

---

## Step 5: Email Campaign Management

### 5.1 Create Email Campaign

**Endpoint:** `POST /api/email/campaigns`

**Request Body:**
```json
{
  "name": "Summer Sale 2025",
  "subject": "🌞 Summer Sale - Up to 50% Off Everything!",
  "templateId": "tpl_001234567890",
  "recipientLists": [
    "premium_customers",
    "newsletter_subscribers"
  ],
  "scheduleTime": "2025-08-15T09:00:00Z",
  "settings": {
    "trackOpens": true,
    "trackClicks": true,
    "enableUnsubscribe": true,
    "sendTimeOptimization": true
  },
  "content": {
    "preheader": "Don't miss out on our biggest sale of the year!",
    "customVariables": {
      "saleEndDate": "August 31, 2025",
      "discountCode": "SUMMER50",
      "websiteUrl": "https://ourstore.com/sale"
    }
  }
}
```

### 5.2 Send Campaign

**Endpoint:** `POST /api/email/campaigns/{campaignId}/send`

**Request Body:**
```json
{
  "testMode": false,
  "testEmails": [],
  "confirmSend": true
}
```

---

## Step 6: Retrieve and Manage Emails

### 6.1 Get Email List

**Endpoint:** `GET /api/email/messages`

**Query Parameters Example:**
```
/api/email/messages?userId=user123&folder=sent&limit=10&offset=0&startDate=2025-08-01&endDate=2025-08-14
```

**Response:**
```json
{
  "success": true,
  "data": {
    "emails": [
      {
        "id": "msg_001234567890",
        "subject": "Welcome to ABC Corporation, John Smith!",
        "from": "noreply@abccorp.com",
        "to": ["newcustomer@example.com"],
        "sentAt": "2025-08-14T10:30:00Z",
        "status": "delivered",
        "opens": 1,
        "clicks": 2,
        "templateUsed": "Customer Welcome Email"
      }
    ],
    "totalCount": 156,
    "hasMore": true
  }
}
```

### 6.2 Get Specific Email Details

**Endpoint:** `GET /api/email/messages/{messageId}`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "msg_001234567890",
    "subject": "Welcome to ABC Corporation, John Smith!",
    "from": "noreply@abccorp.com",
    "to": ["newcustomer@example.com"],
    "cc": [],
    "bcc": [],
    "body": "<html>...</html>",
    "sentAt": "2025-08-14T10:30:00Z",
    "deliveredAt": "2025-08-14T10:30:15Z",
    "status": "delivered",
    "trackingData": {
      "opens": 1,
      "clicks": 2,
      "lastOpenedAt": "2025-08-14T11:15:00Z",
      "clickEvents": [
        {
          "url": "https://abccorp.com/login",
          "clickedAt": "2025-08-14T11:16:00Z"
        }
      ]
    },
    "attachments": []
  }
}
```

---

## Step 7: Email Tracking and Analytics

### 7.1 Get Email Tracking Data

**Endpoint:** `GET /api/email/tracking/{messageId}`

**Response:**
```json
{
  "success": true,
  "data": {
    "messageId": "msg_001234567890",
    "trackingEvents": [
      {
        "eventType": "sent",
        "timestamp": "2025-08-14T10:30:00Z",
        "details": "Email sent successfully"
      },
      {
        "eventType": "delivered", 
        "timestamp": "2025-08-14T10:30:15Z",
        "details": "Email delivered to recipient"
      },
      {
        "eventType": "opened",
        "timestamp": "2025-08-14T11:15:00Z",
        "userAgent": "Mozilla/5.0...",
        "ipAddress": "192.168.1.100"
      },
      {
        "eventType": "clicked",
        "timestamp": "2025-08-14T11:16:00Z",
        "url": "https://abccorp.com/login",
        "userAgent": "Mozilla/5.0...",
        "ipAddress": "192.168.1.100"
      }
    ],
    "summary": {
      "totalOpens": 1,
      "totalClicks": 2,
      "firstOpenedAt": "2025-08-14T11:15:00Z",
      "lastActivityAt": "2025-08-14T11:16:00Z"
    }
  }
}
```

---

## Step 8: Account Management

### 8.1 Get Connected Email Accounts

**Endpoint:** `GET /api/email/accounts/{userId}`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "acc_001234567890",
      "email": "noreply@abccorp.com",
      "displayName": "ABC Corporation",
      "isPrimary": true,
      "isActive": true,
      "connectedAt": "2025-08-14T09:00:00Z",
      "lastUsed": "2025-08-14T10:30:00Z"
    },
    {
      "id": "acc_001234567891", 
      "email": "support@abccorp.com",
      "displayName": "ABC Support Team",
      "isPrimary": false,
      "isActive": true,
      "connectedAt": "2025-08-13T14:30:00Z",
      "lastUsed": "2025-08-14T08:45:00Z"
    }
  ]
}
```

### 8.2 Set Primary Email Account

**Endpoint:** `PUT /api/email/accounts/{accountId}/primary`

**Request Body:**
```json
{
  "isPrimary": true
}
```

---

## Common Request Body Patterns

### Error Handling Response
```json
{
  "success": false,
  "error": {
    "code": "INVALID_EMAIL",
    "message": "The email address 'invalid-email' is not valid",
    "details": [
      "Email must contain @ symbol",
      "Domain part is required"
    ]
  }
}
```

### Pagination Parameters
```json
{
  "page": 1,
  "limit": 25,
  "sortBy": "sentAt",
  "sortOrder": "desc",
  "filters": {
    "status": "delivered",
    "dateRange": {
      "start": "2025-08-01T00:00:00Z",
      "end": "2025-08-14T23:59:59Z"
    }
  }
}
```

---

## Authentication Headers

All API requests require authentication. Include these headers:

```http
Authorization: Bearer your-jwt-token
Content-Type: application/json
X-User-ID: user123
```

---

## Testing with Postman/Thunder Client

### 1. Import Collection
Create a new collection with these requests:

### 2. Environment Variables
```json
{
  "baseUrl": "https://your-domain.com/api",
  "userId": "user123",
  "authToken": "your-jwt-token"
}
```

### 3. Test Sequence
1. Connect Gmail account (OAuth)
2. Send simple email
3. Create template
4. Send template email
5. Check tracking data

---

## Production Considerations

### Rate Limiting
- Gmail API: 1 billion quota units per day
- Recommended: 100 emails per minute
- Use queue system for bulk operations

### Error Codes
- `400` - Bad Request (invalid data)
- `401` - Unauthorized (missing/invalid token)
- `403` - Forbidden (insufficient permissions)
- `429` - Too Many Requests (rate limited)
- `500` - Internal Server Error

### Best Practices
1. Always validate email addresses
2. Use templates for consistent branding
3. Enable tracking for analytics
4. Handle bounces and unsubscribes
5. Monitor delivery rates
6. Keep attachments under 25MB
7. Use meaningful subject lines
8. Test emails before bulk sending

---

**Need Help?** Check the logs in `Services/Implementation/GmailService.cs` or contact the development team.
