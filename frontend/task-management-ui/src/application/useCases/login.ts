import type { AuthRepositoryPort } from "../../domain/ports/AuthRepositoryPort";
import type { AuthSession } from "../../domain/Auth";

export class MissingCredentialsError extends Error {
  constructor() {
    super("Username and password are required.");
    this.name = "MissingCredentialsError";
  }
}

export const login = async (
  repository: AuthRepositoryPort,
  username: string,
  password: string
): Promise<AuthSession> => {
  if (!username.trim() || !password) {
    throw new MissingCredentialsError();
  }
  return repository.login(username.trim(), password);
};
