using TaskManagement.Api.Core.Application.Dtos;
using TaskManagement.Api.Core.Application.Mapping;
using TaskManagement.Api.Core.Domain;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Core.Application;

/// <summary>
/// Application service holding the use-case logic for tasks. Depends only on
/// the IUnitOfWork port (never on EF Core directly), which is what makes this
/// class trivially unit-testable with a mocked unit of work / repository.
/// </summary>
public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(cancellationToken);
        return tasks.Select(t => t.ToDto()).ToList();
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem { Title = request.Title.Trim() };

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }

    public async Task<TaskDto?> ToggleTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.IsCompleted = !task.IsCompleted;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }

    public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        _unitOfWork.Tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
