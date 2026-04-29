-- Migration: Remove presenter_id from sales_demos for many-to-many presenter support

-- 1. Drop foreign key constraint (if exists)
ALTER TABLE public.sales_demos DROP CONSTRAINT IF EXISTS fk_sales_demos_presenter;

-- 2. Drop the presenter_id column
ALTER TABLE public.sales_demos DROP COLUMN IF EXISTS presenter_id;

-- No changes needed to sales_demo_presenters (already supports many-to-many)

-- You may want to update your application code to remove references to sales_demos.presenter_id
