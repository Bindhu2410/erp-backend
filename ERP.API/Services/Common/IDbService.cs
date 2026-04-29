using System.Threading.Tasks;

namespace ERP.API.Services.Common
{
    public interface IDbService
    {
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null);
        Task<T[]> QueryAsync<T>(string sql, object parameters = null);
        Task<int> ExecuteAsync(string sql, object parameters = null);
    }
}
