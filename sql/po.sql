CREATE TABLE public.purchase_order (
    id SERIAL PRIMARY KEY,
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,

    po_id VARCHAR(50) NOT NULL UNIQUE, -- external PO identifier
    purchase_requisition_id VARCHAR(250) REFERENCES public.purchase_requisitions(purchase_requisition_id),
    
    status VARCHAR(50),
    supplier_id INT REFERENCES public.suppliers(id),
    quotation_id INT REFERENCES public.sales_quotations(id),
    sales_order_id INT REFERENCES public.sales_orders(id),
    delivery_date DATE,
    description VARCHAR(250)
);

-- Purchase Order Items Table
CREATE TABLE public.purchase_order_items (
    id SERIAL PRIMARY KEY,
    purchase_order_id INT NOT NULL REFERENCES public.purchase_order(id) ON DELETE CASCADE,
    item_id INT NOT NULL REFERENCES public.item_master(id) ON DELETE CASCADE,
    supplier_id INT REFERENCES public.suppliers(id) ON DELETE SET NULL,
    quantity INT
);