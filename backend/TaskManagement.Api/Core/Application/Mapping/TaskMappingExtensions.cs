using TaskManagement.Api.Core.Application.Dtos;
using TaskManagement.Api.Core.Domain;

namespace TaskManagement.Api.Core.Application.Mapping;

public static class TaskMappingExtensions
{
    public static TaskDto ToDto(this TaskItem task) =>
        new(task.Id, task.Title, task.IsCompleted, task.CreatedAt);
}
