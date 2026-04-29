-- Fix PostgreSQL VARCHAR(255) limit for quotation storage
-- This migration increases column sizes for fields that may contain long text/HTML content
-- These columns are commonly used to store Terms & Conditions, Delivery/Payment/Warranty details,
-- and rendered HTML from the quotation template

BEGIN;

-- 1. Alter terms column (often stores T&C HTML/text)
ALTER TABLE public.sales_quotations
    ALTER COLUMN terms TYPE TEXT USING terms::TEXT;
COMMENT ON COLUMN public.sales_quotations.terms IS 'Terms and conditions for quotation - now supports unlimited length';

-- 2. Alter quotation_for column (may contain lengthy descriptions)
ALTER TABLE public.sales_quotations
    ALTER COLUMN quotation_for TYPE TEXT USING quotation_for::TEXT;
COMMENT ON COLUMN public.sales_quotations.quotation_for IS 'Description of quotation purpose - now supports unlimited length';

-- 3. Alter comments column (general comments that could be lengthy)
ALTER TABLE public.sales_quotations
    ALTER COLUMN comments TYPE TEXT USING comments::TEXT;
COMMENT ON COLUMN public.sales_quotations.comments IS 'General comments - now supports unlimited length';

-- 4. Alter delivery_within column (may contain detailed delivery terms)
ALTER TABLE public.sales_quotations
    ALTER COLUMN delivery_within TYPE TEXT USING delivery_within::TEXT;
COMMENT ON COLUMN public.sales_quotations.delivery_within IS 'Delivery timeline details - now supports unlimited length';

-- 5. Alter delivery_after column (may contain detailed delivery terms)
ALTER TABLE public.sales_quotations
    ALTER COLUMN delivery_after TYPE TEXT USING delivery_after::TEXT;
COMMENT ON COLUMN public.sales_quotations.delivery_after IS 'Delivery preparation details - now supports unlimited length';

-- 6. Alter delivery column (may contain detailed delivery terms)
ALTER TABLE public.sales_quotations
    ALTER COLUMN delivery TYPE TEXT USING delivery::TEXT;
COMMENT ON COLUMN public.sales_quotations.delivery IS 'Delivery terms and conditions - now supports unlimited length';

-- 7. Alter payment column (may contain detailed payment terms)
ALTER TABLE public.sales_quotations
    ALTER COLUMN payment TYPE TEXT USING payment::TEXT;
COMMENT ON COLUMN public.sales_quotations.payment IS 'Payment terms and conditions - now supports unlimited length';

-- 8. Alter warranty column (may contain detailed warranty info)
ALTER TABLE public.sales_quotations
    ALTER COLUMN warranty TYPE TEXT USING warranty::TEXT;
COMMENT ON COLUMN public.sales_quotations.warranty IS 'Warranty details - now supports unlimited length';

COMMIT;

-- Log completion
SELECT 'Migration completed: Quotation table VARCHAR columns converted to TEXT' as migration_status;
