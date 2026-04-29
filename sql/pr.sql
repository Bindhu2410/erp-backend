CREATE TABLE public.purchase_requisitions (
    id serial4 NOT NULL,
    purchase_requisition_id varchar(100) NULL,
    requester_name varchar(250) NULL,
    description text NOT NULL,
    delivery_date date NULL,
    budget_amount numeric(18, 2) NULL,
    status varchar(50) NULL,
    user_created int4 NULL,
    user_updated int4 NULL,
    date_created timestamp DEFAULT now() NULL,
    date_updated timestamp DEFAULT now() NULL,
    supplier_id int4 NULL,
    CONSTRAINT purchase_requisitions_pkey PRIMARY KEY (id),
    CONSTRAINT purchase_requisitions_purchase_requisition_id_key UNIQUE (purchase_requisition_id)
);

-- public.purchase_requisitions foreign keys
ALTER TABLE public.purchase_requisitions 
    ADD CONSTRAINT pr_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id) ON DELETE SET NULL;

ALTER TABLE public.purchase_requisitions 
    ADD CONSTRAINT purchase_requisitions_user_created_fkey FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.purchase_requisitions 
    ADD CONSTRAINT purchase_requisitions_user_updated_fkey FOREIGN KEY (user_updated) REFERENCES public.users(userid);




CREATE TABLE public.purchase_requisition_boms (
    id serial4 NOT NULL,
    purchase_requisition_id int4 NOT NULL,
    item_id int4 NOT NULL,
    supplier_id int4 NULL,
    quantity int4 NULL,
    CONSTRAINT purchase_requisition_boms_pkey PRIMARY KEY (id)
);

-- public.purchase_requisition_boms foreign keys
ALTER TABLE public.purchase_requisition_boms 
    ADD CONSTRAINT fk_purchase_requisition FOREIGN KEY (purchase_requisition_id) REFERENCES public.purchase_requisitions(id) ON DELETE CASCADE;

ALTER TABLE public.purchase_requisition_boms 
    ADD CONSTRAINT fk_item FOREIGN KEY (item_id) REFERENCES public.item_master(id) ON DELETE CASCADE;

ALTER TABLE public.purchase_requisition_boms 
    ADD CONSTRAINT fk_supplier FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id) ON DELETE SET NULL;
