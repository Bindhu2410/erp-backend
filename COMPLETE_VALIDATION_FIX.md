# Complete Fix for VARCHAR(255) Validation Error

## Problem Summary

Error: `"The field Payment must be a string with a maximum length of 255."`

The issue was caused by **three layers of validation** that all had to be fixed:

1. **Database column definition** - VARCHAR(255) limit
2. **C# Entity model** - `[MaxLength(255)]` attributes
3. **DTO request models** - `[StringLength(255)]` attributes ← **This was the missing piece**

## Solution Applied

### 1. Database Schema Migration

**File**: `sql/fix_quotation_varchar_limit.sql`

Converted these columns from VARCHAR(255) to TEXT in the `sales_quotations` table:

- terms
- quotation_for
- comments
- delivery_within
- delivery_after
- delivery
- payment
- warranty

### 2. Entity Model Updates

**File**: `ERP.API/Models/SalesQuotation.cs`

Removed `[MaxLength(255)]` from:

- Terms
- QuotationFor
- Comments
- DeliveryWithin
- DeliveryAfter
- Delivery
- Payment
- Warranty
- SelectedDelivery
- SelectedPayment
- SelectedWarranty

### 3. DTO Request Model Updates ✓ (CRITICAL)

**File**: `ERP.API/Models/DTOs/QuotationDtos.cs`

**Removed `[StringLength(255)]` from `CreateQuotationRequestDto`:**

- Terms
- QuotationFor
- Comments
- DeliveryWithin
- DeliveryAfter
- Delivery
- Payment
- Warranty

This DTO is used by:

- `SalesQuotationWithItemsRequest` (which wraps `CreateQuotationRequestDto`)
- All quotation create/update endpoints

## Why This Fixes It

The validation error was coming from **model binding** in ASP.NET Core. When the request is deserialized:

1. ASP.NET Core creates a `CreateQuotationRequestDto` instance
2. It applies all validation attributes from the DTO class
3. The `[StringLength(255)]` attributes were rejecting any content > 255 characters
4. This happened **before** the request even reached the controller action

By removing the `[StringLength(255)]` constraints from the DTO, the validation now passes, and the data proceeds to:

- The database (which now accepts unlimited text via TEXT columns)
- The entity model (which no longer has MaxLength constraints)

## Implementation Steps

### Step 1: Execute SQL Migration

```sql
psql -U postgres -d your_database -f "sql/fix_quotation_varchar_limit.sql"
```

### Step 2: Verify Code Changes

All changes have already been applied:

- ✓ SalesQuotation.cs (Entity model)
- ✓ QuotationDtos.cs (Request DTOs)
- ✓ fix_quotation_varchar_limit.sql (Database migration)

### Step 3: Rebuild and Test

```bash
cd ERP.API
dotnet build
dotnet run
```

### Step 4: Test the Endpoint

```bash
# POST /api/sales-quotations/{id}/consolidated
# With a payload containing full Terms & Conditions HTML
```

## Files Modified

1. **Database**: `sql/fix_quotation_varchar_limit.sql`
2. **Entity Model**: `ERP.API/Models/SalesQuotation.cs`
3. **DTOs**: `ERP.API/Models/DTOs/QuotationDtos.cs` ← **KEY FIX**

## Validation Flow (Before vs After)

### BEFORE (Error)

```
Request with Payment = long HTML text
    ↓
ASP.NET deserializes to CreateQuotationRequestDto
    ↓
[StringLength(255)] validation fires on Payment field
    ↓
❌ Validation Error: "must be a string with a maximum length of 255"
```

### AFTER (Success)

```
Request with Payment = long HTML text
    ↓
ASP.NET deserializes to CreateQuotationRequestDto
    ↓
✓ No [StringLength(255)] constraint, validation passes
    ↓
Controller receives request
    ↓
Data saved to database TEXT column
    ↓
✓ Success
```

## Additional Notes

- Fields like `Status`, `LostReason`, `QuotationType` retain `[StringLength(255)]` because they typically don't exceed that limit
- The `SalesQuotationWithItemsRequest` DTO uses the fixed `CreateQuotationRequestDto`, so all consolidated updates benefit from this fix
- The database migration is backward compatible - existing data is preserved
