   create table departments (
 id serial4 NOT NULL,
    user_created int4 NULL,
    date_created timestamp NULL,
    user_updated int4 NULL,
    date_updated timestamp NULL,
    name varchar(255),
    head_of_department varchar(255));

    ALTER TABLE public.departments ADD CONSTRAINT departments_user_created_fkey 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.departments ADD CONSTRAINT departments_user_updated_fkey 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

