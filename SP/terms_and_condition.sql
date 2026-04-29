CREATE TABLE public.sales_terms_and_conditions (
    id SERIAL PRIMARY KEY,
    user_created int,
    date_created TIMESTAMP,
    user_updated int,
    date_updated TIMESTAMP,
    taxes VARCHAR(500),
    freight_charges VARCHAR(500),
    delivery VARCHAR(500),
    payment VARCHAR(500),
    warranty VARCHAR(500),
    template_name VARCHAR(255),
    is_default BOOLEAN,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    quotation_id int,

    -- Foreign Key Constraints
    CONSTRAINT fk_terms_user_created FOREIGN KEY (user_created) REFERENCES users(user_id),
    CONSTRAINT fk_terms_user_updated FOREIGN KEY (user_updated) REFERENCES users(user_id),
    constraint fk_terms_quotation_id foreign key (quotation_id) references sales_quotations(id)
);