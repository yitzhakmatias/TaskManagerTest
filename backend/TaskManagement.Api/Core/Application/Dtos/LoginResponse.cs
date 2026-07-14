namespace TaskManagement.Api.Core.Application.Dtos;

/// <summary>Read-model returned by a successful login.</summary>
public record LoginResponse(string Token, DateTime ExpiresAtUtc);
