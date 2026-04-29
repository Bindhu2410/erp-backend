using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// DTO for creating a new user-to-organizational unit assignment
    /// </summary>
    public class CreateUserOrganizationalUnitDTO
    {
        /// <summary>
        /// The ID of the user to be assigned to the organizational unit
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// The ID of the organizational unit to assign the user to
        /// </summary>
        [Required]
        public int UnitId { get; set; }

        /// <summary>
        /// Whether this is the user's primary organizational unit
        /// </summary>
        public bool IsPrimary { get; set; } = true;

        /// <summary>
        /// Optional ID of the user making the assignment
        /// </summary>
        public int? AssignedBy { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing user-to-organizational unit assignment
    /// </summary>
    public class UpdateUserOrganizationalUnitDTO
    {
        /// <summary>
        /// Whether this is the user's primary organizational unit
        /// </summary>
        [Required]
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// DTO representing a user's organizational unit assignment
    /// </summary>
    public class UserOrganizationalUnitDTO
    {
        /// <summary>
        /// The ID of the assigned user
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// The username of the assigned user
        /// </summary>
        public string? Username { get; set; }
        
        /// <summary>
        /// The full name of the assigned user
        /// </summary>
        public string? UserFullName { get; set; }
        
        /// <summary>
        /// The email of the assigned user
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// The ID of the organizational unit
        /// </summary>
        public int UnitId { get; set; }
        
        /// <summary>
        /// The name of the organizational unit
        /// </summary>
        public string? UnitName { get; set; }
        
        /// <summary>
        /// The type of the organizational unit
        /// </summary>
        public string? UnitType { get; set; }
        
        /// <summary>
        /// Whether this is the user's primary organizational unit
        /// </summary>
        public bool IsPrimary { get; set; }
        
        /// <summary>
        /// When the user was assigned to this unit
        /// </summary>
        public DateTime DateAssigned { get; set; }
        
        /// <summary>
        /// ID of the user who made the assignment
        /// </summary>
        public int? AssignedBy { get; set; }
        
        /// <summary>
        /// Name of the user who made the assignment
        /// </summary>
        public string? AssignedByName { get; set; }
    }

    /// <summary>
    /// DTO for bulk assignment of users to an organizational unit
    /// </summary>
    public class BulkUserAssignmentDTO
    {
        /// <summary>
        /// IDs of the users to assign to the organizational unit
        /// </summary>
        [Required]
        public List<int> UserIds { get; set; } = new List<int>();
        
        /// <summary>
        /// ID of the organizational unit to assign users to
        /// </summary>
        [Required]
        public int UnitId { get; set; }
        
        /// <summary>
        /// Whether to make this the primary unit for all assigned users
        /// </summary>
        public bool MakePrimary { get; set; } = false;
        
        /// <summary>
        /// Optional ID of the user making the assignments
        /// </summary>
        public int? AssignedBy { get; set; }
    }
    
    /// <summary>
    /// DTO for assigning a user to multiple organizational units
    /// </summary>
    public class UserMultiUnitAssignmentDTO
    {
        /// <summary>
        /// ID of the user to assign to multiple units
        /// </summary>
        [Required]
        public int UserId { get; set; }
        
        /// <summary>
        /// IDs of the organizational units to assign the user to
        /// </summary>
        [Required]
        public List<int> UnitIds { get; set; } = new List<int>();
        
        /// <summary>
        /// Optional ID of the unit to set as primary (must be in UnitIds list)
        /// </summary>
        public int? PrimaryUnitId { get; set; }
        
        /// <summary>
        /// Optional ID of the user making the assignments
        /// </summary>
        public int? AssignedBy { get; set; }
    }
    
    /// <summary>
    /// DTO for transferring users between organizational units
    /// </summary>
    public class TransferUsersDTO
    {
        /// <summary>
        /// ID of the source organizational unit
        /// </summary>
        [Required]
        public int SourceUnitId { get; set; }
        
        /// <summary>
        /// ID of the target organizational unit
        /// </summary>
        [Required]
        public int TargetUnitId { get; set; }
        
        /// <summary>
        /// Whether to retain primary status when transferring
        /// </summary>
        public bool RetainPrimary { get; set; } = true;
        
        /// <summary>
        /// Optional ID of the user performing the transfer
        /// </summary>
        public int? AssignedBy { get; set; }
    }
    
    /// <summary>
    /// Pagination and search parameters for user-unit assignments
    /// </summary>
    public class UserUnitAssignmentQueryParameters
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Optional search term to filter results
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// Optional unit ID to filter by
        /// </summary>
        public int? UnitId { get; set; }
        
        /// <summary>
        /// Optional filter for primary status
        /// </summary>
        public bool? IsPrimary { get; set; }
    }
    
    /// <summary>
    /// Result DTO for paginated user-unit assignment queries
    /// </summary>
    public class PaginatedUserUnitAssignmentsDTO
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public long TotalCount { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
        
        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
        
        /// <summary>
        /// The user-unit assignments for the current page
        /// </summary>
        public List<UserOrganizationalUnitDTO> Items { get; set; } = new List<UserOrganizationalUnitDTO>();
    }
    
    /// <summary>
    /// Statistics about user assignments by unit type
    /// </summary>
    public class UnitTypeStatisticsDTO
    {
        /// <summary>
        /// The organizational unit type
        /// </summary>
        public string? UnitType { get; set; }
        
        /// <summary>
        /// Total number of user assignments to units of this type
        /// </summary>
        public long TotalUsers { get; set; }
        
        /// <summary>
        /// Number of unique users assigned to units of this type
        /// </summary>
        public long UniqueUsers { get; set; }
    }
    
    /// <summary>
    /// Statistics about an organizational unit's user assignments
    /// </summary>
    public class UnitAssignmentStatsDTO
    {
        /// <summary>
        /// ID of the organizational unit
        /// </summary>
        public int UnitId { get; set; }
        
        /// <summary>
        /// Name of the organizational unit
        /// </summary>
        public string? UnitName { get; set; }
        
        /// <summary>
        /// Type of the organizational unit
        /// </summary>
        public string? UnitType { get; set; }
        
        /// <summary>
        /// Total number of users assigned to this unit
        /// </summary>
        public long TotalUsers { get; set; }
        
        /// <summary>
        /// Number of users with this unit as their primary unit
        /// </summary>
        public long PrimaryUsers { get; set; }
    }
    
    /// <summary>
    /// Response for operations that modify user-unit assignments
    /// </summary>
    public class UserUnitOperationResultDTO
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message describing the result of the operation
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional count of affected items
        /// </summary>
        public int? AffectedCount { get; set; }
    }
}
