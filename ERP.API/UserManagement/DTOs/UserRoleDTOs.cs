using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class UserBasicDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
    }
    public class UserRoleDto
    {
        public int userroleid { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAssigned { get; set; }
        public int? AssignedBy { get; set; }
        public string? AssignedByUsername { get; set; }
    }

    public class RoleUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAssigned { get; set; }
        public int? AssignedBy { get; set; }
        public string? AssignedByUsername { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public int? AssignedBy { get; set; }
    }

    public class AssignRolesToUserDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> RoleIds { get; set; } = new List<int>();

        public int? AssignedBy { get; set; }
    }

    public class AssignRoleToUsersDto
    {
        [Required]
        [MinLength(1)]
        public List<int> UserIds { get; set; } = new List<int>();

        [Required]
        public int RoleId { get; set; }

        public int? AssignedBy { get; set; }
    }

    public class RevokeRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }

    public class UserRoleQueryParametersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RoleUsersResponseDto
    {
        public List<RoleUserDto> Users { get; set; } = new List<RoleUserDto>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class SyncUserRolesDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<int> RoleIds { get; set; } = new List<int>();

        public int? AssignedBy { get; set; }
    }

    public class UserRoleSyncResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int UnchangedCount { get; set; }
    }

    public class UserRoleBatchAssignResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssignedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class RevokeAllResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RevokedCount { get; set; }
    }

    public class UserRoleStatisticsDto
    {
        public long TotalAssignments { get; set; }
        public long UsersWithRoles { get; set; }
        public long RolesAssignedToUsers { get; set; }
        public decimal AvgRolesPerUser { get; set; }
        public decimal AvgUsersPerRole { get; set; }
        public long MaxRolesForUser { get; set; }
        public long MaxUsersForRole { get; set; }
    }

    public class UserHasRoleResultDto
    {
        public bool HasRole { get; set; }
    }

    public class UserPermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserHasPermissionResultDto
    {
        public bool HasPermission { get; set; }
    }

    public class UserPermissionCheckDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UserRoleAssignmentDto
    {
        public int userroleid { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public DateTime DateAssigned { get; set; }
        public int AssignedBy { get; set; }
    }
    public class UnassignedUserDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateUserRoleByIdDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int AssignedBy { get; set; }
        public DateTime? DateAssigned { get; set; }
    }
}


