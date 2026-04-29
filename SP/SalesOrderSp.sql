-- Get all sales orders for grid
CREATE OR REPLACE FUNCTION get_sales_orders_grid()
RETURNS TABLE (
    id INTEGER,
    order_id VARCHAR(20),
    customer_name VARCHAR(100),
    order_date DATE,
    expected_delivery_date DATE,
    status VARCHAR(50),
    po_id VARCHAR(50),
    grand_total NUMERIC(12,2)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        so.id,
        so.order_id,
        c.name as customer_name,
        so.order_date,
        so.expected_delivery_date,
        so.status,
        so.po_id,
        so.grand_total
    FROM sales_orders so
    LEFT JOIN sales_customers c ON so.customer_id = c.id
    ORDER BY so.order_date DESC;
END;
$$ LANGUAGE plpgsql;

-- Get sales order by ID
CREATE OR REPLACE FUNCTION get_sales_order_by_id(p_id INTEGER)
RETURNS TABLE (
    id INTEGER,
    order_id VARCHAR(20),
    customer_id INTEGER,
    order_date DATE,
    expected_delivery_date DATE,
    status VARCHAR(50),
    quotation_id INTEGER,
    po_id VARCHAR(50),
    acceptance_date DATE,
    total_amount NUMERIC(12,2),
    tax_amount NUMERIC(12,2),
    grand_total NUMERIC(12,2),
    notes TEXT,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM sales_orders
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

-- Create new sales order
CREATE OR REPLACE FUNCTION create_sales_order(
    p_order_id VARCHAR(20),
    p_customer_id INTEGER,
    p_order_date DATE,
    p_expected_delivery_date DATE,
    p_status VARCHAR(50),
    p_quotation_id INTEGER,
    p_po_id VARCHAR(50),
    p_acceptance_date DATE,
    p_total_amount NUMERIC(12,2),
    p_tax_amount NUMERIC(12,2),
    p_grand_total NUMERIC(12,2),
    p_notes TEXT,
    p_user_created INTEGER
)
RETURNS INTEGER AS $$
DECLARE
    v_id INTEGER;
BEGIN
    INSERT INTO sales_orders (
        order_id,
        customer_id,
        order_date,
        expected_delivery_date,
        status,
        quotation_id,
        po_id,
        acceptance_date,
        total_amount,
        tax_amount,
        grand_total,
        notes,
        user_created,
        date_created
    )
    VALUES (
        p_order_id,
        p_customer_id,
        p_order_date,
        p_expected_delivery_date,
        p_status,
        p_quotation_id,
        p_po_id,
        p_acceptance_date,
        p_total_amount,
        p_tax_amount,
        p_grand_total,
        p_notes,
        p_user_created,
        CURRENT_TIMESTAMP
    )
    RETURNING id INTO v_id;
    
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- Update sales order
CREATE OR REPLACE FUNCTION update_sales_order(
    p_id INTEGER,
    p_order_id VARCHAR(20),
    p_customer_id INTEGER,
    p_order_date DATE,
    p_expected_delivery_date DATE,
    p_status VARCHAR(50),
    p_quotation_id INTEGER,
    p_po_id VARCHAR(50),
    p_acceptance_date DATE,
    p_total_amount NUMERIC(12,2),
    p_tax_amount NUMERIC(12,2),
    p_grand_total NUMERIC(12,2),
    p_notes TEXT,
    p_user_updated INTEGER
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE sales_orders
    SET
        order_id = p_order_id,
        customer_id = p_customer_id,
        order_date = p_order_date,
        expected_delivery_date = p_expected_delivery_date,
        status = p_status,
        quotation_id = p_quotation_id,
        po_id = p_po_id,
        acceptance_date = p_acceptance_date,
        total_amount = p_total_amount,
        tax_amount = p_tax_amount,
        grand_total = p_grand_total,
        notes = p_notes,
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Delete sales order
CREATE OR REPLACE FUNCTION delete_sales_order(p_id INTEGER)
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM sales_orders
    WHERE id = p_id;
    
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
