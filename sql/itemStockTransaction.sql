CREATE TABLE public.item_stock_transaction (
    id SERIAL PRIMARY KEY,
    item_id INT NOT NULL REFERENCES public.item_master(id) ON DELETE CASCADE,
    warehouse_id INT NOT NULL REFERENCES public.warehouse(id) ON DELETE CASCADE,
    location_id INT REFERENCES public.item_location(id) ON DELETE SET NULL,
    transaction_type VARCHAR(50) NOT NULL,      -- e.g. GRN, ISSUE, TRANSFER, ADJUSTMENT
    reference_no VARCHAR(100),                  -- e.g. GRN No, Invoice No, Transfer Note
    quantity NUMERIC(12,2) NOT NULL,            -- Positive = IN, Negative = OUT
    unit_price NUMERIC(12,2),                   -- Cost per unit at transaction time
    total_value NUMERIC(14,2),                  -- quantity * unit_price
    transaction_date TIMESTAMP DEFAULT now(),
    created_by INT REFERENCES public.users(userid),
    remarks TEXT
);
