-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_sales_demos_customer_name ON sales_demos(customer_name);
CREATE INDEX IF NOT EXISTS idx_sales_demos_demo_date ON sales_demos(demo_date);
CREATE INDEX IF NOT EXISTS idx_sales_demos_status ON sales_demos(status);
CREATE INDEX IF NOT EXISTS idx_sales_demos_demo_approach ON sales_demos(demo_approach);
CREATE INDEX IF NOT EXISTS idx_sales_demos_demo_outcome ON sales_demos(demo_outcome);
CREATE INDEX IF NOT EXISTS idx_sales_demos_date_created ON sales_demos(date_created);
