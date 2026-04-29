using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;

namespace ERP.API.UserManagement.Controllers
{
    /// <summary>
    /// API Controller for User Management operations
    /// </summary>
    [ApiController]
    [Route("api/UmUser")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<UserController> _logger;
        private readonly string _connectionString;
        public UserController(
            IUserService userService,
            IPasswordService passwordService,
            ITwoFactorService twoFactorService,
            ILogger<UserController> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
            _passwordService = passwordService;
            _twoFactorService = twoFactorService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentException("Default connection string not found");
        }
        private readonly IConfiguration _configuration;


        #region Authentication Endpoints

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">User registration information</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if username is available
                bool isUsernameAvailable = await _userService.IsUsernameAvailableAsync(registerDto.Username);
                if (!isUsernameAvailable)
                {
                    ModelState.AddModelError("Username", "Username is already taken");
                    return BadRequest(ModelState);
                }

                // Check if email is available
                bool isEmailAvailable = await _userService.IsEmailAvailableAsync(registerDto.Email);
                if (!isEmailAvailable)
                {
                    ModelState.AddModelError("Email", "Email is already registered");
                    return BadRequest(ModelState);
                }

                var result = await _userService.RegisterUserAsync(registerDto);

                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Username}", registerDto.Username);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Username}", registerDto.Username);
                return StatusCode(500, new { message = "Internal server error during registration" });
            }
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication result with token</returns>
        //[HttpPost("login")]
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        // public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
        // {
        //     try
        //     {
        //         if (!ModelState.IsValid)
        //         {
        //             return BadRequest(ModelState);
        //         }

        //         // Get client IP address
        //         loginDto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        //         loginDto.UserAgent = Request.Headers["User-Agent"].ToString();

        //         var result = await _userService.LoginAsync(loginDto);

        //         if (result.Success)
        //         {
        //             var claims = new List<Claim>
        //              {
        //                  // new Claim(JwtRegisteredClaimNames.Sub, result.Username),
        //                  // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //                  // new Claim(JwtRegisteredClaimNames.Email, request.Email ?? string.Empty),
        //                  // new Claim(ClaimTypes.Name, request.Username),
        //                  // new Claim(ClaimTypes.NameIdentifier, request.UserId.ToString()),
        //                  // new Claim(ClaimTypes.Role, request.Role)
        //         // new Claim(ClaimTypes.NameIdentifier, "Rangaraj".ToString()),


        //                     new Claim("UserId", result.UserId.ToString()),
        //                     new Claim("EmailOrUsername", loginDto.EmailOrUsername ?? string.Empty),
        //                     new Claim("Role", result.RoleDto.RoleId.ToString()),

        //     };

        //             // var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //             var jwtKey = _configuration["Jwt:Key"];
        //             if (string.IsNullOrEmpty(jwtKey))
        //                 return StatusCode(500, new { message = "JWT Key is not configured. Please set Jwt:Key in appsettings.json." });

        //             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        //             var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //             var token = new JwtSecurityToken(
        //                 issuer: _configuration["Jwt:Issuer"],
        //                 audience: _configuration["Jwt:Audience"],
        //                 claims: claims,
        //                 expires: DateTime.UtcNow.AddHours(2),
        //                 signingCredentials: creds
        //             );

        //             var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        //             _logger.LogInformation("User logged in successfully: {UserId}", result.UserId);
        //             result.Token = tokenString;
        //             return Ok(result);
        //             // return Ok(new { token = tokenString });
        //         }
        //         else
        //         {
        //             _logger.LogWarning("Failed login attempt for: {EmailOrUsername}", loginDto.EmailOrUsername);
        //             return Unauthorized(new { message = result.Message });
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error during login: {EmailOrUsername}", loginDto.EmailOrUsername);
        //         return StatusCode(500, new { message = "Internal server error during login" });
        //     }
        // }

        //        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        // Get client IP address and user agent
        //        loginDto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        //        loginDto.UserAgent = Request.Headers["User-Agent"].ToString();

        //        var result = await _userService.LoginAsync(loginDto);
        //        if (result.Success)
        //                {
        //                    var claims = new List<Claim>
        //            {
        //                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
        //                new Claim(ClaimTypes.Name, loginDto.EmailOrUsername ?? string.Empty),
        //                new Claim(ClaimTypes.Role, result.RoleDto?.RoleName ?? "Role"),
        //            };

        //                    var jwtKey = _configuration["Jwt:Key"];
        //                    if (string.IsNullOrEmpty(jwtKey))
        //                        return StatusCode(500, new { message = "JWT Key is not configured. Please set Jwt:Key in appsettings.json." });

        //                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        //                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //                    var token = new JwtSecurityToken(
        //                        issuer: _configuration["Jwt:Issuer"],
        //                        audience: _configuration["Jwt:Audience"],
        //                        claims: claims,
        //                        expires: DateTime.UtcNow.AddHours(2),
        //                        signingCredentials: creds
        //                    );

        //                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        //                    _logger.LogInformation("User logged in successfully: {UserId}", result.UserId);
        //                    result.Token = tokenString;

        //                    return Ok(result);
        //                }
        //                else
        //                {
        //                    _logger.LogWarning("Failed login attempt for: {EmailOrUsername}", loginDto.EmailOrUsername);
        //                    return Unauthorized(new { message = result.Message ?? "Invalid credentials" });
        //                }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error during login: {EmailOrUsername}", loginDto.EmailOrUsername);
        //        return StatusCode(500, new { message = "Internal server error during login" });
        //    }
        //}
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get client IP address and user agent
                loginDto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                loginDto.UserAgent = Request.Headers["User-Agent"].ToString();

                var result = await _userService.LoginAsync(loginDto);

                if (result.Success)
                {
                    // Generate tokens
                    var tokenService = HttpContext.RequestServices.GetRequiredService<ITokenService>();

                    // Generate access token
                    var accessToken = tokenService.GenerateAccessToken(
                        result.UserId,
                        result.UserProfile?.Username ?? loginDto.EmailOrUsername,
                        result.RoleDto?.RoleName ?? "User");

                    // Generate refresh token
                    var refreshToken = tokenService.GenerateRefreshToken(
                        result.UserId,
                        loginDto.IpAddress ?? "Unknown",
                        loginDto.UserAgent ?? "Unknown");

                    // Save refresh token to database
                    await tokenService.SaveRefreshTokenAsync(refreshToken);

                    _logger.LogInformation("User logged in successfully: {UserId}", result.UserId);

                    // Return tokens in response
                    return Ok(new AuthResultDto
                    {
                        Success = true,
                        Message = "Login successful",
                        UserId = result.UserId,
                        Token = accessToken,
                        RefreshToken = refreshToken.Token,
                        TokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15)),
                        RefreshTokenExpiry = refreshToken.ExpiryDate,
                        UserProfile = result.UserProfile,
                        RoleDto = result.RoleDto
                    });
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for: {EmailOrUsername}", loginDto.EmailOrUsername);
                    return Unauthorized(new { message = result.Message ?? "Invalid credentials" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {EmailOrUsername}", loginDto.EmailOrUsername);
                return StatusCode(500, new { message = "Internal server error during login", error = ex.Message });
            }
        }
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { message = "Access token and refresh token are required" });
                }

                var tokenService = HttpContext.RequestServices.GetRequiredService<ITokenService>();

                // Get principal from expired access token
                var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Validate refresh token
                var refreshToken = await tokenService.GetRefreshTokenAsync(request.RefreshToken);
                if (refreshToken == null || refreshToken.UserId != userId || refreshToken.IsRevoked || refreshToken.ExpiryDate <= DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                // Get user information
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { message = "User not found or inactive" });
                }

                // Revoke old refresh token (optional: implement refresh token rotation)
                await tokenService.RevokeRefreshToken(request.RefreshToken, "Refreshed");

                // Generate new tokens
                var newAccessToken = tokenService.GenerateAccessToken(userId, user.Username, "User");

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var newRefreshToken = tokenService.GenerateRefreshToken(userId, ipAddress ?? "Unknown", userAgent ?? "Unknown");
                await tokenService.SaveRefreshTokenAsync(newRefreshToken);

                _logger.LogInformation("Token refreshed for user: {UserId}", userId);

                return Ok(new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
                    RefreshTokenExpiry = newRefreshToken.ExpiryDate,
                    TokenType = "Bearer"
                });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid token during refresh");
                return Unauthorized(new { message = "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Revoke refresh token (logout from all devices)
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RevokeToken([FromBody] string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required" });
                }

                var tokenService = HttpContext.RequestServices.GetRequiredService<ITokenService>();

                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Verify the refresh token belongs to this user
                var storedToken = await tokenService.GetRefreshTokenAsync(refreshToken);
                if (storedToken == null || storedToken.UserId != userId)
                {
                    return BadRequest(new { message = "Invalid refresh token" });
                }

                // Revoke the token
                var result = await tokenService.RevokeRefreshToken(refreshToken, "User revoked");

                if (result)
                {
                    _logger.LogInformation("Refresh token revoked for user: {UserId}", userId);
                    return Ok(new { message = "Token revoked successfully" });
                }

                return BadRequest(new { message = "Failed to revoke token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Revoke all refresh tokens for current user (logout from all devices)
        /// </summary>
        [HttpPost("revoke-all-tokens")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RevokeAllTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
            UPDATE refresh_tokens 
            SET is_revoked = true, 
                revoked_reason = 'User revoked all tokens', 
                revoked_date = @revokedDate
            WHERE user_id = @userId 
            AND is_revoked = false";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("revokedDate", DateTime.UtcNow);

                var result = await command.ExecuteNonQueryAsync();

                _logger.LogInformation("All tokens revoked for user: {UserId}", userId);
                return Ok(new { message = $"All {result} tokens revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        ///// <summary>
        ///// User logout
        ///// </summary>
        ///// <param name="sessionId">Session ID to terminate</param>
        ///// <returns>Logout result</returns>
        //[HttpPost("logout")]
        //// [Authorize]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult> Logout([FromBody] string sessionId)
        //{
        //    try
        //    {
        //        var result = await _userService.LogoutAsync(sessionId);

        //        if (result)
        //        {
        //            return Ok(new { message = "Logged out successfully" });
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Logout failed" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error during logout");
        //        return StatusCode(500, new { message = "Internal server error during logout" });
        //    }
        //}
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Logout([FromBody] string? refreshToken = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // If refresh token is provided, revoke it
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var tokenService = HttpContext.RequestServices.GetRequiredService<ITokenService>();
                    await tokenService.RevokeRefreshToken(refreshToken, "User logout");
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Internal server error during logout" });
            }
        }
        #endregion

        #region User Profile Management

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User profile</returns>
        [HttpGet("{userId:int}")]
        // [Authorize]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetUser(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>Current user profile</returns>
        [HttpGet("profile")]
        // [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="updateDto">Updated user information</param>
        /// <returns>Update result</returns>
        [HttpPut("profile")]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Use UserId from request body; only fall back to token if not provided
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("UserId")?.Value;

                if (updateDto.UserId == 0 && !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int tokenUserId))
                {
                    updateDto.UserId = tokenUserId;
                }

                // Check username/email availability only if being changed from current value
                if (!string.IsNullOrEmpty(updateDto.Username))
                {
                    bool isUsernameAvailable = await _userService.IsUsernameAvailableAsync(updateDto.Username, updateDto.UserId);
                    if (!isUsernameAvailable)
                    {
                        ModelState.AddModelError("Username", "Username is already taken");
                        return BadRequest(ModelState);
                    }
                }

                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    bool isEmailAvailable = await _userService.IsEmailAvailableAsync(updateDto.Email, updateDto.UserId);
                    if (!isEmailAvailable)
                    {
                        ModelState.AddModelError("Email", "Email is already registered");
                        return BadRequest(ModelState);
                    }
                }

                var result = await _userService.UpdateUserAsync(updateDto);

                if (result)
                {
                    return Ok(new { status = true, message = "Profile updated successfully" });
                }
                else
                {
                    return BadRequest(new { status = false, message = "Failed to update profile" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users with pagination and search
        /// </summary>
        /// <param name="searchDto">Search and pagination parameters</param>
        /// <returns>Paginated user list</returns>
        [HttpGet]
        // [Authorize(Roles = "Admin,UserManager")]
        [ProducesResponseType(typeof(UserSearchResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserSearchResultDto>> GetAllUsers([FromQuery] UserSearchDto searchDto)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="changePasswordDto">Password change information</param>
        /// <returns>Change result</returns>
        [HttpPost("change-password")]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                changePasswordDto.UserId = userId;

                var result = await _userService.ChangePasswordAsync(changePasswordDto);

                if (result)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to change password" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        /// <param name="requestDto">Password reset request</param>
        /// <returns>Request result</returns>
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.RequestPasswordResetAsync(requestDto);

                // Always return success for security reasons (don't reveal if email exists)
                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        /// <param name="resetDto">Password reset information</param>
        /// <returns>Reset result</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate reset token first
                bool isTokenValid = await _userService.ValidateResetTokenAsync(resetDto.ResetToken);
                if (!isTokenValid)
                {
                    return BadRequest(new { message = "Invalid or expired reset token" });
                }

                var result = await _userService.ResetPasswordAsync(resetDto);

                if (result)
                {
                    return Ok(new { message = "Password reset successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to reset password" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Password validation result</returns>
        [HttpPost("validate-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PasswordValidationResultDto), StatusCodes.Status200OK)]
        public ActionResult<PasswordValidationResultDto> ValidatePassword([FromBody] string password)
        {
            try
            {
                bool isValid = _passwordService.ValidatePasswordStrength(password);
                string description = _passwordService.GetPasswordStrengthDescription(password);

                var result = new PasswordValidationResultDto
                {
                    IsValid = isValid,
                    StrengthLevel = isValid ? "Strong" : "Weak",
                    Errors = isValid ? new List<string>() : new List<string> { description }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Two-Factor Authentication

        /// <summary>
        /// Enable two-factor authentication
        /// </summary>
        /// <returns>2FA setup information</returns>
        [HttpPost("enable-2fa")]
        // [Authorize]
        [ProducesResponseType(typeof(TwoFactorSetupDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TwoFactorSetupDto>> EnableTwoFactor()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                string secretKey = await _userService.EnableTwoFactorAsync(userId);

                if (string.IsNullOrEmpty(secretKey))
                {
                    return BadRequest(new { message = "Failed to enable 2FA" });
                }

                var qrCodeUri = _twoFactorService.GenerateQrCodeUri(secretKey, user.Email);
                var backupCodes = _twoFactorService.GenerateBackupCodes();

                var setupDto = new TwoFactorSetupDto
                {
                    SecretKey = secretKey,
                    QrCodeUri = qrCodeUri,
                    BackupCodes = backupCodes,
                    ManualEntryKey = secretKey.Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " ") // Format for manual entry
                };

                return Ok(setupDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Disable two-factor authentication
        /// </summary>
        /// <returns>Disable result</returns>
        [HttpPost("disable-2fa")]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DisableTwoFactor()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _userService.DisableTwoFactorAsync(userId);

                if (result)
                {
                    return Ok(new { message = "Two-factor authentication disabled successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to disable 2FA" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Verify two-factor authentication code
        /// </summary>
        /// <param name="verificationDto">2FA verification information</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-2fa")]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> VerifyTwoFactor([FromBody] TwoFactorVerificationDto verificationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                verificationDto.UserId = userId;

                var result = await _userService.VerifyTwoFactorCodeAsync(verificationDto.UserId, verificationDto.Code);

                if (result)
                {
                    return Ok(new { message = "Two-factor code verified successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Invalid two-factor code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA code");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region User Management (Admin)

        /// <summary>
        /// Update user status (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isActive">Active status</param>
        /// <param name="isLocked">Locked status</param>
        /// <returns>Update result</returns>
        [HttpPut("{userId:int}/status")]
        // [Authorize(Roles = "Admin,UserManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserStatus(int userId, [FromQuery] bool isActive, [FromQuery] bool? isLocked = null)
        {
            try
            {
                var result = await _userService.UpdateUserStatusAsync(userId, isActive, isLocked);

                if (result)
                {
                    return Ok(new { message = "User status updated successfully" });
                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="hardDelete">Whether to permanently delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{userId:int}")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(int userId, [FromQuery] bool hardDelete = false)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId, hardDelete);

                if (result)
                {
                    string deleteType = hardDelete ? "permanently deleted" : "deactivated";
                    return Ok(new { message = $"User {deleteType} successfully" });
                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user statistics (Admin only)
        /// </summary>
        /// <returns>User statistics</returns>
        [HttpGet("statistics")]
        // [Authorize(Roles = "Admin,UserManager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetUserStatistics()
        {
            try
            {
                var statistics = await _userService.GetUserStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User profile</returns>
        [HttpGet("username/{username}")]
        // [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email to search for</param>
        /// <returns>User profile</returns>
        [HttpGet("email/{email}")]
        // [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Hard delete a user (permanent deletion)
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        /// <returns>Success result</returns>
        [HttpDelete("{userId:int}/hard")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> HardDeleteUser(int userId)
        {
            try
            {
                bool success = await _userService.DeleteUserAsync(userId, hardDelete: true);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User permanently deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lock user account
        /// </summary>
        /// <param name="userId">User ID to lock</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/lock")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> LockUserAccount(int userId)
        {
            try
            {
                bool success = await _userService.LockUserAccountAsync(userId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User account locked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user account: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/unlock")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UnlockUserAccount(int userId)
        {
            try
            {
                bool success = await _userService.UnlockUserAccountAsync(userId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User account unlocked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user account: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Activate user account
        /// </summary>
        /// <param name="userId">User ID to activate</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/activate")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ActivateUser(int userId)
        {
            try
            {
                bool success = await _userService.ActivateUserAsync(userId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        /// <param name="userId">User ID to deactivate</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/deactivate")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateUser(int userId)
        {
            try
            {
                bool success = await _userService.DeactivateUserAsync(userId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Reset failed login attempts for a user
        /// </summary>
        /// <param name="userId">User ID to reset failed attempts</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/reset-failed-attempts")]
        // [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ResetFailedLoginAttempts(int userId)
        {
            try
            {
                bool success = await _userService.ResetFailedLoginAttemptsAsync(userId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "Failed login attempts reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed login attempts: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Verify user password
        /// </summary>
        /// <param name="verifyDto">Password verification data</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-password")]
        // [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> VerifyPassword([FromBody] VerifyPasswordDto verifyDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user session" });
                }

                // For security, only allow users to verify their own password unless admin
                if (verifyDto.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Use the password service to verify the password
                bool isValid = await _passwordService.VerifyPasswordAsync(verifyDto.UserId, verifyDto.Password);

                return Ok(new { isValid, message = isValid ? "Password is correct" : "Password is incorrect" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user: {UserId}", verifyDto.UserId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Login Management Endpoints

        /// <summary>
        /// Update login information for a user (for system use)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="loginInfoDto">Login information</param>
        /// <returns>Success result</returns>
        [HttpPut("{userId:int}/login-info")]
        // [Authorize(Roles = "Admin,System")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateLoginInfo(int userId, [FromBody] UpdateLoginInfoDto loginInfoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                bool success = await _userService.UpdateLoginInfoAsync(
                    userId,
                    loginInfoDto.Success,
                    loginInfoDto.IpAddress,
                    loginInfoDto.DeviceInfo,
                    loginInfoDto.UserAgent,
                    loginInfoDto.Location,
                    loginInfoDto.SessionId);
                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "Login information updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login info for user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion
    }
}
