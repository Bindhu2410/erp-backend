using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services
{
    public class UserService : IUserService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<UserService> _logger;

        public UserService(IDbConnection connection, ILogger<UserService> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDropdownDto>> GetPresenterDropdownAsync()
        {
            const string sql = @"SELECT userid AS Id, username FROM users ORDER BY username";
            return await _connection.QueryAsync<UserDropdownDto>(sql);
        }

        public async Task<IEnumerable<UserDropdownDto>> GetSalesRepresentativeDropdownAsync()
        {
            const string sql = @"
                SELECT u.userid AS Id, u.username 
                FROM users u
                JOIN userroles ur ON u.userid = ur.userid
                JOIN roles r ON ur.roleid = r.roleid
                WHERE r.rolename = 'Sales Representative'
                AND u.isactive = true
                ORDER BY u.username";
            return await _connection.QueryAsync<UserDropdownDto>(sql);
        }
    }
}
