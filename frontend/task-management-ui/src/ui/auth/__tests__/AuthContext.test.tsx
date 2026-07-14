import { act, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthProvider, useAuth } from "../AuthContext";
import { UNAUTHORIZED_EVENT } from "../../../infrastructure/http/apiClient";
import { DEMO_PASSWORD, DEMO_USERNAME } from "../demoCredentials";
import type { AuthRepositoryPort } from "../../../domain/ports/AuthRepositoryPort";

function Consumer() {
  const { status, errorMessage, retry } = useAuth();
  return (
    <div>
      <span data-testid="status">{status}</span>
      {errorMessage && <span data-testid="error">{errorMessage}</span>}
      <button onClick={retry}>retry</button>
    </div>
  );
}

const validSession = () => ({
  token: "abc.def.ghi",
  expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
});

describe("AuthContext", () => {
  afterEach(() => {
    localStorage.clear();
  });

  it("authenticates automatically on mount using the demo credentials", async () => {
    const repository: AuthRepositoryPort = { login: jest.fn().mockResolvedValue(validSession()) };

    render(
      <AuthProvider repository={repository}>
        <Consumer />
      </AuthProvider>
    );

    expect(screen.getByTestId("status")).toHaveTextContent("authenticating");

    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("ready"));
    expect(repository.login).toHaveBeenCalledWith(DEMO_USERNAME, DEMO_PASSWORD);
  });

  it("shows an error status and message when auto-login fails", async () => {
    const repository: AuthRepositoryPort = { login: jest.fn().mockRejectedValue(new Error("network error")) };

    render(
      <AuthProvider repository={repository}>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("error"));
    expect(screen.getByTestId("error")).toHaveTextContent(/could not connect/i);
  });

  it("re-authenticates automatically when an unauthorized event fires", async () => {
    const repository: AuthRepositoryPort = { login: jest.fn().mockResolvedValue(validSession()) };

    render(
      <AuthProvider repository={repository}>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("ready"));
    expect(repository.login).toHaveBeenCalledTimes(1);

    act(() => {
      window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    });

    await waitFor(() => expect(repository.login).toHaveBeenCalledTimes(2));
    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("ready"));
  });

  it("retries when the retry action is invoked after a failure", async () => {
    const login = jest
      .fn()
      .mockRejectedValueOnce(new Error("network error"))
      .mockResolvedValueOnce(validSession());
    const repository: AuthRepositoryPort = { login };
    const user = userEvent.setup();

    render(
      <AuthProvider repository={repository}>
        <Consumer />
      </AuthProvider>
    );

    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("error"));

    await user.click(screen.getByText("retry"));

    await waitFor(() => expect(screen.getByTestId("status")).toHaveTextContent("ready"));
    expect(login).toHaveBeenCalledTimes(2);
  });

  it("skips auto-login entirely when a valid session is already stored", async () => {
    localStorage.setItem("task-manager.auth-session", JSON.stringify(validSession()));
    const repository: AuthRepositoryPort = { login: jest.fn() };

    render(
      <AuthProvider repository={repository}>
        <Consumer />
      </AuthProvider>
    );

    expect(screen.getByTestId("status")).toHaveTextContent("ready");
    expect(repository.login).not.toHaveBeenCalled();
  });
});
