using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Adapters.Persistence;

/// <summary>
/// Coordinates a single SaveChanges commit across the repositories reached
/// through this unit of work. Does not own/dispose the DbContext - it is
/// injected and its lifetime is managed by DI (AddDbContext is scoped).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TaskDbContext _context;
    private ITaskRepository? _tasks;

    public UnitOfWork(TaskDbContext context)
    {
        _context = context;
    }

    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
