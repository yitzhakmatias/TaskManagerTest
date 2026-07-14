using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using TaskManagement.Api.Adapters.Persistence;
using TaskManagement.Api.Core.Application.Dtos;

namespace TaskManagement.Api.Tests.Endpoints;

/// <summary>
/// End-to-end tests that boot the real ASP.NET Core pipeline (routing, DI,
/// auth, validation, middleware, migrations) in-memory via WebApplicationFactory,
/// with the SQLite DbContext swapped for an isolated in-memory connection
/// per test so each test starts from the same freshly-migrated + seeded state.
/// </summary>
[TestFixture]
public class TaskEndpointsTests
{
    // Must match appsettings.json's DemoUser section.
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin123!";

    private WebApplicationFactory<Program> _factory = null!;
    private SqliteConnection _connection = null!;
    private HttpClient _client = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<TaskDbContext>>();
                services.AddDbContext<TaskDbContext>(options => options.UseSqlite(_connection));
            });
        });

        _client = _factory.CreateClient();

        var token = await AuthenticateAsync(_client, DemoUsername, DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _connection.Dispose();
    }

    private static async Task<string> AuthenticateAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Test]
    public async Task Health_ReturnsOk_WithoutAuthentication()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Login_ReturnsToken_ForValidDemoCredentials()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new LoginRequest(DemoUsername, DemoPassword));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.That(body!.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(body.ExpiresAtUtc, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new LoginRequest(DemoUsername, "wrong-password"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetTasks_ReturnsUnauthorized_WithoutToken()
    {
        using var anonymousClient = _factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/tasks");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetTasks_ReturnsSeededTasks_Initially()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task CreateTask_ReturnsCreated_WithExpectedBody()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Buy milk"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Title, Is.EqualTo("Buy milk"));
        Assert.That(created.IsCompleted, Is.False);
    }

    [Test]
    public async Task CreateTask_ReturnsValidationProblem_WhenTitleIsEmpty()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest(""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ToggleTask_FlipsCompletedStatus()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Toggle me"));
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var toggleResponse = await _client.PatchAsync($"/api/tasks/{created!.Id}/toggle", null);

        Assert.That(toggleResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var toggled = await toggleResponse.Content.ReadFromJsonAsync<TaskDto>();
        Assert.That(toggled!.IsCompleted, Is.True);
    }

    [Test]
    public async Task ToggleTask_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.PatchAsync($"/api/tasks/{Guid.NewGuid()}/toggle", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteTask_RemovesTask_AndSecondDeleteReturnsNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Delete me"));
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{created!.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var secondDelete = await _client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.That(secondDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
