CREATE TABLE public.sales_bank_account (
    id SERIAL PRIMARY KEY,
    user_created INT NULL,
    date_created TIMESTAMP NULL,
    user_updated INT NULL,
    date_updated TIMESTAMP NULL,
    branch VARCHAR(255) NULL,
    registered_company VARCHAR(255) NULL,
    name_of_the_bank VARCHAR(255) NULL,
    account_no VARCHAR(255) NULL,
    ifsc_code VARCHAR(255) NULL,    account_holder_name VARCHAR(255) null,
    isactive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_bank_user_created FOREIGN KEY (user_created) REFERENCES users(user_id),
    CONSTRAINT fk_bank_user_updated FOREIGN KEY (user_updated) REFERENCES users(user_id)
);
