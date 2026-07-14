namespace TaskManagement.Api.Core.Application.Dtos;

/// <summary>Payload accepted by POST /api/auth/login.</summary>
public record LoginRequest(string Username, string Password);
