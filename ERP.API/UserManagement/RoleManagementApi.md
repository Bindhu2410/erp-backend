# Role Management API

This document outlines the API endpoints for managing roles in the system.

## Endpoints Overview

| Method | Endpoint                       | Description                                      | Authorization Required                    |
|--------|--------------------------------|--------------------------------------------------|-------------------------------------------|
| POST   | `/api/Role`                    | Create a new role                                | ManageRoles policy                        |
| GET    | `/api/Role/{id}`               | Get a role by ID                                 | Any authenticated user                    |
| GET    | `/api/Role/name/{name}`        | Get a role by name                               | Any authenticated user                    |
| GET    | `/api/Role`                    | Get a paginated list of roles with filtering     | Any authenticated user                    |
| PUT    | `/api/Role/{id}`               | Update a role                                    | ManageRoles policy                        |
| PUT    | `/api/Role/{id}/deactivate`    | Soft delete (deactivate) a role                  | ManageRoles policy                        |
| DELETE | `/api/Role/{id}`               | Hard delete (permanently remove) a role          | ManageRoles policy                        |
| GET    | `/api/Role/statistics`         | Get statistics about roles                       | ViewRoleStatistics policy                 |

## Detailed Endpoint Documentation

### Create Role

**Endpoint:** `POST /api/Role`

**Authorization:** Requires ManageRoles policy

**Request Body:**
```json
{
  "roleName": "string",
  "description": "string",
  "isSystemRole": false,
  "createdBy": 0
}
```

**Response:**
- 201 Created: Role created successfully
- 400 Bad Request: Invalid input or role name already exists
- 401 Unauthorized: User is not authenticated
- 403 Forbidden: User does not have permission

### Get Role by ID

**Endpoint:** `GET /api/Role/{id}`

**Authorization:** Any authenticated user

**Parameters:**
- id: Integer role ID

**Response:**
- 200 OK: Returns the role details
- 404 Not Found: Role not found
- 401 Unauthorized: User is not authenticated

### Get Role by Name

**Endpoint:** `GET /api/Role/name/{name}`

**Authorization:** Any authenticated user

**Parameters:**
- name: String role name

**Response:**
- 200 OK: Returns the role details
- 404 Not Found: Role not found
- 401 Unauthorized: User is not authenticated

### Get All Roles

**Endpoint:** `GET /api/Role`

**Authorization:** Any authenticated user

**Query Parameters:**
- pageNumber: Integer page number (default: 1)
- pageSize: Integer page size (default: 10)
- searchTerm: String search term (optional)
- isActive: Boolean filter for active/inactive roles (optional)
- isSystemRole: Boolean filter for system/custom roles (optional)

**Response:**
- 200 OK: Returns paginated role list
- 401 Unauthorized: User is not authenticated

### Update Role

**Endpoint:** `PUT /api/Role/{id}`

**Authorization:** Requires ManageRoles policy

**Parameters:**
- id: Integer role ID

**Request Body:**
```json
{
  "roleName": "string",
  "description": "string",
  "isSystemRole": true,
  "isActive": true
}
```

**Response:**
- 200 OK: Role updated successfully
- 400 Bad Request: Invalid input or duplicate role name
- 404 Not Found: Role not found
- 401 Unauthorized: User is not authenticated
- 403 Forbidden: User does not have permission

### Deactivate Role

**Endpoint:** `PUT /api/Role/{id}/deactivate`

**Authorization:** Requires ManageRoles policy

**Parameters:**
- id: Integer role ID

**Response:**
- 200 OK: Role deactivated successfully
- 400 Bad Request: Cannot deactivate system role
- 404 Not Found: Role not found
- 401 Unauthorized: User is not authenticated
- 403 Forbidden: User does not have permission

### Delete Role

**Endpoint:** `DELETE /api/Role/{id}`

**Authorization:** Requires ManageRoles policy

**Parameters:**
- id: Integer role ID

**Response:**
- 200 OK: Role deleted successfully
- 400 Bad Request: Cannot delete system role or role with dependencies
- 404 Not Found: Role not found
- 401 Unauthorized: User is not authenticated
- 403 Forbidden: User does not have permission

### Get Role Statistics

**Endpoint:** `GET /api/Role/statistics`

**Authorization:** Requires ViewRoleStatistics policy

**Response:**
- 200 OK: Returns role statistics
- 401 Unauthorized: User is not authenticated
- 403 Forbidden: User does not have permission

**Response Body:**
```json
{
  "totalRoles": 0,
  "activeRoles": 0,
  "inactiveRoles": 0,
  "systemRoles": 0,
  "customRoles": 0
}
```

## Integration with Authorization

The Role API is designed to work with the application's authorization system. Some endpoints require specific policies:

- **ManageRoles**: Required for creating, updating, and deleting roles
- **ViewRoleStatistics**: Required for accessing role statistics

These policies should be configured in the application's authorization service.
