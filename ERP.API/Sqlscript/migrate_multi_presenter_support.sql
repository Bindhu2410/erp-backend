-- Migration script to support multiple presenters per sales demo

-- 1. Create a new table to store demo-presenter relationships
CREATE TABLE IF NOT EXISTS public.sales_demo_presenters (
    id serial PRIMARY KEY,
    demo_id integer NOT NULL REFERENCES public.sales_demos(id) ON DELETE CASCADE,
    presenter_id integer NOT NULL REFERENCES public.users(user_id) ON DELETE CASCADE
);

-- 2. Migrate existing presenter_id data to the new table
INSERT INTO public.sales_demo_presenters (demo_id, presenter_id)
SELECT id, presenter_id FROM public.sales_demos WHERE presenter_id IS NOT NULL;

-- 3. (Optional) Remove presenter_id column from sales_demos if you want to enforce only the new structure
-- ALTER TABLE public.sales_demos DROP COLUMN presenter_id;

-- 4. (Optional) Add unique constraint if you want to prevent duplicate presenter assignments per demo
-- ALTER TABLE public.sales_demo_presenters ADD CONSTRAINT uq_demo_presenter UNIQUE (demo_id, presenter_id);
