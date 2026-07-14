import type { AuthSession } from "../Auth";

/** Secondary (driven) port for authentication - the app core knows nothing about HTTP/JWT. */
export interface AuthRepositoryPort {
  login(username: string, password: string): Promise<AuthSession>;
}
