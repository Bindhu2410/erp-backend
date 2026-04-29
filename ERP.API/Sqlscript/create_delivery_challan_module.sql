-- Create delivery_challans table
CREATE TABLE IF NOT EXISTS delivery_challans (
    id SERIAL PRIMARY KEY,
    delivery_challan_id VARCHAR(50) UNIQUE,
    delivery_date TIMESTAMP NOT NULL,
    sales_order_id INT REFERENCES sales_orders(id),
    salesman_id INT REFERENCES employees(id),
    party_id INT REFERENCES sales_customers(id),
    delivery_status VARCHAR(50) DEFAULT 'Pending',
    dispatch_address TEXT,
    priority VARCHAR(50),
    transporter_name VARCHAR(100),
    vehicle_no VARCHAR(50),
    driver_name VARCHAR(100),
    driver_contact BIGINT,
    mode_of_delivery VARCHAR(50),
    notes TEXT,
    
    -- New Fields from UI
    location VARCHAR(100),
    form_20_sno VARCHAR(100),
    form_20_no VARCHAR(100),
    ref_no VARCHAR(100),
    ref_date TIMESTAMP,
    dispatched_by VARCHAR(100),
    delivered_by VARCHAR(100),
    goods_consign_from VARCHAR(100),
    goods_consign_to VARCHAR(100),
    booking_address TEXT,
    booking_qty DECIMAL(18, 2),
    app_value DECIMAL(18, 2),
    delivery_at VARCHAR(100),
    delivery_add1 VARCHAR(100),
    delivery_add2 VARCHAR(100),
    document_through VARCHAR(100),
    invoice_no VARCHAR(100),
    invoice_date TIMESTAMP,
    
    -- Footer Fields
    gross_amount DECIMAL(18, 2),
    net_amount DECIMAL(18, 2),
    total_qty DECIMAL(18, 2),
    amount_in_words TEXT,
    delivery_to TEXT,
    remarks TEXT,
    prepared_by VARCHAR(100),
    authorized_by VARCHAR(100),
    received_by VARCHAR(100),
    
    user_created INT,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create delivery_challan_items table
CREATE TABLE IF NOT EXISTS delivery_challan_items (
    id SERIAL PRIMARY KEY,
    delivery_challan_id INT REFERENCES delivery_challans(id) ON DELETE CASCADE,
    item_id INT REFERENCES item_master(id),
    qty DECIMAL(18, 2) NOT NULL,
    unit_price DECIMAL(18, 2),
    amount DECIMAL(18, 2),
    
    -- New Fields from UI
    so_no VARCHAR(100),
    make VARCHAR(100),
    category VARCHAR(100),
    product VARCHAR(100),
    model VARCHAR(100),
    visual_item_id VARCHAR(100),
    equl_ins VARCHAR(100),
    match_no VARCHAR(100),
    ord_qty DECIMAL(18, 2),
    current_stock DECIMAL(18, 2),
    unit VARCHAR(50),
    
    user_created INT,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Index for faster searches
CREATE INDEX IF NOT EXISTS idx_delivery_challans_dc_id ON delivery_challans(delivery_challan_id);
CREATE INDEX IF NOT EXISTS idx_delivery_challans_so_id ON delivery_challans(sales_order_id);
CREATE INDEX IF NOT EXISTS idx_delivery_challans_party_id ON delivery_challans(party_id);
