-- Add invoice_id column to purchase_order table
ALTER TABLE purchase_order ADD COLUMN invoice_id VARCHAR(100);
