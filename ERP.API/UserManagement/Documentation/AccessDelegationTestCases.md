# Access Delegation API Test Cases

## Test Setup
These test cases can be executed using tools like Postman, curl, or automated testing frameworks.

### Prerequisites
1. API server running on `http://localhost:5104`
2. Valid JWT authentication token
3. Test users with IDs: 123 (John), 456 (Sarah), 789 (Mike)

### Environment Variables
```bash
export API_BASE_URL="http://localhost:5104"
export JWT_TOKEN="your_jwt_token_here"
```

## Test Cases

### 1. Basic CRUD Operations

#### Test 1.1: Create Access Delegation (Success)
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "fromUserId": 123,
       "toUserId": 456,
       "startDate": "2024-07-04T08:00:00Z",
       "endDate": "2024-07-10T17:00:00Z",
       "reason": "Vacation coverage - handling customer inquiries",
       "isActive": true,
       "createdBy": 123
     }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Access delegation created successfully",
  "data": 1
}
```

#### Test 1.2: Create Access Delegation (Validation Error)
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "fromUserId": 123,
       "toUserId": 123,
       "startDate": "2024-07-10T08:00:00Z",
       "endDate": "2024-07-05T17:00:00Z",
       "reason": "Invalid delegation",
       "isActive": true,
       "createdBy": 123
     }'
```

**Expected Response:**
```json
{
  "success": false,
  "message": "Cannot delegate to yourself",
  "errors": ["Invalid user delegation"]
}
```

#### Test 1.3: Get Access Delegation by ID (Success)
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/1" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Access delegation retrieved successfully",
  "data": {
    "delegationId": 1,
    "fromUserId": 123,
    "toUserId": 456,
    "startDate": "2024-07-04T08:00:00Z",
    "endDate": "2024-07-10T17:00:00Z",
    "reason": "Vacation coverage - handling customer inquiries",
    "isActive": true,
    "createdBy": 123,
    "dateCreated": "2024-07-03T10:30:00Z"
  }
}
```

#### Test 1.4: Get Access Delegation by ID (Not Found)
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/999" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

**Expected Response:**
```json
{
  "success": false,
  "message": "Access delegation not found",
  "errors": ["Delegation with ID 999 not found"]
}
```

#### Test 1.5: Update Access Delegation (Success)
```bash
curl -X PUT "$API_BASE_URL/api/accessdelegation/1" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "delegationId": 1,
       "endDate": "2024-07-15T17:00:00Z",
       "reason": "Extended vacation coverage"
     }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Access delegation updated successfully",
  "data": true
}
```

#### Test 1.6: Delete Access Delegation (Success)
```bash
curl -X DELETE "$API_BASE_URL/api/accessdelegation/1" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Access delegation deleted successfully",
  "data": true
}
```

### 2. Listing and Pagination Tests

#### Test 2.1: Get All Delegations (Paginated)
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation?pageNumber=1&pageSize=5" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

#### Test 2.2: Get Current Active Delegations
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/active?pageNumber=1&pageSize=10" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

#### Test 2.3: Get Delegations by From User
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/from-user/123" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

#### Test 2.4: Get Delegations by To User
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/to-user/456" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

### 3. Advanced Query Tests

#### Test 3.1: Search Delegations
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/search" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "fromUserId": 123,
       "isActive": true,
       "searchText": "vacation",
       "pageNumber": 1,
       "pageSize": 10
     }'
```

#### Test 3.2: Get Delegations by Date Range
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/date-range" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "startDate": "2024-07-01T00:00:00Z",
       "endDate": "2024-07-31T23:59:59Z",
       "pageNumber": 1,
       "pageSize": 10,
       "activeOnly": false
     }'
```

#### Test 3.3: Get User Delegation History
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/user/123/history?pageNumber=1&pageSize=20" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

### 4. Delegation Management Tests

#### Test 4.1: Extend Delegation
```bash
# First create a delegation
DELEGATION_ID=$(curl -s -X POST "$API_BASE_URL/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "fromUserId": 123,
       "toUserId": 456,
       "startDate": "2024-07-04T08:00:00Z",
       "endDate": "2024-07-10T17:00:00Z",
       "reason": "Test delegation for extension",
       "isActive": true,
       "createdBy": 123
     }' | jq -r '.data')

# Then extend it
curl -X POST "$API_BASE_URL/api/accessdelegation/extend" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d "{
       \"delegationId\": $DELEGATION_ID,
       \"newEndDate\": \"2024-07-20T17:00:00Z\"
     }"
```

#### Test 4.2: Check Active Delegation
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/check-active/123/456" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

### 5. Statistics and Analytics Tests

#### Test 5.1: Get Delegation Statistics
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/statistics" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'
```

#### Test 5.2: Get Most Active Delegators
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/most-active-delegators" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "limit": 5,
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'
```

#### Test 5.3: Get Most Popular Delegates
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/most-popular-delegates" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "limit": 5,
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'
```

### 6. Maintenance Tests

#### Test 6.1: Deactivate Expired Delegations
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation/deactivate-expired" \
     -H "Authorization: Bearer $JWT_TOKEN"
```

### 7. Error Handling Tests

#### Test 7.1: Unauthorized Access (No Token)
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/1"
```

**Expected Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

#### Test 7.2: Invalid Token
```bash
curl -X GET "$API_BASE_URL/api/accessdelegation/1" \
     -H "Authorization: Bearer invalid_token"
```

#### Test 7.3: Malformed JSON
```bash
curl -X POST "$API_BASE_URL/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{"fromUserId": 123, "toUserId":}'
```

### 8. Load Testing (Optional)

#### Test 8.1: Create Multiple Delegations
```bash
for i in {1..10}; do
  curl -X POST "$API_BASE_URL/api/accessdelegation" \
       -H "Content-Type: application/json" \
       -H "Authorization: Bearer $JWT_TOKEN" \
       -d "{
         \"fromUserId\": $((123 + i)),
         \"toUserId\": $((456 + i)),
         \"startDate\": \"2024-07-$(printf %02d $i)T08:00:00Z\",
         \"endDate\": \"2024-07-$(printf %02d $((i + 5)))T17:00:00Z\",
         \"reason\": \"Test delegation $i\",
         \"isActive\": true,
         \"createdBy\": $((123 + i))
       }" &
done
wait
```

#### Test 8.2: Concurrent Read Operations
```bash
for i in {1..20}; do
  curl -s -X GET "$API_BASE_URL/api/accessdelegation?pageNumber=$i&pageSize=5" \
       -H "Authorization: Bearer $JWT_TOKEN" > /dev/null &
done
wait
```

## Test Results Validation

### Success Criteria
1. All CRUD operations return expected status codes
2. Validation errors are properly handled
3. Pagination works correctly
4. Search and filtering return accurate results
5. Statistics provide meaningful data
6. Error responses are properly formatted

### Performance Criteria
1. API response time < 500ms for simple operations
2. API response time < 2s for complex queries
3. No memory leaks during concurrent operations
4. Database connections are properly managed

### Security Criteria
1. Unauthorized requests are rejected
2. JWT tokens are properly validated
3. Input validation prevents injection attacks
4. Audit logging captures all operations

## Automated Test Script

```bash
#!/bin/bash

# Access Delegation API Test Suite
API_BASE_URL="http://localhost:5104"
JWT_TOKEN="your_jwt_token_here"

echo "Starting Access Delegation API Test Suite..."

# Test 1: Health Check
echo "Test 1: API Health Check"
response=$(curl -s -o /dev/null -w "%{http_code}" "$API_BASE_URL/api/accessdelegation?pageNumber=1&pageSize=1" \
           -H "Authorization: Bearer $JWT_TOKEN")
if [ "$response" -eq 200 ]; then
    echo "✓ API is responding"
else
    echo "✗ API health check failed (HTTP $response)"
    exit 1
fi

# Test 2: Create Delegation
echo "Test 2: Create Delegation"
delegation_response=$(curl -s -X POST "$API_BASE_URL/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $JWT_TOKEN" \
     -d '{
       "fromUserId": 123,
       "toUserId": 456,
       "startDate": "2024-07-04T08:00:00Z",
       "endDate": "2024-07-10T17:00:00Z",
       "reason": "Automated test delegation",
       "isActive": true,
       "createdBy": 123
     }')

delegation_id=$(echo "$delegation_response" | jq -r '.data')
if [ "$delegation_id" != "null" ] && [ "$delegation_id" != "" ]; then
    echo "✓ Delegation created with ID: $delegation_id"
else
    echo "✗ Delegation creation failed"
    echo "$delegation_response"
    exit 1
fi

# Test 3: Get Delegation
echo "Test 3: Get Delegation"
get_response=$(curl -s -X GET "$API_BASE_URL/api/accessdelegation/$delegation_id" \
               -H "Authorization: Bearer $JWT_TOKEN")
success=$(echo "$get_response" | jq -r '.success')
if [ "$success" = "true" ]; then
    echo "✓ Delegation retrieved successfully"
else
    echo "✗ Delegation retrieval failed"
    echo "$get_response"
fi

# Test 4: Update Delegation
echo "Test 4: Update Delegation"
update_response=$(curl -s -X PUT "$API_BASE_URL/api/accessdelegation/$delegation_id" \
                  -H "Content-Type: application/json" \
                  -H "Authorization: Bearer $JWT_TOKEN" \
                  -d "{
                    \"delegationId\": $delegation_id,
                    \"reason\": \"Updated automated test delegation\"
                  }")
success=$(echo "$update_response" | jq -r '.success')
if [ "$success" = "true" ]; then
    echo "✓ Delegation updated successfully"
else
    echo "✗ Delegation update failed"
    echo "$update_response"
fi

# Test 5: Check Active Delegation
echo "Test 5: Check Active Delegation"
check_response=$(curl -s -X GET "$API_BASE_URL/api/accessdelegation/check-active/123/456" \
                 -H "Authorization: Bearer $JWT_TOKEN")
success=$(echo "$check_response" | jq -r '.success')
if [ "$success" = "true" ]; then
    echo "✓ Active delegation check completed"
else
    echo "✗ Active delegation check failed"
    echo "$check_response"
fi

# Test 6: Delete Delegation
echo "Test 6: Delete Delegation"
delete_response=$(curl -s -X DELETE "$API_BASE_URL/api/accessdelegation/$delegation_id" \
                  -H "Authorization: Bearer $JWT_TOKEN")
success=$(echo "$delete_response" | jq -r '.success')
if [ "$success" = "true" ]; then
    echo "✓ Delegation deleted successfully"
else
    echo "✗ Delegation deletion failed"
    echo "$delete_response"
fi

echo "Access Delegation API Test Suite completed!"
```

Save this script as `test_access_delegation_api.sh` and run with:
```bash
chmod +x test_access_delegation_api.sh
./test_access_delegation_api.sh
```

## Manual Testing Checklist

- [ ] Create delegation with valid data
- [ ] Create delegation with invalid data (validation)
- [ ] Get delegation by valid ID
- [ ] Get delegation by invalid ID
- [ ] Update delegation with valid data
- [ ] Update delegation with invalid data
- [ ] Delete existing delegation
- [ ] Delete non-existent delegation
- [ ] List delegations with pagination
- [ ] Get active delegations
- [ ] Search delegations with filters
- [ ] Get delegation statistics
- [ ] Extend delegation
- [ ] Check delegation status
- [ ] Test unauthorized access
- [ ] Test with expired JWT token
- [ ] Test concurrent operations
- [ ] Verify audit logging
- [ ] Test error handling
- [ ] Validate response formats
