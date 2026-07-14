namespace TaskManagement.Api.Core.Application.Dtos;

/// <summary>Read-model returned to API clients.</summary>
public record TaskDto(Guid Id, string Title, bool IsCompleted, DateTime CreatedAt);
