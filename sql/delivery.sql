-- public.deliveries definition

-- Drop table

-- DROP TABLE public.deliveries;

CREATE TABLE public.deliveries (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT now() NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	sales_order_id varchar(50) NULL,
	po_id varchar(50) NULL,
	delivery_id varchar(50) NOT NULL,
	delivery_date date DEFAULT CURRENT_DATE NOT NULL,
	delivery_status varchar(30) DEFAULT 'Pending'::character varying NOT NULL,
	priority varchar(100) NULL,
	transporter_name varchar(100) NULL,
	dispatch_address varchar(100) NULL,
	vehicle_no varchar(100) NULL,
	driver_name varchar(100) NULL,
	driver_contact int4 NULL,
	mode_of_delivery varchar(100) NULL,
	invoice_id varchar(100) NULL,
	CONSTRAINT deliveries_delivery_id_key UNIQUE (delivery_id),
	CONSTRAINT deliveries_delivery_status_check CHECK (((delivery_status)::text = ANY ((ARRAY['Pending'::character varying, 'Shipped'::character varying, 'In Transit'::character varying, 'Delivered'::character varying, 'Failed'::character varying, 'Returned'::character varying])::text[]))),
	CONSTRAINT deliveries_pkey PRIMARY KEY (id)
);


-- public.deliveries foreign keys

ALTER TABLE public.deliveries ADD CONSTRAINT deliveries_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.sales_invoices(invoice_id);
ALTER TABLE public.deliveries ADD CONSTRAINT deliveries_po_id_fkey FOREIGN KEY (po_id) REFERENCES public.purchase_order(po_id);
ALTER TABLE public.deliveries ADD CONSTRAINT deliveries_sales_order_id_fkey FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(order_id);
ALTER TABLE public.deliveries ADD CONSTRAINT deliveries_user_created_fkey FOREIGN KEY (user_created) REFERENCES public.users(user_id);
ALTER TABLE public.deliveries ADD CONSTRAINT deliveries_user_updated_fkey FOREIGN KEY (user_updated) REFERENCES public.users(user_id);
-- Trigger function to update deliveries.invoice_id when a new invoice is created
CREATE OR REPLACE FUNCTION update_delivery_invoice_id()
RETURNS TRIGGER AS $$
BEGIN
	IF NEW.delivery_id IS NOT NULL THEN
		UPDATE public.deliveries
		SET invoice_id = NEW.invoice_id
		WHERE delivery_id = NEW.delivery_id;
	END IF;
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger on sales_invoices to update deliveries.invoice_id after insert
CREATE TRIGGER trg_update_delivery_invoice_id
AFTER INSERT ON public.sales_invoices
FOR EACH ROW
EXECUTE FUNCTION update_delivery_invoice_id();