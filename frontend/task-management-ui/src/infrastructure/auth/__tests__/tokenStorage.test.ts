import { clearToken, loadToken, saveToken } from "../tokenStorage";
import type { AuthSession } from "../../../domain/Auth";

describe("tokenStorage", () => {
  afterEach(() => {
    localStorage.clear();
  });

  it("round-trips a valid, unexpired session", () => {
    const session: AuthSession = { token: "abc.def.ghi", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() };

    saveToken(session);

    expect(loadToken()).toEqual(session);
  });

  it("returns null and clears storage for an expired session", () => {
    const session: AuthSession = { token: "abc.def.ghi", expiresAtUtc: new Date(Date.now() - 60_000).toISOString() };
    saveToken(session);

    expect(loadToken()).toBeNull();
    expect(localStorage.getItem("task-manager.auth-session")).toBeNull();
  });

  it("returns null when nothing is stored", () => {
    expect(loadToken()).toBeNull();
  });

  it("clearToken removes a stored session", () => {
    saveToken({ token: "abc", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() });

    clearToken();

    expect(loadToken()).toBeNull();
  });
});
