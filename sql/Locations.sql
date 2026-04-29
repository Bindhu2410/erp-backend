CREATE TABLE sales_countries (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  description TEXT 
);
CREATE TABLE sales_states (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  sales_countries_id INTEGER REFERENCES sales_countries(id)
);
CREATE TABLE sales_territories (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  alias VARCHAR(255)
)

CREATE TABLE sales_districts (
  id SERIAL PRIMARY KEY NOT NULL,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  sales_territories_id INTEGER REFERENCES sales_territories(id)
);

CREATE TABLE sales_cities (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  sales_districts_id INTEGER REFERENCES sales_districts(id),
city_code varchar(255) null,
description text null
);

CREATE TABLE sales_areas (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  name VARCHAR(255),
  sales_cities_id INTEGER REFERENCES sales_cities(id),
pincode int null,
description text null
);

CREATE TABLE pincodes (
  id SERIAL PRIMARY KEY,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  pincode VARCHAR(10),
  sales_areas_id INTEGER REFERENCES sales_areas(id)
);
