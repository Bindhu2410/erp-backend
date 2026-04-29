using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("employees")]
    public class SalesEmployee : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("employee_id")]
        [MaxLength(100)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? EmployeeId { get; set; }

        // General Info
        [Column("first_name")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Column("fathers_name")]
        [MaxLength(100)]
        public string? FathersName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("disability")]
        [MaxLength(255)]
        public string? Disability { get; set; }

        [Column("identification")]
        public string? Identification { get; set; }

        [Column("blood_group")]
        [MaxLength(10)]
        public string? BloodGroup { get; set; }

        [Column("height")]
        public decimal? Height { get; set; }

        [Column("weight")]
        public decimal? Weight { get; set; }

        [Column("type_of_employment")]
        [MaxLength(255)]
        public string? TypeOfEmployment { get; set; }

        [Column("salary_debit_acc")]
        [MaxLength(255)]
        public string? SalaryDebitAcc { get; set; }

        [Column("salary_credit_acc")]
        [MaxLength(255)]
        public string? SalaryCreditAcc { get; set; }

        [Column("image_url")]
        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Column("sales_man")]
        [MaxLength(255)]
        public string? SalesMan { get; set; }

        // Salary Details
        [Column("salary")]
        public decimal? Salary { get; set; }

        [Column("basic_salary")]
        public decimal? BasicSalary { get; set; }

        [Column("hra")]
        public decimal? Hra { get; set; }

        [Column("conveyance")]
        public decimal? Conveyance { get; set; }

        [Column("city_compn")]
        public decimal? CityCompn { get; set; }

        [Column("esi_app")]
        public bool? EsiApp { get; set; }

        [Column("esi_num")]
        [MaxLength(50)]
        public string? EsiNum { get; set; }

        [Column("esi_dt")]
        public DateTime? EsiDt { get; set; }

        [Column("esi_per")]
        public decimal? EsiPer { get; set; }

        [Column("pf_app")]
        public bool? PfApp { get; set; }

        [Column("pf_num")]
        [MaxLength(50)]
        public string? PfNum { get; set; }

        [Column("pf_dt")]
        public DateTime? PfDt { get; set; }

        [Column("pf_per")]
        public decimal? PfPer { get; set; }

        [Column("tds")]
        public decimal? Tds { get; set; }

        [Column("eff_from")]
        public DateTime? EffFrom { get; set; }

        [Column("eff_to")]
        public DateTime? EffTo { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Assignment Details
        [Column("date_of_joining")]
        public DateTime? DateOfJoining { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }

        [Column("designation")]
        public int? Designation { get; set; }

        [Column("last_working_date")]
        public DateTime? LastWorkingDate { get; set; }

        [Column("rejoinee_date")]
        public DateTime? RejoineeDate { get; set; }

        [Column("employee_grade")]
        [MaxLength(50)]
        public string? EmployeeGrade { get; set; }

        [Column("report_manager")]
        [MaxLength(100)]
        public string? ReportManager { get; set; }

        [Column("report_manager_code")]
        [MaxLength(50)]
        public string? ReportManagerCode { get; set; }

        [Column("reporting_head_mail")]
        [MaxLength(200)]
        public string? ReportingHeadMail { get; set; }

        [Column("notice_period")]
        public int? NoticePeriod { get; set; }

        [Column("cost_center")]
        [MaxLength(50)]
        public string? CostCenter { get; set; }

        [Column("id_card_no")]
        [MaxLength(50)]
        public string? IdCardNo { get; set; }

        [Column("country")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Column("city")]
        [MaxLength(100)]
        public string? City { get; set; }

        // Personal Details
        [Column("birth_place")]
        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        [Column("religion")]
        [MaxLength(50)]
        public string? Religion { get; set; }

        [Column("home_state")]
        [MaxLength(100)]
        public string? HomeState { get; set; }

        [Column("nationality")]
        [MaxLength(100)]
        public string? Nationality { get; set; }

        [Column("country_birth")]
        [MaxLength(100)]
        public string? CountryBirth { get; set; }

        [Column("is_ex_service")]
        public bool? IsExService { get; set; }

        [Column("nominee")]
        [MaxLength(100)]
        public string? Nominee { get; set; }

        [Column("nominee_relationship")]
        [MaxLength(50)]
        public string? NomineeRelationship { get; set; }

        [Column("recruiter_name")]
        [MaxLength(100)]
        public string? RecruiterName { get; set; }

        [Column("reference")]
        [MaxLength(150)]
        public string? Reference { get; set; }

        [Column("language_known")]
        [MaxLength(100)]
        public string? LanguageKnown { get; set; }

        // Additional Information
        [Column("passport_no")]
        [MaxLength(50)]
        public string? PassportNo { get; set; }

        [Column("name_as_per_passport")]
        [MaxLength(150)]
        public string? NameAsPerPassport { get; set; }

        [Column("passport_expiry_date")]
        public DateTime? PassportExpiryDate { get; set; }

        [Column("passport_issue_place")]
        [MaxLength(100)]
        public string? PassportIssuePlace { get; set; }

        [Column("passport_issue_date")]
        public DateTime? PassportIssueDate { get; set; }

        [Column("mothers_maiden_name")]
        [MaxLength(150)]
        public string? MothersMaidenName { get; set; }

        [Column("old_passport_no")]
        [MaxLength(50)]
        public string? OldPassportNo { get; set; }

        [Column("insurance_name")]
        [MaxLength(100)]
        public string? InsuranceName { get; set; }

        [Column("insurance_no")]
        [MaxLength(50)]
        public string? InsuranceNo { get; set; }

        [Column("bank_name")]
        [MaxLength(100)]
        public string? BankName { get; set; }

        [Column("branch_name")]
        [MaxLength(100)]
        public string? BranchName { get; set; }

        [Column("bank_ac_no")]
        [MaxLength(100)]
        public string? BankAcNo { get; set; }

        [Column("ifsc_code")]
        [MaxLength(50)]
        public string? IfscCode { get; set; }

        [Column("pan_no")]
        [MaxLength(50)]
        public string? PanNo { get; set; }

        [Column("esi_no")]
        [MaxLength(50)]
        public string? EsiNo { get; set; }

        [Column("esi_eff_date")]
        public DateTime? EsiEffDate { get; set; }

        [Column("pf_no")]
        [MaxLength(50)]
        public string? PfNo { get; set; }

        [Column("pf_eff_date")]
        public DateTime? PfEffDate { get; set; }

        [Column("voter_id")]
        [MaxLength(50)]
        public string? VoterId { get; set; }

        [Column("driving_license_no")]
        [MaxLength(50)]
        public string? DrivingLicenseNo { get; set; }

        [Column("aadhar_no")]
        [MaxLength(20)]
        public string? AadharNo { get; set; }

        // Contact Details: Permanent
        [Column("perm_address")]
        [MaxLength(255)]
        public string? PermAddress { get; set; }

        [Column("perm_city")]
        [MaxLength(100)]
        public string? PermCity { get; set; }

        [Column("perm_state")]
        [MaxLength(100)]
        public string? PermState { get; set; }

        [Column("perm_telephone")]
        [MaxLength(20)]
        public string? PermTelephone { get; set; }

        [Column("perm_email")]
        [MaxLength(150)]
        public string? PermEmail { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Column("perm_contact_person")]
        [MaxLength(100)]
        public string? PermContactPerson { get; set; }

        [Column("perm_pincode")]
        [MaxLength(10)]
        public string? PermPincode { get; set; }

        [Column("perm_country")]
        [MaxLength(100)]
        public string? PermCountry { get; set; }

        [Column("perm_mobile")]
        [MaxLength(50)]
        public string? PermMobile { get; set; }

        // Communication Contact
        [Column("comm_address")]
        [MaxLength(255)]
        public string? CommAddress { get; set; }

        [Column("comm_city")]
        [MaxLength(100)]
        public string? CommCity { get; set; }

        [Column("comm_state")]
        [MaxLength(100)]
        public string? CommState { get; set; }

        [Column("comm_telephone")]
        [MaxLength(20)]
        public string? CommTelephone { get; set; }

        [Column("comm_email")]
        [MaxLength(150)]
        public string? CommEmail { get; set; }

        [Column("comm_contact_person")]
        [MaxLength(100)]
        public string? CommContactPerson { get; set; }

        [Column("comm_pincode")]
        [MaxLength(10)]
        public string? CommPincode { get; set; }

        [Column("comm_country")]
        [MaxLength(100)]
        public string? CommCountry { get; set; }

        [Column("comm_mobile")]
        [MaxLength(20)]
        public string? CommMobile { get; set; }

        // Family Details
        [Column("family_name")]
        [MaxLength(100)]
        public string? FamilyName { get; set; }

        [Column("family_age")]
        public int? FamilyAge { get; set; }

        [Column("family_relationship")]
        [MaxLength(50)]
        public string? FamilyRelationship { get; set; }

        [Column("family_occupation")]
        [MaxLength(100)]
        public string? FamilyOccupation { get; set; }

        [Column("family_primary_contact")]
        [MaxLength(20)]
        public string? FamilyPrimaryContact { get; set; }

        [Column("family_contact")]
        [MaxLength(20)]
        public string? FamilyContact { get; set; }

        [Column("family_email")]
        [MaxLength(100)]
        public string? FamilyEmail { get; set; }

        // Education Skills
        [Column("edu_course")]
        [MaxLength(150)]
        public string? EduCourse { get; set; }

        [Column("edu_board")]
        [MaxLength(150)]
        public string? EduBoard { get; set; }

        [Column("edu_institution")]
        [MaxLength(150)]
        public string? EduInstitution { get; set; }

        [Column("edu_pass_date")]
        [MaxLength(20)]
        public string? EduPassDate { get; set; }

        [Column("edu_percentage")]
        public decimal? EduPercentage { get; set; }

        // Special achievements
        [Column("achievement_what")]
        public string? AchievementWhat { get; set; }

        [Column("achievement_when")]
        public DateTime? AchievementWhen { get; set; }

        [Column("achievement_where")]
        [MaxLength(100)]
        public string? AchievementWhere { get; set; }

        [Column("achievement_remarks")]
        public string? AchievementRemarks { get; set; }

        // Previous Employment
        [Column("prev_company_name")]
        [MaxLength(150)]
        public string? PrevCompanyName { get; set; }

        [Column("prev_last_designation")]
        [MaxLength(150)]
        public string? PrevLastDesignation { get; set; }

        [Column("prev_relevant_exp_year")]
        public int? PrevRelevantExpYear { get; set; }

        [Column("prev_relevant_exp_month")]
        public int? PrevRelevantExpMonth { get; set; }

        [Column("prev_ppf_no")]
        [MaxLength(50)]
        public string? PrevPpfNo { get; set; }

        [Column("prev_pesi_no")]
        [MaxLength(50)]
        public string? PrevPesiNo { get; set; }

        [Column("prev_start_date")]
        public DateTime? PrevStartDate { get; set; }

        [Column("prev_end_date")]
        public DateTime? PrevEndDate { get; set; }

        // Navigation property for multiple allowances
        public virtual ICollection<EmployeeAllowance>? EmployeeAllowances { get; set; }
    }
}
