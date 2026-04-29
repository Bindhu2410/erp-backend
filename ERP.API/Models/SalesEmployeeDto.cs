using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class SalesEmployeeCreateDto
    {
        // General Info
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
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

        // Additional Information
        public string? PassportNo { get; set; }
        public string? NameAsPerPassport { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? PassportIssuePlace { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public string? MothersMaidenName { get; set; }
        public string? OldPassportNo { get; set; }
        public string? InsuranceName { get; set; }
        public string? InsuranceNo { get; set; }
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? BankAcNo { get; set; }
        public string? IfscCode { get; set; }
        public string? PanNo { get; set; }
        public string? EsiNo { get; set; }
        public DateTime? EsiEffDate { get; set; }
        public string? PfNo { get; set; }
        public DateTime? PfEffDate { get; set; }
        public string? VoterId { get; set; }
        public string? DrivingLicenseNo { get; set; }
        public string? AadharNo { get; set; }

        // Contact Details: Permanent
        public string? PermAddress { get; set; }
        public string? PermCity { get; set; }
        public string? PermState { get; set; }
        public string? PermTelephone { get; set; }
        public string? PermEmail { get; set; }
        public string? PermContactPerson { get; set; }
        public string? PermPincode { get; set; }
        public string? PermCountry { get; set; }
        public string? PermMobile { get; set; }

        // Communication Contact
        public string? CommAddress { get; set; }
        public string? CommCity { get; set; }
        public string? CommState { get; set; }
        public string? CommTelephone { get; set; }
        public string? CommEmail { get; set; }
        public string? CommContactPerson { get; set; }
        public string? CommPincode { get; set; }
        public string? CommCountry { get; set; }
        public string? CommMobile { get; set; }

        // Family Details
        public string? FamilyName { get; set; }
        public int? FamilyAge { get; set; }
        public string? FamilyRelationship { get; set; }
        public string? FamilyOccupation { get; set; }
        public string? FamilyPrimaryContact { get; set; }
        public string? FamilyContact { get; set; }
        public string? FamilyEmail { get; set; }

        // Education Skills
        public string? EduCourse { get; set; }
        public string? EduBoard { get; set; }
        public string? EduInstitution { get; set; }
        public string? EduPassDate { get; set; }
        public decimal? EduPercentage { get; set; }

        // Special achievements
        public string? AchievementWhat { get; set; }
        public DateTime? AchievementWhen { get; set; }
        public string? AchievementWhere { get; set; }
        public string? AchievementRemarks { get; set; }

        // Previous Employment
        public string? PrevCompanyName { get; set; }
        public string? PrevLastDesignation { get; set; }
        public int? PrevRelevantExpYear { get; set; }
        public int? PrevRelevantExpMonth { get; set; }
        public string? PrevPpfNo { get; set; }
        public string? PrevPesiNo { get; set; }
        public DateTime? PrevStartDate { get; set; }
        public DateTime? PrevEndDate { get; set; }

        // Allowance Details - Now handled by separate EmployeeAllowance model
        public List<EmployeeAllowanceCreateDto>? EmployeeAllowances { get; set; }
        public int? UserCreated { get; set; }

        // Optional link to auth user account
        public int? UserId { get; set; }
    }

    public class SalesEmployeeUpdateDto
    {
        // General Info
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

        // Additional Information
        public string? PassportNo { get; set; }
        public string? NameAsPerPassport { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? PassportIssuePlace { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public string? MothersMaidenName { get; set; }
        public string? OldPassportNo { get; set; }
        public string? InsuranceName { get; set; }
        public string? InsuranceNo { get; set; }
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? BankAcNo { get; set; }
        public string? IfscCode { get; set; }
        public string? PanNo { get; set; }
        public string? EsiNo { get; set; }
        public DateTime? EsiEffDate { get; set; }
        public string? PfNo { get; set; }
        public DateTime? PfEffDate { get; set; }
        public string? VoterId { get; set; }
        public string? DrivingLicenseNo { get; set; }
        public string? AadharNo { get; set; }

        // Contact Details: Permanent
        public string? PermAddress { get; set; }
        public string? PermCity { get; set; }
        public string? PermState { get; set; }
        public string? PermTelephone { get; set; }
        public string? PermEmail { get; set; }
        public string? PermContactPerson { get; set; }
        public string? PermPincode { get; set; }
        public string? PermCountry { get; set; }
        public string? PermMobile { get; set; }

        // Communication Contact
        public string? CommAddress { get; set; }
        public string? CommCity { get; set; }
        public string? CommState { get; set; }
        public string? CommTelephone { get; set; }
        public string? CommEmail { get; set; }
        public string? CommContactPerson { get; set; }
        public string? CommPincode { get; set; }
        public string? CommCountry { get; set; }
        public string? CommMobile { get; set; }

        // Family Details
        public string? FamilyName { get; set; }
        public int? FamilyAge { get; set; }
        public string? FamilyRelationship { get; set; }
        public string? FamilyOccupation { get; set; }
        public string? FamilyPrimaryContact { get; set; }
        public string? FamilyContact { get; set; }
        public string? FamilyEmail { get; set; }

        // Education Skills
        public string? EduCourse { get; set; }
        public string? EduBoard { get; set; }
        public string? EduInstitution { get; set; }
        public string? EduPassDate { get; set; }
        public decimal? EduPercentage { get; set; }

        // Special achievements
        public string? AchievementWhat { get; set; }
        public DateTime? AchievementWhen { get; set; }
        public string? AchievementWhere { get; set; }
        public string? AchievementRemarks { get; set; }

        // Previous Employment
        public string? PrevCompanyName { get; set; }
        public string? PrevLastDesignation { get; set; }
        public int? PrevRelevantExpYear { get; set; }
        public int? PrevRelevantExpMonth { get; set; }
        public string? PrevPpfNo { get; set; }
        public string? PrevPesiNo { get; set; }
        public DateTime? PrevStartDate { get; set; }
        public DateTime? PrevEndDate { get; set; }

        // Allowance Details - Now handled by separate EmployeeAllowance model
        public List<EmployeeAllowanceUpdateDto>? EmployeeAllowances { get; set; }

        public int? UserUpdated { get; set; }

        // Optional link to auth user account
        public int? UserId { get; set; }
    }

    public class SalesEmployeeDetailDto
    {
        public int? Id { get; set; }
        public string? EmployeeId { get; set; }
        
        // General Info
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
        public string? DepartmentName { get; set; }
        public int? Designation { get; set; }
        public string? DesignationName { get; set; }
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

        // Additional Information
        public string? PassportNo { get; set; }
        public string? NameAsPerPassport { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? PassportIssuePlace { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public string? MothersMaidenName { get; set; }
        public string? OldPassportNo { get; set; }
        public string? InsuranceName { get; set; }
        public string? InsuranceNo { get; set; }
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? BankAcNo { get; set; }
        public string? IfscCode { get; set; }
        public string? PanNo { get; set; }
        public string? EsiNo { get; set; }
        public DateTime? EsiEffDate { get; set; }
        public string? PfNo { get; set; }
        public DateTime? PfEffDate { get; set; }
        public string? VoterId { get; set; }
        public string? DrivingLicenseNo { get; set; }
        public string? AadharNo { get; set; }

        // Contact Details: Permanent
        public string? PermAddress { get; set; }
        public string? PermCity { get; set; }
        public string? PermState { get; set; }
        public string? PermTelephone { get; set; }
        public string? PermEmail { get; set; }
        public string? PermContactPerson { get; set; }
        public string? PermPincode { get; set; }
        public string? PermCountry { get; set; }
        public string? PermMobile { get; set; }

        // Communication Contact
        public string? CommAddress { get; set; }
        public string? CommCity { get; set; }
        public string? CommState { get; set; }
        public string? CommTelephone { get; set; }
        public string? CommEmail { get; set; }
        public string? CommContactPerson { get; set; }
        public string? CommPincode { get; set; }
        public string? CommCountry { get; set; }
        public string? CommMobile { get; set; }

        // Family Details
        public string? FamilyName { get; set; }
        public int? FamilyAge { get; set; }
        public string? FamilyRelationship { get; set; }
        public string? FamilyOccupation { get; set; }
        public string? FamilyPrimaryContact { get; set; }
        public string? FamilyContact { get; set; }
        public string? FamilyEmail { get; set; }

        // Education Skills
        public string? EduCourse { get; set; }
        public string? EduBoard { get; set; }
        public string? EduInstitution { get; set; }
        public string? EduPassDate { get; set; }
        public decimal? EduPercentage { get; set; }

        // Special achievements
        public string? AchievementWhat { get; set; }
        public DateTime? AchievementWhen { get; set; }
        public string? AchievementWhere { get; set; }
        public string? AchievementRemarks { get; set; }

        // Previous Employment
        public string? PrevCompanyName { get; set; }
        public string? PrevLastDesignation { get; set; }
        public int? PrevRelevantExpYear { get; set; }
        public int? PrevRelevantExpMonth { get; set; }
        public string? PrevPpfNo { get; set; }
        public string? PrevPesiNo { get; set; }
        public DateTime? PrevStartDate { get; set; }
        public DateTime? PrevEndDate { get; set; }

        // Allowance Details
        public List<EmployeeAllowanceDto>? EmployeeAllowances { get; set; }

        // Audit info
        public int? UserCreated { get; set; }
        public int? UserId { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}