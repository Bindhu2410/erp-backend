using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using System.Text.Json;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// User service implementation using PostgreSQL stored procedures
    /// </summary>
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly IPasswordService _passwordService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IConfiguration configuration,
            IPasswordService passwordService,
            ITwoFactorService twoFactorService,
            ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Default connection string not found");
            _passwordService = passwordService;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user using stored procedure
        /// </summary>
        public async Task<AuthResultDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
            try
            {
                // Validate password strength
                if (!_passwordService.ValidatePasswordStrength(registerDto.Password))
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = _passwordService.GetPasswordStrengthDescription(registerDto.Password)
                    };
                }

                // Generate salt and hash password
                string salt = _passwordService.GenerateSalt();
                string hashedPassword = _passwordService.HashPassword(registerDto.Password, salt);

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_create_user_with_hash(@p_username, @p_email, @p_firstname, @p_lastname, @p_passwordhash, @p_passwordsalt, @p_phonenumber, @p_profileimageurl, @p_preferredlanguage, @p_timezone, @p_twofactorenabled, @p_twofactorkey, @p_notes)", connection);

                command.Parameters.AddWithValue("p_username", registerDto.Username);
                command.Parameters.AddWithValue("p_email", registerDto.Email);
                command.Parameters.AddWithValue("p_firstname", registerDto.FirstName);
                command.Parameters.AddWithValue("p_lastname", registerDto.LastName);
                command.Parameters.AddWithValue("p_passwordhash", hashedPassword);
                command.Parameters.AddWithValue("p_passwordsalt", salt);
                command.Parameters.AddWithValue("p_phonenumber", (object?)registerDto.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("p_profileimageurl", (object?)registerDto.ProfileImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("p_preferredlanguage", registerDto.PreferredLanguage);
                command.Parameters.AddWithValue("p_timezone", registerDto.TimeZone);
                command.Parameters.AddWithValue("p_twofactorenabled", registerDto.TwoFactorEnabled);
                command.Parameters.AddWithValue("p_twofactorkey", DBNull.Value);
                command.Parameters.AddWithValue("p_notes", (object?)registerDto.Notes ?? DBNull.Value);

                await using var reader = await command.ExecuteReaderAsync();



                if (await reader.ReadAsync())
                {
                    var userId = reader.GetInt32("userid");
                    var success = reader.GetBoolean("success");
                    var message = reader.GetString("message");

                    if (success)
                    {
                        return new AuthResultDto
                        {
                            Success = true,
                            Message = "User registered successfully",
                            UserId = userId
                        };
                    }
                    else
                    {
                        return new AuthResultDto
                        {
                            Success = false,
                            Message = message
                        };
                    }
                }

                return new AuthResultDto { Success = false, Message = "Registration failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", registerDto.Username);
                return new AuthResultDto { Success = false, Message = "Registration failed due to server error" };
            }
        }

        /// <summary>
        /// Authenticates user login using stored procedure
        /// </summary>
        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // First get user by username/email
                await using var getUserCommand = new NpgsqlCommand("SELECT * FROM sp_um_get_user_by_username(@p_username)", connection);
                getUserCommand.Parameters.AddWithValue("p_username", loginDto.EmailOrUsername);

                await using var reader = await getUserCommand.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    // Try by email if username didn't work
                    await reader.CloseAsync();

                    await using var getByEmailCommand = new NpgsqlCommand("SELECT * FROM sp_um_get_user_by_email(@p_email)", connection);
                    getByEmailCommand.Parameters.AddWithValue("p_email", loginDto.EmailOrUsername);

                    await using var emailReader = await getByEmailCommand.ExecuteReaderAsync();

                    if (!await emailReader.ReadAsync())
                    {
                        return new AuthResultDto { Success = false, Message = "Invalid credentials" };
                    }

                    return await ProcessLoginAttempt(emailReader, loginDto, connection);
                }

                return await ProcessLoginAttempt(reader, loginDto, connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for: {EmailOrUsername}", loginDto.EmailOrUsername);
                return new AuthResultDto { Success = false, Message = "Login failed due to server error" };
            }
        }

        private async Task<AuthResultDto> ProcessLoginAttempt(NpgsqlDataReader reader, LoginDto loginDto, NpgsqlConnection connection)
        {
            var userId = reader.GetInt32("userid");
            var username = reader.GetString("username");
            var email = reader.GetString("email");
            var firstName = reader.GetString("firstname");
            var lastName = reader.GetString("lastname");
            var storedHash = reader.GetString("passwordhash");
            var storedSalt = reader.GetString("passwordsalt");
            var isActive = reader.GetBoolean("isactive");
            var isLocked = reader.GetBoolean("islocked");
            var twoFactorEnabled = reader.GetBoolean("twofactorenabled");
            // var roleId = reader.GetInt32("roleid"); // Make sure your SP returns this
            int? roleId = reader.IsDBNull(reader.GetOrdinal("roleid")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("roleid"));
            if (roleId == null)
            {
                _logger.LogWarning("User {UserId} has no role assigned.", userId);
                await reader.CloseAsync();
                return new AuthResultDto { Success = false, Message = "User has no role assigned. Please contact administrator." };
            }


            await reader.CloseAsync();

            // Check account status
            if (!isActive)
                return new AuthResultDto { Success = false, Message = "Account is inactive" };

            if (isLocked)
                return new AuthResultDto { Success = false, Message = "Account is locked" };

            // Verify password
            bool isPasswordValid = _passwordService.VerifyPassword(loginDto.Password, storedHash, storedSalt);

            // Update login information
            await UpdateLoginInfoAsync(
                userId,
                isPasswordValid,
                loginDto.IpAddress,
                loginDto.DeviceInfo,
                loginDto.UserAgent,
                null,  // Location not available in LoginDto 
                null); // SessionId not used here

            if (!isPasswordValid)
                return new AuthResultDto { Success = false, Message = "Invalid credentials" };

            // Check two-factor authentication
            twoFactorEnabled = false;
            if (twoFactorEnabled && string.IsNullOrEmpty(loginDto.TwoFactorCode))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Two-factor authentication required",
                    RequiresTwoFactor = true,
                    UserId = userId
                };
            }

            if (twoFactorEnabled && !string.IsNullOrEmpty(loginDto.TwoFactorCode))
            {
                bool is2FAValid = await VerifyTwoFactorCodeAsync(userId, loginDto.TwoFactorCode);
                if (!is2FAValid)
                    return new AuthResultDto { Success = false, Message = "Invalid two-factor authentication code" };
            }

            // Fetch full role details using roleId
            RoleDto roleDto = null;
            await using (var roleCmd = new NpgsqlCommand(@"
    SELECT r.roleid, r.rolename, r.description, r.issystemrole, r.datecreated, 
           r.createdby, u.username AS createdbyusername, r.isactive
    FROM roles r
    LEFT JOIN users u ON r.createdby = u.userid
    WHERE r.roleid = @roleid", connection))
            {
                roleCmd.Parameters.AddWithValue("roleid", roleId);
                await using var roleReader = await roleCmd.ExecuteReaderAsync();
                if (await roleReader.ReadAsync())
                {
                    roleDto = new RoleDto
                    {
                        RoleId = roleReader.GetInt32(roleReader.GetOrdinal("roleid")),
                        RoleName = roleReader.GetString(roleReader.GetOrdinal("rolename")),
                        Description = roleReader.IsDBNull(roleReader.GetOrdinal("description")) ? null : roleReader.GetString(roleReader.GetOrdinal("description")),
                        IsSystemRole = roleReader.GetBoolean(roleReader.GetOrdinal("issystemrole")),
                        DateCreated = roleReader.GetDateTime(roleReader.GetOrdinal("datecreated")),
                        CreatedBy = roleReader.IsDBNull(roleReader.GetOrdinal("createdby")) ? (int?)null : roleReader.GetInt32(roleReader.GetOrdinal("createdby")),
                        CreatedByUsername = roleReader.IsDBNull(roleReader.GetOrdinal("createdbyusername")) ? null : roleReader.GetString(roleReader.GetOrdinal("createdbyusername")),
                        IsActive = roleReader.GetBoolean(roleReader.GetOrdinal("isactive"))
                    };
                }
                await roleReader.CloseAsync();
            }

            // Create user profile for response
            var userProfile = new UserProfileDto
            {
                UserId = userId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = isActive,
                TwoFactorEnabled = twoFactorEnabled
            };

            return new AuthResultDto
            {
                Success = true,
                Message = "Login successful",
                UserId = userId,
                UserProfile = userProfile,
                RoleDto = roleDto
                // Note: JWT token generation would be handled by a separate token service
            };
        }

        /// <summary>
        /// Gets user by ID
        /// </summary>
        public async Task<UserProfileDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_user_by_id(@p_userid)", connection);
                command.Parameters.AddWithValue("p_userid", userId);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapReaderToUserProfile(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Updates user login information
        /// </summary>
        public async Task<bool> UpdateLoginInfoAsync(int userId, bool success, string? ipAddress = null, string? deviceInfo = null, string? userAgent = null, string? location = null, string? sessionId = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(
                    "SELECT * FROM sp_um_update_user_login_extended(@p_userid, @p_success, @p_ip_address, @p_device_info, @p_user_agent, @p_location, @p_session_id)",
                    connection);

                command.Parameters.AddWithValue("p_userid", userId);
                command.Parameters.AddWithValue("p_success", success);
                command.Parameters.AddWithValue("p_ip_address", ipAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_device_info", deviceInfo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_user_agent", userAgent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_location", location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_session_id", sessionId ?? (object)DBNull.Value);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.GetBoolean("success");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login info for user: {UserId}", userId);
                return false;
            }
        }

        // Helper method to map database reader to UserProfileDto
        private UserProfileDto MapReaderToUserProfile(NpgsqlDataReader reader)
        {
            return new UserProfileDto
            {
                UserId = reader.GetInt32("userid"),
                Username = reader.GetString("username"),
                Email = reader.GetString("email"),
                FirstName = reader.GetString("firstname"),
                LastName = reader.GetString("lastname"),
                PhoneNumber = reader.IsDBNull("phonenumber") ? null : reader.GetString("phonenumber"),
                ProfileImageUrl = reader.IsDBNull("profileimageurl") ? null : reader.GetString("profileimageurl"),
                DateCreated = reader.GetDateTime("datecreated"),
                LastLoginDate = reader.IsDBNull("lastlogindate") ? null : reader.GetDateTime("lastlogindate"),
                IsActive = reader.GetBoolean("isactive"),
                IsLocked = reader.GetBoolean("islocked"),
                FailedLoginAttempts = reader.GetInt32("failedloginattempts"),
                PreferredLanguage = reader.GetString("preferredlanguage"),
                TimeZone = reader.GetString("timezone"),
                TwoFactorEnabled = reader.GetBoolean("twofactorenabled"),
                LastPasswordChangeDate = reader.IsDBNull("lastpasswordchangedate") ? null : reader.GetDateTime("lastpasswordchangedate"),
                RequirePasswordChange = reader.GetBoolean("requirepasswordchange"),
                Notes = reader.IsDBNull("notes") ? null : reader.GetString("notes")
            };
        }

        /// <summary>
        /// Enables two-factor authentication for a user
        /// </summary>
        public async Task<string> EnableTwoFactorAsync(int userId)
        {
            try
            {
                // Generate secret key
                string secretKey = _twoFactorService.GenerateSecretKey();

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_update_user(@p_userid, @p_username, @p_email, @p_firstname, @p_lastname, @p_phonenumber, @p_profileimageurl, @p_preferredlanguage, @p_timezone, @p_twofactorenabled, @p_twofactorkey, @p_notes)", connection);

                command.Parameters.AddWithValue("p_userid", userId);
                command.Parameters.AddWithValue("p_username", DBNull.Value);
                command.Parameters.AddWithValue("p_email", DBNull.Value);
                command.Parameters.AddWithValue("p_firstname", DBNull.Value);
                command.Parameters.AddWithValue("p_lastname", DBNull.Value);
                command.Parameters.AddWithValue("p_phonenumber", DBNull.Value);
                command.Parameters.AddWithValue("p_profileimageurl", DBNull.Value);
                command.Parameters.AddWithValue("p_preferredlanguage", DBNull.Value);
                command.Parameters.AddWithValue("p_timezone", DBNull.Value);
                command.Parameters.AddWithValue("p_twofactorenabled", true);
                command.Parameters.AddWithValue("p_twofactorkey", secretKey);
                command.Parameters.AddWithValue("p_notes", DBNull.Value);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var success = reader.GetBoolean("success");
                    if (success)
                    {
                        return secretKey;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for user: {UserId}", userId);
                return string.Empty;
            }
        }

        /// <summary>
        /// Disables two-factor authentication for a user
        /// </summary>
        public async Task<bool> DisableTwoFactorAsync(int userId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_update_user(@p_userid, @p_username, @p_email, @p_firstname, @p_lastname, @p_phonenumber, @p_profileimageurl, @p_preferredlanguage, @p_timezone, @p_twofactorenabled, @p_twofactorkey, @p_notes)", connection);

                command.Parameters.AddWithValue("p_userid", userId);
                command.Parameters.AddWithValue("p_username", DBNull.Value);
                command.Parameters.AddWithValue("p_email", DBNull.Value);
                command.Parameters.AddWithValue("p_firstname", DBNull.Value);
                command.Parameters.AddWithValue("p_lastname", DBNull.Value);
                command.Parameters.AddWithValue("p_phonenumber", DBNull.Value);
                command.Parameters.AddWithValue("p_profileimageurl", DBNull.Value);
                command.Parameters.AddWithValue("p_preferredlanguage", DBNull.Value);
                command.Parameters.AddWithValue("p_timezone", DBNull.Value);
                command.Parameters.AddWithValue("p_twofactorenabled", false);
                command.Parameters.AddWithValue("p_twofactorkey", DBNull.Value);
                command.Parameters.AddWithValue("p_notes", DBNull.Value);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader.GetBoolean("success");
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Verifies two-factor authentication code
        /// </summary>
        public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
        {
            try
            {
                // Get user's 2FA key
                var user = await GetUserByIdAsync(userId);
                if (user == null || !user.TwoFactorEnabled)
                    return false;

                // Get the stored 2FA key from database
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_user_by_id(@p_userid)", connection);
                command.Parameters.AddWithValue("p_userid", userId);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var twoFactorKey = reader.IsDBNull("twofactorkey") ? null : reader.GetString("twofactorkey");

                    if (string.IsNullOrEmpty(twoFactorKey))
                        return false;

                    return _twoFactorService.VerifyTotpCode(twoFactorKey, code);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA code for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets user statistics
        /// </summary>
        public async Task<object> GetUserStatisticsAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_user_statistics()", connection);
                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new
                    {
                        TotalUsers = reader.GetInt64("total_users"),
                        ActiveUsers = reader.GetInt64("active_users"),
                        InactiveUsers = reader.GetInt64("inactive_users"),
                        LockedUsers = reader.GetInt64("locked_users"),
                        UsersWithTwoFactor = reader.GetInt64("users_with_2fa"),
                        UsersRequiringPasswordChange = reader.GetInt64("users_requiring_password_change")
                    };
                }

                return new { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return new { };
            }
        }

        /// <summary>
        /// Checks if username is available
        /// </summary>
        public async Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(
                    "SELECT userid FROM public.users WHERE username = @username", connection);
                command.Parameters.AddWithValue("username", username);

                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return true; // username not taken

                int foundId = Convert.ToInt32(result);
                // Available if the found user IS the one being updated
                return excludeUserId.HasValue && excludeUserId.Value == foundId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability: {Username}", username);
                return false;
            }
        }

        /// <summary>
        /// Checks if email is available
        /// </summary>
        public async Task<bool> IsEmailAvailableAsync(string email, int? excludeUserId = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(
                    "SELECT userid FROM public.users WHERE email = @email", connection);
                command.Parameters.AddWithValue("email", email);

                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return true; // email not taken

                int foundId = Convert.ToInt32(result);
                // Available if the found user IS the one being updated
                return excludeUserId.HasValue && excludeUserId.Value == foundId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);
                return false;
            }
        }

        // Placeholder implementations for other interface methods
        // These would be implemented similarly using the stored procedures

        public Task<bool> LogoutAsync(string sessionId) => throw new NotImplementedException();
        public Task<UserProfileDto?> GetUserByUsernameAsync(string username) => throw new NotImplementedException();
        public Task<UserProfileDto?> GetUserByEmailAsync(string email) => throw new NotImplementedException();
        public Task<UserSearchResultDto> GetAllUsersAsync(UserSearchDto searchDto) => throw new NotImplementedException();
        
        /// <summary>
        /// Gets users based on hierarchy - Admin sees all, Sales Manager sees their team
        /// </summary>
        public async Task<UserSearchResultDto> GetUsersByHierarchyAsync(int currentUserId, UserSearchDto searchDto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand("SELECT * FROM get_users_by_hierarchy(@p_user_id)", connection);
                command.Parameters.AddWithValue("p_user_id", currentUserId);

                await using var reader = await command.ExecuteReaderAsync();
                var users = new List<UserProfileDto>();

                while (await reader.ReadAsync())
                {
                    users.Add(new UserProfileDto
                    {
                        UserId = reader.GetInt32("userid"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        FirstName = reader.GetString("firstname"),
                        LastName = reader.GetString("lastname"),
                        PhoneNumber = reader.IsDBNull("phonenumber") ? null : reader.GetString("phonenumber"),
                        IsActive = reader.GetBoolean("isactive")
                    });
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    users = users.Where(u => 
                        u.Username.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.FirstName.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply pagination
                var totalCount = users.Count;
                var pagedUsers = users
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToList();

                return new UserSearchResultDto
                {
                    Users = pagedUsers,
                    TotalCount = totalCount,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by hierarchy for user: {UserId}", currentUserId);
                return new UserSearchResultDto { Users = new List<UserProfileDto>() };
            }
        }
        public async Task<bool> UpdateUserAsync(UpdateUserDto updateDto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Direct SQL update for profile fields
                await using (var command = new NpgsqlCommand(@"
                    UPDATE public.users SET
                        username            = COALESCE(@p_username, username),
                        email               = COALESCE(@p_email, email),
                        firstname           = COALESCE(@p_firstname, firstname),
                        lastname            = COALESCE(@p_lastname, lastname),
                        phonenumber         = COALESCE(@p_phonenumber, phonenumber),
                        profileimageurl     = COALESCE(@p_profileimageurl, profileimageurl),
                        preferredlanguage   = COALESCE(@p_preferredlanguage, preferredlanguage),
                        timezone            = COALESCE(@p_timezone, timezone),
                        twofactorenabled    = COALESCE(@p_twofactorenabled, twofactorenabled),
                        notes               = COALESCE(@p_notes, notes)
                    WHERE userid = @p_userid", connection))
                {
                    command.Parameters.AddWithValue("p_userid", updateDto.UserId);
                    command.Parameters.AddWithValue("p_username",          (object?)updateDto.Username          ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_email",             (object?)updateDto.Email             ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_firstname",         (object?)updateDto.FirstName         ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_lastname",          (object?)updateDto.LastName          ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_phonenumber",       (object?)updateDto.PhoneNumber       ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_profileimageurl",   (object?)updateDto.ProfileImageUrl   ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_preferredlanguage", (object?)updateDto.PreferredLanguage ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_timezone",          (object?)updateDto.TimeZone          ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_twofactorenabled",  (object?)updateDto.TwoFactorEnabled  ?? DBNull.Value);
                    command.Parameters.AddWithValue("p_notes",             (object?)updateDto.Notes             ?? DBNull.Value);

                    var rows = await command.ExecuteNonQueryAsync();
                    if (rows == 0) return false;
                }

                // If password is provided, update it
                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    string newSalt = _passwordService.GenerateSalt();
                    string newHash = _passwordService.HashPassword(updateDto.Password, newSalt);

                    await using (var pwdCmd = new NpgsqlCommand(@"
                        UPDATE public.users SET
                            passwordhash            = @p_passwordhash,
                            passwordsalt            = @p_passwordsalt,
                            lastpasswordchangedate  = CURRENT_TIMESTAMP,
                            requirepasswordchange   = false
                        WHERE userid = @p_userid", connection))
                    {
                        pwdCmd.Parameters.AddWithValue("p_userid",       updateDto.UserId);
                        pwdCmd.Parameters.AddWithValue("p_passwordhash", newHash);
                        pwdCmd.Parameters.AddWithValue("p_passwordsalt", newSalt);

                        var pwdRows = await pwdCmd.ExecuteNonQueryAsync();
                        if (pwdRows == 0) return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile (with password support)");
                return false;
            }
        }
        public Task<bool> UpdateUserStatusAsync(int userId, bool isActive, bool? isLocked = null) => throw new NotImplementedException();
        public Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto) => throw new NotImplementedException();
        public Task<bool> RequestPasswordResetAsync(PasswordResetRequestDto requestDto) => throw new NotImplementedException();
        public Task<bool> ResetPasswordAsync(PasswordResetDto resetDto) => throw new NotImplementedException();
        public Task<bool> ValidateResetTokenAsync(string resetToken) => throw new NotImplementedException();
        public Task<bool> LockUserAccountAsync(int userId) => throw new NotImplementedException();
        public Task<bool> UnlockUserAccountAsync(int userId) => throw new NotImplementedException();
        public Task<bool> DeactivateUserAsync(int userId) => throw new NotImplementedException();
        public Task<bool> ActivateUserAsync(int userId) => throw new NotImplementedException();
        public Task<bool> DeleteUserAsync(int userId, bool hardDelete = false) => throw new NotImplementedException();
        public Task<bool> ResetFailedLoginAttemptsAsync(int userId) => throw new NotImplementedException();
    }
}
