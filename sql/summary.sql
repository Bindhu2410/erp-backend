CREATE TABLE "sales_summaries"
("id" SERIAL PRIMARY KEY NOT NULL, 
"user_created" int, 
"date_created" TIMESTAMP NULL,
"user_updated" int, 
"date_updated" TIMESTAMP NULL, 
"icon_url" varchar(255) NULL, 
"title" varchar(255) NULL, 
"description" TEXT, 
"date_time" TIMESTAMP NULL,
"isactive" BOOLEAN NOT NULL DEFAULT FALSE, 
"stage_item_id" varchar(255) null,
"stage" varchar(255) null,
"entities" TEXT);
alter table sales_summaries 
add constraint fk_summaaries_user_created foreign key("user_created") references sales_employees(id),
add constraint fk_summaaries_user_updted foreign key("user_updated") references sales_employees(id);

