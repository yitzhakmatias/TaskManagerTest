import { API_BASE_URL } from "./env";
import { clearToken, loadToken } from "../auth/tokenStorage";

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

/** Dispatched on window when a request comes back 401 - AuthContext listens for this to log the user out. */
export const UNAUTHORIZED_EVENT = "task-manager:unauthorized";

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const session = loadToken();

  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (session) {
    headers.Authorization = `Bearer ${session.token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: { ...headers, ...(init?.headers as Record<string, string> | undefined) },
  });

  if (response.status === 401) {
    clearToken();
    window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    throw new ApiError(401, "Session expired. Please log in again.");
  }

  if (!response.ok) {
    throw new ApiError(response.status, `Request to ${path} failed with status ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
