using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IDemoChecklistService
    {
        Task<IEnumerable<DemoChecklist>> GetChecklistsByItemIdAsync(int itemId);
        // Add other method signatures as needed
    }
}
