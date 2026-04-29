CREATE TABLE sales_lead_solution_products (
    id SERIAL PRIMARY KEY NOT NULL,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    product_code VARCHAR(255),
    product_name VARCHAR(255),
    description TEXT,
    qty INTEGER,
    price FLOAT,
    amount FLOAT,
    sales_lead_business_challenges_id INTEGER,
    sales_products_id INTEGER,
    CONSTRAINT fk_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id),
    CONSTRAINT fk_sales_business_challenges FOREIGN KEY (sales_lead_business_challenges_id) REFERENCES sales_lead_business_challenges(id),
    CONSTRAINT fk_sales_products FOREIGN KEY (sales_products_id) REFERENCES sales_products(id)
);
