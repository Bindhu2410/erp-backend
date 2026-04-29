using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// DTO for creating a delegation permission
    /// </summary>
    public class CreateDelegationPermissionDto
    {
        [Required(ErrorMessage = "DelegationId is required")]
        public int DelegationId { get; set; }

        [Required(ErrorMessage = "PermissionId is required")]
        public int PermissionId { get; set; }
    }

    /// <summary>
    /// DTO for bulk updating delegation permissions
    /// </summary>
    public class UpdateDelegationPermissionsDto
    {
        [Required(ErrorMessage = "DelegationId is required")]
        public int DelegationId { get; set; }

        [Required(ErrorMessage = "PermissionIds is required")]
        public List<int> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// DTO for delegation permission response
    /// </summary>
    public class DelegationPermissionDto
    {
        public int DelegationId { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for detailed delegation permission with delegation info
    /// </summary>
    public class DetailedDelegationPermissionDto
    {
        public int DelegationId { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public string DelegationName { get; set; } = string.Empty;
        public string DelegatorUsername { get; set; } = string.Empty;
        public string DelegateUsername { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for paged delegation permissions request
    /// </summary>
    public class PagedDelegationPermissionsRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        public int? DelegationId { get; set; }
    }

    /// <summary>
    /// DTO for paged delegation permissions response
    /// </summary>
    public class PagedDelegationPermissionsResponseDto
    {
        public List<DetailedDelegationPermissionDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// DTO for available permissions for delegation
    /// </summary>
    public class AvailablePermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for delegation permission summary
    /// </summary>
    public class DelegationPermissionSummaryDto
    {
        public int DelegationId { get; set; }
        public string DelegationName { get; set; } = string.Empty;
        public string DelegatorUsername { get; set; } = string.Empty;
        public string DelegateUsername { get; set; } = string.Empty;
        public long TotalPermissions { get; set; }
        public long ActivePermissions { get; set; }
        public List<string> Categories { get; set; } = new();
    }

    /// <summary>
    /// DTO for delegation permission operation result
    /// </summary>
    public class DelegationPermissionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for bulk delegation permission operation result
    /// </summary>
    public class BulkDelegationPermissionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
    }

    /// <summary>
    /// DTO for delete all delegation permissions result
    /// </summary>
    public class DeleteAllDelegationPermissionsResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
    }
}
