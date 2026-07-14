using FluentValidation;
using TaskManagement.Api.Core.Application.Dtos;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Adapters.Http;

/// <summary>
/// Primary (driving) adapter: thin, function-style HTTP handlers for the Tasks
/// resource (Minimal API). Each handler validates the transport-layer request
/// shape and delegates everything else to ITaskService (the driving port) - no
/// business logic and no EF Core lives here.
/// </summary>
public static class TaskFunctions
{
    public static RouteGroupBuilder MapTaskFunctions(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllTasks)
            .WithName("GetAllTasks")
            .WithSummary("Returns every task.")
            .Produces<IEnumerable<TaskDto>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .WithSummary("Creates a new task.")
            .Produces<TaskDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPatch("/{id:guid}/toggle", ToggleTask)
            .WithName("ToggleTaskCompletion")
            .WithSummary("Toggles a task's completed status.")
            .Produces<TaskDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteTask)
            .WithName("DeleteTask")
            .WithSummary("Deletes a task.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAllTasks(
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var tasks = await taskService.GetAllTasksAsync(cancellationToken);
        return TypedResults.Ok(tasks);
    }

    private static async Task<IResult> CreateTask(
        CreateTaskRequest request,
        ITaskService taskService,
        IValidator<CreateTaskRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var created = await taskService.CreateTaskAsync(request, cancellationToken);
        return TypedResults.Created($"/api/tasks/{created.Id}", created);
    }

    private static async Task<IResult> ToggleTask(
        Guid id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var task = await taskService.ToggleTaskAsync(id, cancellationToken);
        return task is null ? TypedResults.NotFound() : TypedResults.Ok(task);
    }

    private static async Task<IResult> DeleteTask(
        Guid id,
        ITaskService taskService,
        CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteTaskAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
