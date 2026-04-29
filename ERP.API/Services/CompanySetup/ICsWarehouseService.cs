using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsWarehouseService
    {
        Task<CsWarehouseCreateResponseDto> CreateWarehouseAsync(CreateCsWarehouseDto createDto);
        Task<(bool Success, string Message)> UpdateWarehouseAsync(UpdateCsWarehouseDto updateDto);
        Task<(bool Success, string Message)> DeleteWarehouseAsync(int warehouseId);
        Task<CsWarehouseDto?> GetWarehouseByIdAsync(int warehouseId);
        Task<IEnumerable<CsWarehouseDto>> GetWarehousesByCompanyAsync(int companyId);
        Task<IEnumerable<CsWarehouseDto>> GetWarehousesByBranchAsync(int branchId);
        Task<IEnumerable<CsWarehouseDropdownDto>> GetWarehousesDropdownByCompanyAsync(int companyId);
        Task<IEnumerable<CsWarehouseDropdownDto>> GetWarehousesDropdownByBranchAsync(int branchId);
        Task<IEnumerable<CsWarehouseDto>> GetAllWarehousesAsync();
    }
}
