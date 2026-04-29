-- Email System Database Tables for Gmail API Integration
-- Created: 2025-08-14

-- 1. Email Accounts Table - Store OAuth credentials and account details
CREATE TABLE IF NOT EXISTS email_accounts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    email_address VARCHAR(255) NOT NULL UNIQUE,
    display_name VARCHAR(255),
    access_token TEXT,
    refresh_token TEXT,
    token_expiry TIMESTAMP,
    is_active BOOLEAN DEFAULT true,
    is_primary BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,
    updated_by INTEGER
);

-- 2. Email Templates Table - Store reusable email templates
CREATE TABLE IF NOT EXISTS email_templates (
    id SERIAL PRIMARY KEY,
    template_name VARCHAR(255) NOT NULL,
    subject VARCHAR(500) NOT NULL,
    body_html TEXT,
    body_text TEXT,
    template_type VARCHAR(100), -- 'lead_followup', 'quotation', 'invoice', 'general'
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,
    updated_by INTEGER
);

-- 3. Email Campaigns Table - Manage email campaigns
CREATE TABLE IF NOT EXISTS email_campaigns (
    id SERIAL PRIMARY KEY,
    campaign_name VARCHAR(255) NOT NULL,
    campaign_description TEXT,
    template_id INTEGER REFERENCES email_templates(id),
    sender_email_account_id INTEGER REFERENCES email_accounts(id),
    status VARCHAR(50) DEFAULT 'draft', -- 'draft', 'scheduled', 'active', 'completed', 'paused'
    scheduled_at TIMESTAMP,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    total_recipients INTEGER DEFAULT 0,
    sent_count INTEGER DEFAULT 0,
    delivered_count INTEGER DEFAULT 0,
    opened_count INTEGER DEFAULT 0,
    clicked_count INTEGER DEFAULT 0,
    bounced_count INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,
    updated_by INTEGER
);

-- 4. Email Messages Table - Store all email messages
CREATE TABLE IF NOT EXISTS email_messages (
    id SERIAL PRIMARY KEY,
    gmail_message_id VARCHAR(255), -- Gmail's unique message ID
    gmail_thread_id VARCHAR(255), -- Gmail's thread ID
    sender_email_account_id INTEGER REFERENCES email_accounts(id),
    campaign_id INTEGER REFERENCES email_campaigns(id),
    message_type VARCHAR(50) DEFAULT 'outbound', -- 'outbound', 'inbound', 'reply'
    subject VARCHAR(500),
    body_html TEXT,
    body_text TEXT,
    from_email VARCHAR(255) NOT NULL,
    from_name VARCHAR(255),
    to_emails TEXT NOT NULL, -- JSON array of email addresses
    cc_emails TEXT, -- JSON array of email addresses
    bcc_emails TEXT, -- JSON array of email addresses
    reply_to VARCHAR(255),
    status VARCHAR(50) DEFAULT 'draft', -- 'draft', 'queued', 'sent', 'delivered', 'failed', 'bounced'
    priority VARCHAR(20) DEFAULT 'normal', -- 'low', 'normal', 'high'
    scheduled_at TIMESTAMP,
    sent_at TIMESTAMP,
    delivered_at TIMESTAMP,
    error_message TEXT,
    retry_count INTEGER DEFAULT 0,
    max_retries INTEGER DEFAULT 3,
    related_entity_type VARCHAR(100), -- 'lead', 'deal', 'customer', 'order', 'invoice'
    related_entity_id INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,
    updated_by INTEGER
);

-- 5. Email Attachments Table - Store email attachments metadata
CREATE TABLE IF NOT EXISTS email_attachments (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES email_messages(id) ON DELETE CASCADE,
    filename VARCHAR(255) NOT NULL,
    file_size BIGINT,
    mime_type VARCHAR(100),
    file_path VARCHAR(500), -- Local file path or cloud storage URL
    gmail_attachment_id VARCHAR(255), -- Gmail's attachment ID
    is_inline BOOLEAN DEFAULT false,
    content_id VARCHAR(255), -- For inline attachments
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 6. Email Recipients Table - Track individual recipients and their interactions
CREATE TABLE IF NOT EXISTS email_recipients (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES email_messages(id) ON DELETE CASCADE,
    campaign_id INTEGER REFERENCES email_campaigns(id),
    recipient_email VARCHAR(255) NOT NULL,
    recipient_name VARCHAR(255),
    recipient_type VARCHAR(20) DEFAULT 'to', -- 'to', 'cc', 'bcc'
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'sent', 'delivered', 'opened', 'clicked', 'bounced', 'failed'
    sent_at TIMESTAMP,
    delivered_at TIMESTAMP,
    opened_at TIMESTAMP,
    last_opened_at TIMESTAMP,
    open_count INTEGER DEFAULT 0,
    clicked_at TIMESTAMP,
    last_clicked_at TIMESTAMP,
    click_count INTEGER DEFAULT 0,
    bounced_at TIMESTAMP,
    bounce_reason TEXT,
    unsubscribed_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 7. Email Tracking Events Table - Track detailed email events
CREATE TABLE IF NOT EXISTS email_tracking_events (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES email_messages(id) ON DELETE CASCADE,
    recipient_id INTEGER REFERENCES email_recipients(id) ON DELETE CASCADE,
    event_type VARCHAR(50) NOT NULL, -- 'sent', 'delivered', 'opened', 'clicked', 'bounced', 'unsubscribed'
    event_data JSONB, -- Additional event data
    user_agent TEXT,
    ip_address INET,
    device_type VARCHAR(50),
    operating_system VARCHAR(100),
    browser VARCHAR(100),
    location_data JSONB, -- Geographic data
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 8. Email Queue Table - Queue for processing emails
CREATE TABLE IF NOT EXISTS email_queue (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES email_messages(id) ON DELETE CASCADE,
    priority INTEGER DEFAULT 5, -- 1-10, where 1 is highest priority
    attempts INTEGER DEFAULT 0,
    max_attempts INTEGER DEFAULT 3,
    next_attempt_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_error TEXT,
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'processing', 'completed', 'failed'
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 9. Email Folders Table - Organize emails in folders
CREATE TABLE IF NOT EXISTS email_folders (
    id SERIAL PRIMARY KEY,
    email_account_id INTEGER REFERENCES email_accounts(id) ON DELETE CASCADE,
    folder_name VARCHAR(255) NOT NULL,
    gmail_label_id VARCHAR(255), -- Gmail label ID
    folder_type VARCHAR(50) DEFAULT 'custom', -- 'inbox', 'sent', 'draft', 'trash', 'spam', 'custom'
    parent_folder_id INTEGER REFERENCES email_folders(id),
    is_system BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 10. Email Message Folders Table - Many-to-many relationship for message folders
CREATE TABLE IF NOT EXISTS email_message_folders (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES email_messages(id) ON DELETE CASCADE,
    folder_id INTEGER REFERENCES email_folders(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(message_id, folder_id)
);

-- 11. Email Signatures Table - Store email signatures
CREATE TABLE IF NOT EXISTS email_signatures (
    id SERIAL PRIMARY KEY,
    email_account_id INTEGER REFERENCES email_accounts(id) ON DELETE CASCADE,
    signature_name VARCHAR(255) NOT NULL,
    signature_html TEXT,
    signature_text TEXT,
    is_default BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,
    updated_by INTEGER
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_email_messages_gmail_message_id ON email_messages(gmail_message_id);
CREATE INDEX IF NOT EXISTS idx_email_messages_sender_account ON email_messages(sender_email_account_id);
CREATE INDEX IF NOT EXISTS idx_email_messages_status ON email_messages(status);
CREATE INDEX IF NOT EXISTS idx_email_messages_created_at ON email_messages(created_at);
CREATE INDEX IF NOT EXISTS idx_email_messages_related_entity ON email_messages(related_entity_type, related_entity_id);

CREATE INDEX IF NOT EXISTS idx_email_recipients_message_id ON email_recipients(message_id);
CREATE INDEX IF NOT EXISTS idx_email_recipients_email ON email_recipients(recipient_email);
CREATE INDEX IF NOT EXISTS idx_email_recipients_status ON email_recipients(status);

CREATE INDEX IF NOT EXISTS idx_email_tracking_events_message_id ON email_tracking_events(message_id);
CREATE INDEX IF NOT EXISTS idx_email_tracking_events_recipient_id ON email_tracking_events(recipient_id);
CREATE INDEX IF NOT EXISTS idx_email_tracking_events_type ON email_tracking_events(event_type);
CREATE INDEX IF NOT EXISTS idx_email_tracking_events_created_at ON email_tracking_events(created_at);

CREATE INDEX IF NOT EXISTS idx_email_queue_status ON email_queue(status);
CREATE INDEX IF NOT EXISTS idx_email_queue_next_attempt ON email_queue(next_attempt_at);
CREATE INDEX IF NOT EXISTS idx_email_queue_priority ON email_queue(priority);

-- Insert default email templates
INSERT INTO email_templates (template_name, subject, body_html, body_text, template_type, created_by) VALUES
('Lead Follow-up', 'Follow-up on your inquiry', 
'<html><body><h2>Thank you for your interest!</h2><p>Dear {{customer_name}},</p><p>Thank you for your inquiry about {{product_name}}. We would love to discuss how our solution can help your business.</p><p>Best regards,<br>{{sender_name}}</p></body></html>',
'Thank you for your interest!\n\nDear {{customer_name}},\n\nThank you for your inquiry about {{product_name}}. We would love to discuss how our solution can help your business.\n\nBest regards,\n{{sender_name}}',
'lead_followup', 1),

('Quotation Sent', 'Your Quotation - {{quotation_number}}',
'<html><body><h2>Quotation</h2><p>Dear {{customer_name}},</p><p>Please find attached your quotation {{quotation_number}} for {{product_name}}.</p><p>This quotation is valid until {{validity_date}}.</p><p>Best regards,<br>{{sender_name}}</p></body></html>',
'Quotation\n\nDear {{customer_name}},\n\nPlease find attached your quotation {{quotation_number}} for {{product_name}}.\n\nThis quotation is valid until {{validity_date}}.\n\nBest regards,\n{{sender_name}}',
'quotation', 1),

('Invoice Sent', 'Invoice - {{invoice_number}}',
'<html><body><h2>Invoice</h2><p>Dear {{customer_name}},</p><p>Please find attached invoice {{invoice_number}} for the amount of {{invoice_amount}}.</p><p>Payment due date: {{due_date}}</p><p>Best regards,<br>{{sender_name}}</p></body></html>',
'Invoice\n\nDear {{customer_name}},\n\nPlease find attached invoice {{invoice_number}} for the amount of {{invoice_amount}}.\n\nPayment due date: {{due_date}}\n\nBest regards,\n{{sender_name}}',
'invoice', 1);

-- Add triggers for updated_at timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_email_accounts_updated_at BEFORE UPDATE ON email_accounts FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_templates_updated_at BEFORE UPDATE ON email_templates FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_campaigns_updated_at BEFORE UPDATE ON email_campaigns FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_messages_updated_at BEFORE UPDATE ON email_messages FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_recipients_updated_at BEFORE UPDATE ON email_recipients FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_queue_updated_at BEFORE UPDATE ON email_queue FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_folders_updated_at BEFORE UPDATE ON email_folders FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_email_signatures_updated_at BEFORE UPDATE ON email_signatures FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
