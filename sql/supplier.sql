CREATE TABLE suppliers (
    id SERIAL PRIMARY KEY,                  -- Unique Vendor ID
    vendor_code VARCHAR(50) UNIQUE NOT NULL, -- Internal code (like V001, V002)
    vendor_name VARCHAR(255) NOT NULL,      -- Vendor company / individual name
    phone VARCHAR(50)[],                      -- Phone number
    email VARCHAR(255)[],                     -- Email address
    address TEXT,  
    door_no varchar(100),
    street varchar(100),
    area varchar(100),-- Full address
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    pincode VARCHAR(20),
    gst_number VARCHAR(100),                 -- For Indian vendors (optional tax ID)
    is_registered bool ,
    pan_number VARCHAR(50),                 -- Another ID (optional)
    bank_name VARCHAR(255),
    bank_account_number VARCHAR(100),
    ifsc_code VARCHAR(50),  
    account_holder_name varchar(255),
    is_active BOOLEAN DEFAULT TRUE,         -- Active/Inactive vendor
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
