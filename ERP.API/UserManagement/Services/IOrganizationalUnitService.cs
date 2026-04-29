using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    public interface IOrganizationalUnitService
    {
        /// <summary>
        /// Creates a new organizational unit
        /// </summary>
        Task<CreateUnitResultDto> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto dto, int? createdBy);
        
        /// <summary>
        /// Gets an organizational unit by ID
        /// </summary>
        Task<OrganizationalUnitDto?> GetOrganizationalUnitByIdAsync(int unitId);
        
        /// <summary>
        /// Updates an existing organizational unit
        /// </summary>
        Task<OperationResultDto> UpdateOrganizationalUnitAsync(int unitId, UpdateOrganizationalUnitDto dto);
        
        /// <summary>
        /// Deletes an organizational unit
        /// </summary>
        Task<OperationResultDto> DeleteOrganizationalUnitAsync(int unitId);
        
        /// <summary>
        /// Sets the active status of an organizational unit
        /// </summary>
        Task<OperationResultDto> SetOrganizationalUnitStatusAsync(int unitId, bool isActive);
        
        /// <summary>
        /// Gets all organizational units with optional active filter
        /// </summary>
        Task<List<OrganizationalUnitDto>> GetAllOrganizationalUnitsAsync(bool? isActive = null);
        
        /// <summary>
        /// Gets paginated organizational units with filtering
        /// </summary>
        Task<OrganizationalUnitPaginatedResponseDto> GetOrganizationalUnitsPaginatedAsync(OrganizationalUnitQueryParametersDto parameters);
        
        /// <summary>
        /// Gets child units for a parent unit
        /// </summary>
        Task<List<OrganizationalUnitChildDto>> GetChildUnitsAsync(int parentUnitId, bool? isActive = null);
        
        /// <summary>
        /// Gets top-level units (units without a parent)
        /// </summary>
        Task<List<OrganizationalUnitChildDto>> GetTopLevelUnitsAsync(bool? isActive = null);
        
        /// <summary>
        /// Gets the organizational unit hierarchy
        /// </summary>
        Task<List<OrganizationalUnitHierarchyDto>> GetUnitHierarchyAsync(int? unitId = null, bool? isActive = true);
        
        /// <summary>
        /// Searches for organizational units
        /// </summary>
        Task<List<OrganizationalUnitSearchResultDto>> SearchOrganizationalUnitsAsync(string searchTerm, string? unitType = null, bool? isActive = null);
        
        /// <summary>
        /// Gets units managed by a specific manager
        /// </summary>
        Task<List<OrganizationalUnitDto>> GetUnitsByManagerAsync(int managerId, bool? isActive = null);
        
        /// <summary>
        /// Gets all available organizational unit types
        /// </summary>
        Task<List<OrganizationalUnitTypeDto>> GetUnitTypesAsync();
        
        /// <summary>
        /// Gets organizational unit statistics
        /// </summary>
        Task<OrganizationalUnitStatisticsDto> GetOrganizationalUnitStatisticsAsync();
        
        /// <summary>
        /// Assigns a manager to an organizational unit
        /// </summary>
        Task<OperationResultDto> AssignManagerToUnitAsync(int unitId, int managerId);
        
        /// <summary>
        /// Moves an organizational unit to a new parent
        /// </summary>
        Task<OperationResultDto> MoveOrganizationalUnitAsync(int unitId, int? newParentId);
    }
}
