-- Create Receipt Tables Script
-- This script creates the master and detail tables for the Receipt module.

-- 1. Receipt Header Table
CREATE TABLE IF NOT EXISTS public.receipt (
    id SERIAL PRIMARY KEY,
    user_created INTEGER,
    date_created TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP WITH TIME ZONE,
    location_id VARCHAR(255),
    bom_id TEXT[], -- Array of BOM IDs
    received_from VARCHAR(255),
    customer_name VARCHAR(255) NOT NULL,
    sales_representative VARCHAR(255),
    salesman VARCHAR(255),
    hospital_name VARCHAR(255),
    doc_id VARCHAR(50) UNIQUE,
    receipt_date TIMESTAMP WITH TIME ZONE,
    doc_date TIMESTAMP WITH TIME ZONE,
    ref_no VARCHAR(50),
    ref_date TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50),
    comments TEXT,
    issue_id VARCHAR(50),
    gross DECIMAL(18, 2),
    total_qty DECIMAL(18, 2),
    amount_in_words TEXT,
    narration TEXT
);

-- 2. Receipt Items Table (Issue Detail Grid)
CREATE TABLE IF NOT EXISTS public.receipt_items (
    id SERIAL PRIMARY KEY,
    receipt_id INTEGER NOT NULL REFERENCES public.receipt(id) ON DELETE CASCADE,
    issue_no VARCHAR(50),
    batch_no VARCHAR(50),
    acc_yn VARCHAR(1),
    quantity DECIMAL(18, 2),
    unit VARCHAR(20),
    rate DECIMAL(18, 2),
    amount DECIMAL(18, 2),
    remarks TEXT,
    make VARCHAR(255),
    category VARCHAR(255),
    product VARCHAR(255),
    model VARCHAR(255),
    item VARCHAR(255)
);

-- 3. Receipt Optional Items Table
CREATE TABLE IF NOT EXISTS public.receipt_optional_items (
    id SERIAL PRIMARY KEY,
    receipt_id INTEGER NOT NULL REFERENCES public.receipt(id) ON DELETE CASCADE,
    make VARCHAR(255),
    category VARCHAR(255),
    product VARCHAR(255),
    model VARCHAR(255),
    item VARCHAR(255),
    description TEXT,
    quantity DECIMAL(18, 2),
    rate DECIMAL(18, 2)
);

-- 4. Receipt Accessories Table (DC5 Grid)
CREATE TABLE IF NOT EXISTS public.receipt_accessories (
    id SERIAL PRIMARY KEY,
    receipt_id INTEGER NOT NULL REFERENCES public.receipt(id) ON DELETE CASCADE,
    s_no INTEGER,
    accessories VARCHAR(255),
    iss_acc_qty DECIMAL(18, 2),
    re_acc_qty DECIMAL(18, 2)
);

-- Add indexes for performance
CREATE INDEX IF NOT EXISTS idx_receipt_doc_id ON public.receipt(doc_id);
CREATE INDEX IF NOT EXISTS idx_receipt_items_receipt_id ON public.receipt_items(receipt_id);
CREATE INDEX IF NOT EXISTS idx_receipt_opt_items_receipt_id ON public.receipt_optional_items(receipt_id);
CREATE INDEX IF NOT EXISTS idx_receipt_acc_receipt_id ON public.receipt_accessories(receipt_id);
