-- Migration: Add presenter_ids int[] array column to sales_demos

ALTER TABLE public.sales_demos ADD COLUMN presenter_ids int[] NULL;

-- Note: You must update your application code to read/write this array column if you use it.
