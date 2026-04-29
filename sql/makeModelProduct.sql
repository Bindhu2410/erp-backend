create table make(
id serial primary key ,
 user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    name varchar(255) not null,
    is_active bool
)
create table model(
id serial primary key ,
 user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    name varchar(255) not null,
        is_active bool
)
 create table product(
id serial primary key ,
 user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    name varchar(255) not null,
        is_active bool
)
