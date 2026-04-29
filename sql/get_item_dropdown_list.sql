-- Function: get_item_dropdown_list
-- Returns: ItemId, CategoryName, GroupName, ValuationMethodName, Make, Model, Product, ItemName, ItemCode

CREATE OR REPLACE FUNCTION get_item_dropdown_list()
RETURNS TABLE (
    ItemId INT,
    CategoryName VARCHAR(255),
    GroupName VARCHAR(255),
    ValuationMethodName VARCHAR(255),
    Make VARCHAR(255),
    Model VARCHAR(255),
    Product VARCHAR(255),
    ItemName VARCHAR(255),
    ItemCode VARCHAR(255),
    UnitPrice DECIMAL(18,2)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        im.id AS ItemId,
        cat.name AS CategoryName,
        ig.name AS GroupName,
        vm.name AS ValuationMethodName,
        m.name AS Make,
        mo.name AS Model,
        p.name AS Product,
        im.item_name AS ItemName,
        im.item_code AS ItemCode,
        im.unit_price AS UnitPrice
    FROM item_master im
    LEFT JOIN categories cat ON im.category_id = cat.id
    LEFT JOIN inventory_group ig ON im.group_id = ig.id
    LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
    LEFT JOIN make m ON im.make_id = m.id
    LEFT JOIN model mo ON im.model_id = mo.id
    LEFT JOIN product p ON im.product_id = p.id
    WHERE im.item_code IS NOT NULL AND im.item_name IS NOT NULL
    ORDER BY im.item_name;
END;
$$ LANGUAGE plpgsql;
