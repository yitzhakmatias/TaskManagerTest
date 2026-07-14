using Moq;
using NUnit.Framework;
using TaskManagement.Api.Core.Application;
using TaskManagement.Api.Core.Application.Dtos;
using TaskManagement.Api.Core.Domain;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Tests.Services;

/// <summary>
/// Pure unit tests for TaskService with IUnitOfWork/ITaskRepository mocked via
/// Moq - no database, no HTTP pipeline. These exercise the business logic
/// (trimming, toggling, existence checks, when SaveChanges is/isn't called)
/// in isolation from persistence.
/// </summary>
[TestFixture]
public class TaskServiceTests
{
    private Mock<ITaskRepository> _taskRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private TaskService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.SetupGet(u => u.Tasks).Returns(_taskRepositoryMock.Object);

        _sut = new TaskService(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GetAllTasksAsync_MapsRepositoryResultsToDtos()
    {
        var tasks = new List<TaskItem>
        {
            new() { Title = "One" },
            new() { Title = "Two", IsCompleted = true }
        };
        _taskRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        var result = await _sut.GetAllTasksAsync();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(t => t.Title), Is.EquivalentTo(new[] { "One", "Two" }));
    }

    [Test]
    public async Task CreateTaskAsync_TrimsTitle_StagesAndSaves()
    {
        var request = new CreateTaskRequest("  Buy milk  ");

        var result = await _sut.CreateTaskAsync(request);

        Assert.That(result.Title, Is.EqualTo("Buy milk"));
        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.Is<TaskItem>(t => t.Title == "Buy milk"), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ToggleTaskAsync_FlipsStatus_WhenTaskExists()
    {
        var task = new TaskItem { Title = "Existing", IsCompleted = false };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.ToggleTaskAsync(task.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCompleted, Is.True);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ToggleTaskAsync_ReturnsNull_AndDoesNotSave_WhenTaskDoesNotExist()
    {
        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _sut.ToggleTaskAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteTaskAsync_RemovesTask_WhenItExists()
    {
        var task = new TaskItem { Title = "Existing" };
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.DeleteTaskAsync(task.Id);

        Assert.That(result, Is.True);
        _taskRepositoryMock.Verify(r => r.Remove(task), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteTaskAsync_ReturnsFalse_AndDoesNotSave_WhenTaskDoesNotExist()
    {
        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _sut.DeleteTaskAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
