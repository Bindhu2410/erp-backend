DROP TABLE IF EXISTS public.bill_of_material_child_items CASCADE;
DROP TABLE IF EXISTS public.bill_of_materials CASCADE;

-- Main Bill of Materials table
CREATE TABLE public.bill_of_materials (
    id SERIAL PRIMARY KEY,
    bom_id VARCHAR(255),
    bom_name VARCHAR(255),   -- BOM descriptive name
    bom_type VARCHAR(255)    -- BOM type (e.g., Production, Engineering, etc.)
);

-- Child items of the BOM
CREATE TABLE public.bill_of_material_child_items (
    id SERIAL PRIMARY KEY,
    bill_of_material_id INT NOT NULL REFERENCES public.bill_of_materials(id) ON DELETE CASCADE,
    child_item_id INT NOT NULL REFERENCES public.item_master(id) ON DELETE CASCADE,
    quantity NUMERIC(18, 4) NOT NULL DEFAULT 1
);
