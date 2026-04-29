using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesEmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesEmployeeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all employees with allowance details
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesEmployeeDetailDto>>> GetAll()
        {
            var employees = await _context.SalesEmployees
                .Include(e => e.EmployeeAllowances)
                .Where(e => e.Active == true)
                .ToListAsync();

            var departmentIds = employees.Where(e => e.DepartmentId.HasValue).Select(e => e.DepartmentId.Value).Distinct().ToList();
            var designationIds = employees.Where(e => e.Designation.HasValue).Select(e => e.Designation.Value).Distinct().ToList();

            var departments = await _context.Departments.Where(d => departmentIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);
            var designations = await _context.Designations.Where(d => designationIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);

            var employeeDtos = employees.Select(employee => new SalesEmployeeDetailDto
            {
                Id = employee.Id,
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                UserId = employee.UserId,
                BasicSalary = employee.BasicSalary,
                Active = employee.Active,
                DateOfJoining = employee.DateOfJoining,
                EmployeeGrade = employee.EmployeeGrade,
                Designation = employee.Designation,
                DesignationName = employee.Designation.HasValue && designations.ContainsKey(employee.Designation.Value) ? designations[employee.Designation.Value] : null,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.DepartmentId.HasValue && departments.ContainsKey(employee.DepartmentId.Value) ? departments[employee.DepartmentId.Value] : null,
                EmployeeAllowances = employee.EmployeeAllowances?
                    .Where(a => a.Active == true)
                    .Select(a => new EmployeeAllowanceDto
                    {
                        Id = a.Id,
                        EmployeeId = a.EmployeeId,
                        AllowanceType = a.AllowanceType,
                        AllowanceAmount = a.AllowanceAmount,
                        AllowEffFrom = a.AllowEffFrom,
                        AllowEffTo = a.AllowEffTo
                    })
                    .ToList() ?? new List<EmployeeAllowanceDto>()
            }).ToList();

            return Ok(employeeDtos);
        }

        /// <summary>
        /// Get employee by ID with allowance details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesEmployeeDetailDto>> GetById(int id)
        {
            var employee = await _context.SalesEmployees
                .Include(e => e.EmployeeAllowances)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            string? departmentName = null;
            string? designationName = null;

            if (employee.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == employee.DepartmentId.Value);
                departmentName = department?.Name;
            }

            if (employee.Designation.HasValue)
            {
                var designation = await _context.Designations.FirstOrDefaultAsync(d => d.Id == employee.Designation.Value);
                designationName = designation?.Name;
            }

            var employeeDto = new SalesEmployeeDetailDto
            {
                Id = employee.Id,
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FathersName = employee.FathersName,
                DateOfBirth = employee.DateOfBirth,
                Disability = employee.Disability,
                Identification = employee.Identification,
                BloodGroup = employee.BloodGroup,
                Height = employee.Height,
                Weight = employee.Weight,
                TypeOfEmployment = employee.TypeOfEmployment,
                SalaryDebitAcc = employee.SalaryDebitAcc,
                SalaryCreditAcc = employee.SalaryCreditAcc,
                ImageUrl = employee.ImageUrl,
                SalesMan = employee.SalesMan,
                Salary = employee.Salary,
                BasicSalary = employee.BasicSalary,
                Hra = employee.Hra,
                Conveyance = employee.Conveyance,
                CityCompn = employee.CityCompn,
                EsiApp = employee.EsiApp,
                EsiNum = employee.EsiNum,
                EsiDt = employee.EsiDt,
                EsiPer = employee.EsiPer,
                PfApp = employee.PfApp,
                PfNum = employee.PfNum,
                PfDt = employee.PfDt,
                PfPer = employee.PfPer,
                Tds = employee.Tds,
                EffFrom = employee.EffFrom,
                EffTo = employee.EffTo,
                Active = employee.Active,
                DateOfJoining = employee.DateOfJoining,
                DepartmentId = employee.DepartmentId,
                DepartmentName = departmentName,
                Designation = employee.Designation,
                DesignationName = designationName,
                LastWorkingDate = employee.LastWorkingDate,
                RejoineeDate = employee.RejoineeDate,
                EmployeeGrade = employee.EmployeeGrade,
                ReportManager = employee.ReportManager,
                ReportManagerCode = employee.ReportManagerCode,
                ReportingHeadMail = employee.ReportingHeadMail,
                NoticePeriod = employee.NoticePeriod,
                CostCenter = employee.CostCenter,
                IdCardNo = employee.IdCardNo,
                Country = employee.Country,
                City = employee.City,
                BirthPlace = employee.BirthPlace,
                Religion = employee.Religion,
                HomeState = employee.HomeState,
                Nationality = employee.Nationality,
                CountryBirth = employee.CountryBirth,
                IsExService = employee.IsExService,
                Nominee = employee.Nominee,
                NomineeRelationship = employee.NomineeRelationship,
                RecruiterName = employee.RecruiterName,
                Reference = employee.Reference,
                LanguageKnown = employee.LanguageKnown,
                EmployeeAllowances = employee.EmployeeAllowances?
                    .Where(a => a.Active == true)
                    .Select(a => new EmployeeAllowanceDto
                    {
                        Id = a.Id,
                        EmployeeId = a.EmployeeId,
                        AllowanceType = a.AllowanceType,
                        AllowanceAmount = a.AllowanceAmount,
                        AllowEffFrom = a.AllowEffFrom,
                        AllowEffTo = a.AllowEffTo
                    })
                    .ToList() ?? new List<EmployeeAllowanceDto>(),
                UserCreated = employee.UserCreated,
                DateCreated = employee.DateCreated,
                UserUpdated = employee.UserUpdated,
                DateUpdated = employee.DateUpdated
            };

            return Ok(employeeDto);
        }

        /// <summary>
        /// Get employee by Employee ID
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<SalesEmployee>> GetByEmployeeId(string employeeId)
        {
            var employee = await _context.SalesEmployees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return NotFound(new { message = $"Employee with Employee ID {employeeId} not found." });

            return Ok(employee);
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SalesEmployee>> Create([FromBody] SalesEmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = new SalesEmployee
            {
                // General Info
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FathersName = dto.FathersName,
                DateOfBirth = dto.DateOfBirth,
                Disability = dto.Disability,
                Identification = dto.Identification,
                BloodGroup = dto.BloodGroup,
                Height = dto.Height,
                Weight = dto.Weight,
                TypeOfEmployment = dto.TypeOfEmployment,
                SalaryDebitAcc = dto.SalaryDebitAcc,
                SalaryCreditAcc = dto.SalaryCreditAcc,
                ImageUrl = dto.ImageUrl,
                SalesMan = dto.SalesMan,

                // Salary Details
                Salary = dto.Salary,
                BasicSalary = dto.BasicSalary,
                Hra = dto.Hra,
                Conveyance = dto.Conveyance,
                CityCompn = dto.CityCompn,
                EsiApp = dto.EsiApp,
                EsiNum = dto.EsiNum,
                EsiDt = dto.EsiDt,
                EsiPer = dto.EsiPer,
                PfApp = dto.PfApp,
                PfNum = dto.PfNum,
                PfDt = dto.PfDt,
                PfPer = dto.PfPer,
                Tds = dto.Tds,
                EffFrom = dto.EffFrom,
                EffTo = dto.EffTo,
                Active = dto.Active ?? true,

                // Assignment Details
                DateOfJoining = dto.DateOfJoining,
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation,
                LastWorkingDate = dto.LastWorkingDate,
                RejoineeDate = dto.RejoineeDate,
                EmployeeGrade = dto.EmployeeGrade,
                ReportManager = dto.ReportManager,
                ReportManagerCode = dto.ReportManagerCode,
                ReportingHeadMail = dto.ReportingHeadMail,
                NoticePeriod = dto.NoticePeriod,
                CostCenter = dto.CostCenter,
                IdCardNo = dto.IdCardNo,
                Country = dto.Country,
                City = dto.City,

                // Personal Details
                BirthPlace = dto.BirthPlace,
                Religion = dto.Religion,
                HomeState = dto.HomeState,
                Nationality = dto.Nationality,
                CountryBirth = dto.CountryBirth,
                IsExService = dto.IsExService,
                Nominee = dto.Nominee,
                NomineeRelationship = dto.NomineeRelationship,
                RecruiterName = dto.RecruiterName,
                Reference = dto.Reference,
                LanguageKnown = dto.LanguageKnown,

                // Additional Information
                PassportNo = dto.PassportNo,
                NameAsPerPassport = dto.NameAsPerPassport,
                PassportExpiryDate = dto.PassportExpiryDate,
                PassportIssuePlace = dto.PassportIssuePlace,
                PassportIssueDate = dto.PassportIssueDate,
                MothersMaidenName = dto.MothersMaidenName,
                OldPassportNo = dto.OldPassportNo,
                InsuranceName = dto.InsuranceName,
                InsuranceNo = dto.InsuranceNo,
                BankName = dto.BankName,
                BranchName = dto.BranchName,
                BankAcNo = dto.BankAcNo,
                IfscCode = dto.IfscCode,
                PanNo = dto.PanNo,
                EsiNo = dto.EsiNo,
                EsiEffDate = dto.EsiEffDate,
                PfNo = dto.PfNo,
                PfEffDate = dto.PfEffDate,
                VoterId = dto.VoterId,
                DrivingLicenseNo = dto.DrivingLicenseNo,
                AadharNo = dto.AadharNo,

                // Contact Details: Permanent
                PermAddress = dto.PermAddress,
                PermCity = dto.PermCity,
                PermState = dto.PermState,
                PermTelephone = dto.PermTelephone,
                PermEmail = dto.PermEmail,
                PermContactPerson = dto.PermContactPerson,
                PermPincode = dto.PermPincode,
                PermCountry = dto.PermCountry,
                PermMobile = dto.PermMobile,

                // Communication Contact
                CommAddress = dto.CommAddress,
                CommCity = dto.CommCity,
                CommState = dto.CommState,
                CommTelephone = dto.CommTelephone,
                CommEmail = dto.CommEmail,
                CommContactPerson = dto.CommContactPerson,
                CommPincode = dto.CommPincode,
                CommCountry = dto.CommCountry,
                CommMobile = dto.CommMobile,

                // Family Details
                FamilyName = dto.FamilyName,
                FamilyAge = dto.FamilyAge,
                FamilyRelationship = dto.FamilyRelationship,
                FamilyOccupation = dto.FamilyOccupation,
                FamilyPrimaryContact = dto.FamilyPrimaryContact,
                FamilyContact = dto.FamilyContact,
                FamilyEmail = dto.FamilyEmail,

                // Education Skills
                EduCourse = dto.EduCourse,
                EduBoard = dto.EduBoard,
                EduInstitution = dto.EduInstitution,
                EduPassDate = dto.EduPassDate,
                EduPercentage = dto.EduPercentage,

                // Special achievements
                AchievementWhat = dto.AchievementWhat,
                AchievementWhen = dto.AchievementWhen,
                AchievementWhere = dto.AchievementWhere,
                AchievementRemarks = dto.AchievementRemarks,

                // Previous Employment
                PrevCompanyName = dto.PrevCompanyName,
                PrevLastDesignation = dto.PrevLastDesignation,
                PrevRelevantExpYear = dto.PrevRelevantExpYear,
                PrevRelevantExpMonth = dto.PrevRelevantExpMonth,
                PrevPpfNo = dto.PrevPpfNo,
                PrevPesiNo = dto.PrevPesiNo,
                PrevStartDate = dto.PrevStartDate,
                PrevEndDate = dto.PrevEndDate,

                UserCreated = dto.UserCreated,
                DateCreated = DateTime.UtcNow,
                UserUpdated = dto.UserCreated,
                DateUpdated = DateTime.UtcNow
            };

            _context.SalesEmployees.Add(employee);
            await _context.SaveChangesAsync();

            // Add allowance details if provided
            if (dto.EmployeeAllowances != null && dto.EmployeeAllowances.Any())
            {
                foreach (var allowanceDto in dto.EmployeeAllowances)
                {
                    var allowance = new EmployeeAllowance
                    {
                        EmployeeId = employee.Id.Value,
                        AllowanceType = allowanceDto.AllowanceType,
                        AllowanceAmount = allowanceDto.AllowanceAmount,
                        AllowEffFrom = allowanceDto.AllowEffFrom,
                        AllowEffTo = allowanceDto.AllowEffTo,
                        UserCreated = allowanceDto.UserCreated ?? dto.UserCreated,
                        DateCreated = DateTime.UtcNow,
                        UserUpdated = allowanceDto.UserCreated ?? dto.UserCreated,
                        DateUpdated = DateTime.UtcNow,
                        Active = true
                    };
                    _context.EmployeeAllowances.Add(allowance);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesEmployeeUpdateDto dto)
        {
            try
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _context.SalesEmployees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            // Update fields if provided
            if (dto.FirstName != null) employee.FirstName = dto.FirstName;
            if (dto.LastName != null) employee.LastName = dto.LastName;
            if (dto.FathersName != null) employee.FathersName = dto.FathersName;
            if (dto.DateOfBirth.HasValue) employee.DateOfBirth = dto.DateOfBirth;
            if (dto.Disability != null) employee.Disability = dto.Disability;
            if (dto.Identification != null) employee.Identification = dto.Identification;
            if (dto.BloodGroup != null) employee.BloodGroup = dto.BloodGroup;
            if (dto.Height.HasValue) employee.Height = dto.Height;
            if (dto.Weight.HasValue) employee.Weight = dto.Weight;
            if (dto.TypeOfEmployment != null) employee.TypeOfEmployment = dto.TypeOfEmployment;
            if (dto.SalaryDebitAcc != null) employee.SalaryDebitAcc = dto.SalaryDebitAcc;
            if (dto.SalaryCreditAcc != null) employee.SalaryCreditAcc = dto.SalaryCreditAcc;
            if (dto.ImageUrl != null) employee.ImageUrl = dto.ImageUrl;
            if (dto.SalesMan != null) employee.SalesMan = dto.SalesMan;

            // Salary Details
            if (dto.Salary.HasValue) employee.Salary = dto.Salary;
            if (dto.BasicSalary.HasValue) employee.BasicSalary = dto.BasicSalary;
            if (dto.Hra.HasValue) employee.Hra = dto.Hra;
            if (dto.Conveyance.HasValue) employee.Conveyance = dto.Conveyance;
            if (dto.CityCompn.HasValue) employee.CityCompn = dto.CityCompn;
            if (dto.EsiApp.HasValue) employee.EsiApp = dto.EsiApp;
            if (dto.EsiNum != null) employee.EsiNum = dto.EsiNum;
            if (dto.EsiDt.HasValue) employee.EsiDt = dto.EsiDt;
            if (dto.EsiPer.HasValue) employee.EsiPer = dto.EsiPer;
            if (dto.PfApp.HasValue) employee.PfApp = dto.PfApp;
            if (dto.PfNum != null) employee.PfNum = dto.PfNum;
            if (dto.PfDt.HasValue) employee.PfDt = dto.PfDt;
            if (dto.PfPer.HasValue) employee.PfPer = dto.PfPer;
            if (dto.Tds.HasValue) employee.Tds = dto.Tds;
            if (dto.EffFrom.HasValue) employee.EffFrom = dto.EffFrom;
            if (dto.EffTo.HasValue) employee.EffTo = dto.EffTo;
            if (dto.Active.HasValue) employee.Active = dto.Active.Value;

            // Assignment Details
            if (dto.DateOfJoining.HasValue) employee.DateOfJoining = dto.DateOfJoining;
            if (dto.DepartmentId.HasValue) employee.DepartmentId = dto.DepartmentId;
            if (dto.Designation.HasValue) employee.Designation = dto.Designation;
            if (dto.LastWorkingDate.HasValue) employee.LastWorkingDate = dto.LastWorkingDate;
            if (dto.RejoineeDate.HasValue) employee.RejoineeDate = dto.RejoineeDate;
            if (dto.EmployeeGrade != null) employee.EmployeeGrade = dto.EmployeeGrade;
            if (dto.ReportManager != null) employee.ReportManager = dto.ReportManager;
            if (dto.ReportManagerCode != null) employee.ReportManagerCode = dto.ReportManagerCode;
            if (dto.ReportingHeadMail != null) employee.ReportingHeadMail = dto.ReportingHeadMail;
            if (dto.NoticePeriod.HasValue) employee.NoticePeriod = dto.NoticePeriod;
            if (dto.CostCenter != null) employee.CostCenter = dto.CostCenter;
            if (dto.IdCardNo != null) employee.IdCardNo = dto.IdCardNo;
            if (dto.Country != null) employee.Country = dto.Country;
            if (dto.City != null) employee.City = dto.City;

            // Personal Details
            if (dto.BirthPlace != null) employee.BirthPlace = dto.BirthPlace;
            if (dto.Religion != null) employee.Religion = dto.Religion;
            if (dto.HomeState != null) employee.HomeState = dto.HomeState;
            if (dto.Nationality != null) employee.Nationality = dto.Nationality;
            if (dto.CountryBirth != null) employee.CountryBirth = dto.CountryBirth;
            if (dto.IsExService.HasValue) employee.IsExService = dto.IsExService;
            if (dto.Nominee != null) employee.Nominee = dto.Nominee;
            if (dto.NomineeRelationship != null) employee.NomineeRelationship = dto.NomineeRelationship;
            if (dto.RecruiterName != null) employee.RecruiterName = dto.RecruiterName;
            if (dto.Reference != null) employee.Reference = dto.Reference;
            if (dto.LanguageKnown != null) employee.LanguageKnown = dto.LanguageKnown;

            // Additional Information
            if (dto.PassportNo != null) employee.PassportNo = dto.PassportNo;
            if (dto.NameAsPerPassport != null) employee.NameAsPerPassport = dto.NameAsPerPassport;
            if (dto.PassportExpiryDate.HasValue) employee.PassportExpiryDate = dto.PassportExpiryDate;
            if (dto.PassportIssuePlace != null) employee.PassportIssuePlace = dto.PassportIssuePlace;
            if (dto.PassportIssueDate.HasValue) employee.PassportIssueDate = dto.PassportIssueDate;
            if (dto.MothersMaidenName != null) employee.MothersMaidenName = dto.MothersMaidenName;
            if (dto.OldPassportNo != null) employee.OldPassportNo = dto.OldPassportNo;
            if (dto.InsuranceName != null) employee.InsuranceName = dto.InsuranceName;
            if (dto.InsuranceNo != null) employee.InsuranceNo = dto.InsuranceNo;
            if (dto.BankName != null) employee.BankName = dto.BankName;
            if (dto.BranchName != null) employee.BranchName = dto.BranchName;
            if (dto.BankAcNo != null) employee.BankAcNo = dto.BankAcNo;
            if (dto.IfscCode != null) employee.IfscCode = dto.IfscCode;
            if (dto.PanNo != null) employee.PanNo = dto.PanNo;
            if (dto.EsiNo != null) employee.EsiNo = dto.EsiNo;
            if (dto.EsiEffDate.HasValue) employee.EsiEffDate = dto.EsiEffDate;
            if (dto.PfNo != null) employee.PfNo = dto.PfNo;
            if (dto.PfEffDate.HasValue) employee.PfEffDate = dto.PfEffDate;
            if (dto.VoterId != null) employee.VoterId = dto.VoterId;
            if (dto.DrivingLicenseNo != null) employee.DrivingLicenseNo = dto.DrivingLicenseNo;
            if (dto.AadharNo != null) employee.AadharNo = dto.AadharNo;

            // Contact Details: Permanent
            if (dto.PermAddress != null) employee.PermAddress = dto.PermAddress;
            if (dto.PermCity != null) employee.PermCity = dto.PermCity;
            if (dto.PermState != null) employee.PermState = dto.PermState;
            if (dto.PermTelephone != null) employee.PermTelephone = dto.PermTelephone;
            if (dto.PermEmail != null) employee.PermEmail = dto.PermEmail;
            if (dto.PermContactPerson != null) employee.PermContactPerson = dto.PermContactPerson;
            if (dto.PermPincode != null) employee.PermPincode = dto.PermPincode;
            if (dto.PermCountry != null) employee.PermCountry = dto.PermCountry;
            if (dto.PermMobile != null) employee.PermMobile = dto.PermMobile;

            // Communication Contact
            if (dto.CommAddress != null) employee.CommAddress = dto.CommAddress;
            if (dto.CommCity != null) employee.CommCity = dto.CommCity;
            if (dto.CommState != null) employee.CommState = dto.CommState;
            if (dto.CommTelephone != null) employee.CommTelephone = dto.CommTelephone;
            if (dto.CommEmail != null) employee.CommEmail = dto.CommEmail;
            if (dto.CommContactPerson != null) employee.CommContactPerson = dto.CommContactPerson;
            if (dto.CommPincode != null) employee.CommPincode = dto.CommPincode;
            if (dto.CommCountry != null) employee.CommCountry = dto.CommCountry;
            if (dto.CommMobile != null) employee.CommMobile = dto.CommMobile;

            // Family Details
            if (dto.FamilyName != null) employee.FamilyName = dto.FamilyName;
            if (dto.FamilyAge.HasValue) employee.FamilyAge = dto.FamilyAge;
            if (dto.FamilyRelationship != null) employee.FamilyRelationship = dto.FamilyRelationship;
            if (dto.FamilyOccupation != null) employee.FamilyOccupation = dto.FamilyOccupation;
            if (dto.FamilyPrimaryContact != null) employee.FamilyPrimaryContact = dto.FamilyPrimaryContact;
            if (dto.FamilyContact != null) employee.FamilyContact = dto.FamilyContact;
            if (dto.FamilyEmail != null) employee.FamilyEmail = dto.FamilyEmail;

            // Education Skills
            if (dto.EduCourse != null) employee.EduCourse = dto.EduCourse;
            if (dto.EduBoard != null) employee.EduBoard = dto.EduBoard;
            if (dto.EduInstitution != null) employee.EduInstitution = dto.EduInstitution;
            if (dto.EduPassDate != null) employee.EduPassDate = dto.EduPassDate;
            if (dto.EduPercentage.HasValue) employee.EduPercentage = dto.EduPercentage;

            // Special achievements
            if (dto.AchievementWhat != null) employee.AchievementWhat = dto.AchievementWhat;
            if (dto.AchievementWhen.HasValue) employee.AchievementWhen = dto.AchievementWhen;
            if (dto.AchievementWhere != null) employee.AchievementWhere = dto.AchievementWhere;
            if (dto.AchievementRemarks != null) employee.AchievementRemarks = dto.AchievementRemarks;

            // Previous Employment
            if (dto.PrevCompanyName != null) employee.PrevCompanyName = dto.PrevCompanyName;
            if (dto.PrevLastDesignation != null) employee.PrevLastDesignation = dto.PrevLastDesignation;
            if (dto.PrevRelevantExpYear.HasValue) employee.PrevRelevantExpYear = dto.PrevRelevantExpYear;
            if (dto.PrevRelevantExpMonth.HasValue) employee.PrevRelevantExpMonth = dto.PrevRelevantExpMonth;
            if (dto.PrevPpfNo != null) employee.PrevPpfNo = dto.PrevPpfNo;
            if (dto.PrevPesiNo != null) employee.PrevPesiNo = dto.PrevPesiNo;
            if (dto.PrevStartDate.HasValue) employee.PrevStartDate = dto.PrevStartDate;
            if (dto.PrevEndDate.HasValue) employee.PrevEndDate = dto.PrevEndDate;

            employee.UserUpdated = dto.UserUpdated;
            employee.DateUpdated = DateTime.UtcNow;

            // Normalize all DateTime properties to UTC (Npgsql requires Kind=Utc for timestamptz)
            foreach (var prop in typeof(SalesEmployee).GetProperties())
            {
                if (prop.PropertyType == typeof(DateTime?) && prop.CanRead && prop.CanWrite)
                {
                    var val = (DateTime?)prop.GetValue(employee);
                    if (val.HasValue && val.Value.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(employee, DateTime.SpecifyKind(val.Value, DateTimeKind.Utc));
                    }
                }
                else if (prop.PropertyType == typeof(DateTime) && prop.CanRead && prop.CanWrite)
                {
                    var val = (DateTime)prop.GetValue(employee);
                    if (val.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(employee, DateTime.SpecifyKind(val, DateTimeKind.Utc));
                    }
                }
            }

            _context.SalesEmployees.Update(employee);
            await _context.SaveChangesAsync();

            // Handle allowance details if provided
            if (dto.EmployeeAllowances != null)
            {
                var existingAllowances = await _context.EmployeeAllowances
                    .Where(a => a.EmployeeId == employee.Id && a.Active == true)
                    .ToListAsync();

                // Mark old allowances as inactive
                foreach (var existingAllowance in existingAllowances)
                {
                    existingAllowance.Active = false;
                    existingAllowance.UserUpdated = dto.UserUpdated;
                    existingAllowance.DateUpdated = DateTime.UtcNow;
                    _context.EmployeeAllowances.Update(existingAllowance);
                }

                // Add new allowances
                foreach (var allowanceDto in dto.EmployeeAllowances)
                {
                    var allowance = new EmployeeAllowance
                    {
                        EmployeeId = employee.Id.Value,
                        AllowanceType = allowanceDto.AllowanceType,
                        AllowanceAmount = allowanceDto.AllowanceAmount,
                        AllowEffFrom = allowanceDto.AllowEffFrom,
                        AllowEffTo = allowanceDto.AllowEffTo,
                        UserCreated = dto.UserUpdated,
                        DateCreated = DateTime.UtcNow,
                        UserUpdated = dto.UserUpdated,
                        DateUpdated = DateTime.UtcNow,
                        Active = true
                    };
                    _context.EmployeeAllowances.Add(allowance);
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Employee updated successfully.", id = employee.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating employee", error = ex.Message, innerError = ex.InnerException?.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Delete an employee (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userUpdated)
        {
            var employee = await _context.SalesEmployees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found." });

            employee.Active = false;
            employee.UserUpdated = userUpdated;
            employee.DateUpdated = DateTime.UtcNow;

            _context.SalesEmployees.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee deleted successfully." });
        }

        /// <summary>
        /// Search employees by name
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SalesEmployee>>> Search([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(new { message = "Search name is required." });

            var employees = await _context.SalesEmployees
                .Where(e => (e.FirstName.ToLower().Contains(name.ToLower()) || 
                           e.LastName.ToLower().Contains(name.ToLower())) && 
                           e.Active == true)
                .ToListAsync();

            return Ok(employees);
        }

        /// <summary>
        /// Get employees by department
        /// </summary>
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<SalesEmployee>>> GetByDepartment(int departmentId)
        {
            var employees = await _context.SalesEmployees
                .Where(e => e.DepartmentId == departmentId && e.Active == true)
                .ToListAsync();

            return Ok(employees);
        }
    }
}