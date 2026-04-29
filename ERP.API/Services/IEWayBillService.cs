using System.Threading.Tasks;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface IEWayBillService
    {
        Task<EWayBillResponseDto> GenerateEWayBillAsync(int issueId);
        Task<EWayBillResponseDto> GetEWayBillAsync(string ewayBillNo);
        Task<EWayBillResponseDto> CancelEWayBillAsync(EWayBillCancelRequestDto request);
        Task<EWayBillResponseDto> UpdateVehicleAsync(EWayBillUpdateVehicleRequestDto request);
        Task<string> GetAccessTokenAsync();
    }
}
