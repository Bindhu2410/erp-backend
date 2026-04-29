
-- Update issues table with new fields
ALTER TABLE issues ADD COLUMN IF NOT EXISTS ref_date TIMESTAMP;
ALTER TABLE issues ADD COLUMN IF NOT EXISTS generate_invoice VARCHAR(10);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS bill_no VARCHAR(100);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS bill_date TIMESTAMP;
ALTER TABLE issues ADD COLUMN IF NOT EXISTS doctor_name VARCHAR(200);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS billing_description TEXT;
ALTER TABLE issues ADD COLUMN IF NOT EXISTS billing_amount DECIMAL(18, 2);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS gross DECIMAL(18, 2);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS total_qty DECIMAL(18, 2);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS amount_in_words TEXT;
ALTER TABLE issues ADD COLUMN IF NOT EXISTS eway_bill_no VARCHAR(100);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS eway_bill_date TIMESTAMP;
ALTER TABLE issues ADD COLUMN IF NOT EXISTS transporter VARCHAR(200);
ALTER TABLE issues ADD COLUMN IF NOT EXISTS vehicle_no VARCHAR(50);

-- Create issue_optional_items table
CREATE TABLE IF NOT EXISTS issue_optional_items (
    id SERIAL PRIMARY KEY,
    issue_id INTEGER NOT NULL REFERENCES issues(id) ON DELETE CASCADE,
    s_no INTEGER NOT NULL,
    opt_make VARCHAR(255),
    opt_category VARCHAR(255),
    opt_product VARCHAR(255),
    opt_model VARCHAR(255),
    opt_item VARCHAR(255),
    opt_item_desc TEXT,
    opt_qty DECIMAL(18, 2),
    opt_rate DECIMAL(18, 2),
    opt_amount DECIMAL(18, 2)
);

-- Create issue_items table (Detailed Grid)
CREATE TABLE IF NOT EXISTS issue_items (
    id SERIAL PRIMARY KEY,
    issue_id INTEGER NOT NULL REFERENCES issues(id) ON DELETE CASCADE,
    s_no INTEGER NOT NULL,
    make VARCHAR(255),
    category VARCHAR(255),
    product VARCHAR(255),
    model VARCHAR(255),
    item VARCHAR(255),
    equ_ins VARCHAR(255),
    batch_no VARCHAR(255),
    receipt_no VARCHAR(255),
    unit VARCHAR(100),
    qty DECIMAL(18, 2),
    rate DECIMAL(18, 2),
    amount DECIMAL(18, 2),
    remarks TEXT
);
