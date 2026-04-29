CREATE TABLE sales_leads_business_challenges ( id SERIAL PRIMARY KEY NOT NULL,
                                                                     user_created INTEGER, date_created TIMESTAMP,
                                                                                                        user_updated INTEGER, date_updated TIMESTAMP,
                                                                                                                                           solution TEXT, challenges TEXT, isactive BOOLEAN DEFAULT FALSE,
                                                                                                                                                                                                    sales_leads_id INTEGER, CONSTRAINT fk_user_created
                                              FOREIGN KEY (user_created) REFERENCES sales_employees(id),
                                                                                    CONSTRAINT fk_user_updated
                                              FOREIGN KEY (user_updated) REFERENCES sales_employees(id),
                                                                                    CONSTRAINT fk_sales_leads
                                              FOREIGN KEY (sales_leads_id) REFERENCES sales_leads(id));

