-- =============================================
-- Database Table Creation for Issue Management
-- Database: PostgreSQL
-- =============================================

-- 1. Table: issues (Header Table)
CREATE TABLE IF NOT EXISTS issues (
    id SERIAL PRIMARY KEY,
    user_created INTEGER,
    date_created TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP WITHOUT TIME ZONE,
    location_id VARCHAR(50),
    bom_id TEXT[], -- Array of strings for BomIds
    iss_to VARCHAR(100),
    issue_to VARCHAR(255),
    customer_name VARCHAR(255),
    party_branch VARCHAR(255),
    status VARCHAR(50),
    sales_representative VARCHAR(100),
    goods_consign_from VARCHAR(255),
    goods_consign_to VARCHAR(255),
    delivered_by VARCHAR(255),
    booking_address TEXT,
    booking_qty INTEGER,
    app_value NUMERIC(18,2),
    received_on TIMESTAMP WITHOUT TIME ZONE,
    bom_name VARCHAR(255),
    demo_from VARCHAR(100),
    demo_report VARCHAR(255),
    demo_request VARCHAR(255),
    demo_remarks TEXT,
    doc_id VARCHAR(50), -- Document reference number
    issue_date TIMESTAMP WITHOUT TIME ZONE,
    ref_no VARCHAR(100),
    ref_date TIMESTAMP WITHOUT TIME ZONE,
    comments TEXT,
    narration TEXT,
    receipt_id VARCHAR(100),
    
    -- Billing Section
    generate_invoice VARCHAR(10), -- YES/NO
    bill_no VARCHAR(100),
    bill_date TIMESTAMP WITHOUT TIME ZONE,
    doctor_name VARCHAR(255),
    billing_description TEXT,
    billing_amount NUMERIC(18,2),
    
    -- Footer fields
    gross NUMERIC(18,2),
    total_qty NUMERIC(18,4),
    amount_in_words TEXT,
    
    -- Eway Bill Section
    eway_bill_no VARCHAR(100),
    eway_bill_date TIMESTAMP WITHOUT TIME ZONE,
    transporter VARCHAR(255),
    vehicle_no VARCHAR(50)
);

-- 2. Table: issue_items (Detailed Grid)
CREATE TABLE IF NOT EXISTS issue_items (
    id SERIAL PRIMARY KEY,
    issue_id INTEGER NOT NULL,
    s_no INTEGER NOT NULL,
    make VARCHAR(100),
    category VARCHAR(100),
    product VARCHAR(255),
    model VARCHAR(100),
    item VARCHAR(255),
    equ_ins VARCHAR(255),
    batch_no VARCHAR(100),
    receipt_no VARCHAR(100),
    unit VARCHAR(50),
    qty_avl NUMERIC(18,4), -- Qty Available (persisted)
    qty NUMERIC(18,4),
    rate NUMERIC(18,4),
    amount NUMERIC(18,4),
    remarks TEXT,
    CONSTRAINT fk_issue_items_issue FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
);

-- 3. Table: issue_optional_items (Optional Grid)
CREATE TABLE IF NOT EXISTS issue_optional_items (
    id SERIAL PRIMARY KEY,
    issue_id INTEGER NOT NULL,
    s_no INTEGER NOT NULL,
    opt_make VARCHAR(100),
    opt_category VARCHAR(100),
    opt_product VARCHAR(255),
    opt_model VARCHAR(100),
    opt_item VARCHAR(255),
    opt_item_desc TEXT,
    opt_qty NUMERIC(18,4),
    opt_rate NUMERIC(18,4),
    opt_amount NUMERIC(18,4),
    CONSTRAINT fk_issue_optional_items_issue FOREIGN KEY (issue_id) REFERENCES issues(id) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_issue_items_issue_id ON issue_items(issue_id);
CREATE INDEX IF NOT EXISTS idx_issue_optional_items_issue_id ON issue_optional_items(issue_id);
CREATE INDEX IF NOT EXISTS idx_issues_doc_id ON issues(doc_id);
CREATE INDEX IF NOT EXISTS idx_issues_customer ON issues(customer_name);
