-- Update existing rows to be active by default
UPDATE public.sales_lead SET isactive = true WHERE isactive IS NULL;

-- Change the default value for future rows
ALTER TABLE public.sales_lead ALTER COLUMN isactive SET DEFAULT true;
