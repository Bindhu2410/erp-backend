using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; }

        public string Description { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; }
    }

    public class UpdatePermissionDto
    {
        [StringLength(100)]
        public string PermissionName { get; set; }
        
        public string Description { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class PermissionListDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public long TotalCount { get; set; }
    }

    public class PaginatedPermissionsDto
    {
        public IEnumerable<PermissionListDto> Permissions { get; set; } = new List<PermissionListDto>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PermissionStatisticsDto
    {
        public long TotalPermissions { get; set; }
        public long ActivePermissions { get; set; }
        public long InactivePermissions { get; set; }
        public long CategoriesCount { get; set; }
    }

    public class BatchCreatePermissionsDto
    {
        [Required]
        public List<CreatePermissionDto> Permissions { get; set; }
    }

    public class BatchCreatePermissionsResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int CreatedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
