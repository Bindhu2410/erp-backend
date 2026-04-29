# Role Permission API Endpoints Documentation

This document describes the API endpoints for managing role-permission associations in the ERP system.

## Assign Permission to Role

Assigns a single permission to a role.

- **URL**: `/api/RolePermission/assign`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
  ```json
  {
    "roleId": 1,
    "permissionId": 1,
    "assignedBy": 1
  }
  ```
- **Response**:
  ```json
  {
    "message": "Permission assigned to role successfully"
  }
  ```

## Batch Assign Permissions to Role

Assigns multiple permissions to a role in a single operation.

- **URL**: `/api/RolePermission/assign/batch`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
  ```json
  {
    "roleId": 1,
    "permissionIds": [1, 2, 3],
    "assignedBy": 1
  }
  ```
- **Response**:
  ```json
  {
    "success": true,
    "message": "Assigned 3 permissions, 0 failed",
    "assignedCount": 3,
    "failedCount": 0
  }
  ```

## Revoke Permission from Role

Removes a permission from a role.

- **URL**: `/api/RolePermission/revoke`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
  ```json
  {
    "roleId": 1,
    "permissionId": 1
  }
  ```
- **Response**:
  ```json
  {
    "message": "Permission revoked from role successfully"
  }
  ```

## Revoke All Permissions from Role

Removes all permissions from a role.

- **URL**: `/api/RolePermission/revoke/all/{roleId}`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Response**:
  ```json
  {
    "success": true,
    "message": "All permissions revoked from role successfully",
    "revokedCount": 5
  }
  ```

## Get Role Permissions

Retrieves all permissions assigned to a specific role.

- **URL**: `/api/RolePermission/role/{roleId}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  ```json
  [
    {
      "roleId": 1,
      "permissionId": 1,
      "permissionName": "users.create",
      "description": "Create new users",
      "category": "User Management",
      "isActive": true,
      "dateAssigned": "2025-07-03T12:00:00Z",
      "assignedBy": 1,
      "assignedByUsername": "admin"
    },
    ...
  ]
  ```

## Get Permission Roles

Retrieves all roles that have a specific permission.

- **URL**: `/api/RolePermission/permission/{permissionId}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  ```json
  [
    {
      "roleId": 1,
      "roleName": "Administrator",
      "description": "System administrator",
      "isSystemRole": true,
      "isActive": true,
      "dateAssigned": "2025-07-03T12:00:00Z",
      "assignedBy": 1,
      "assignedByUsername": "admin"
    },
    ...
  ]
  ```

## Check Role Has Permission

Checks if a role has a specific permission by permission ID.

- **URL**: `/api/RolePermission/check/{roleId}/{permissionId}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  ```json
  {
    "hasPermission": true
  }
  ```

## Check Role Has Permission by Name

Checks if a role has a specific permission by permission name.

- **URL**: `/api/RolePermission/check/{roleId}/byname/{permissionName}`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Response**:
  ```json
  {
    "hasPermission": true
  }
  ```

## Get Unassigned Permissions for Role

Retrieves all permissions that are not yet assigned to a role.

- **URL**: `/api/RolePermission/unassigned/{roleId}?isActive=true`
- **Method**: `GET`
- **Authorization**: Authenticated User
- **Query Parameters**:
  - `isActive` (optional): Filter by active status. Default is true.
- **Response**:
  ```json
  [
    {
      "permissionId": 4,
      "permissionName": "reports.view",
      "description": "View system reports",
      "category": "Reporting",
      "isActive": true
    },
    ...
  ]
  ```

## Sync Role Permissions

Replaces all current permissions for a role with a new set.

- **URL**: `/api/RolePermission/sync`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
  ```json
  {
    "roleId": 1,
    "permissionIds": [1, 2, 3, 4],
    "assignedBy": 1
  }
  ```
- **Response**:
  ```json
  {
    "success": true,
    "message": "Permissions synced successfully: 2 added, 1 removed, 2 unchanged",
    "addedCount": 2,
    "removedCount": 1,
    "unchangedCount": 2
  }
  ```

## Get Role Permission Statistics

Retrieves statistics about role-permission assignments.

- **URL**: `/api/RolePermission/statistics`
- **Method**: `GET`
- **Authorization**: Requires Admin Role
- **Response**:
  ```json
  {
    "totalAssignments": 45,
    "rolesWithPermissions": 8,
    "avgPermissionsPerRole": 5.63,
    "maxPermissionsForRole": 15
  }
  ```
