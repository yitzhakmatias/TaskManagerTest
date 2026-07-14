import type { AuthRepositoryPort } from "../../domain/ports/AuthRepositoryPort";
import type { AuthSession } from "../../domain/Auth";
import { apiFetch } from "./apiClient";

interface LoginResponseDto {
  token: string;
  expiresAtUtc: string;
}

export class HttpAuthRepository implements AuthRepositoryPort {
  async login(username: string, password: string): Promise<AuthSession> {
    const dto = await apiFetch<LoginResponseDto>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ username, password }),
    });
    return { token: dto.token, expiresAtUtc: dto.expiresAtUtc };
  }
}
