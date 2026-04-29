CREATE TABLE public.goods_receipt_note (
    id SERIAL PRIMARY KEY,
    grn_no VARCHAR(50) NOT NULL UNIQUE,
    grn_date DATE NOT NULL,
    po_id VARCHAR(50) REFERENCES purchase_order(po_id),
    supplier_id INT REFERENCES public.suppliers(id),
    narration TEXT,
    status VARCHAR(30),
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP
);

-- Junction table for items of a GRN
CREATE TABLE public.goods_receipt_note_items (
    id SERIAL PRIMARY KEY,
    grn_id INT NOT NULL REFERENCES public.goods_receipt_note(id) ON DELETE CASCADE,
        qc_passed BOOLEAN DEFAULT false,
    item_id INT NOT NULL REFERENCES public.item_master(id),
    grn_qty NUMERIC(18,3) DEFAULT 0,
    pending_qty NUMERIC(18,3) DEFAULT 0,
    billed_qty NUMERIC(18,3) DEFAULT 0,
    amount NUMERIC(18,3) DEFAULT 0
);

alter table goods_receipt_note_items
add column ordered_qty NUMERIC(18,3) DEFAULT 0;