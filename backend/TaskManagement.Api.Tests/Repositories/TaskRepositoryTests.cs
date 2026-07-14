using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskManagement.Api.Adapters.Persistence;
using TaskManagement.Api.Core.Domain;

namespace TaskManagement.Api.Tests.Repositories;

[TestFixture]
public class TaskRepositoryTests
{
    private SqliteConnection _connection = null!;
    private TaskDbContext _context = null!;
    private TaskRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        // A single open in-memory SQLite connection kept alive for the test's
        // lifetime, so the schema and data survive across separate operations
        // exactly like a real SQLite file would. Migrate() (rather than
        // EnsureCreated) is used deliberately so these tests also exercise the
        // hand-authored InitialCreate migration and its seed data.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TaskDbContext(options);
        _context.Database.Migrate();

        _sut = new TaskRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Test]
    public async Task Migrate_AppliesSeedData()
    {
        var all = await _sut.GetAllAsync();

        Assert.That(all, Has.Count.EqualTo(3));
        Assert.That(all.Select(t => t.Title), Is.EquivalentTo(new[]
        {
            "Design the database schema",
            "Build the REST API",
            "Wire up the React frontend"
        }));
    }

    [Test]
    public async Task AddAsync_StagesOnly_NotVisibleUntilSaveChanges()
    {
        var task = new TaskItem { Title = "Write README" };

        await _sut.AddAsync(task);
        var beforeSave = await _sut.GetAllAsync();
        Assert.That(beforeSave.Any(t => t.Id == task.Id), Is.False, "AddAsync should not commit without SaveChangesAsync");

        await _context.SaveChangesAsync();
        var afterSave = await _sut.GetAllAsync();

        Assert.That(afterSave.Any(t => t.Id == task.Id), Is.True);
    }

    [Test]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        var first = await AddAndSave(new TaskItem { Title = "First", CreatedAt = DateTime.UtcNow.AddMinutes(-5) });
        var second = await AddAndSave(new TaskItem { Title = "Second", CreatedAt = DateTime.UtcNow });

        var allIds = (await _sut.GetAllAsync()).Select(t => t.Id).ToList();

        // Seeded rows have older, fixed dates, so the two tasks just created
        // (with "now"-based timestamps) should sort ahead of them.
        Assert.That(allIds.Take(2), Is.EqualTo(new[] { second.Id, first.Id }));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsTrackedEntity_MutationsPersistAfterSaveChanges()
    {
        var task = await AddAndSave(new TaskItem { Title = "Toggle me" });

        var fetched = await _sut.GetByIdAsync(task.Id);
        fetched!.IsCompleted = true;
        await _context.SaveChangesAsync();

        var reloaded = await _sut.GetByIdAsync(task.Id);
        Assert.That(reloaded!.IsCompleted, Is.True);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Remove_StagesOnly_DeletedAfterSaveChanges()
    {
        var task = await AddAndSave(new TaskItem { Title = "Delete me" });

        _sut.Remove(task);
        Assert.That(await _sut.GetByIdAsync(task.Id), Is.Not.Null, "Remove should not commit without SaveChangesAsync");

        await _context.SaveChangesAsync();

        Assert.That(await _sut.GetByIdAsync(task.Id), Is.Null);
    }

    private async Task<TaskItem> AddAndSave(TaskItem task)
    {
        await _sut.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }
}
