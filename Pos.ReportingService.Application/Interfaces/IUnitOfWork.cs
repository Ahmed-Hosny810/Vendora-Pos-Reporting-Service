using Pos.SalesService.Application.Wrappers;

namespace Pos.SalesService.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
