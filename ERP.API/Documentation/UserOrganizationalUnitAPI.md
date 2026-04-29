# User-Organizational Unit API Documentation

This document describes the API endpoints for managing user assignments to organizational units.

## Table of Contents

- [Overview](#overview)
- [Base URL](#base-url)
- [Authentication](#authentication)
- [Endpoints](#endpoints)
  - [Assign User to Unit](#assign-user-to-unit)
  - [Get User-Unit Assignment](#get-user-unit-assignment)
  - [Update User-Unit Assignment](#update-user-unit-assignment)
  - [Remove User from Unit](#remove-user-from-unit)
  - [Get Users in Unit](#get-users-in-unit)
  - [Get User Units](#get-user-units)
  - [Get User Primary Unit](#get-user-primary-unit)
  - [Set User Primary Unit](#set-user-primary-unit)
  - [Bulk Assign Users to Unit](#bulk-assign-users-to-unit)
  - [Assign User to Multiple Units](#assign-user-to-multiple-units)
  - [Get Users Without Unit](#get-users-without-unit)
  - [Get Empty Units](#get-empty-units)
  - [Get Unit Assignment Stats](#get-unit-assignment-stats)
  - [Get Paginated Assignments](#get-paginated-assignments)
  - [Get Unit Type Statistics](#get-unit-type-statistics)
  - [Transfer Users Between Units](#transfer-users-between-units)
  - [Ensure Users Have Primary Unit](#ensure-users-have-primary-unit)
- [Data Models](#data-models)

## Overview

The User-Organizational Unit API allows you to manage assignments between users and organizational units in the system. It provides endpoints for creating, retrieving, updating, and deleting these assignments, as well as various bulk operations and statistical reports.

## Base URL

All API endpoints are relative to `/api/UserOrganizationalUnit`.

## Authentication

All endpoints require authentication. Include a valid JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Endpoints

### Assign User to Unit

Assigns a user to an organizational unit.

- **URL:** `/api/UserOrganizationalUnit/assign`
- **Method:** `POST`
- **Auth Required:** Yes

**Request Body:**
```json
{
  "userId": 123,
  "unitId": 456,
  "isPrimary": true,
  "assignedBy": 789
}
```

**Response:**
```json
{
  "success": true,
  "message": "User successfully assigned to organizational unit"
}
```

### Get User-Unit Assignment

Gets details about a specific user-to-unit assignment.

- **URL:** `/api/UserOrganizationalUnit/{userId}/{unitId}`
- **Method:** `GET`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user
- `unitId` - ID of the organizational unit

**Response:**
```json
{
  "userId": 123,
  "username": "jdoe",
  "userFullName": "John Doe",
  "email": "john.doe@example.com",
  "unitId": 456,
  "unitName": "Sales Department",
  "unitType": "Department",
  "isPrimary": true,
  "dateAssigned": "2023-05-15T10:30:00Z",
  "assignedBy": 789,
  "assignedByName": "Jane Smith"
}
```

### Update User-Unit Assignment

Updates a user's assignment to an organizational unit (primary status).

- **URL:** `/api/UserOrganizationalUnit/{userId}/{unitId}`
- **Method:** `PUT`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user
- `unitId` - ID of the organizational unit

**Request Body:**
```json
{
  "isPrimary": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "User assignment updated successfully"
}
```

### Remove User from Unit

Removes a user from an organizational unit.

- **URL:** `/api/UserOrganizationalUnit/{userId}/{unitId}`
- **Method:** `DELETE`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user
- `unitId` - ID of the organizational unit

**Response:**
```json
{
  "success": true,
  "message": "User successfully removed from organizational unit"
}
```

### Get Users in Unit

Gets all users assigned to a specific organizational unit.

- **URL:** `/api/UserOrganizationalUnit/unit/{unitId}`
- **Method:** `GET`
- **Auth Required:** Yes

**Path Parameters:**
- `unitId` - ID of the organizational unit

**Query Parameters:**
- `includeChildUnits` (optional) - Whether to include users from child units (default: false)

**Response:**
```json
[
  {
    "userId": 123,
    "username": "jdoe",
    "userFullName": "John Doe",
    "email": "john.doe@example.com",
    "unitId": 456,
    "unitName": "Sales Department",
    "unitType": "Department",
    "isPrimary": true,
    "dateAssigned": "2023-05-15T10:30:00Z"
  },
  {
    "userId": 124,
    "username": "asmith",
    "userFullName": "Alice Smith",
    "email": "alice.smith@example.com",
    "unitId": 456,
    "unitName": "Sales Department",
    "unitType": "Department",
    "isPrimary": true,
    "dateAssigned": "2023-06-20T09:15:00Z"
  }
]
```

### Get User Units

Gets all organizational units a user is assigned to.

- **URL:** `/api/UserOrganizationalUnit/user/{userId}`
- **Method:** `GET`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user

**Response:**
```json
[
  {
    "userId": 123,
    "unitId": 456,
    "unitName": "Sales Department",
    "unitType": "Department",
    "isPrimary": true,
    "dateAssigned": "2023-05-15T10:30:00Z"
  },
  {
    "userId": 123,
    "unitId": 457,
    "unitName": "Marketing Team",
    "unitType": "Team",
    "isPrimary": false,
    "dateAssigned": "2023-07-10T14:45:00Z"
  }
]
```

### Get User Primary Unit

Gets a user's primary organizational unit.

- **URL:** `/api/UserOrganizationalUnit/user/{userId}/primary`
- **Method:** `GET`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user

**Response:**
```json
{
  "userId": 123,
  "unitId": 456,
  "unitName": "Sales Department",
  "unitType": "Department",
  "isPrimary": true,
  "dateAssigned": "2023-05-15T10:30:00Z"
}
```

### Set User Primary Unit

Sets a user's primary organizational unit.

- **URL:** `/api/UserOrganizationalUnit/user/{userId}/primary/{unitId}`
- **Method:** `PUT`
- **Auth Required:** Yes

**Path Parameters:**
- `userId` - ID of the user
- `unitId` - ID of the organizational unit to set as primary

**Response:**
```json
{
  "success": true,
  "message": "Primary organizational unit updated successfully"
}
```

### Bulk Assign Users to Unit

Assigns multiple users to an organizational unit.

- **URL:** `/api/UserOrganizationalUnit/bulk-assign-to-unit`
- **Method:** `POST`
- **Auth Required:** Yes

**Request Body:**
```json
{
  "userIds": [123, 124, 125],
  "unitId": 456,
  "makePrimary": false,
  "assignedBy": 789
}
```

**Response:**
```json
{
  "success": true,
  "message": "Users assigned to organizational unit successfully",
  "affectedCount": 3
}
```

### Assign User to Multiple Units

Assigns a user to multiple organizational units.

- **URL:** `/api/UserOrganizationalUnit/assign-to-multiple-units`
- **Method:** `POST`
- **Auth Required:** Yes

**Request Body:**
```json
{
  "userId": 123,
  "unitIds": [456, 457, 458],
  "primaryUnitId": 456,
  "assignedBy": 789
}
```

**Response:**
```json
{
  "success": true,
  "message": "User assigned to organizational units successfully",
  "affectedCount": 3
}
```

### Get Users Without Unit

Gets users that aren't assigned to any organizational unit.

- **URL:** `/api/UserOrganizationalUnit/users-without-unit`
- **Method:** `GET`
- **Auth Required:** Yes

**Response:**
```json
[
  {
    "userId": 126,
    "username": "bwilson",
    "userFullName": "Bob Wilson",
    "email": "bob.wilson@example.com"
  },
  {
    "userId": 127,
    "username": "cjones",
    "userFullName": "Carol Jones",
    "email": "carol.jones@example.com"
  }
]
```

### Get Empty Units

Gets organizational units that have no users assigned.

- **URL:** `/api/UserOrganizationalUnit/empty-units`
- **Method:** `GET`
- **Auth Required:** Yes

**Response:**
```json
[
  {
    "unitId": 459,
    "unitName": "Research Team",
    "unitType": "Team"
  },
  {
    "unitId": 460,
    "unitName": "QA Department",
    "unitType": "Department"
  }
]
```

### Get Unit Assignment Stats

Gets assignment statistics for all organizational units.

- **URL:** `/api/UserOrganizationalUnit/unit-stats`
- **Method:** `GET`
- **Auth Required:** Yes

**Response:**
```json
[
  {
    "unitId": 456,
    "unitName": "Sales Department",
    "unitType": "Department",
    "totalUsers": 25,
    "primaryUsers": 15
  },
  {
    "unitId": 457,
    "unitName": "Marketing Team",
    "unitType": "Team",
    "totalUsers": 12,
    "primaryUsers": 8
  }
]
```

### Get Paginated Assignments

Gets a paginated list of user-unit assignments with search capabilities.

- **URL:** `/api/UserOrganizationalUnit/paginated`
- **Method:** `GET`
- **Auth Required:** Yes

**Query Parameters:**
- `pageNumber` (optional) - Page number (default: 1)
- `pageSize` (optional) - Number of items per page (default: 10)
- `searchTerm` (optional) - Search term to filter results
- `unitId` (optional) - Filter by unit ID
- `isPrimary` (optional) - Filter by primary status

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "items": [
    {
      "userId": 123,
      "username": "jdoe",
      "userFullName": "John Doe",
      "email": "john.doe@example.com",
      "unitId": 456,
      "unitName": "Sales Department",
      "unitType": "Department",
      "isPrimary": true,
      "dateAssigned": "2023-05-15T10:30:00Z",
      "assignedBy": 789,
      "assignedByName": "Jane Smith"
    },
    {
      "userId": 124,
      "username": "asmith",
      "userFullName": "Alice Smith",
      "email": "alice.smith@example.com",
      "unitId": 456,
      "unitName": "Sales Department",
      "unitType": "Department",
      "isPrimary": true,
      "dateAssigned": "2023-06-20T09:15:00Z",
      "assignedBy": 789,
      "assignedByName": "Jane Smith"
    }
    // 8 more items...
  ]
}
```

### Get Unit Type Statistics

Gets statistics about user assignments by unit type.

- **URL:** `/api/UserOrganizationalUnit/unit-type-stats`
- **Method:** `GET`
- **Auth Required:** Yes

**Response:**
```json
[
  {
    "unitType": "Department",
    "totalUsers": 120,
    "uniqueUsers": 75
  },
  {
    "unitType": "Team",
    "totalUsers": 85,
    "uniqueUsers": 45
  },
  {
    "unitType": "Division",
    "totalUsers": 35,
    "uniqueUsers": 30
  }
]
```

### Transfer Users Between Units

Transfers all users from one organizational unit to another.

- **URL:** `/api/UserOrganizationalUnit/transfer`
- **Method:** `POST`
- **Auth Required:** Yes

**Request Body:**
```json
{
  "sourceUnitId": 459,
  "targetUnitId": 460,
  "retainPrimary": true,
  "assignedBy": 789
}
```

**Response:**
```json
{
  "success": true,
  "message": "Users transferred successfully",
  "affectedCount": 15
}
```

### Ensure Users Have Primary Unit

Ensures all users have a primary organizational unit set.

- **URL:** `/api/UserOrganizationalUnit/ensure-primary-units`
- **Method:** `POST`
- **Auth Required:** Yes

**Response:**
```json
{
  "success": true,
  "message": "Updated primary unit assignments for users",
  "affectedCount": 8
}
```

## Data Models

### CreateUserOrganizationalUnitDTO
```json
{
  "userId": 123,
  "unitId": 456,
  "isPrimary": true,
  "assignedBy": 789
}
```

### UpdateUserOrganizationalUnitDTO
```json
{
  "isPrimary": true
}
```

### UserOrganizationalUnitDTO
```json
{
  "userId": 123,
  "username": "jdoe",
  "userFullName": "John Doe",
  "email": "john.doe@example.com",
  "unitId": 456,
  "unitName": "Sales Department",
  "unitType": "Department",
  "isPrimary": true,
  "dateAssigned": "2023-05-15T10:30:00Z",
  "assignedBy": 789,
  "assignedByName": "Jane Smith"
}
```

### BulkUserAssignmentDTO
```json
{
  "userIds": [123, 124, 125],
  "unitId": 456,
  "makePrimary": false,
  "assignedBy": 789
}
```

### UserMultiUnitAssignmentDTO
```json
{
  "userId": 123,
  "unitIds": [456, 457, 458],
  "primaryUnitId": 456,
  "assignedBy": 789
}
```

### TransferUsersDTO
```json
{
  "sourceUnitId": 459,
  "targetUnitId": 460,
  "retainPrimary": true,
  "assignedBy": 789
}
```

### UserUnitAssignmentQueryParameters
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "sales",
  "unitId": 456,
  "isPrimary": true
}
```

### PaginatedUserUnitAssignmentsDTO
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "items": [
    // Array of UserOrganizationalUnitDTO objects
  ]
}
```

### UnitAssignmentStatsDTO
```json
{
  "unitId": 456,
  "unitName": "Sales Department",
  "unitType": "Department",
  "totalUsers": 25,
  "primaryUsers": 15
}
```

### UnitTypeStatisticsDTO
```json
{
  "unitType": "Department",
  "totalUsers": 120,
  "uniqueUsers": 75
}
```

### UserUnitOperationResultDTO
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "affectedCount": 15
}
```
