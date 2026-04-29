using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    public class OrganizationalUnitDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentUnitId { get; set; }
        public string? ParentUnitName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByUsername { get; set; }
        public long ChildCount { get; set; }
    }

    public class CreateOrganizationalUnitDto
    {
        [Required]
        [StringLength(100)]
        public string UnitName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UnitType { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int? ParentUnitId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOrganizationalUnitDto
    {
        [Required]
        [StringLength(100)]
        public string UnitName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UnitType { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int? ParentUnitId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class OrganizationalUnitChildDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentUnitId { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long ChildCount { get; set; }
    }

    public class OrganizationalUnitHierarchyDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int? ParentUnitId { get; set; }
        public string? ParentUnitName { get; set; }
        public bool IsActive { get; set; }
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;
        public long ChildCount { get; set; }
    }

    public class OrganizationalUnitSearchResultDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentUnitId { get; set; }
        public string? ParentUnitName { get; set; }
        public bool IsActive { get; set; }
        public string MatchType { get; set; } = string.Empty;
    }

    public class OrganizationalUnitTypeDto
    {
        public string UnitType { get; set; } = string.Empty;
        public long Count { get; set; }
    }

    public class OrganizationalUnitStatisticsDto
    {
        public long TotalUnits { get; set; }
        public long ActiveUnits { get; set; }
        public long InactiveUnits { get; set; }
        public long TopLevelUnits { get; set; }
        public long TotalUnitTypes { get; set; }
        public int MaxHierarchyDepth { get; set; }
    }

    public class OrganizationalUnitQueryParametersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? UnitType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class OrganizationalUnitPaginatedResponseDto
    {
        public List<OrganizationalUnitDto> Units { get; set; } = new List<OrganizationalUnitDto>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AssignManagerDto
    {
        [Required]
        public int UnitId { get; set; }

        [Required]
        public int ManagerId { get; set; }
    }

    public class MoveUnitDto
    {
        [Required]
        public int UnitId { get; set; }

        public int? NewParentId { get; set; }
    }

    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateUnitResultDto : OperationResultDto
    {
        public int UnitId { get; set; }
    }
}
