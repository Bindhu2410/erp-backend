create table claim_voucher_items (
    id SERIAL PRIMARY KEY,
    claim_voucher_id int4 NOT NULL,
    sales_man varchar(255),
    debit_account varchar(255),
    credit_account varchar(255),
    amount decimal(18,2),
    notes varchar(255)
);

ALTER TABLE public.claim_voucher_items ADD CONSTRAINT claim_voucher_items_claim_voucher_id_fkey
    FOREIGN KEY (claim_voucher_id) REFERENCES public.claim_voucher(id) ON DELETE CASCADE;

-- Trigger function to update claim_voucher.total_amount whenever items change
CREATE OR REPLACE FUNCTION public.update_claim_voucher_total_amount()
RETURNS trigger AS $$
DECLARE
    cid int;
    total decimal;
BEGIN
    IF (TG_OP = 'INSERT') THEN
        cid := NEW.claim_voucher_id;
    ELSIF (TG_OP = 'DELETE') THEN
        cid := OLD.claim_voucher_id;
    ELSE
        cid := NEW.claim_voucher_id;
    END IF;

    SELECT COALESCE(SUM(amount),0) INTO total FROM public.claim_voucher_items WHERE claim_voucher_id = cid;
    UPDATE public.claim_voucher SET total_amount = total WHERE id = cid;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_update_claim_voucher_total_amount ON public.claim_voucher_items;
CREATE TRIGGER trg_update_claim_voucher_total_amount
AFTER INSERT OR UPDATE OR DELETE ON public.claim_voucher_items
FOR EACH ROW EXECUTE FUNCTION public.update_claim_voucher_total_amount();
