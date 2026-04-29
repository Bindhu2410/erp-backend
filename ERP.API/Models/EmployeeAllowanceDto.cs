using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
 using ERP.API.Models;
 
namespace ERP.API.Models
{
    public class EmployeeAllowanceDto
    {
        public int? Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [MaxLength(100)]
        public string? AllowanceType { get; set; }

        public decimal? AllowanceAmount { get; set; }

        public DateTime? AllowEffFrom { get; set; }

        public DateTime? AllowEffTo { get; set; }
    }

    public class EmployeeAllowanceCreateDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [MaxLength(100)]
        public string? AllowanceType { get; set; }

        public decimal? AllowanceAmount { get; set; }

        public DateTime? AllowEffFrom { get; set; }

        public DateTime? AllowEffTo { get; set; }

        public int? UserCreated { get; set; }
    }

    public class EmployeeAllowanceUpdateDto
    {
        [MaxLength(100)]
        public string? AllowanceType { get; set; }

        public decimal? AllowanceAmount { get; set; }

        public DateTime? AllowEffFrom { get; set; }

        public DateTime? AllowEffTo { get; set; }

        public int? UserUpdated { get; set; }
    }

    /// <summary>
    /// DTO for returning employee with allowance details
    /// </summary>
    public class SalesEmployeeWithAllowancesDto
    {
        public int? Id { get; set; }
        public string? EmployeeId { get; set; }
        // Optional link to auth user account
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FathersName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Disability { get; set; }
        public string? Identification { get; set; }
        public string? BloodGroup { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? TypeOfEmployment { get; set; }
        public string? SalaryDebitAcc { get; set; }
        public string? SalaryCreditAcc { get; set; }
        public string? ImageUrl { get; set; }
        public string? SalesMan { get; set; }

        // Salary Details
        public decimal? Salary { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? Hra { get; set; }
        public decimal? Conveyance { get; set; }
        public decimal? CityCompn { get; set; }
        public bool? EsiApp { get; set; }
        public string? EsiNum { get; set; }
        public DateTime? EsiDt { get; set; }
        public decimal? EsiPer { get; set; }
        public bool? PfApp { get; set; }
        public string? PfNum { get; set; }
        public DateTime? PfDt { get; set; }
        public decimal? PfPer { get; set; }
        public decimal? Tds { get; set; }
        public DateTime? EffFrom { get; set; }
        public DateTime? EffTo { get; set; }
        public bool? Active { get; set; }

        // Assignment Details
        public DateTime? DateOfJoining { get; set; }
        public int? DepartmentId { get; set; }
        public int? Designation { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public DateTime? RejoineeDate { get; set; }
        public string? EmployeeGrade { get; set; }
        public string? ReportManager { get; set; }
        public string? ReportManagerCode { get; set; }
        public string? ReportingHeadMail { get; set; }
        public int? NoticePeriod { get; set; }
        public string? CostCenter { get; set; }
        public string? IdCardNo { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        // Personal Details
        public string? BirthPlace { get; set; }
        public string? Religion { get; set; }
        public string? HomeState { get; set; }
        public string? Nationality { get; set; }
        public string? CountryBirth { get; set; }
        public bool? IsExService { get; set; }
        public string? Nominee { get; set; }
        public string? NomineeRelationship { get; set; }
        public string? RecruiterName { get; set; }
        public string? Reference { get; set; }
        public string? LanguageKnown { get; set; }

        // Allowance Details
        public List<EmployeeAllowanceDto>? EmployeeAllowances { get; set; } = new List<EmployeeAllowanceDto>();

        // Audit
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
