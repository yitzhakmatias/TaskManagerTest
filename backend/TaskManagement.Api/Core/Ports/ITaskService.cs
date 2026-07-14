using TaskManagement.Api.Core.Application.Dtos;

namespace TaskManagement.Api.Core.Ports;

/// <summary>
/// Primary (driving) port. This is what the HTTP adapter (or any other driving
/// adapter - a CLI, a message consumer, a test) calls into. It speaks only in
/// DTOs, never in EF Core or ASP.NET Core types.
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default);

    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDto?> ToggleTaskAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
