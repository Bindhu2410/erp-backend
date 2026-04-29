# User Management API Endpoints Guide

This document provides a comprehensive guide to all User Management API endpoints implemented in the ERP system.

## Base URL
```
http://localhost:5104/api/user
```

## Authentication Endpoints

### 1. Register User
**POST** `/api/user/register`

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "department": "IT"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "userId": 123,
  "token": "jwt_token_here"
}
```

### 2. User Login
**POST** `/api/user/login`

**Request Body:**
```json
{
  "emailOrUsername": "john.doe@example.com",
  "password": "SecurePassword123!",
  "rememberMe": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "userId": 123,
  "token": "jwt_token_here",
  "expiresAt": "2024-12-01T12:00:00Z"
}
```

### 3. User Logout
**POST** `/api/user/logout`
**Authorization:** Bearer token required

**Request Body:**
```json
"session_id_here"
```

## User Profile Management

### 4. Get User Profile
**GET** `/api/user/{userId}`
**Authorization:** Bearer token required

**Response:**
```json
{
  "userId": 123,
  "username": "johndoe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "department": "IT",
  "isActive": true,
  "createdDate": "2024-01-01T00:00:00Z",
  "lastLoginDate": "2024-01-15T10:30:00Z"
}
```

### 5. Get Current User Profile
**GET** `/api/user/profile`
**Authorization:** Bearer token required

### 6. Update User Profile
**PUT** `/api/user/profile`
**Authorization:** Bearer token required

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "department": "IT",
  "email": "john.doe@example.com"
}
```

### 7. Get All Users (Admin)
**GET** `/api/user?pageNumber=1&pageSize=10&searchTerm=john`
**Authorization:** Bearer token required (Admin/UserManager role)

**Query Parameters:**
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `searchTerm`: Search in username, email, firstName, lastName
- `isActive`: Filter by active status

## Password Management

### 8. Change Password
**POST** `/api/user/change-password`
**Authorization:** Bearer token required

**Request Body:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

### 9. Request Password Reset
**POST** `/api/user/request-password-reset`

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

### 10. Reset Password
**POST** `/api/user/reset-password`

**Request Body:**
```json
{
  "resetToken": "reset_token_here",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

### 11. Validate Password Strength
**POST** `/api/user/validate-password`

**Request Body:**
```json
"PasswordToValidate123!"
```

**Response:**
```json
{
  "isValid": true,
  "strengthLevel": "Strong",
  "errors": []
}
```

## Two-Factor Authentication

### 12. Enable 2FA
**POST** `/api/user/enable-2fa`
**Authorization:** Bearer token required

**Response:**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/ERP%20System:john.doe@example.com?secret=JBSWY3DPEHPK3PXP&issuer=ERP%20System",
  "backupCodes": ["12345678", "87654321", "..."],
  "manualEntryKey": "JBSW Y3DP EHPK 3PXP"
}
```

### 13. Disable 2FA
**POST** `/api/user/disable-2fa`
**Authorization:** Bearer token required

### 14. Verify 2FA Code
**POST** `/api/user/verify-2fa`
**Authorization:** Bearer token required

**Request Body:**
```json
{
  "code": "123456"
}
```

## User Management (Admin)

### 15. Update User Status
**PUT** `/api/user/{userId}/status?isActive=true&isLocked=false`
**Authorization:** Bearer token required (Admin/UserManager role)

### 16. Delete User
**DELETE** `/api/user/{userId}?hardDelete=false`
**Authorization:** Bearer token required (Admin role)

### 17. Get User Statistics
**GET** `/api/user/statistics`
**Authorization:** Bearer token required (Admin/UserManager role)

**Response:**
```json
{
  "totalUsers": 150,
  "activeUsers": 120,
  "inactiveUsers": 30,
  "usersRegisteredToday": 5,
  "usersRegisteredThisMonth": 25,
  "twoFactorEnabledUsers": 75
}
```

## Utility Endpoints

### 18. Check Username Availability
**GET** `/api/user/check-username/{username}`

**Response:**
```json
{
  "username": "johndoe",
  "isAvailable": false
}
```

### 19. Check Email Availability
**GET** `/api/user/check-email/{email}`

**Response:**
```json
{
  "email": "john.doe@example.com",
  "isAvailable": true
}
```

## Error Responses

All endpoints return consistent error responses:

**400 Bad Request:**
```json
{
  "message": "Validation failed",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 12 characters"]
  }
}
```

**401 Unauthorized:**
```json
{
  "message": "Unauthorized access"
}
```

**403 Forbidden:**
```json
{
  "message": "Insufficient permissions"
}
```

**404 Not Found:**
```json
{
  "message": "User not found"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Internal server error"
}
```

## Security Features

### Password Policy
- Minimum 12 characters
- Must contain uppercase and lowercase letters
- Must contain at least one digit
- Must contain at least one special character
- Password history (prevents reuse of last 5 passwords)
- Password expiry (90 days by default)

### Account Security
- Account lockout after 5 failed login attempts
- Lockout duration: 15 minutes
- Session timeout: 30 minutes
- IP address tracking
- Device fingerprinting

### Two-Factor Authentication
- TOTP-based authentication
- QR code generation for easy setup
- Backup codes for recovery
- 30-second token validity period
- Clock drift tolerance

## Testing with Swagger

1. Start the application
2. Navigate to: `http://localhost:5104/swagger`
3. Use the "Authorize" button to add your JWT token
4. Test endpoints directly in the Swagger UI

## Authentication Flow

1. **Register** a new user or **Login** with existing credentials
2. Use the returned JWT token for subsequent requests
3. Add the token to the Authorization header: `Bearer your_jwt_token_here`
4. **Enable 2FA** for additional security (optional)
5. **Update profile** or **change password** as needed

## Sample JWT Token Usage

```bash
curl -X GET "http://localhost:5104/api/user/profile" \
     -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

All endpoints are now ready for testing and integration with your frontend application!

## Access Delegation API Endpoints

### Base URL
```
http://localhost:5104/api/accessdelegation
```

### 1. Create Access Delegation
**POST** `/api/accessdelegation`

**Request Body:**
```json
{
  "fromUserId": 123,
  "toUserId": 456,
  "startDate": "2024-01-15T08:00:00Z",
  "endDate": "2024-01-20T17:00:00Z",
  "reason": "Vacation coverage - handling customer inquiries",
  "isActive": true,
  "createdBy": 123
}
```

**Response:**
```json
{
  "success": true,
  "message": "Access delegation created successfully",
  "data": 789
}
```

### 2. Get Access Delegation by ID
**GET** `/api/accessdelegation/{id}`

**Response:**
```json
{
  "success": true,
  "message": "Access delegation retrieved successfully",
  "data": {
    "delegationId": 789,
    "fromUserId": 123,
    "toUserId": 456,
    "startDate": "2024-01-15T08:00:00Z",
    "endDate": "2024-01-20T17:00:00Z",
    "reason": "Vacation coverage - handling customer inquiries",
    "isActive": true,
    "createdBy": 123,
    "dateCreated": "2024-01-10T10:30:00Z"
  }
}
```

### 3. Update Access Delegation
**PUT** `/api/accessdelegation/{id}`

**Request Body:**
```json
{
  "delegationId": 789,
  "endDate": "2024-01-25T17:00:00Z",
  "reason": "Extended vacation coverage",
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Access delegation updated successfully",
  "data": true
}
```

### 4. Delete Access Delegation
**DELETE** `/api/accessdelegation/{id}`

**Response:**
```json
{
  "success": true,
  "message": "Access delegation deleted successfully",
  "data": true
}
```

### 5. Get All Delegations (Paginated)
**GET** `/api/accessdelegation?pageNumber=1&pageSize=10`

**Response:**
```json
{
  "success": true,
  "message": "Access delegations retrieved successfully",
  "data": {
    "delegations": [
      {
        "delegationId": 789,
        "fromUserId": 123,
        "toUserId": 456,
        "startDate": "2024-01-15T08:00:00Z",
        "endDate": "2024-01-20T17:00:00Z",
        "reason": "Vacation coverage",
        "isActive": true,
        "createdBy": 123,
        "dateCreated": "2024-01-10T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### 6. Get Current Active Delegations
**GET** `/api/accessdelegation/active?pageNumber=1&pageSize=10`

**Response:**
```json
{
  "success": true,
  "message": "Current active delegations retrieved successfully",
  "data": {
    "delegations": [
      {
        "delegationId": 789,
        "fromUserId": 123,
        "toUserId": 456,
        "startDate": "2024-01-15T08:00:00Z",
        "endDate": "2024-01-20T17:00:00Z",
        "reason": "Vacation coverage",
        "isActive": true,
        "createdBy": 123,
        "dateCreated": "2024-01-10T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 7. Get Delegations by From User
**GET** `/api/accessdelegation/from-user/{userId}`

**Response:**
```json
{
  "success": true,
  "message": "Delegations retrieved successfully",
  "data": [
    {
      "delegationId": 789,
      "fromUserId": 123,
      "toUserId": 456,
      "startDate": "2024-01-15T08:00:00Z",
      "endDate": "2024-01-20T17:00:00Z",
      "reason": "Vacation coverage",
      "isActive": true,
      "createdBy": 123,
      "dateCreated": "2024-01-10T10:30:00Z"
    }
  ]
}
```

### 8. Get Delegations by To User
**GET** `/api/accessdelegation/to-user/{userId}`

**Response:**
```json
{
  "success": true,
  "message": "Delegations retrieved successfully",
  "data": [
    {
      "delegationId": 789,
      "fromUserId": 123,
      "toUserId": 456,
      "startDate": "2024-01-15T08:00:00Z",
      "endDate": "2024-01-20T17:00:00Z",
      "reason": "Vacation coverage",
      "isActive": true,
      "createdBy": 123,
      "dateCreated": "2024-01-10T10:30:00Z"
    }
  ]
}
```

### 9. Get Delegations by From User (Paginated)
**GET** `/api/accessdelegation/from-user/{userId}/paged?pageNumber=1&pageSize=10&includeInactive=false`

**Response:**
```json
{
  "success": true,
  "message": "Delegations retrieved successfully",
  "data": {
    "delegations": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### 10. Get Delegations by To User (Paginated)
**GET** `/api/accessdelegation/to-user/{userId}/paged?pageNumber=1&pageSize=10&includeInactive=false`

**Response:**
```json
{
  "success": true,
  "message": "Delegations retrieved successfully",
  "data": {
    "delegations": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 8,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 11. Search Delegations
**POST** `/api/accessdelegation/search`

**Request Body:**
```json
{
  "fromUserId": 123,
  "toUserId": null,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "isActive": true,
  "searchText": "vacation",
  "pageNumber": 1,
  "pageSize": 10
}
```

**Response:**
```json
{
  "success": true,
  "message": "Search completed successfully",
  "data": {
    "delegations": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 3,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 12. Get Delegations by Date Range
**POST** `/api/accessdelegation/date-range`

**Request Body:**
```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z",
  "pageNumber": 1,
  "pageSize": 10,
  "activeOnly": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Delegations retrieved successfully",
  "data": {
    "delegations": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 12,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### 13. Get Delegation History by User
**GET** `/api/accessdelegation/user/{userId}/history?pageNumber=1&pageSize=10`

**Response:**
```json
{
  "success": true,
  "message": "Delegation history retrieved successfully",
  "data": {
    "history": [
      {
        "delegationId": 789,
        "fromUserId": 123,
        "toUserId": 456,
        "startDate": "2024-01-15T08:00:00Z",
        "endDate": "2024-01-20T17:00:00Z",
        "reason": "Vacation coverage",
        "isActive": false,
        "createdBy": 123,
        "dateCreated": "2024-01-10T10:30:00Z",
        "delegationRole": "Delegator"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 14. Extend Delegation
**POST** `/api/accessdelegation/extend`

**Request Body:**
```json
{
  "delegationId": 789,
  "newEndDate": "2024-01-25T17:00:00Z"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Delegation extended successfully",
  "data": true
}
```

### 15. Check Active Delegation
**GET** `/api/accessdelegation/check-active/{fromUserId}/{toUserId}`

**Response:**
```json
{
  "success": true,
  "message": "Delegation status checked successfully",
  "data": true
}
```

### 16. Get Delegation Statistics
**POST** `/api/accessdelegation/statistics`

**Request Body:**
```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Statistics retrieved successfully",
  "data": {
    "totalDelegations": 150,
    "activeDelegations": 25,
    "expiredDelegations": 100,
    "upcomingDelegations": 25,
    "averageDurationDays": 7.5
  }
}
```

### 17. Get Most Active Delegators
**POST** `/api/accessdelegation/most-active-delegators`

**Request Body:**
```json
{
  "limit": 10,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Most active delegators retrieved successfully",
  "data": [
    {
      "userId": 123,
      "delegationCount": 15
    },
    {
      "userId": 456,
      "delegationCount": 12
    }
  ]
}
```

### 18. Get Most Popular Delegates
**POST** `/api/accessdelegation/most-popular-delegates`

**Request Body:**
```json
{
  "limit": 10,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Most popular delegates retrieved successfully",
  "data": [
    {
      "userId": 789,
      "delegationCount": 20
    },
    {
      "userId": 321,
      "delegationCount": 18
    }
  ]
}
```

### 19. Deactivate Expired Delegations
**POST** `/api/accessdelegation/deactivate-expired`

**Response:**
```json
{
  "success": true,
  "message": "Successfully deactivated 5 expired delegations",
  "data": 5
}
```

## Access Delegation Error Responses

### Validation Error Example
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Start date must be before end date",
    "Cannot delegate to yourself"
  ]
}
```

### Not Found Error Example
```json
{
  "success": false,
  "message": "Access delegation not found",
  "errors": [
    "Delegation with ID 999 not found"
  ]
}
```

### Server Error Example
```json
{
  "success": false,
  "message": "An error occurred while creating the delegation",
  "errors": [
    "Database connection failed"
  ]
}
```

## Access Delegation Usage Examples

### Example 1: Creating a Vacation Coverage Delegation
```bash
curl -X POST "http://localhost:5104/api/accessdelegation" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer your_jwt_token_here" \
     -d '{
       "fromUserId": 123,
       "toUserId": 456,
       "startDate": "2024-02-01T08:00:00Z",
       "endDate": "2024-02-14T17:00:00Z",
       "reason": "Annual vacation - delegate all customer service responsibilities",
       "isActive": true,
       "createdBy": 123
     }'
```

### Example 2: Checking Active Delegation Status
```bash
curl -X GET "http://localhost:5104/api/accessdelegation/check-active/123/456" \
     -H "Authorization: Bearer your_jwt_token_here"
```

### Example 3: Extending a Delegation
```bash
curl -X POST "http://localhost:5104/api/accessdelegation/extend" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer your_jwt_token_here" \
     -d '{
       "delegationId": 789,
       "newEndDate": "2024-02-21T17:00:00Z"
     }'
```

### Example 4: Getting User's Delegation History
```bash
curl -X GET "http://localhost:5104/api/accessdelegation/user/123/history?pageNumber=1&pageSize=20" \
     -H "Authorization: Bearer your_jwt_token_here"
```

### Example 5: Searching Delegations
```bash
curl -X POST "http://localhost:5104/api/accessdelegation/search" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer your_jwt_token_here" \
     -d '{
       "fromUserId": 123,
       "isActive": true,
       "searchText": "vacation",
       "pageNumber": 1,
       "pageSize": 10
     }'
```

### Example 6: Getting Delegation Statistics
```bash
curl -X POST "http://localhost:5104/api/accessdelegation/statistics" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer your_jwt_token_here" \
     -d '{
       "startDate": "2024-01-01T00:00:00Z",
       "endDate": "2024-12-31T23:59:59Z"
     }'
```

All Access Delegation endpoints are now ready for testing and integration with your frontend application!
