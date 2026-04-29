CREATE TABLE "sales_documents" 
("id" SERIAL PRIMARY KEY NOT NULL,
"user_created" int,
"date_created" TIMESTAMP NULL,
"user_updated" int,
"date_updated" TIMESTAMP NULL,
"file_url" varchar(255) NULL,
"title" varchar(255) NULL, 
"file_type" varchar(255) NULL, 
"file_name" varchar(255) NULL,
"icon_url" varchar(255) NULL, 
"description" TEXT, 
"isactive" BOOLEAN NOT NULL DEFAULT FALSE,
"document_id" varchar(255) NULL, 
"stage" varchar(255) null,
"stage_item_id" varchar(255) null);
alter table sales_documents 
add constraint fk_sales_sales_documents_user_created foreign key ("user_created") references sales_employees(id),
add constraint fk_sales_sales_documents_user_updated foreign key ("user_updated") references sales_employees(id);