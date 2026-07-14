import type { AuthSession } from "../../domain/Auth";

const STORAGE_KEY = "task-manager.auth-session";

/**
 * localStorage-backed token persistence. Trade-off: vulnerable to XSS reading
 * the token, vs. an httpOnly cookie which the backend would need to issue
 * instead - see README trade-offs. Kept isolated here so swapping the storage
 * strategy later touches only this file.
 */
export function saveToken(session: AuthSession): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function loadToken(): AuthSession | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as AuthSession;
    if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
      clearToken();
      return null;
    }
    return parsed;
  } catch {
    clearToken();
    return null;
  }
}

export function clearToken(): void {
  localStorage.removeItem(STORAGE_KEY);
}
