# Organizational Unit API Documentation

This document provides details of the Organizational Unit API endpoints available in the ERP system.

## Base URL
`/api/OrganizationalUnit`

## Authentication
All endpoints require authentication via JWT token unless otherwise specified.

## Endpoints

### Get All Organizational Units
Retrieves all organizational units with optional active status filtering.

- **URL**: `/api/OrganizationalUnit`
- **Method**: `GET`
- **Query Parameters**:
  - `isActive` (optional): Filter by active status (true/false)
- **Response**: Array of organizational unit objects
- **Example Response**:
```json
[
  {
    "unitId": 1,
    "unitName": "Finance Department",
    "unitType": "Department",
    "description": "Financial operations and accounting",
    "parentUnitId": null,
    "parentUnitName": null,
    "managerId": 101,
    "managerName": "John Smith",
    "isActive": true,
    "dateCreated": "2025-06-15T14:30:00",
    "createdBy": 1,
    "createdByUsername": "admin",
    "childCount": 3
  },
  ...
]
```

### Get Organizational Unit by ID
Retrieves a specific organizational unit by its ID.

- **URL**: `/api/OrganizationalUnit/{id}`
- **Method**: `GET`
- **URL Parameters**:
  - `id`: The ID of the organizational unit
- **Response**: Organizational unit object
- **Example Response**:
```json
{
  "unitId": 1,
  "unitName": "Finance Department",
  "unitType": "Department",
  "description": "Financial operations and accounting",
  "parentUnitId": null,
  "parentUnitName": null,
  "managerId": 101,
  "managerName": "John Smith",
  "isActive": true,
  "dateCreated": "2025-06-15T14:30:00",
  "createdBy": 1,
  "createdByUsername": "admin",
  "childCount": 3
}
```

### Create Organizational Unit
Creates a new organizational unit.

- **URL**: `/api/OrganizationalUnit`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "unitName": "HR Department",
  "unitType": "Department",
  "description": "Human Resources operations",
  "parentUnitId": null,
  "managerId": 102,
  "isActive": true
}
```
- **Response**: Created unit details with success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Organizational unit created successfully",
  "unitId": 2
}
```

### Update Organizational Unit
Updates an existing organizational unit.

- **URL**: `/api/OrganizationalUnit/{id}`
- **Method**: `PUT`
- **Authorization**: Requires Admin Role
- **URL Parameters**:
  - `id`: The ID of the organizational unit
- **Request Body**:
```json
{
  "unitName": "HR & Training Department",
  "unitType": "Department",
  "description": "Human Resources and Training operations",
  "parentUnitId": null,
  "managerId": 102,
  "isActive": true
}
```
- **Response**: Success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Organizational unit updated successfully"
}
```

### Delete Organizational Unit
Deletes an organizational unit.

- **URL**: `/api/OrganizationalUnit/{id}`
- **Method**: `DELETE`
- **Authorization**: Requires Admin Role
- **URL Parameters**:
  - `id`: The ID of the organizational unit
- **Response**: Success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Organizational unit deleted successfully"
}
```

### Set Organizational Unit Status
Activates or deactivates an organizational unit.

- **URL**: `/api/OrganizationalUnit/{id}/status`
- **Method**: `PATCH`
- **Authorization**: Requires Admin Role
- **URL Parameters**:
  - `id`: The ID of the organizational unit
- **Query Parameters**:
  - `isActive`: Status to set (true/false)
- **Response**: Success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Organizational unit activated successfully"
}
```

### Get Paginated Organizational Units
Retrieves organizational units with pagination and filtering.

- **URL**: `/api/OrganizationalUnit/paginated`
- **Method**: `GET`
- **Query Parameters**:
  - `pageNumber` (default=1): Page number
  - `pageSize` (default=10): Number of items per page
  - `searchTerm` (optional): Search text
  - `unitType` (optional): Filter by unit type
  - `isActive` (optional): Filter by active status
- **Response**: Paginated list of organizational units
- **Example Response**:
```json
{
  "units": [
    {
      "unitId": 1,
      "unitName": "Finance Department",
      "unitType": "Department",
      "description": "Financial operations and accounting",
      "parentUnitId": null,
      "parentUnitName": null,
      "managerId": 101,
      "managerName": "John Smith",
      "isActive": true,
      "dateCreated": "2025-06-15T14:30:00",
      "createdBy": 1,
      "createdByUsername": "admin",
      "childCount": 3
    },
    ...
  ],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10
}
```

### Get Child Units
Retrieves child units for a parent unit.

- **URL**: `/api/OrganizationalUnit/{id}/children`
- **Method**: `GET`
- **URL Parameters**:
  - `id`: The ID of the parent organizational unit
- **Query Parameters**:
  - `isActive` (optional): Filter by active status
- **Response**: Array of child unit objects
- **Example Response**:
```json
[
  {
    "unitId": 3,
    "unitName": "Accounts Payable",
    "unitType": "Team",
    "description": "Manages outgoing payments",
    "parentUnitId": 1,
    "managerId": 105,
    "managerName": "Alice Johnson",
    "isActive": true,
    "dateCreated": "2025-06-15T15:00:00",
    "childCount": 0
  },
  ...
]
```

### Get Top-Level Units
Retrieves units without a parent.

- **URL**: `/api/OrganizationalUnit/top-level`
- **Method**: `GET`
- **Query Parameters**:
  - `isActive` (optional): Filter by active status
- **Response**: Array of top-level unit objects
- **Example Response**:
```json
[
  {
    "unitId": 1,
    "unitName": "Finance Department",
    "unitType": "Department",
    "description": "Financial operations and accounting",
    "managerId": 101,
    "managerName": "John Smith",
    "isActive": true,
    "dateCreated": "2025-06-15T14:30:00",
    "childCount": 3
  },
  ...
]
```

### Get Unit Hierarchy
Retrieves the hierarchical structure of organizational units.

- **URL**: `/api/OrganizationalUnit/hierarchy`
- **Method**: `GET`
- **Query Parameters**:
  - `unitId` (optional): Starting point for hierarchy (null for full hierarchy)
  - `isActive` (default=true): Filter by active status
- **Response**: Array of hierarchical unit objects
- **Example Response**:
```json
[
  {
    "unitId": 1,
    "unitName": "Finance Department",
    "unitType": "Department",
    "parentUnitId": null,
    "parentUnitName": null,
    "isActive": true,
    "level": 1,
    "path": "Finance Department",
    "childCount": 3
  },
  {
    "unitId": 3,
    "unitName": "Accounts Payable",
    "unitType": "Team",
    "parentUnitId": 1,
    "parentUnitName": "Finance Department",
    "isActive": true,
    "level": 2,
    "path": "Finance Department > Accounts Payable",
    "childCount": 0
  },
  ...
]
```

### Search Organizational Units
Searches for organizational units by name or description.

- **URL**: `/api/OrganizationalUnit/search`
- **Method**: `GET`
- **Query Parameters**:
  - `searchTerm` (required): Text to search for
  - `unitType` (optional): Filter by unit type
  - `isActive` (optional): Filter by active status
- **Response**: Array of matching unit objects with match type
- **Example Response**:
```json
[
  {
    "unitId": 1,
    "unitName": "Finance Department",
    "unitType": "Department",
    "description": "Financial operations and accounting",
    "parentUnitId": null,
    "parentUnitName": null,
    "isActive": true,
    "matchType": "Name Match"
  },
  {
    "unitId": 5,
    "unitName": "Budgeting Team",
    "unitType": "Team",
    "description": "Handles financial planning and budgeting",
    "parentUnitId": 1,
    "parentUnitName": "Finance Department",
    "isActive": true,
    "matchType": "Description Match"
  },
  ...
]
```

### Get Units by Manager
Retrieves units managed by a specific employee.

- **URL**: `/api/OrganizationalUnit/by-manager/{managerId}`
- **Method**: `GET`
- **URL Parameters**:
  - `managerId`: The ID of the manager
- **Query Parameters**:
  - `isActive` (optional): Filter by active status
- **Response**: Array of unit objects
- **Example Response**:
```json
[
  {
    "unitId": 1,
    "unitName": "Finance Department",
    "unitType": "Department",
    "description": "Financial operations and accounting",
    "parentUnitId": null,
    "parentUnitName": null,
    "managerId": 101,
    "managerName": "John Smith",
    "isActive": true,
    "dateCreated": "2025-06-15T14:30:00",
    "createdBy": 1,
    "createdByUsername": "admin",
    "childCount": 3
  },
  ...
]
```

### Get Unit Types
Retrieves all organizational unit types with counts.

- **URL**: `/api/OrganizationalUnit/types`
- **Method**: `GET`
- **Response**: Array of unit type objects with counts
- **Example Response**:
```json
[
  {
    "unitType": "Department",
    "count": 5
  },
  {
    "unitType": "Team",
    "count": 12
  },
  {
    "unitType": "Division",
    "count": 3
  },
  ...
]
```

### Get Organizational Unit Statistics
Retrieves statistics about organizational units.

- **URL**: `/api/OrganizationalUnit/statistics`
- **Method**: `GET`
- **Authorization**: Requires Admin Role
- **Response**: Statistics object
- **Example Response**:
```json
{
  "totalUnits": 20,
  "activeUnits": 18,
  "inactiveUnits": 2,
  "topLevelUnits": 5,
  "totalUnitTypes": 3,
  "maxHierarchyDepth": 4
}
```

### Assign Manager to Unit
Assigns a manager to an organizational unit.

- **URL**: `/api/OrganizationalUnit/assign-manager`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "unitId": 1,
  "managerId": 103
}
```
- **Response**: Success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Manager assigned to unit successfully"
}
```

### Move Organizational Unit
Moves a unit to a new parent or to the top level.

- **URL**: `/api/OrganizationalUnit/move`
- **Method**: `POST`
- **Authorization**: Requires Admin Role
- **Request Body**:
```json
{
  "unitId": 3,
  "newParentId": 2
}
```
- **Response**: Success message
- **Example Response**:
```json
{
  "success": true,
  "message": "Unit moved to new parent successfully"
}
```

## Error Responses

All endpoints may return the following error responses:

### Bad Request (400)
```json
{
  "success": false,
  "message": "Error message details"
}
```

### Not Found (404)
```json
{
  "message": "Organizational unit with ID 100 not found."
}
```

### Internal Server Error (500)
```json
{
  "message": "An error occurred while processing your request."
}
```
