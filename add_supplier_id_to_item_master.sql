-- Migration: Add supplier_id column to item_master table
-- Purpose: Link items to suppliers

-- Check if column already exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'item_master' AND column_name = 'supplier_id'
    ) THEN
        ALTER TABLE public.item_master
        ADD COLUMN supplier_id INT;
        
        -- Add foreign key constraint
        ALTER TABLE public.item_master
        ADD CONSTRAINT fk_item_master_supplier 
        FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id);
        
        RAISE NOTICE 'Column supplier_id added to item_master table';
    ELSE
        RAISE NOTICE 'Column supplier_id already exists in item_master table';
    END IF;
END $$;

-- Create index on supplier_id for faster queries
CREATE INDEX IF NOT EXISTS idx_item_master_supplier_id 
ON public.item_master(supplier_id);
