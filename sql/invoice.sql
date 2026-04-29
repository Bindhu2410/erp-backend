-- public.sales_invoices definition

-- Drop table

-- DROP TABLE public.sales_invoices;

CREATE TABLE public.sales_invoices (
	id text DEFAULT nextval('sales_invoices_id_seq'::regclass) NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT now() NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	quotation_id int4 NULL,
	po_id varchar(50) NULL,
	sales_order_id varchar(50) NULL,
	invoice_id varchar(50) NOT NULL,
	invoice_date date DEFAULT CURRENT_DATE NOT NULL,
	total_amount numeric(15, 4) NOT NULL,
	status varchar(20) DEFAULT 'Draft'::character varying NOT NULL,
	quantity numeric(12, 2) NULL,
	item_id int4 NULL,
	unit_price numeric(12, 2) NOT NULL,
	amount numeric(15, 4) NOT NULL,
	delivery_id varchar(50) NULL,
	CONSTRAINT sales_invoices_invoice_id_key UNIQUE (invoice_id),
	CONSTRAINT sales_invoices_pkey PRIMARY KEY (id),
	CONSTRAINT sales_invoices_status_check CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Issued'::character varying, 'Paid'::character varying, 'Partially Paid'::character varying, 'Cancelled'::character varying, 'Refunded'::character varying])::text[])))
);


-- public.sales_invoices foreign keys

ALTER TABLE public.sales_invoices ADD CONSTRAINT fk_delivery_id FOREIGN KEY (delivery_id) REFERENCES public.deliveries(delivery_id);
ALTER TABLE public.sales_invoices ADD CONSTRAINT fk_invoice_user_created FOREIGN KEY (user_created) REFERENCES public.users(user_id);
ALTER TABLE public.sales_invoices ADD CONSTRAINT fk_invoice_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(user_id);
ALTER TABLE public.sales_invoices ADD CONSTRAINT sales_invoices_po_id_fkey FOREIGN KEY (po_id) REFERENCES public.purchase_order(po_id);
ALTER TABLE public.sales_invoices ADD CONSTRAINT sales_invoices_quotation_id_fkey FOREIGN KEY (quotation_id) REFERENCES public.sales_quotations(id);
ALTER TABLE public.sales_invoices ADD CONSTRAINT sales_invoices_sales_order_id_fkey FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(order_id);