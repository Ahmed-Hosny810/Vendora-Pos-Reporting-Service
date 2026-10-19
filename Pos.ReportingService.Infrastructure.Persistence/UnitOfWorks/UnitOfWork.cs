using Microsoft.EntityFrameworkCore;
using Pos.SalesService.Application.Interfaces;
using Pos.ReportingService.Infrastructure.Persistence.Contexts;
using Pos.ReportingService.Application.Exceptions;

namespace Pos.SalesService.Infrastructure.Persistence.UnitofWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context; 
    }

    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("The record changed while saving.", exception);
        }
    }
}
