# Email System Integration Examples for ERP

## 🏢 Real ERP Workflow Examples

### 1. Customer Onboarding Email Flow

When a new customer is created in your ERP system:

```csharp
// In your CustomerService.cs
public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
{
    // 1. Create customer in database
    var customer = await _customerRepository.CreateAsync(request);
    
    // 2. Send welcome email
    var emailRequest = new SendEmailRequest
    {
        To = new[] { customer.Email },
        Subject = $"Welcome to {_companyName}, {customer.Name}!",
        Body = $@"
            <h2>Welcome {customer.Name}!</h2>
            <p>Your customer account has been created successfully.</p>
            <p><strong>Customer ID:</strong> {customer.Id}</p>
            <p><strong>Account Manager:</strong> {customer.AccountManager}</p>
            <p>You can access your account at: <a href='{_portalUrl}'>Customer Portal</a></p>
        ",
        IsHtml = true,
        TrackOpens = true,
        TrackClicks = true
    };
    
    await _emailHelper.SendCustomerWelcomeEmailAsync(customer.Id, emailRequest);
    
    return customer;
}
```

### 2. Order Confirmation Emails

```csharp
// In your OrderService.cs
public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
{
    var order = await _orderRepository.CreateAsync(request);
    
    // Send order confirmation email
    var emailRequest = new SendEmailRequest
    {
        To = new[] { order.CustomerEmail },
        Subject = $"Order Confirmation #{order.OrderNumber}",
        Body = GenerateOrderConfirmationHtml(order),
        IsHtml = true,
        Attachments = new[]
        {
            new EmailAttachment
            {
                FileName = $"Order_{order.OrderNumber}.pdf",
                MimeType = "application/pdf",
                Content = await _pdfService.GenerateOrderPdfAsync(order.Id)
            }
        }
    };
    
    await _gmailService.SendEmailAsync(emailRequest, order.CreatedByUserId);
    
    return order;
}

private string GenerateOrderConfirmationHtml(Order order)
{
    var itemsHtml = string.Join("", order.Items.Select(item => 
        $"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>${item.UnitPrice:F2}</td><td>${item.Total:F2}</td></tr>"));
    
    return $@"
        <html>
        <body>
            <h2>Order Confirmation</h2>
            <p>Dear {order.CustomerName},</p>
            <p>Thank you for your order! Here are the details:</p>
            
            <h3>Order #{order.OrderNumber}</h3>
            <p><strong>Order Date:</strong> {order.OrderDate:yyyy-MM-dd}</p>
            <p><strong>Expected Delivery:</strong> {order.ExpectedDelivery:yyyy-MM-dd}</p>
            
            <table border='1' style='border-collapse: collapse; width: 100%;'>
                <tr style='background-color: #f2f2f2;'>
                    <th>Product</th><th>Qty</th><th>Unit Price</th><th>Total</th>
                </tr>
                {itemsHtml}
                <tr style='font-weight: bold;'>
                    <td colspan='3'>Total Amount:</td>
                    <td>${order.TotalAmount:F2}</td>
                </tr>
            </table>
            
            <p>If you have any questions, contact us at support@company.com</p>
            <p>Best regards,<br/>Sales Team</p>
        </body>
        </html>";
}
```

### 3. Invoice Generation and Email

```csharp
// In your InvoiceService.cs
public async Task<InvoiceDto> GenerateAndSendInvoiceAsync(int orderId)
{
    var order = await _orderRepository.GetByIdAsync(orderId);
    var invoice = await _invoiceRepository.CreateFromOrderAsync(order);
    
    // Generate PDF
    var invoicePdf = await _pdfService.GenerateInvoicePdfAsync(invoice.Id);
    
    // Send invoice email using template
    var templateRequest = new SendTemplateEmailRequest
    {
        To = new[] { order.CustomerEmail },
        Variables = new Dictionary<string, string>
        {
            {"customerName", order.CustomerName},
            {"invoiceNumber", invoice.InvoiceNumber},
            {"invoiceDate", invoice.InvoiceDate.ToString("yyyy-MM-dd")},
            {"dueDate", invoice.DueDate.ToString("yyyy-MM-dd")},
            {"totalAmount", invoice.TotalAmount.ToString("C")},
            {"paymentLink", $"{_portalUrl}/payments/{invoice.Id}"}
        },
        Attachments = new[]
        {
            new EmailAttachment
            {
                FileName = $"Invoice_{invoice.InvoiceNumber}.pdf",
                MimeType = "application/pdf",
                Content = Convert.ToBase64String(invoicePdf)
            }
        },
        TrackOpens = true,
        TrackClicks = true
    };
    
    await _gmailService.SendTemplateEmailAsync("invoice_template", templateRequest, invoice.CreatedByUserId);
    
    return invoice;
}
```

### 4. Payment Reminder Campaign

```csharp
// In your PaymentReminderService.cs
public async Task SendPaymentRemindersAsync()
{
    // Get overdue invoices
    var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();
    
    var recipients = overdueInvoices.Select(invoice => new BulkEmailRecipient
    {
        Email = invoice.CustomerEmail,
        Name = invoice.CustomerName,
        CustomData = new Dictionary<string, string>
        {
            {"invoiceNumber", invoice.InvoiceNumber},
            {"dueDate", invoice.DueDate.ToString("yyyy-MM-dd")},
            {"amount", invoice.TotalAmount.ToString("C")},
            {"daysOverdue", (DateTime.Now - invoice.DueDate).Days.ToString()},
            {"paymentLink", $"{_portalUrl}/payments/{invoice.Id}"}
        }
    }).ToList();
    
    var bulkRequest = new BulkEmailRequest
    {
        Recipients = recipients,
        Subject = "Payment Reminder - Invoice {{invoiceNumber}}",
        Body = @"
            Dear {{name}},
            
            This is a friendly reminder that your invoice {{invoiceNumber}} 
            was due on {{dueDate}} ({{daysOverdue}} days ago).
            
            Amount Due: {{amount}}
            
            Please make your payment as soon as possible using this link:
            {{paymentLink}}
            
            If you have already made the payment, please disregard this message.
            
            Best regards,
            Accounts Receivable Team
        ",
        IsHtml = false,
        TrackOpens = true
    };
    
    await _gmailService.SendBulkEmailAsync(bulkRequest, "system_user");
}
```

### 5. Sales Quotation Follow-up

```csharp
// In your QuotationService.cs
public async Task SendQuotationFollowUpAsync(int quotationId)
{
    var quotation = await _quotationRepository.GetByIdAsync(quotationId);
    
    // Check if it's been 3 days since last follow-up
    var lastFollowUp = await _emailRepository.GetLastFollowUpAsync(quotationId);
    if (lastFollowUp != null && (DateTime.Now - lastFollowUp.SentAt).Days < 3)
        return;
    
    var emailRequest = new SendEmailRequest
    {
        To = new[] { quotation.CustomerEmail },
        Subject = $"Follow-up: Quotation #{quotation.QuotationNumber}",
        Body = $@"
            <h2>Following up on your quotation</h2>
            <p>Dear {quotation.CustomerName},</p>
            
            <p>We wanted to follow up on the quotation we sent you on {quotation.CreatedDate:yyyy-MM-dd}.</p>
            
            <h3>Quotation Summary:</h3>
            <ul>
                <li><strong>Quotation #:</strong> {quotation.QuotationNumber}</li>
                <li><strong>Total Amount:</strong> {quotation.TotalAmount:C}</li>
                <li><strong>Valid Until:</strong> {quotation.ValidUntil:yyyy-MM-dd}</li>
            </ul>
            
            <p>Do you have any questions about our proposal? We'd be happy to discuss:</p>
            <ul>
                <li>Product specifications</li>
                <li>Pricing options</li>
                <li>Delivery timelines</li>
                <li>Payment terms</li>
            </ul>
            
            <p>
                <a href='{_portalUrl}/quotations/{quotation.Id}' 
                   style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                   View Quotation Online
                </a>
            </p>
            
            <p>Best regards,<br/>
            {quotation.SalesPersonName}<br/>
            Sales Team<br/>
            Phone: {quotation.SalesPersonPhone}<br/>
            Email: {quotation.SalesPersonEmail}</p>
        ",
        IsHtml = true,
        TrackOpens = true,
        TrackClicks = true
    };
    
    await _gmailService.SendEmailAsync(emailRequest, quotation.SalesPersonId);
}
```

### 6. Inventory Low Stock Alerts

```csharp
// In your InventoryService.cs (Background Service)
public async Task CheckLowStockAndNotifyAsync()
{
    var lowStockItems = await _inventoryRepository.GetLowStockItemsAsync();
    
    if (!lowStockItems.Any()) return;
    
    // Group by category for better organization
    var groupedItems = lowStockItems.GroupBy(item => item.Category);
    
    var emailBody = "<h2>Low Stock Alert</h2>";
    
    foreach (var group in groupedItems)
    {
        emailBody += $"<h3>{group.Key}</h3><ul>";
        foreach (var item in group)
        {
            emailBody += $"<li><strong>{item.Name}</strong> - Current: {item.CurrentStock}, Minimum: {item.MinimumStock}</li>";
        }
        emailBody += "</ul>";
    }
    
    // Get all purchasing managers
    var purchasingManagers = await _userRepository.GetUsersByRoleAsync("PurchasingManager");
    
    var emailRequest = new SendEmailRequest
    {
        To = purchasingManagers.Select(u => u.Email).ToArray(),
        Subject = $"Low Stock Alert - {lowStockItems.Count} items need attention",
        Body = emailBody,
        IsHtml = true,
        Priority = "high"
    };
    
    await _gmailService.SendEmailAsync(emailRequest, "system_user");
}
```

### 7. Monthly Sales Report Email

```csharp
// In your ReportingService.cs
public async Task SendMonthlySalesReportAsync()
{
    var salesData = await _salesRepository.GetMonthlySalesDataAsync();
    var reportPdf = await _reportService.GenerateSalesReportPdfAsync(salesData);
    
    // Get all managers
    var managers = await _userRepository.GetUsersByRoleAsync("Manager");
    
    var templateRequest = new SendTemplateEmailRequest
    {
        To = managers.Select(m => m.Email).ToArray(),
        Variables = new Dictionary<string, string>
        {
            {"month", DateTime.Now.AddMonths(-1).ToString("MMMM yyyy")},
            {"totalSales", salesData.TotalSales.ToString("C")},
            {"totalOrders", salesData.TotalOrders.ToString()},
            {"topProduct", salesData.TopSellingProduct},
            {"growthPercentage", salesData.GrowthPercentage.ToString("F1")}
        },
        Attachments = new[]
        {
            new EmailAttachment
            {
                FileName = $"Sales_Report_{DateTime.Now:yyyy_MM}.pdf",
                MimeType = "application/pdf",
                Content = Convert.ToBase64String(reportPdf)
            }
        }
    };
    
    await _gmailService.SendTemplateEmailAsync("monthly_sales_report", templateRequest, "system_user");
}
```

## 🔧 Helper Service Integration

Create a helper service to simplify email operations:

```csharp
// EmailIntegrationHelper.cs (already created)
public class EmailIntegrationHelper
{
    private readonly IGmailService _gmailService;
    private readonly ILogger<EmailIntegrationHelper> _logger;
    
    public async Task<bool> SendCustomerWelcomeEmailAsync(string customerId, SendEmailRequest request)
    {
        try
        {
            var response = await _gmailService.SendEmailAsync(request, "system_user");
            _logger.LogInformation($"Welcome email sent to customer {customerId}: {response.MessageId}");
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email to customer {customerId}");
            return false;
        }
    }
    
    public async Task<bool> SendOrderConfirmationAsync(string orderId, Order order)
    {
        // Implementation here
    }
    
    // Add more helper methods for common email scenarios
}
```

## 📅 Scheduled Email Tasks

Add to your background service:

```csharp
// In your BackgroundTaskService.cs
public class ScheduledEmailService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Daily tasks at 9 AM
                if (DateTime.Now.Hour == 9 && DateTime.Now.Minute == 0)
                {
                    await SendDailyRemindersAsync();
                    await CheckLowStockAsync();
                }
                
                // Weekly reports on Monday at 8 AM
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour == 8)
                {
                    await SendWeeklyReportsAsync();
                }
                
                // Monthly reports on 1st of month
                if (DateTime.Now.Day == 1 && DateTime.Now.Hour == 9)
                {
                    await SendMonthlyReportsAsync();
                }
                
                // Wait 1 minute before next check
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled email service");
            }
        }
    }
}
```

This integration guide shows how to seamlessly incorporate the email system into your existing ERP workflows for maximum efficiency and automation!
