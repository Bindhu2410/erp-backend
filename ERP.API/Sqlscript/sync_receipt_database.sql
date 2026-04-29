-- Comprehensive Receipt Database Synchronization Script
-- This script adds ALL columns defined in the C# models to the existing PostgreSQL tables.
-- It is designed to be safe for repeated execution.

-- 1. Sync Receipt Header Table
DO $$ 
BEGIN
    -- Standard Fields
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='location_id') THEN
        ALTER TABLE public.receipt ADD COLUMN location_id VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='bom_id') THEN
        ALTER TABLE public.receipt ADD COLUMN bom_id TEXT[];
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='received_from') THEN
        ALTER TABLE public.receipt ADD COLUMN received_from VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='customer_name') THEN
        ALTER TABLE public.receipt ADD COLUMN customer_name VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='sales_representative') THEN
        ALTER TABLE public.receipt ADD COLUMN sales_representative VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='salesman') THEN
        ALTER TABLE public.receipt ADD COLUMN salesman VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='hospital_name') THEN
        ALTER TABLE public.receipt ADD COLUMN hospital_name VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='doc_id') THEN
        ALTER TABLE public.receipt ADD COLUMN doc_id VARCHAR(50);
    END IF;

    -- Date Fields
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='receipt_date') THEN
        ALTER TABLE public.receipt ADD COLUMN receipt_date TIMESTAMP WITH TIME ZONE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='doc_date') THEN
        ALTER TABLE public.receipt ADD COLUMN doc_date TIMESTAMP WITH TIME ZONE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='ref_no') THEN
        ALTER TABLE public.receipt ADD COLUMN ref_no VARCHAR(50);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='ref_date') THEN
        ALTER TABLE public.receipt ADD COLUMN ref_date TIMESTAMP WITH TIME ZONE;
    END IF;

    -- Meta/Misc Fields
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='status') THEN
        ALTER TABLE public.receipt ADD COLUMN status VARCHAR(50);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='comments') THEN
        ALTER TABLE public.receipt ADD COLUMN comments TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='issue_id') THEN
        ALTER TABLE public.receipt ADD COLUMN issue_id VARCHAR(50);
    END IF;

    -- Footer/Financial Fields
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='gross') THEN
        ALTER TABLE public.receipt ADD COLUMN gross DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='total_qty') THEN
        ALTER TABLE public.receipt ADD COLUMN total_qty DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='amount_in_words') THEN
        ALTER TABLE public.receipt ADD COLUMN amount_in_words TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt' AND column_name='narration') THEN
        ALTER TABLE public.receipt ADD COLUMN narration TEXT;
    END IF;

    -- Explicitly remove NOT NULL constraints that might be causing save failures
    ALTER TABLE public.receipt ALTER COLUMN sales_representative DROP NOT NULL;
    ALTER TABLE public.receipt ALTER COLUMN salesman DROP NOT NULL;
END $$;

-- 2. Sync Receipt Items Table
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='issue_no') THEN
        ALTER TABLE public.receipt_items ADD COLUMN issue_no VARCHAR(50);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='batch_no') THEN
        ALTER TABLE public.receipt_items ADD COLUMN batch_no VARCHAR(50);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='acc_yn') THEN
        ALTER TABLE public.receipt_items ADD COLUMN acc_yn VARCHAR(1);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='quantity') THEN
        ALTER TABLE public.receipt_items ADD COLUMN quantity DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='unit') THEN
        ALTER TABLE public.receipt_items ADD COLUMN unit VARCHAR(20);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='rate') THEN
        ALTER TABLE public.receipt_items ADD COLUMN rate DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='amount') THEN
        ALTER TABLE public.receipt_items ADD COLUMN amount DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='remarks') THEN
        ALTER TABLE public.receipt_items ADD COLUMN remarks TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='make') THEN
        ALTER TABLE public.receipt_items ADD COLUMN make VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='category') THEN
        ALTER TABLE public.receipt_items ADD COLUMN category VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='product') THEN
        ALTER TABLE public.receipt_items ADD COLUMN product VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='model') THEN
        ALTER TABLE public.receipt_items ADD COLUMN model VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_items' AND column_name='item') THEN
        ALTER TABLE public.receipt_items ADD COLUMN item VARCHAR(255);
    END IF;
END $$;

-- 3. Sync Receipt Optional Items Table
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='make') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN make VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='category') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN category VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='product') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN product VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='model') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN model VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='item') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN item VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='description') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN description TEXT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='quantity') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN quantity DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_optional_items' AND column_name='rate') THEN
        ALTER TABLE public.receipt_optional_items ADD COLUMN rate DECIMAL(18, 2);
    END IF;
END $$;

-- 4. Sync Receipt Accessories Table
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_accessories' AND column_name='s_no') THEN
        ALTER TABLE public.receipt_accessories ADD COLUMN s_no INTEGER;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_accessories' AND column_name='accessories') THEN
        ALTER TABLE public.receipt_accessories ADD COLUMN accessories VARCHAR(255);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_accessories' AND column_name='iss_acc_qty') THEN
        ALTER TABLE public.receipt_accessories ADD COLUMN iss_acc_qty DECIMAL(18, 2);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receipt_accessories' AND column_name='re_acc_qty') THEN
        ALTER TABLE public.receipt_accessories ADD COLUMN re_acc_qty DECIMAL(18, 2);
    END IF;
END $$;

-- Summary Check
SELECT table_name, count(column_name) as column_count 
FROM information_schema.columns 
WHERE table_name IN ('receipt', 'receipt_items', 'receipt_optional_items', 'receipt_accessories') 
GROUP BY table_name;
