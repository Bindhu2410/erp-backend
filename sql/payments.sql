-- public.payments definition

-- Drop table

-- DROP TABLE public.payments;

CREATE TABLE public.payments (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT now() NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	invoice_id varchar(50) NOT NULL,
	payment_date date DEFAULT CURRENT_DATE NOT NULL,
	due_date date NULL,
	payment_method varchar(50) NOT NULL,
	amount_paid numeric(15, 4) NOT NULL,
	payment_status varchar(20) DEFAULT 'Pending'::character varying NOT NULL,
	outstanding_amount numeric(15, 4) NULL,
	total_amount numeric(15, 4) NULL,
	CONSTRAINT payments_amount_paid_check CHECK ((amount_paid > (0)::numeric)),
	CONSTRAINT payments_payment_method_check CHECK (((payment_method)::text = ANY ((ARRAY['Cash'::character varying, 'Bank Transfer'::character varying, 'Credit Card'::character varying, 'Cheque'::character varying, 'UPI'::character varying, 'Other'::character varying])::text[]))),
	CONSTRAINT payments_payment_status_check CHECK (((payment_status)::text = ANY ((ARRAY['Pending'::character varying, 'Confirmed'::character varying, 'Failed'::character varying, 'Refunded'::character varying, 'Completed'::character varying])::text[]))),
	CONSTRAINT payments_pkey PRIMARY KEY (id)
);


-- public.payments foreign keys

ALTER TABLE public.payments ADD CONSTRAINT fk_payment_user_created FOREIGN KEY (user_created) REFERENCES public.users(user_id);
ALTER TABLE public.payments ADD CONSTRAINT fk_payment_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(user_id);
ALTER TABLE public.payments ADD CONSTRAINT payments_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.sales_invoices(invoice_id);