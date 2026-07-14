namespace TaskManagement.Api.Core.Ports;

/// <summary>
/// Secondary (driven) port that coordinates a single atomic commit across
/// whatever repositories a use case touches. Only one aggregate (Tasks) exists
/// today, but the seam is here so a second repository could join the same
/// transaction without any change to TaskService's shape.
/// </summary>
public interface IUnitOfWork
{
    ITaskRepository Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
