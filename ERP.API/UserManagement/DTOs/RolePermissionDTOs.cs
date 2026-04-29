using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool HasPermission { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAssigned { get; set; }
        public int? AssignedBy { get; set; }
        public string? AssignedByUsername { get; set; }
    }

    public class AssignPermissionDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public int? AssignedBy { get; set; }
    }

    public class BatchAssignPermissionsDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> PermissionIds { get; set; } = new List<int>();

        public int? AssignedBy { get; set; }
    }

    public class RevokePermissionDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }
    }

    public class PermissionRoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAssigned { get; set; }
        public int? AssignedBy { get; set; }
        public string? AssignedByUsername { get; set; }
    }

    public class SyncRolePermissionsDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new List<int>();

        public int? AssignedBy { get; set; }
    }

    public class SyncResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int UnchangedCount { get; set; }
    }

    public class RolePermissionStatisticsDto
    {
        public long TotalAssignments { get; set; }
        public long RolesWithPermissions { get; set; }
        public decimal AvgPermissionsPerRole { get; set; }
        public long MaxPermissionsForRole { get; set; }
    }

    public class BatchAssignResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class RevokeAllPermissionsResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RevokedCount { get; set; }
    }

    public class RoleHasPermissionResultDto
    {
        public bool HasPermission { get; set; }
    }
}
