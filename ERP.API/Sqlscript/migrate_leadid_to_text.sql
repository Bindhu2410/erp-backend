-- Migration script to change leadid columns from integer to text for string IDs like 'LEAD-001'

-- 1. Drop foreign key constraints referencing leadid (if any)
ALTER TABLE public.sales_lead_interestedproducts DROP CONSTRAINT IF EXISTS fk_sales_leads;

-- 2. Alter leadid columns to text
ALTER TABLE public.sales_demos ALTER COLUMN leadid TYPE text;
ALTER TABLE public.sales_lead_interestedproducts ALTER COLUMN leadid TYPE text;

-- 3. (Optional) Recreate foreign key constraints if needed, referencing the new type
-- Example: If sales_leads.id is still integer, you cannot directly reference it from a text column.
-- You may need to update your schema logic or remove the FK constraint if not needed.

-- 4. (Optional) Repeat for any other tables with a leadid column as integer that should be text.

-- Review and run each statement in your PostgreSQL database.
