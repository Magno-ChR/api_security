using api_security.domain.Abstractions;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Histories
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task UpdateAsync(History history);
    }
}