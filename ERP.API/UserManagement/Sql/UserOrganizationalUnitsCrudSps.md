# User-Organizational Units API

This SQL script contains the stored procedures necessary for managing user assignments to organizational units in the ERP system.

## Table Schema

The `UserOrganizationalUnits` table represents the assignments between users and organizational units:

```sql
CREATE TABLE public."UserOrganizationalUnits" (
    "UserId" int4 NOT NULL,
    "UnitId" int4 NOT NULL,
    "IsPrimary" bool DEFAULT true NOT NULL,
    "DateAssigned" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "AssignedBy" int4 NULL,
    CONSTRAINT "UserOrganizationalUnits_pkey" PRIMARY KEY ("UserId", "UnitId")
);

-- Foreign keys
ALTER TABLE public."UserOrganizationalUnits" ADD CONSTRAINT "UserOrganizationalUnits_AssignedBy_fkey" FOREIGN KEY ("AssignedBy") REFERENCES public.users(userid);
ALTER TABLE public."UserOrganizationalUnits" ADD CONSTRAINT "UserOrganizationalUnits_UnitId_fkey" FOREIGN KEY ("UnitId") REFERENCES public."OrganizationalUnits"("UnitId");
ALTER TABLE public."UserOrganizationalUnits" ADD CONSTRAINT "UserOrganizationalUnits_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES public.users(userid);
```

## Stored Procedures

The following stored procedures are included in this SQL script:

### CRUD Operations

- `sp_um_assign_user_to_unit` - Assigns a user to an organizational unit
- `sp_um_get_user_unit_assignment` - Gets details about a specific user-to-unit assignment
- `sp_um_update_user_unit_assignment` - Updates a user's assignment (primary status)
- `sp_um_remove_user_from_unit` - Removes a user from an organizational unit

### Bulk Operations

- `sp_um_get_users_in_unit` - Gets all users assigned to a specific unit
- `sp_um_get_user_units` - Gets all units a user is assigned to
- `sp_um_get_user_primary_unit` - Gets a user's primary organizational unit
- `sp_um_set_user_primary_unit` - Sets a user's primary organizational unit

### Batch Operations

- `sp_um_assign_users_to_unit` - Assigns multiple users to an organizational unit
- `sp_um_assign_user_to_units` - Assigns a user to multiple organizational units

### Reporting & Analysis

- `sp_um_get_users_without_unit` - Gets users not assigned to any unit
- `sp_um_get_empty_units` - Gets units with no assigned users
- `sp_um_get_unit_assignment_stats` - Gets user statistics for each unit
- `sp_um_get_user_unit_assignments_paginated` - Gets paginated list of assignments
- `sp_um_get_user_count_by_unit_type` - Gets statistics by unit type

### Special Operations

- `sp_um_transfer_users_between_units` - Transfers users from one unit to another
- `sp_um_ensure_users_have_primary_unit` - Ensures all users have a primary unit

## Usage

These stored procedures are used by the `UserOrganizationalUnitService` class to implement the business logic for managing user-to-unit assignments in the ERP system.

For details on API endpoints that use these stored procedures, see the `UserOrganizationalUnitAPI.md` documentation file.
