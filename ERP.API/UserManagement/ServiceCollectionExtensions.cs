using ERP.API.UserManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement
{
    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds User Management services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserManagement(this IServiceCollection services)
        {
            // Get connection string from service provider
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string is not configured");
            
            // Register Password Service with connection string
            services.AddScoped<IPasswordService>(sp => new PasswordService(
                connectionString, 
                sp.GetRequiredService<ILogger<PasswordService>>()));
            
            // Register Two-Factor Authentication Service
            services.AddScoped<ITwoFactorService, TwoFactorService>();
            
            // Register User Service with IConfiguration (different pattern)
            services.AddScoped<IUserService, UserService>();
            
            // Register Role Service with IConfiguration (different pattern)
            services.AddScoped<IRoleService, RoleService>();
            
            // Register Permission Service with connection string
            services.AddScoped<IPermissionService>(sp => new PermissionService(
                connectionString, 
                sp.GetRequiredService<ILogger<PermissionService>>()));
            
            // Register RolePermission Service with connection string
            services.AddScoped<IRolePermissionService>(sp => new RolePermissionService(
                connectionString, 
                sp.GetRequiredService<ILogger<RolePermissionService>>()));
            
            // Register UserRole Service with connection string
            services.AddScoped<IUserRoleService>(sp => new UserRoleService(
                connectionString, 
                sp.GetRequiredService<ILogger<UserRoleService>>()));
            
            // Register OrganizationalUnit Service with connection string
            services.AddScoped<IOrganizationalUnitService>(sp => new OrganizationalUnitService(
                connectionString, 
                sp.GetRequiredService<ILogger<OrganizationalUnitService>>()));
            
            // Register UserOrganizationalUnit Service
            services.AddScoped<IUserOrganizationalUnitService, UserOrganizationalUnitService>();
            
            // Register UserSession Service with connection string
            services.AddScoped<IUserSessionService>(sp => new UserSessionService(
                connectionString, 
                sp.GetRequiredService<ILogger<UserSessionService>>()));
            
            // Register UserAuditLog Service with connection string
            services.AddScoped<IUserAuditLogService>(sp => new UserAuditLogService(
                connectionString, 
                sp.GetRequiredService<ILogger<UserAuditLogService>>()));
            
            // Register Access Delegation Service
            services.AddScoped<IAccessDelegationService, AccessDelegationService>();
            
            // Register Delegation Permission Service
            services.AddScoped<IDelegationPermissionService, DelegationPermissionService>();
            
            // Register User Preference Service
            services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            
            return services;
        }

        /// <summary>
        /// Adds User Management services with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserManagement(
            this IServiceCollection services, 
            Action<UserManagementOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services.AddUserManagement();
        }
    }

    /// <summary>
    /// Configuration options for User Management
    /// </summary>
    public class UserManagementOptions
    {
        /// <summary>
        /// Password policy settings
        /// </summary>
        public PasswordPolicyOptions PasswordPolicy { get; set; } = new();

        /// <summary>
        /// Security settings
        /// </summary>
        public SecurityOptions Security { get; set; } = new();

        /// <summary>
        /// Two-factor authentication settings
        /// </summary>
        public TwoFactorOptions TwoFactor { get; set; } = new();
    }

    /// <summary>
    /// Password policy configuration
    /// </summary>
    public class PasswordPolicyOptions
    {
        /// <summary>
        /// Minimum password length (default: 12)
        /// </summary>
        public int MinimumLength { get; set; } = 12;

        /// <summary>
        /// Require uppercase letter (default: true)
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Require lowercase letter (default: true)
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Require digit (default: true)
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// Require special character (default: true)
        /// </summary>
        public bool RequireSpecialCharacter { get; set; } = true;

        /// <summary>
        /// Password history count to prevent reuse (default: 5)
        /// </summary>
        public int PasswordHistoryCount { get; set; } = 5;

        /// <summary>
        /// Password expiry in days (0 = no expiry, default: 90)
        /// </summary>
        public int PasswordExpiryDays { get; set; } = 90;
    }

    /// <summary>
    /// Security configuration options
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// Maximum failed login attempts before lockout (default: 5)
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Account lockout duration in minutes (default: 15)
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Session timeout in minutes (default: 30)
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Password reset token expiry in hours (default: 24)
        /// </summary>
        public int PasswordResetTokenExpiryHours { get; set; } = 24;

        /// <summary>
        /// Enable IP address tracking (default: true)
        /// </summary>
        public bool EnableIpTracking { get; set; } = true;

        /// <summary>
        /// Enable device fingerprinting (default: true)
        /// </summary>
        public bool EnableDeviceTracking { get; set; } = true;
    }

    /// <summary>
    /// Two-factor authentication options
    /// </summary>
    public class TwoFactorOptions
    {
        /// <summary>
        /// Enable two-factor authentication (default: true)
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// TOTP issuer name (default: "ERP System")
        /// </summary>
        public string IssuerName { get; set; } = "ERP System";

        /// <summary>
        /// TOTP token validity period in seconds (default: 30)
        /// </summary>
        public int TokenValidityPeriod { get; set; } = 30;

        /// <summary>
        /// Number of time steps to allow for clock drift (default: 1)
        /// </summary>
        public int ClockDriftTolerance { get; set; } = 1;
    }
}
