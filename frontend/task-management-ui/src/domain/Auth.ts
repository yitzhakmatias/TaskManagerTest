/** A successfully-issued session: the bearer token plus when it expires. */
export interface AuthSession {
  token: string;
  expiresAtUtc: string;
}
