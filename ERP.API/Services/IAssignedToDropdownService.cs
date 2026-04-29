using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface IAssignedToDropdownService
    {
        Task<IEnumerable<AssignedToDropdownDto>> GetAssignedToDropdownAsync(int userId);
    }
}
