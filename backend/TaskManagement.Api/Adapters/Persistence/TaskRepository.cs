using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Core.Domain;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Adapters.Persistence;

/// <summary>
/// EF Core / SQLite implementation of the ITaskRepository port. Add/Remove only
/// stage changes on the DbContext's change tracker - nothing is written to the
/// database until IUnitOfWork.SaveChangesAsync is called.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Intentionally tracked (no AsNoTracking): callers use this to fetch an
        // entity they are about to mutate and commit via IUnitOfWork.
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    public void Remove(TaskItem task)
    {
        _context.Tasks.Remove(task);
    }
}
