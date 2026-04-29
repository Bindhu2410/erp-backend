using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDropdownDto>> GetPresenterDropdownAsync();
        Task<IEnumerable<UserDropdownDto>> GetSalesRepresentativeDropdownAsync();
    }

    public class UserDropdownDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
