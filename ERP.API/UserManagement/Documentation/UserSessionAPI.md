# User Session API Documentation

This document outlines the API endpoints available for managing user sessions in the ERP system.

## Base URL

All endpoints are relative to `/api/UserSession`.

## Authentication

All endpoints require authentication via Bearer token unless explicitly stated otherwise. Admin-only endpoints require the "Admin" role.

## Endpoints

### Create Session

Creates a new user session.

- **URL**: `/`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: None
- **Request Body**:
  ```json
  {
    "sessionId": "string",
    "userId": 123,
    "ipAddress": "192.168.1.1",
    "deviceInfo": "Chrome 100.0.4896.60",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
  }
  ```
- **Success Response**:
  - **Code**: 201 CREATED
  - **Content**:
    ```json
    {
      "sessionId": "string",
      "userId": 123,
      "username": "johnsmith",
      "loginTime": "2025-07-03T10:30:00Z",
      "logoutTime": null,
      "ipAddress": "192.168.1.1",
      "deviceInfo": "Chrome 100.0.4896.60",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...",
      "isActive": true,
      "sessionDuration": null
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
        "SessionId": ["The SessionId field is required."],
        "UserId": ["The UserId field is required."]
      }
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while creating the session"
    }
    ```

### Get Session By ID

Retrieves a session by its ID.

- **URL**: `/{sessionId}`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: None or Admin for other users' sessions
- **URL Parameters**:
  - `sessionId=[string]` (required): The ID of the session
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "sessionId": "string",
      "userId": 123,
      "username": "johnsmith",
      "loginTime": "2025-07-03T10:30:00Z",
      "logoutTime": null,
      "ipAddress": "192.168.1.1",
      "deviceInfo": "Chrome 100.0.4896.60",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...",
      "isActive": true,
      "sessionDuration": "PT1H30M15S"
    }
    ```
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Session with ID string not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving the session"
    }
    ```

### Update Session

Updates a session (typically to end it).

- **URL**: `/{sessionId}`
- **Method**: `PUT`
- **Auth required**: Yes
- **Permissions required**: None or Admin for other users' sessions
- **URL Parameters**:
  - `sessionId=[string]` (required): The ID of the session
- **Request Body**:
  ```json
  {
    "logoutTime": "2025-07-03T12:30:00Z",
    "isActive": false
  }
  ```
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "sessionId": "string",
      "userId": 123,
      "username": "johnsmith",
      "loginTime": "2025-07-03T10:30:00Z",
      "logoutTime": "2025-07-03T12:30:00Z",
      "ipAddress": "192.168.1.1",
      "deviceInfo": "Chrome 100.0.4896.60",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...",
      "isActive": false,
      "sessionDuration": "PT2H"
    }
    ```
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Session with ID string not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while updating the session"
    }
    ```

### Delete Session

Deletes a session.

- **URL**: `/{sessionId}`
- **Method**: `DELETE`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `sessionId=[string]` (required): The ID of the session
- **Success Response**:
  - **Code**: 204 NO CONTENT
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Session with ID string not found"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while deleting the session"
    }
    ```

### End Session

Ends a user session (marks as inactive and sets logout time).

- **URL**: `/{sessionId}/end`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: None or Admin for other users' sessions
- **URL Parameters**:
  - `sessionId=[string]` (required): The ID of the session
- **Success Response**:
  - **Code**: 204 NO CONTENT
- **Error Response**:
  - **Code**: 404 NOT FOUND
    ```json
    {
      "message": "Session with ID string not found or already inactive"
    }
    ```
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while ending the session"
    }
    ```

### End All User Sessions

Ends all active sessions for a user (except optionally the current session).

- **URL**: `/users/{userId}/end-all`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `userId=[integer]` (required): The ID of the user
- **Query Parameters**:
  - `currentSessionId=[string]` (optional): The current session ID to exclude
- **Success Response**:
  - **Code**: 200 OK
  - **Content**: `5` (number of sessions ended)
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while ending user sessions"
    }
    ```

### Get Active Sessions

Gets all active sessions with pagination.

- **URL**: `/active`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `pageNumber=[integer]` (optional, default=1): The page number
  - `pageSize=[integer]` (optional, default=10): The page size
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "sessions": [
        {
          "sessionId": "string1",
          "userId": 123,
          "username": "johnsmith",
          "loginTime": "2025-07-03T10:30:00Z",
          "logoutTime": null,
          "ipAddress": "192.168.1.1",
          "deviceInfo": "Chrome 100.0.4896.60",
          "userAgent": "Mozilla/5.0...",
          "isActive": true,
          "sessionDuration": "PT1H30M15S"
        },
        {
          "sessionId": "string2",
          "userId": 456,
          "username": "janedoe",
          "loginTime": "2025-07-03T11:00:00Z",
          "logoutTime": null,
          "ipAddress": "192.168.1.2",
          "deviceInfo": "Firefox 98.0",
          "userAgent": "Mozilla/5.0...",
          "isActive": true,
          "sessionDuration": "PT1H0M15S"
        }
      ],
      "totalCount": 42,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 5
    }
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving active sessions"
    }
    ```

### Get User Session History

Gets session history for a user with pagination and optional date filtering.

- **URL**: `/users/{userId}/history`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: None or Admin for other users' sessions
- **URL Parameters**:
  - `userId=[integer]` (required): The ID of the user
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Start date filter
  - `endDate=[datetime]` (optional): End date filter
  - `pageNumber=[integer]` (optional, default=1): The page number
  - `pageSize=[integer]` (optional, default=10): The page size
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "sessions": [
        {
          "sessionId": "string1",
          "userId": 123,
          "username": "johnsmith",
          "loginTime": "2025-07-02T10:30:00Z",
          "logoutTime": "2025-07-02T12:30:00Z",
          "ipAddress": "192.168.1.1",
          "deviceInfo": "Chrome 100.0.4896.60",
          "userAgent": "Mozilla/5.0...",
          "isActive": false,
          "sessionDuration": "PT2H"
        },
        {
          "sessionId": "string2",
          "userId": 123,
          "username": "johnsmith",
          "loginTime": "2025-07-03T11:00:00Z",
          "logoutTime": null,
          "ipAddress": "192.168.1.1",
          "deviceInfo": "Chrome 100.0.4896.60",
          "userAgent": "Mozilla/5.0...",
          "isActive": true,
          "sessionDuration": "PT1H30M15S"
        }
      ],
      "totalCount": 15,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 2
    }
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving session history"
    }
    ```

### Get Session Statistics

Gets session statistics by date range.

- **URL**: `/statistics`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Start date filter
  - `endDate=[datetime]` (optional): End date filter
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "date": "2025-07-01",
        "totalSessions": 235,
        "uniqueUsers": 120,
        "averageSessionDuration": "PT45M30S"
      },
      {
        "date": "2025-07-02",
        "totalSessions": 253,
        "uniqueUsers": 132,
        "averageSessionDuration": "PT50M15S"
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving session statistics"
    }
    ```

### Get User Active Sessions

Gets active sessions for a user.

- **URL**: `/users/{userId}/active`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: None or Admin for other users' sessions
- **URL Parameters**:
  - `userId=[integer]` (required): The ID of the user
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "sessionId": "string1",
        "userId": 123,
        "loginTime": "2025-07-03T10:30:00Z",
        "ipAddress": "192.168.1.1",
        "deviceInfo": "Chrome 100.0.4896.60",
        "sessionDuration": "PT1H30M15S"
      },
      {
        "sessionId": "string2",
        "userId": 123,
        "loginTime": "2025-07-03T11:00:00Z",
        "ipAddress": "192.168.1.2",
        "deviceInfo": "Mobile Safari 15.4",
        "sessionDuration": "PT1H0M15S"
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving user's active sessions"
    }
    ```

### Cleanup Old Sessions

Cleans up old inactive sessions.

- **URL**: `/cleanup`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `daysOld=[integer]` (optional, default=90): Age in days for sessions to remove
- **Success Response**:
  - **Code**: 200 OK
  - **Content**: `125` (number of sessions removed)
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while cleaning up old sessions"
    }
    ```

### Fix Incomplete Sessions

Fixes incomplete sessions (sessions still marked active but older than 24 hours).

- **URL**: `/fix-incomplete`
- **Method**: `POST`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Success Response**:
  - **Code**: 200 OK
  - **Content**: `8` (number of sessions fixed)
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while fixing incomplete sessions"
    }
    ```

### Get Concurrent Sessions

Gets concurrent sessions count by time slots.

- **URL**: `/concurrent`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Start date filter
  - `endDate=[datetime]` (optional): End date filter
  - `intervalMinutes=[integer]` (optional, default=60): Interval in minutes for the time slots
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "timeSlot": "2025-07-01T09:00:00Z",
        "concurrentSessions": 45
      },
      {
        "timeSlot": "2025-07-01T10:00:00Z",
        "concurrentSessions": 78
      },
      {
        "timeSlot": "2025-07-01T11:00:00Z",
        "concurrentSessions": 102
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving concurrent sessions"
    }
    ```

### Get User Login Frequency

Gets user login frequency statistics.

- **URL**: `/login-frequency`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `topN=[integer]` (optional, default=10): Number of users to return
  - `startDate=[datetime]` (optional): Start date filter
  - `endDate=[datetime]` (optional): End date filter
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "userId": 123,
        "username": "johnsmith",
        "fullName": "John Smith",
        "loginCount": 45,
        "lastLogin": "2025-07-03T10:30:00Z"
      },
      {
        "userId": 456,
        "username": "janedoe",
        "fullName": "Jane Doe",
        "loginCount": 38,
        "lastLogin": "2025-07-03T11:00:00Z"
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving user login frequency"
    }
    ```

### Get Sessions By IP

Gets sessions by IP address with pagination.

- **URL**: `/ip/{ipAddress}`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **URL Parameters**:
  - `ipAddress=[string]` (required): The IP address to search for
- **Query Parameters**:
  - `pageNumber=[integer]` (optional, default=1): The page number
  - `pageSize=[integer]` (optional, default=10): The page size
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "sessions": [
        {
          "sessionId": "string1",
          "userId": 123,
          "username": "johnsmith",
          "loginTime": "2025-07-03T10:30:00Z",
          "logoutTime": null,
          "deviceInfo": "Chrome 100.0.4896.60",
          "isActive": true,
          "totalCount": 15
        },
        {
          "sessionId": "string2",
          "userId": 456,
          "username": "janedoe",
          "loginTime": "2025-07-03T11:00:00Z",
          "logoutTime": "2025-07-03T12:00:00Z",
          "deviceInfo": "Firefox 98.0",
          "isActive": false,
          "totalCount": 15
        }
      ],
      "totalCount": 15,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 2
    }
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving sessions by IP"
    }
    ```

### Get Device Statistics

Gets device usage statistics.

- **URL**: `/device-statistics`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `startDate=[datetime]` (optional): Start date filter
  - `endDate=[datetime]` (optional): End date filter
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "deviceInfo": "Chrome 100.0.4896.60",
        "userCount": 85,
        "sessionCount": 245,
        "averageSessionDuration": "PT45M30S"
      },
      {
        "deviceInfo": "Firefox 98.0",
        "userCount": 42,
        "sessionCount": 120,
        "averageSessionDuration": "PT50M15S"
      },
      {
        "deviceInfo": "Mobile Safari 15.4",
        "userCount": 65,
        "sessionCount": 180,
        "averageSessionDuration": "PT30M45S"
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving device statistics"
    }
    ```

### Get Long Running Sessions

Gets long-running active sessions.

- **URL**: `/long-running`
- **Method**: `GET`
- **Auth required**: Yes
- **Permissions required**: Admin
- **Query Parameters**:
  - `hoursThreshold=[integer]` (optional, default=8): Minimum duration in hours
- **Success Response**:
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "sessionId": "string1",
        "userId": 123,
        "username": "johnsmith",
        "loginTime": "2025-07-02T10:30:00Z",
        "ipAddress": "192.168.1.1",
        "deviceInfo": "Chrome 100.0.4896.60",
        "sessionDuration": "PT25H30M15S"
      },
      {
        "sessionId": "string2",
        "userId": 456,
        "username": "janedoe",
        "loginTime": "2025-07-02T16:00:00Z",
        "ipAddress": "192.168.1.2",
        "deviceInfo": "Firefox 98.0",
        "sessionDuration": "PT20H0M15S"
      }
    ]
    ```
- **Error Response**:
  - **Code**: 500 INTERNAL SERVER ERROR
    ```json
    {
      "message": "An error occurred while retrieving long running sessions"
    }
    ```

## Usage Examples

### Creating a new session

```
POST /api/UserSession
Content-Type: application/json
Authorization: Bearer <token>

{
  "sessionId": "session-guid-12345",
  "userId": 123,
  "ipAddress": "192.168.1.1",
  "deviceInfo": "Chrome 100.0.4896.60",
  "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
}
```

### Ending a session

```
POST /api/UserSession/session-guid-12345/end
Authorization: Bearer <token>
```

### Getting session statistics for the last month

```
GET /api/UserSession/statistics?startDate=2025-06-01T00:00:00Z&endDate=2025-07-01T00:00:00Z
Authorization: Bearer <token>
```

### Cleaning up old sessions

```
POST /api/UserSession/cleanup?daysOld=60
Authorization: Bearer <token>
```

### Getting concurrent session data with 30-minute intervals

```
GET /api/UserSession/concurrent?intervalMinutes=30&startDate=2025-07-01T00:00:00Z&endDate=2025-07-02T00:00:00Z
Authorization: Bearer <token>
```
