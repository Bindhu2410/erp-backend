using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// DTO for creating a new access delegation
    /// </summary>
    public class CreateAccessDelegationDto
    {
        [Required(ErrorMessage = "FromUserId is required")]
        public int FromUserId { get; set; }

        [Required(ErrorMessage = "ToUserId is required")]
        public int ToUserId { get; set; }

        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }

        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing access delegation
    /// </summary>
    public class UpdateAccessDelegationDto
    {
        [Required(ErrorMessage = "DelegationId is required")]
        public int DelegationId { get; set; }

        public int? FromUserId { get; set; }

        public int? ToUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        public string? Reason { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for access delegation details
    /// </summary>
    public class AccessDelegationDto
    {
        public int DelegationId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
    }

    /// <summary>
    /// DTO for paginated access delegation list
    /// </summary>
    public class AccessDelegationPagedDto
    {
        public List<AccessDelegationDto> Delegations { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO for delegation search parameters
    /// </summary>
    public class DelegationSearchDto
    {
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchText { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO for delegation history with role information
    /// </summary>
    public class DelegationHistoryDto
    {
        public int DelegationId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string DelegationRole { get; set; } = string.Empty; // "Delegator" or "Delegate"
    }

    /// <summary>
    /// DTO for paginated delegation history
    /// </summary>
    public class DelegationHistoryPagedDto
    {
        public List<DelegationHistoryDto> History { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO for delegation statistics
    /// </summary>
    public class DelegationStatisticsDto
    {
        public long TotalDelegations { get; set; }
        public long ActiveDelegations { get; set; }
        public long ExpiredDelegations { get; set; }
        public long UpcomingDelegations { get; set; }
        public decimal AverageDurationDays { get; set; }
    }

    /// <summary>
    /// DTO for user delegation activity
    /// </summary>
    public class UserDelegationActivityDto
    {
        public int UserId { get; set; }
        public long DelegationCount { get; set; }
    }

    /// <summary>
    /// DTO for extending delegation end date
    /// </summary>
    public class ExtendDelegationDto
    {
        [Required(ErrorMessage = "DelegationId is required")]
        public int DelegationId { get; set; }

        [Required(ErrorMessage = "NewEndDate is required")]
        public DateTime NewEndDate { get; set; }
    }

    /// <summary>
    /// DTO for date range query parameters
    /// </summary>
    public class DateRangeQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool ActiveOnly { get; set; } = false;
    }

    /// <summary>
    /// DTO for user-specific delegation query parameters
    /// </summary>
    public class UserDelegationQueryDto
    {
        public int UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// DTO for API response wrapper
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO for delegation statistics query parameters
    /// </summary>
    public class DelegationStatsQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO for most active users query parameters
    /// </summary>
    public class MostActiveUsersQueryDto
    {
        public int Limit { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
