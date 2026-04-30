# Sales Opportunity Grid API Updates Summary

## Overview
Updated the Sales Opportunity Grid API to match the Sales Lead Grid API pattern, specifically adding `UserCreated` column filtering functionality.

## Files Modified

### 1. Models/SalesOpportunityGridRequest.cs
- **Added**: `UserCreated` property with `[DefaultValue(null)]` attribute
- **Purpose**: Allows filtering opportunities by the user who created them

### 2. Controllers/SalesOpportunityController.cs
- **Added**: `using System.Security.Claims;` for authentication support
- **Added**: `GetCurrentUserId()` private method to extract user ID from JWT claims
- **Modified**: `GetOpportunitiesGrid()` method to support user-based filtering logic
  - Gets current user ID from authentication context
  - Uses `UserCreated` from request body if provided, otherwise falls back to current user
  - Calls appropriate service method based on user context (user-specific vs. general)
- **Modified**: `GetOpportunityCardsStatus()` method to support user-based filtering
  - Added try-catch error handling
  - Added user-based filtering logic similar to leads

### 3. Services/ISalesOpportunityService.cs
- **Added**: `GetOpportunitiesGridByUserAsync()` method signature for user-based grid filtering
- **Added**: `GetOpportunityCardsStatusByUserAsync()` method signature for user-based cards

### 4. Services/SalesOpportunityService.cs
- **Added**: `GetOpportunitiesGridByUserAsync()` method implementation
  - Calls `fn_get_sales_opportunities_grid_by_user` SQL function
  - Includes user ID in the request JSON
  - Provides detailed logging for user-specific queries
- **Added**: `GetOpportunityCardsStatusByUserAsync()` method implementation
  - Filters opportunity counts by `user_created` field
  - Maintains same structure as general cards method

### 5. SQL Function (New)
- **Created**: `Sqlscript/drop_and_create_fn_get_sales_opportunities_grid_by_user.sql`
  - New PostgreSQL function `fn_get_sales_opportunities_grid_by_user`
  - Accepts `UserCreated` parameter in JSON request
  - Filters opportunities by `so.user_created = v_userCreated`
  - Maintains same return structure as original function
  - Includes proper permissions with `GRANT EXECUTE TO PUBLIC`

### 6. Test Script (New)
- **Created**: `Sqlscript/test_opportunity_grid_by_user.sql`
  - Verification script for the new SQL function
  - Tests various scenarios (different users, search text, etc.)
  - Compares results between user-specific and general functions

## Key Features Implemented

### 1. User-Based Filtering
- Opportunities are filtered by the `user_created` field
- Supports both explicit user ID from request and current authenticated user
- Falls back to general (non-user-specific) method when no user context available

### 2. Backward Compatibility
- Original functionality preserved for non-authenticated or general access
- Existing API contracts maintained
- No breaking changes to existing endpoints

### 3. Consistent Pattern
- Follows the exact same pattern as Sales Lead Grid API
- Same naming conventions and method signatures
- Consistent error handling and logging

### 4. Authentication Integration
- Uses JWT claims to extract current user ID
- Supports both authenticated and non-authenticated scenarios
- Secure user context handling

### 5. Database Integration
- New SQL function with proper PostgreSQL syntax
- Efficient filtering at database level
- Proper parameter handling and validation

## API Usage Examples

### Grid API with User Filtering
```json
POST /api/SalesOpportunity/grid
{
  "SearchText": "test",
  "PageNumber": 1,
  "PageSize": 10,
  "UserCreated": 123
}
```

### Cards API (Automatic User Detection)
```
GET /api/SalesOpportunity/cards-status
```

## Database Deployment
1. Execute `fix_opportunity_grid_functions.sql` to create/fix both functions (recommended)
   - OR execute `drop_and_create_fn_get_sales_opportunities_grid.sql` (original function fix)
   - AND execute `drop_and_create_fn_get_sales_opportunities_grid_by_user.sql` (new user-based function)
2. Optionally run `test_opportunity_grid_by_user.sql` to verify functionality

**Important Fix**: The original SQL functions had an issue with JSON array extraction that caused "cannot extract elements from a scalar" errors. This has been fixed with proper JSON type checking and error handling.

## Benefits
- **Security**: Users only see opportunities they created (when filtered)
- **Performance**: Database-level filtering reduces data transfer
- **Consistency**: Matches existing Lead Grid API patterns
- **Flexibility**: Supports both user-specific and general views
- **Maintainability**: Clear separation of concerns and consistent code structure
