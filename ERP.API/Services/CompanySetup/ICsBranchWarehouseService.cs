using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsBranchWarehouseService
    {
        Task<int> CreateBranchWarehouseAsync(CsBranchWarehouseDto createDto);
        Task<bool> UpdateBranchWarehouseAsync(CsBranchWarehouseDto updateDto);
        Task<bool> DeleteBranchWarehouseAsync(int warehouseId);
        Task<CsBranchWarehouseDto?> GetBranchWarehouseByIdAsync(int warehouseId);
        Task<IEnumerable<CsBranchWarehouseDto>> GetBranchWarehousesByBranchAsync(int branchId);
        Task<IEnumerable<CsBranchWarehouseDto>> GetBranchWarehousesDropdownAsync(int branchId);
    }
}
