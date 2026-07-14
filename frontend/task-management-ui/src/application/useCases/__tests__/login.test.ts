import { login, MissingCredentialsError } from "../login";
import type { AuthRepositoryPort } from "../../../domain/ports/AuthRepositoryPort";
import type { AuthSession } from "../../../domain/Auth";

describe("login", () => {
  it("delegates to the repository with a trimmed username", async () => {
    const expected: AuthSession = { token: "abc.def.ghi", expiresAtUtc: "2026-01-01T00:00:00Z" };
    const repository: AuthRepositoryPort = { login: jest.fn().mockResolvedValue(expected) };

    const result = await login(repository, "  admin  ", "admin123!");

    expect(repository.login).toHaveBeenCalledWith("admin", "admin123!");
    expect(result).toEqual(expected);
  });

  it("throws MissingCredentialsError and never calls the repository when username is blank", async () => {
    const repository: AuthRepositoryPort = { login: jest.fn() };

    await expect(login(repository, "   ", "admin123!")).rejects.toThrow(MissingCredentialsError);
    expect(repository.login).not.toHaveBeenCalled();
  });

  it("throws MissingCredentialsError when password is blank", async () => {
    const repository: AuthRepositoryPort = { login: jest.fn() };

    await expect(login(repository, "admin", "")).rejects.toThrow(MissingCredentialsError);
    expect(repository.login).not.toHaveBeenCalled();
  });
});
