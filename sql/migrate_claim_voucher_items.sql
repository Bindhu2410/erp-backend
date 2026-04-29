-- Migration: move per-voucher item columns into claim_voucher_items then drop the columns
BEGIN;

-- 1) Create items table if not exists
CREATE TABLE IF NOT EXISTS public.claim_voucher_items (
    id SERIAL PRIMARY KEY,
    claim_voucher_id int4 NOT NULL,
    sales_man varchar(255),
    debit_account varchar(255),
    credit_account varchar(255),
    amount decimal(18,2),
    notes varchar(255)
);

ALTER TABLE IF EXISTS public.claim_voucher_items
    ADD CONSTRAINT IF NOT EXISTS claim_voucher_items_claim_voucher_id_fkey
    FOREIGN KEY (claim_voucher_id) REFERENCES public.claim_voucher(id) ON DELETE CASCADE;

-- 2) Migrate data: for any claim_voucher that has per-voucher values, create one item row
INSERT INTO public.claim_voucher_items (claim_voucher_id, sales_man, debit_account, credit_account, amount, notes)
SELECT id, sales_man, debit_account, credit_account, amount, notes
FROM public.claim_voucher
WHERE COALESCE(sales_man IS NOT NULL, false)
   OR COALESCE(debit_account IS NOT NULL, false)
   OR COALESCE(credit_account IS NOT NULL, false)
   OR COALESCE(amount IS NOT NULL, false)
   OR COALESCE(notes IS NOT NULL, false);

-- 3) Recompute total_amount from items
UPDATE public.claim_voucher cv
SET total_amount = COALESCE((SELECT SUM(amount) FROM public.claim_voucher_items WHERE claim_voucher_id = cv.id),0);

-- 4) Safely drop columns (if you want to preserve them keep them instead)
ALTER TABLE public.claim_voucher
    DROP COLUMN IF EXISTS sales_man,
    DROP COLUMN IF EXISTS debit_account,
    DROP COLUMN IF EXISTS credit_account,
    DROP COLUMN IF EXISTS amount,
    DROP COLUMN IF EXISTS notes;

COMMIT;

-- After running this script, update application code + tests to stop using the dropped columns.
