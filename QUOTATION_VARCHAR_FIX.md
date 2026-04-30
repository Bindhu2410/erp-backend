# Fix for PostgreSQL VARCHAR(255) Limit Error

## Problem

Error: `22001: value too long for type character varying(255)`

This occurs when the quotation consolidation endpoint tries to save content (Terms, Delivery, Payment, Warranty, Comments) that exceeds 255 characters.

## Root Cause

1. **Database Schema**: The `sales_quotations` table had multiple VARCHAR(255) columns that were too restrictive for storing:
   - Terms and Conditions (T&C) text
   - Delivery, Payment, and Warranty details
   - Comments and other descriptive fields
2. **C# Model Validation**: The `SalesQuotation` model had `[MaxLength(255)]` attributes on these fields, causing API validation errors when receiving longer content.

## Solution Applied

### 1. Database Migration (SQL)

File: `d:\Radhika\Brindha\Backend\JBS4IR_v2\Backend\ERP-1\sql\fix_quotation_varchar_limit.sql`

Converted the following columns from `VARCHAR(255)` to `TEXT` in the `sales_quotations` table:

- `terms` - Terms and conditions
- `quotation_for` - Quotation purpose description
- `comments` - General comments
- `delivery_within` - Delivery timeline
- `delivery_after` - Delivery preparation details
- `delivery` - Delivery terms
- `payment` - Payment terms
- `warranty` - Warranty details

### 2. C# Model Updates

File: `d:\Radhika\Brindha\Backend\JBS4IR_v2\Backend\ERP-1\ERP.API\Models\SalesQuotation.cs`

**Removed** `[MaxLength(255)]` attributes from:

- `Terms`
- `QuotationFor`
- `Comments`
- `DeliveryWithin`
- `DeliveryAfter`
- `Delivery`
- `Payment`
- `Warranty`
- `SelectedDelivery`
- `SelectedPayment`
- `SelectedWarranty`

## Implementation Steps

### Step 1: Apply Database Migration

```sql
-- Connect to your PostgreSQL database and execute:
psql -U your_user -d your_database -f sql/fix_quotation_varchar_limit.sql
```

Or manually run the migration file at:
`d:\Radhika\Brindha\Backend\JBS4IR_v2\Backend\ERP-1\sql\fix_quotation_varchar_limit.sql`

### Step 2: Deploy Updated C# Model

The model changes are already in place. The C# attributes have been removed from the problematic fields.

### Step 3: Rebuild and Test

```bash
cd d:\Radhika\Brindha\Backend\JBS4IR_v2\Backend\ERP-1\ERP.API
dotnet build
dotnet run
```

### Step 4: Test the Endpoint

POST to: `http://localhost:5104/api/sales-quotations/7/consolidated`

With quotation data that includes full Terms & Conditions HTML, Delivery, Payment, and Warranty details.

## Why This Works

1. **PostgreSQL TEXT type**: Unlike VARCHAR(n), TEXT in PostgreSQL doesn't have a length limit, allowing for arbitrarily long content.

2. **Removed validation**: By removing `[MaxLength(255)]` from the C# model, ASP.NET Core validation no longer rejects long content at the API layer.

3. **Backward compatible**: Existing shorter content continues to work. NULL values and empty strings are still supported.

## Files Modified

1. **Database**: `sql/fix_quotation_varchar_limit.sql` (new migration file)
2. **Code**: `ERP.API/Models/SalesQuotation.cs` (removed MaxLength attributes)

## Verification

After applying these changes:

- The database will accept unlimited-length content in the specified columns
- The API will accept quotations with full HTML templates and detailed T&C
- Model validation will no longer reject content based on character count for these fields

## Additional Notes

The `FreightCharge` column was NOT converted because it's a `decimal` type in the database, not VARCHAR.

Other VARCHAR(255) fields like `Status`, `LostReason`, `QuotationType`, etc., were kept as-is because they typically don't exceed 255 characters and serve as valid constraint fields.
