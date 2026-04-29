-- Remove the foreign key constraint before altering the column to array type
ALTER TABLE public.purchase_requisitions
    DROP CONSTRAINT IF EXISTS pr_bom_id_fkey;

-- Now alter the column to array type
ALTER TABLE public.purchase_requisitions
    ALTER COLUMN bom_id TYPE VARCHAR(250)[] USING CASE WHEN bom_id IS NULL THEN NULL ELSE ARRAY[bom_id] END;
