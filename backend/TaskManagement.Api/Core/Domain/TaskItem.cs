namespace TaskManagement.Api.Core.Domain;

/// <summary>
/// Core domain entity representing a single task. Deliberately framework-agnostic
/// (no EF Core / JSON attributes) so persistence and transport concerns stay
/// out of the domain model - this is what sits at the center of the hexagon.
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
