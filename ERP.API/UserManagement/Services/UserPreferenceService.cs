using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using Newtonsoft.Json;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// User Preference service implementation using PostgreSQL stored procedures
    /// </summary>
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserPreferenceService> _logger;

        public UserPreferenceService(
            IConfiguration configuration,
            ILogger<UserPreferenceService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("Default connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// Creates or updates a user preference (UPSERT)
        /// </summary>
        public async Task<UserPreferenceResultDto> CreateUserPreferenceAsync(CreateUserPreferenceDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_create_um_user_preference(@p_user_id, @p_preference_key, @p_preference_value)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", createDto.UserId);
                command.Parameters.AddWithValue("p_preference_key", createDto.PreferenceKey);
                command.Parameters.AddWithValue("p_preference_value", createDto.PreferenceValue ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserPreferenceResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user preference for UserId: {UserId}, Key: {Key}", 
                    createDto.UserId, createDto.PreferenceKey);
                
                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = $"Error creating user preference: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets a specific user preference by key
        /// </summary>
        public async Task<UserPreferenceDto?> GetUserPreferenceAsync(int userId, string preferenceKey)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_user_preference(@p_user_id, @p_preference_key)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);
                command.Parameters.AddWithValue("p_preference_key", preferenceKey);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserPreferenceDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        PreferenceKey = reader.GetString("preference_key"),
                        PreferenceValue = reader.IsDBNull("preference_value") ? null : reader.GetString("preference_value"),
                        DateModified = reader.GetDateTime("date_modified")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preference for UserId: {UserId}, Key: {Key}", userId, preferenceKey);
                throw;
            }
        }

        /// <summary>
        /// Gets all preferences for a user
        /// </summary>
        public async Task<List<UserPreferenceDto>> GetUserPreferencesAsync(int userId)
        {
            try
            {
                var preferences = new List<UserPreferenceDto>();

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_user_preferences(@p_user_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    preferences.Add(new UserPreferenceDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        PreferenceKey = reader.GetString("preference_key"),
                        PreferenceValue = reader.IsDBNull("preference_value") ? null : reader.GetString("preference_value"),
                        DateModified = reader.GetDateTime("date_modified")
                    });
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences for UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets all user preferences with pagination
        /// </summary>
        public async Task<PagedUserPreferencesResponseDto> GetAllUserPreferencesAsync(PagedUserPreferencesRequestDto request)
        {
            try
            {
                var items = new List<DetailedUserPreferenceDto>();
                long totalCount = 0;

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_all_user_preferences(@p_page_number, @p_page_size, @p_user_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_page_number", request.PageNumber);
                command.Parameters.AddWithValue("p_page_size", request.PageSize);
                command.Parameters.AddWithValue("p_user_id", request.UserId ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (totalCount == 0)
                        totalCount = reader.GetInt64("total_count");

                    items.Add(new DetailedUserPreferenceDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        PreferenceKey = reader.GetString("preference_key"),
                        PreferenceValue = reader.IsDBNull("preference_value") ? null : reader.GetString("preference_value"),
                        DateModified = reader.GetDateTime("date_modified")
                    });
                }

                return new PagedUserPreferencesResponseDto
                {
                    Items = items,
                    TotalCount = (int)totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all user preferences");
                throw;
            }
        }

        /// <summary>
        /// Checks if a user preference exists
        /// </summary>
        public async Task<bool> CheckUserPreferenceExistsAsync(int userId, string preferenceKey)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT sp_check_um_user_preference_exists(@p_user_id, @p_preference_key)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);
                command.Parameters.AddWithValue("p_preference_key", preferenceKey);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                
                return result != null && (bool)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user preference existence for UserId: {UserId}, Key: {Key}", 
                    userId, preferenceKey);
                return false;
            }
        }

        /// <summary>
        /// Updates a user preference
        /// </summary>
        public async Task<UserPreferenceResultDto> UpdateUserPreferenceAsync(UpdateUserPreferenceDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_update_um_user_preference(@p_user_id, @p_preference_key, @p_preference_value)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", updateDto.UserId);
                command.Parameters.AddWithValue("p_preference_key", updateDto.PreferenceKey);
                command.Parameters.AddWithValue("p_preference_value", updateDto.PreferenceValue ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserPreferenceResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preference for UserId: {UserId}, Key: {Key}", 
                    updateDto.UserId, updateDto.PreferenceKey);
                
                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = $"Error updating user preference: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Bulk updates user preferences
        /// </summary>
        public async Task<BulkUserPreferenceResultDto> BulkUpdateUserPreferencesAsync(BulkUpdateUserPreferencesDto bulkUpdateDto)
        {
            try
            {
                // Convert preferences to JSON format expected by stored procedure
                var preferencesJson = JsonConvert.SerializeObject(
                    bulkUpdateDto.Preferences.Select(p => new { key = p.Key, value = p.Value })
                );

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_update_um_user_preferences_bulk(@p_user_id, @p_preferences)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", bulkUpdateDto.UserId);
                command.Parameters.Add(new NpgsqlParameter("p_preferences", NpgsqlTypes.NpgsqlDbType.Jsonb) 
                { 
                    Value = preferencesJson 
                });

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new BulkUserPreferenceResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message"),
                        UpdatedCount = reader.GetInt32("updated_count")
                    };
                }

                return new BulkUserPreferenceResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure",
                    UpdatedCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating user preferences for UserId: {UserId}", bulkUpdateDto.UserId);
                
                return new BulkUserPreferenceResultDto
                {
                    Success = false,
                    Message = $"Error bulk updating user preferences: {ex.Message}",
                    UpdatedCount = 0
                };
            }
        }

        /// <summary>
        /// Deletes a specific user preference
        /// </summary>
        public async Task<UserPreferenceResultDto> DeleteUserPreferenceAsync(int userId, string preferenceKey)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_delete_um_user_preference(@p_user_id, @p_preference_key)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);
                command.Parameters.AddWithValue("p_preference_key", preferenceKey);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserPreferenceResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user preference for UserId: {UserId}, Key: {Key}", 
                    userId, preferenceKey);
                
                return new UserPreferenceResultDto
                {
                    Success = false,
                    Message = $"Error deleting user preference: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes all preferences for a user
        /// </summary>
        public async Task<DeleteAllUserPreferencesResultDto> DeleteAllUserPreferencesAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_delete_um_all_user_preferences(@p_user_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DeleteAllUserPreferencesResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message"),
                        DeletedCount = reader.GetInt32("deleted_count")
                    };
                }

                return new DeleteAllUserPreferencesResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure",
                    DeletedCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all user preferences for UserId: {UserId}", userId);
                
                return new DeleteAllUserPreferencesResultDto
                {
                    Success = false,
                    Message = $"Error deleting all user preferences: {ex.Message}",
                    DeletedCount = 0
                };
            }
        }

        /// <summary>
        /// Gets user preferences by key pattern
        /// </summary>
        public async Task<List<UserPreferenceDto>> GetUserPreferencesByPatternAsync(int userId, string keyPattern)
        {
            try
            {
                var preferences = new List<UserPreferenceDto>();

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_user_preferences_by_pattern(@p_user_id, @p_key_pattern)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);
                command.Parameters.AddWithValue("p_key_pattern", keyPattern);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    preferences.Add(new UserPreferenceDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        PreferenceKey = reader.GetString("preference_key"),
                        PreferenceValue = reader.IsDBNull("preference_value") ? null : reader.GetString("preference_value"),
                        DateModified = reader.GetDateTime("date_modified")
                    });
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences by pattern for UserId: {UserId}, Pattern: {Pattern}", 
                    userId, keyPattern);
                throw;
            }
        }

        /// <summary>
        /// Gets user preferences summary
        /// </summary>
        public async Task<UserPreferencesSummaryDto?> GetUserPreferencesSummaryAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_user_preferences_summary(@p_user_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_user_id", userId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserPreferencesSummaryDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        TotalPreferences = reader.GetInt64("total_preferences"),
                        LastModified = reader.IsDBNull("last_modified") ? null : reader.GetDateTime("last_modified")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences summary for UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets users with a specific preference
        /// </summary>
        public async Task<List<UserWithPreferenceDto>> GetUsersWithPreferenceAsync(string preferenceKey, string? preferenceValue = null)
        {
            try
            {
                var users = new List<UserWithPreferenceDto>();

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_users_with_preference(@p_preference_key, @p_preference_value)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_preference_key", preferenceKey);
                command.Parameters.AddWithValue("p_preference_value", preferenceValue ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    users.Add(new UserWithPreferenceDto
                    {
                        UserId = reader.GetInt32("user_id"),
                        Username = reader.GetString("username"),
                        PreferenceValue = reader.IsDBNull("preference_value") ? null : reader.GetString("preference_value"),
                        DateModified = reader.GetDateTime("date_modified")
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with preference Key: {Key}, Value: {Value}", 
                    preferenceKey, preferenceValue);
                throw;
            }
        }
    }
}
