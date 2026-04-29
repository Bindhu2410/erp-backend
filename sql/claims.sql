create table claims(
 id SERIAL PRIMARY KEY,
    user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp null,
	claim_no varchar(100) not null unique,
	claim_date date not null,
	user_name varchar(255),
	claim_type varchar(255),
	from_place varchar(255),
	to_place varchar(255),
	mode_of_travel varchar(100),
	expense_type varchar(255),
	amount decimal(18,2) not null,
	comments varchar(255)
	
);

ALTER TABLE public.claims ADD CONSTRAINT claims_user_created_fkey 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.claims ADD CONSTRAINT claims_user_updated_fkey 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

	 alter table claims 
 add column actual_km int;
   
alter table claims 
add column bill_url varchar(255);

	-- New table to store multiple items (rows) per claim. Each claim may contain multiple expense items.
	create table claim_items(
		id SERIAL PRIMARY KEY,
		claim_id int NOT NULL,
		from_place varchar(255),
		to_place varchar(255),
		mode_of_travel varchar(100),
		expense_type varchar(255),
		amount decimal(18,2),
		actual_km decimal(18,2),
		comments varchar(255),
		bill_url varchar(255)
	);

	ALTER TABLE public.claim_items ADD CONSTRAINT claim_items_claim_id_fkey
		FOREIGN KEY (claim_id) REFERENCES public.claims(id) ON DELETE CASCADE;

	-- Migrate data from existing per-claim expense columns into claim_items if needed
	-- (Optional manual migration steps could be added here.)

	-- Drop columns that are now represented by claim_items to support multiple rows per claim
	alter table claims drop column if exists from_place;
	alter table claims drop column if exists to_place;
	alter table claims drop column if exists expense_type;
	alter table claims drop column if exists amount;
	alter table claims drop column if exists actual_km;
	alter table claims drop column if exists comments;
	alter table claims drop column if exists bill_url;
