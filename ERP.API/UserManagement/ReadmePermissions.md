# Permission Management API

This document provides information about the Permission Management API endpoints.

## Endpoints

### Create Permission

- **URL**: `/api/Permission`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "permissionName": "users.create",
  "description": "Create new users in the system",
  "category": "User Management"
}
```
- **Response**:
  - **Success**: 201 Created
  - **Error**: 400 Bad Request or 500 Internal Server Error

### Get Permission by ID

- **URL**: `/api/Permission/{id}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  - **Success**: 200 OK
  - **Error**: 404 Not Found or 500 Internal Server Error

### Get Permission by Name

- **URL**: `/api/Permission/name/{name}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  - **Success**: 200 OK
  - **Error**: 404 Not Found or 500 Internal Server Error

### Get All Permissions

- **URL**: `/api/Permission`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Query Parameters**:
  - `pageNumber` (default: 1)
  - `pageSize` (default: 10)
  - `searchTerm` (optional)
  - `category` (optional)
  - `isActive` (optional)
- **Response**:
  - **Success**: 200 OK
  - **Error**: 500 Internal Server Error

### Get Permissions by Category

- **URL**: `/api/Permission/category/{category}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Query Parameters**:
  - `isActive` (optional, default: true)
- **Response**:
  - **Success**: 200 OK
  - **Error**: 500 Internal Server Error

### Get Permission Categories

- **URL**: `/api/Permission/categories`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  - **Success**: 200 OK
  - **Error**: 500 Internal Server Error

### Update Permission

- **URL**: `/api/Permission/{id}`
- **Method**: `PUT`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "permissionName": "users.create",
  "description": "Updated description",
  "category": "User Management",
  "isActive": true
}
```
- **Response**:
  - **Success**: 200 OK
  - **Error**: 400 Bad Request or 500 Internal Server Error

### Deactivate Permission (Soft Delete)

- **URL**: `/api/Permission/{id}/deactivate`
- **Method**: `PATCH`
- **Authorization**: Requires Admin Role
- **Response**:
  - **Success**: 200 OK
  - **Error**: 400 Bad Request or 500 Internal Server Error

### Hard Delete Permission

- **URL**: `/api/Permission/{id}`
- **Method**: `DELETE`
- **Authorization**: Requires Admin Role
- **Response**:
  - **Success**: 200 OK
  - **Error**: 400 Bad Request or 500 Internal Server Error

### Get Permission Statistics

- **URL**: `/api/Permission/statistics`
- **Method**: `GET`
- **Authorization**: Requires Admin Role
- **Response**:
  - **Success**: 200 OK
  - **Error**: 500 Internal Server Error

### Batch Create Permissions

- **URL**: `/api/Permission/batch`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "permissions": [
    {
      "permissionName": "users.view",
      "description": "View user details",
      "category": "User Management"
    },
    {
      "permissionName": "users.edit",
      "description": "Edit user details",
      "category": "User Management"
    }
  ]
}
```
- **Response**:
  - **Success**: 200 OK
  - **Error**: 400 Bad Request or 500 Internal Server Error

## Models

### PermissionDto

```json
{
  "permissionId": 1,
  "permissionName": "users.create",
  "description": "Create new users in the system",
  "category": "User Management",
  "isActive": true
}
```

### PaginatedPermissionsDto

```json
{
  "permissions": [
    {
      "permissionId": 1,
      "permissionName": "users.create",
      "description": "Create new users in the system",
      "category": "User Management",
      "isActive": true,
      "totalCount": 10
    }
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 10
}
```

### PermissionStatisticsDto

```json
{
  "totalPermissions": 10,
  "activePermissions": 8,
  "inactivePermissions": 2,
  "categoriesCount": 3
}
```

### BatchCreatePermissionsResultDto

```json
{
  "success": true,
  "message": "Created 2 permissions, 1 failed",
  "createdCount": 2,
  "failedCount": 1
}
```
