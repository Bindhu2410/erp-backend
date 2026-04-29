# User Role Management API

This document describes the API endpoints for managing user roles in the system.

## Endpoints

### Assign Role to User
- **POST** `/api/UserRole/assign`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Assigns a single role to a user
- **Request Body:**
  ```json
  {
    "userId": 1,
    "roleId": 2,
    "assignedBy": 1
  }
  ```
- **Response:**
  ```json
  {
    "message": "Role assigned to user successfully"
  }
  ```

### Assign Multiple Roles to User
- **POST** `/api/UserRole/assign/roles`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Assigns multiple roles to a user
- **Request Body:**
  ```json
  {
    "userId": 1,
    "roleIds": [1, 2, 3],
    "assignedBy": 1
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Assigned 3 roles, 0 failed",
    "assignedCount": 3,
    "failedCount": 0
  }
  ```

### Assign Role to Multiple Users
- **POST** `/api/UserRole/assign/users`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Assigns a role to multiple users
- **Request Body:**
  ```json
  {
    "userIds": [1, 2, 3],
    "roleId": 1,
    "assignedBy": 1
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Assigned role to 3 users, 0 failed",
    "assignedCount": 3,
    "failedCount": 0
  }
  ```

### Revoke Role from User
- **POST** `/api/UserRole/revoke`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Revokes a role from a user
- **Request Body:**
  ```json
  {
    "userId": 1,
    "roleId": 2
  }
  ```
- **Response:**
  ```json
  {
    "message": "Role revoked from user successfully"
  }
  ```

### Revoke All Roles from User
- **POST** `/api/UserRole/revoke/all/user/{userId}`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Revokes all roles from a user
- **Response:**
  ```json
  {
    "success": true,
    "message": "All roles revoked from user successfully",
    "revokedCount": 3
  }
  ```

### Revoke Role from All Users
- **POST** `/api/UserRole/revoke/all/role/{roleId}`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Revokes a role from all users
- **Response:**
  ```json
  {
    "success": true,
    "message": "Role revoked from 5 users successfully",
    "revokedCount": 5
  }
  ```

### Get User Roles
- **GET** `/api/UserRole/user/{userId}`
- **Auth Required:** Yes
- **Description:** Gets all roles assigned to a user
- **Response:**
  ```json
  [
    {
      "roleId": 1,
      "roleName": "Administrator",
      "description": "System administrator with full access",
      "isSystemRole": true,
      "isActive": true,
      "dateAssigned": "2025-07-03T10:30:00",
      "assignedBy": 1,
      "assignedByUsername": "admin"
    },
    {
      "roleId": 2,
      "roleName": "User",
      "description": "Regular user",
      "isSystemRole": true,
      "isActive": true,
      "dateAssigned": "2025-07-03T10:30:00",
      "assignedBy": 1,
      "assignedByUsername": "admin"
    }
  ]
  ```

### Get Users with Role
- **GET** `/api/UserRole/role/{roleId}`
- **Auth Required:** Yes
- **Description:** Gets all users assigned to a role
- **Response:**
  ```json
  [
    {
      "userId": 1,
      "username": "johndoe",
      "email": "john.doe@example.com",
      "fullName": "John Doe",
      "isActive": true,
      "dateAssigned": "2025-07-03T10:30:00",
      "assignedBy": 1,
      "assignedByUsername": "admin"
    }
  ]
  ```

### Get Users with Role (Paginated)
- **GET** `/api/UserRole/role/{roleId}/paginated`
- **Auth Required:** Yes
- **Description:** Gets paginated list of users assigned to a role
- **Query Parameters:**
  - `pageNumber`: Page number (default: 1)
  - `pageSize`: Page size (default: 10)
  - `searchTerm`: Optional search term
  - `isActive`: Optional filter by active status
- **Response:**
  ```json
  {
    "users": [
      {
        "userId": 1,
        "username": "johndoe",
        "email": "john.doe@example.com",
        "fullName": "John Doe",
        "isActive": true,
        "dateAssigned": "2025-07-03T10:30:00",
        "assignedBy": 1,
        "assignedByUsername": "admin"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10
  }
  ```

### Check If User Has Role
- **GET** `/api/UserRole/check/{userId}/{roleId}`
- **Auth Required:** Yes
- **Description:** Checks if a user has a specific role
- **Response:**
  ```json
  {
    "hasRole": true
  }
  ```

### Check If User Has Role By Name
- **GET** `/api/UserRole/check/{userId}/byname/{roleName}`
- **Auth Required:** Yes
- **Description:** Checks if a user has a specific role by name
- **Response:**
  ```json
  {
    "hasRole": true
  }
  ```

### Get Unassigned Roles for User
- **GET** `/api/UserRole/unassigned/roles/{userId}`
- **Auth Required:** Yes
- **Description:** Gets all roles not assigned to a user
- **Query Parameters:**
  - `isActive`: Optional filter by active status (default: true)
- **Response:**
  ```json
  [
    {
      "roleId": 3,
      "roleName": "Manager",
      "description": "Department manager",
      "isSystemRole": false,
      "isActive": true
    }
  ]
  ```

### Get Users Without Role
- **GET** `/api/UserRole/unassigned/users/{roleId}`
- **Auth Required:** Yes
- **Description:** Gets all users not assigned to a role
- **Query Parameters:**
  - `isActive`: Optional filter by active status (default: true)
- **Response:**
  ```json
  [
    {
      "userId": 2,
      "username": "janedoe",
      "email": "jane.doe@example.com",
      "fullName": "Jane Doe",
      "isActive": true
    }
  ]
  ```

### Sync User Roles
- **POST** `/api/UserRole/sync`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Syncs user roles by replacing all current roles with the provided list
- **Request Body:**
  ```json
  {
    "userId": 1,
    "roleIds": [1, 2, 3],
    "assignedBy": 1
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Roles synced successfully: 2 added, 1 removed, 1 unchanged",
    "addedCount": 2,
    "removedCount": 1,
    "unchangedCount": 1
  }
  ```

### Get User Role Statistics
- **GET** `/api/UserRole/statistics`
- **Auth Required:** Yes, with AdminRole policy
- **Description:** Gets statistics about user role assignments
- **Response:**
  ```json
  {
    "totalAssignments": 50,
    "usersWithRoles": 20,
    "rolesAssignedToUsers": 5,
    "avgRolesPerUser": 2.5,
    "avgUsersPerRole": 10.0,
    "maxRolesForUser": 4,
    "maxUsersForRole": 20
  }
  ```

### Check If User Has Any Permission
- **POST** `/api/UserRole/permissions/any`
- **Auth Required:** Yes
- **Description:** Checks if a user has any of the specified permissions through their roles
- **Request Body:**
  ```json
  {
    "userId": 1,
    "permissionIds": [1, 2, 3]
  }
  ```
- **Response:**
  ```json
  {
    "hasPermission": true
  }
  ```

### Check If User Has All Permissions
- **POST** `/api/UserRole/permissions/all`
- **Auth Required:** Yes
- **Description:** Checks if a user has all of the specified permissions through their roles
- **Request Body:**
  ```json
  {
    "userId": 1,
    "permissionIds": [1, 2, 3]
  }
  ```
- **Response:**
  ```json
  {
    "hasPermission": true
  }
  ```

### Get All User Permissions
- **GET** `/api/UserRole/permissions/{userId}`
- **Auth Required:** Yes
- **Description:** Gets all permissions a user has through their roles
- **Response:**
  ```json
  [
    {
      "permissionId": 1,
      "permissionName": "users.view",
      "description": "View user details",
      "category": "User Management",
      "isActive": true,
      "roleId": 1,
      "roleName": "Administrator"
    },
    {
      "permissionId": 2,
      "permissionName": "users.create",
      "description": "Create new users",
      "category": "User Management",
      "isActive": true,
      "roleId": 1,
      "roleName": "Administrator"
    }
  ]
  ```

## Status Codes

- `200 OK`: Request succeeded
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `500 Internal Server Error`: Server error

## Error Responses

Error responses include a message field describing the error:

```json
{
  "message": "Error message details"
}
```
