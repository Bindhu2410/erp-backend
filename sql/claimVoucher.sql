create table claim_voucher (
	id SERIAL PRIMARY KEY,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp null,
	doc_id varchar(100) unique,
	date Date,
	from_date Date,
	to_date Date ,
	status varchar(100),
	total_amount decimal(18,2)
)
ALTER TABLE public.claim_voucher ADD CONSTRAINT claim_voucher_user_created_fkey 
	FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.claim_voucher ADD CONSTRAINT claim_voucher_user_updated_fkey 
	FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- NOTE: per-item fields (sales_man, debit_account, credit_account, amount, notes)
-- have been moved to `claim_voucher_items` to support multiple items per voucher.