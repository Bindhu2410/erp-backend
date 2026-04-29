-- Migration: Add qty_avl column to issue_items table
-- Run this against your PostgreSQL database

ALTER TABLE issue_items 
ADD COLUMN IF NOT EXISTS qty_avl NUMERIC(18,4);

-- Verify
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'issue_items' 
ORDER BY ordinal_position;
