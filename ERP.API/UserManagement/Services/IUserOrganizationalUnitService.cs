using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Service interface for managing user assignments to organizational units
    /// </summary>
    public interface IUserOrganizationalUnitService
    {
        /// <summary>
        /// Assigns a user to an organizational unit
        /// </summary>
        /// <param name="dto">The assignment details</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> AssignUserToUnitAsync(CreateUserOrganizationalUnitDTO dto);
        
        /// <summary>
        /// Gets a specific user-to-unit assignment
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <returns>The assignment details or null if not found</returns>
        Task<UserOrganizationalUnitDTO?> GetUserUnitAssignmentAsync(int userId, int unitId);
        
        /// <summary>
        /// Updates a user's assignment to an organizational unit (primary status)
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <param name="dto">The updated assignment details</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> UpdateUserUnitAssignmentAsync(int userId, int unitId, UpdateUserOrganizationalUnitDTO dto);
        
        /// <summary>
        /// Removes a user from an organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> RemoveUserFromUnitAsync(int userId, int unitId);
        
        /// <summary>
        /// Gets all users assigned to a specific organizational unit
        /// </summary>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <param name="includeChildUnits">Whether to include users from child units</param>
        /// <returns>List of users assigned to the unit</returns>
        Task<List<UserOrganizationalUnitDTO>> GetUsersInUnitAsync(int unitId, bool includeChildUnits = false);
        
        /// <summary>
        /// Gets all organizational units a user is assigned to
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of organizational units the user is assigned to</returns>
        Task<List<UserOrganizationalUnitDTO>> GetUserUnitsAsync(int userId);
        
        /// <summary>
        /// Gets a user's primary organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>The user's primary organizational unit or null if not found</returns>
        Task<UserOrganizationalUnitDTO?> GetUserPrimaryUnitAsync(int userId);
        
        /// <summary>
        /// Sets a user's primary organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit to set as primary</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> SetUserPrimaryUnitAsync(int userId, int unitId);
        
        /// <summary>
        /// Assigns multiple users to an organizational unit
        /// </summary>
        /// <param name="dto">The bulk assignment details</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> AssignUsersToUnitAsync(BulkUserAssignmentDTO dto);
        
        /// <summary>
        /// Assigns a user to multiple organizational units
        /// </summary>
        /// <param name="dto">The multi-unit assignment details</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> AssignUserToUnitsAsync(UserMultiUnitAssignmentDTO dto);
        
        /// <summary>
        /// Gets users that aren't assigned to any organizational unit
        /// </summary>
        /// <returns>List of users without unit assignments</returns>
        Task<List<UserOrganizationalUnitDTO>> GetUsersWithoutUnitAsync();
        
        /// <summary>
        /// Gets organizational units that have no users assigned
        /// </summary>
        /// <returns>List of empty organizational units</returns>
        Task<List<UserOrganizationalUnitDTO>> GetEmptyUnitsAsync();
        
        /// <summary>
        /// Gets assignment statistics for all organizational units
        /// </summary>
        /// <returns>List of unit statistics</returns>
        Task<List<UnitAssignmentStatsDTO>> GetUnitAssignmentStatsAsync();
        
        /// <summary>
        /// Gets a paginated list of user-unit assignments with search capabilities
        /// </summary>
        /// <param name="parameters">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of assignments</returns>
        Task<PaginatedUserUnitAssignmentsDTO> GetUserUnitAssignmentsPaginatedAsync(UserUnitAssignmentQueryParameters parameters);
        
        /// <summary>
        /// Gets statistics about user assignments by unit type
        /// </summary>
        /// <returns>List of unit type statistics</returns>
        Task<List<UnitTypeStatisticsDTO>> GetUserCountByUnitTypeAsync();
        
        /// <summary>
        /// Transfers all users from one organizational unit to another
        /// </summary>
        /// <param name="dto">The transfer details</param>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> TransferUsersBetweenUnitsAsync(TransferUsersDTO dto);
        
        /// <summary>
        /// Ensures all users have a primary organizational unit set
        /// </summary>
        /// <returns>Result of the operation</returns>
        Task<UserUnitOperationResultDTO> EnsureUsersPrimaryUnitAsync();
    }
}
