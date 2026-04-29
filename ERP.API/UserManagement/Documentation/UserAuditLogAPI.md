# User Audit Log API Documentation

This document outlines the API endpoints available for managing user audit logs in the ERP system.

## Base URL

All endpoints are relative to `/api/UserAuditLog`.

## Authentication

All endpoints require authentication via Bearer token unless explicitly stated otherwise. Admin-only endpoints require the "Admin" role.

## Endpoints

### Create Audit Log Entry

Creates a new user audit log entry.

- **URL**: `/`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: None
- **Request Body**:
  ```json
  {
    "userId": 123,
    "action": "Login",
    "entityName": "UserAccount",
    "entityId": "user123",
    "details": "User logged in from Chrome on Windows",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
  }
  ```
- **Success Response**:
  - **Code**: 201 CREATED
  - **Content**:
    ```json
    {
      "auditLogId": 1001,
      "userId": 123,
      "username": "johnsmith",
      "action": "Login",
      "timestamp": "2025-07-03T10:30:00Z",
      "entityName": "UserAccount",
      "entityId": "user123",
      "details": "User logged in from Chrome on Windows",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
    }
    ```
- **Error Response**:
  - **Code**: 400 BAD REQUEST
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "UserId": ["The UserId field is required."],
        "Action": ["The Action field is required."]
      }
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while creating the audit log entry"
    }
    ```

### Get Audit Log By ID

Retrieves an audit log entry by its ID.

- **URL**: `/{auditLogId}`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `auditLogId=[int]` (required): The ID of the audit log entry
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "auditLogId": 1001,
      "userId": 123,
      "username": "johnsmith",
      "action": "Login",
      "timestamp": "2025-07-03T10:30:00Z",
      "entityName": "UserAccount",
      "entityId": "user123",
      "details": "User logged in from Chrome on Windows",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
    }
    ```
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Audit log with ID 1001 not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving the audit log entry"
    }
    ```

### Update Audit Log Entry

Updates an audit log entry.

- **URL**: `/{auditLogId}`
- **Method**: `PUT`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `auditLogId=[int]` (required): The ID of the audit log entry
- **Request Body**:
  ```json
  {
    "details": "Updated description of the event",
    "entityName": "UpdatedEntityName"
  }
  ```
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "auditLogId": 1001,
      "userId": 123,
      "username": "johnsmith",
      "action": "Login",
      "timestamp": "2025-07-03T10:30:00Z",
      "entityName": "UpdatedEntityName",
      "entityId": "user123",
      "details": "Updated description of the event",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
    }
    ```
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Audit log with ID 1001 not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while updating the audit log entry"
    }
    ```

### Delete Audit Log Entry

Deletes an audit log entry.

- **URL**: `/{auditLogId}`
- **Method**: `DELETE`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `auditLogId=[int]` (required): The ID of the audit log entry
- **Success Response**:
  - **Code**: 204 NO CONTENT
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Audit log with ID 1001 not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while deleting the audit log entry"
    }
    ```

### Search Audit Logs

Searches audit logs based on provided filters.

- **URL**: `/search`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Request Body**:
  ```json
  {
    "userId": 123,
    "action": "Login",
    "entityName": "UserAccount",
    "entityId": "user123",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-07-05T23:59:59Z",
    "ipAddress": "192.168.1.1",
    "pageNumber": 1,
    "pageSize": 20,
    "sortBy": "timestamp",
    "sortDirection": "desc"
  }
  ```
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3,
      "results": [
        {
          "auditLogId": 1001,
          "userId": 123,
          "username": "johnsmith",
          "action": "Login",
          "timestamp": "2025-07-03T10:30:00Z",
          "entityName": "UserAccount",
          "entityId": "user123",
          "details": "User logged in from Chrome on Windows",
          "ipAddress": "192.168.1.1",
          "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
        },
        // Additional results...
      ]
    }
    ```
- **Error Response**:
  - **Code**: 400 BAD REQUEST
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "EndDate": ["End date must be greater than or equal to start date."]
      }
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while searching audit logs"
    }
    ```

### Get User Activity

Retrieves activity logs for a specific user.

- **URL**: `/user/{userId}/activity`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin or Self
- **URL Parameters**:
  - `userId=[int]` (required): The ID of the user
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Filter by start date
  - `endDate=[datetime]` (optional): Filter by end date
  - `pageNumber=[int]` (optional): Page number, default is 1
  - `pageSize=[int]` (optional): Page size, default is 20
  - `sortBy=[string]` (optional): Field to sort by, default is "timestamp"
  - `sortDirection=[string]` (optional): Sort direction, "asc" or "desc", default is "desc"
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3,
      "results": [
        {
          "auditLogId": 1001,
          "userId": 123,
          "username": "johnsmith",
          "action": "Login",
          "timestamp": "2025-07-03T10:30:00Z",
          "entityName": "UserAccount",
          "entityId": "user123",
          "details": "User logged in from Chrome on Windows",
          "ipAddress": "192.168.1.1",
          "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
        },
        // Additional results...
      ]
    }
    ```
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "User with ID 123 not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving user activity"
    }
    ```

### Get Analytics

Retrieves analytics for user audit logs.

- **URL**: `/analytics`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Filter by start date
  - `endDate=[datetime]` (optional): Filter by end date
  - `groupBy=[string]` (optional): Group results by ("action", "user", "entityName", "date", "hour")
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "startDate": "2025-07-01T00:00:00Z",
      "endDate": "2025-07-05T23:59:59Z",
      "totalEvents": 1245,
      "uniqueUsers": 87,
      "actionDistribution": [
        {
          "action": "Login",
          "count": 352
        },
        {
          "action": "Logout",
          "count": 298
        },
        {
          "action": "DataUpdate",
          "count": 595
        }
      ],
      "timeDistribution": [
        {
          "date": "2025-07-01",
          "count": 245
        },
        {
          "date": "2025-07-02",
          "count": 312
        },
        // Additional data...
      ]
    }
    ```
- **Error Response**:
  - **Code**: 400 BAD REQUEST
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "GroupBy": ["Invalid group by parameter. Allowed values are: action, user, entityName, date, hour"]
      }
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving analytics data"
    }
    ```

### Clean Up Audit Logs

Removes audit logs based on specified criteria (admin operation).

- **URL**: `/cleanup`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Request Body**:
  ```json
  {
    "olderThan": "2025-01-01T00:00:00Z",
    "actions": ["Login", "Logout"],
    "entityNames": ["UserAccount"],
    "preserveImportant": true
  }
  ```
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "recordsDeleted": 1532,
      "spaceFreed": "2.3 MB",
      "remainingRecords": 4298
    }
    ```
- **Error Response**:
  - **Code**: 400 BAD REQUEST
    ```json
    {
      "message": "OlderThan date is required for cleanup operation"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while cleaning up audit logs"
    }
    ```

### Export Audit Logs

Exports audit logs to CSV/JSON format based on filters.

- **URL**: `/export`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Request Body**:
  ```json
  {
    "userId": 123,
    "action": "Login",
    "entityName": "UserAccount",
    "startDate": "2025-07-01T00:00:00Z",
    "endDate": "2025-07-05T23:59:59Z",
    "format": "csv"
  }
  ```
- **Success Response**:
  - **Code**: 200 OK
  - **Content**: File download
- **Error Response**:
  - **Code**: 400 BAD REQUEST
    ```json
    {
      "message": "Invalid export format. Allowed values are: csv, json"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while exporting audit logs"
    }
    ```

### Get Security Analytics

Retrieves security-focused analytics from the audit logs.

- **URL**: `/security-analytics`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `days=[int]` (optional): Number of days to analyze, default is 30
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "failedLogins": 45,
      "unusualIpAddresses": [
        {
          "ipAddress": "203.0.113.42",
          "country": "Unknown",
          "count": 5,
          "users": ["johnsmith", "admin"]
        }
      ],
      "accountsWithMultipleDevices": [
        {
          "userId": 123,
          "username": "johnsmith",
          "deviceCount": 5
        }
      ],
      "suspiciousActivities": [
        {
          "type": "MultipleFailedLogins",
          "count": 15,
          "details": "5+ failed login attempts for 3 users"
        }
      ]
    }
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving security analytics"
    }
    ```

## Data Models

### Create Audit Log Request
```json
{
  "userId": 123,
  "action": "string",
  "entityName": "string",
  "entityId": "string",
  "details": "string",
  "ipAddress": "string",
  "userAgent": "string"
}
```

### Update Audit Log Request
```json
{
  "details": "string",
  "entityName": "string",
  "entityId": "string"
}
```

### Audit Log Response
```json
{
  "auditLogId": 0,
  "userId": 0,
  "username": "string",
  "action": "string",
  "timestamp": "2025-07-03T10:30:00Z",
  "entityName": "string",
  "entityId": "string",
  "details": "string",
  "ipAddress": "string",
  "userAgent": "string"
}
```

### Search Request
```json
{
  "userId": 0,
  "action": "string",
  "entityName": "string",
  "entityId": "string",
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-07-05T23:59:59Z",
  "ipAddress": "string",
  "pageNumber": 1,
  "pageSize": 20,
  "sortBy": "string",
  "sortDirection": "asc|desc"
}
```

### Cleanup Request
```json
{
  "olderThan": "2025-01-01T00:00:00Z",
  "actions": ["string"],
  "entityNames": ["string"],
  "preserveImportant": true
}
```

### Export Request
```json
{
  "userId": 0,
  "action": "string",
  "entityName": "string", 
  "startDate": "2025-07-01T00:00:00Z",
  "endDate": "2025-07-05T23:59:59Z",
  "format": "csv|json"
}
```
