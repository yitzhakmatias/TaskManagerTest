using TaskManagement.Api.Core.Domain;

namespace TaskManagement.Api.Core.Ports;

/// <summary>
/// Secondary (driven) port for task persistence. Deliberately does NOT commit
/// any changes itself - Add/Remove only stage changes against the current unit
/// of work; IUnitOfWork.SaveChangesAsync is what commits them. This lets a
/// caller stage several repository operations and persist them atomically.
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    void Remove(TaskItem task);
}
