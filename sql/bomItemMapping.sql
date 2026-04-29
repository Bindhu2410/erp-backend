-- DDL: bom_item_mapping table
CREATE TABLE IF NOT EXISTS public.bom_item_mapping (
    id SERIAL PRIMARY KEY,
    bom_name_id INT REFERENCES public.bom_name(id),
    item_id INT REFERENCES public.item_master(id),
    UNIQUE (bom_name_id, item_id)
);

-- Optional: add quantity/role/parent columns later if needed
