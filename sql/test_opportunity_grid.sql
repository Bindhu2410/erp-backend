-- Test script for fn_get_sales_opportunities_grid
-- Replace 1 with a valid user ID in your system

DO $$
DECLARE
    result RECORD;
BEGIN
    FOR result IN SELECT * FROM fn_get_sales_opportunities_grid(
        '{
            "SearchText": null,
            "CustomerNames": null,
            "Statuses": null,
            "OpportunityTypes": null,
            "LeadIds": null,
            "PageNumber": 1,
            "PageSize": 10,
            "OrderBy": "date_created",
            "OrderDirection": "DESC",
            "CurrentUserId": 1
        }'::jsonb
    )
    LOOP
        RAISE NOTICE '%', row_to_json(result);
    END LOOP;
END $$;

-- Test script for get_sales_products_by_opportunity
-- Replace 123 and 1 with valid opportunity ID and user ID
SELECT * FROM get_sales_products_by_opportunity(123, 1);
