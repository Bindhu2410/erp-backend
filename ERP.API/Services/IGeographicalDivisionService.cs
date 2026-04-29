using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface IGeographicalDivisionService
    {
        Task<GeographicalDivision> GetByIdAsync(long divisionId);
        Task<IEnumerable<GeographicalDivision>> GetAllAsync();
        Task<long> CreateAsync(GeographicalDivision division);
        Task<bool> UpdateAsync(GeographicalDivision division);
        Task<bool> DeleteAsync(long divisionId);
        Task<IEnumerable<GeographicalHierarchyDto>> GetHierarchyByPincodeAsync(string pincode);
    }
}
