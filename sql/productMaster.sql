create table products( 
id serial primary key not null,
 user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
name varchar(255) null,
    CONSTRAINT fk_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);
CREATE TABLE makes (
	id serial primary key NOT NULL,
	user_created int,
	date_created timestamp NULL,
	user_updated int,
	date_updated timestamp NULL,
	"name" varchar(255) NULL,
    CONSTRAINT fk_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);

CREATE TABLE public.models (
	id serial primary key NOT NULL,
	user_created int,
	date_created timestamp NULL,
	user_updated int,
	date_updated timestamp NULL,
	"name" varchar(255) NULL,
    CONSTRAINT fk_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);

CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    name VARCHAR(255),

    -- Optional foreign key constraints
    CONSTRAINT fk_category_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_category_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);
