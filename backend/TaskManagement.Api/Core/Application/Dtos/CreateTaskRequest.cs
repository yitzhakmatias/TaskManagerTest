namespace TaskManagement.Api.Core.Application.Dtos;

/// <summary>Payload accepted by POST /api/tasks.</summary>
public record CreateTaskRequest(string Title);
