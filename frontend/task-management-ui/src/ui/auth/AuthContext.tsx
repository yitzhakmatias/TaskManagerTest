import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import type { AuthRepositoryPort } from "../../domain/ports/AuthRepositoryPort";
import { HttpAuthRepository } from "../../infrastructure/http/HttpAuthRepository";
import { UNAUTHORIZED_EVENT } from "../../infrastructure/http/apiClient";
import { clearToken, loadToken, saveToken } from "../../infrastructure/auth/tokenStorage";
import { login as loginUseCase } from "../../application/useCases/login";
import { DEMO_PASSWORD, DEMO_USERNAME } from "./demoCredentials";

type AuthStatus = "authenticating" | "ready" | "error";

interface AuthContextValue {
  status: AuthStatus;
  errorMessage: string | null;
  retry: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const defaultAuthRepository: AuthRepositoryPort = new HttpAuthRepository();

interface AuthProviderProps {
  children: ReactNode;
  /** Injectable for tests - defaults to the real HTTP adapter. */
  repository?: AuthRepositoryPort;
}

/**
 * There is no visible sign-in screen. The API is still JWT-protected (per
 * the security requirement), but the frontend authenticates transparently
 * with a single demo account on startup, and again automatically if a token
 * expires or is rejected mid-session - see demoCredentials.ts.
 */
export function AuthProvider({ children, repository = defaultAuthRepository }: AuthProviderProps) {
  const [status, setStatus] = useState<AuthStatus>(() => (loadToken() ? "ready" : "authenticating"));
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isAuthenticatingRef = useRef(false);

  const authenticate = useCallback(async () => {
    if (isAuthenticatingRef.current) {
      return;
    }
    isAuthenticatingRef.current = true;
    setStatus("authenticating");
    setErrorMessage(null);
    try {
      const session = await loginUseCase(repository, DEMO_USERNAME, DEMO_PASSWORD);
      saveToken(session);
      setStatus("ready");
    } catch {
      setStatus("error");
      setErrorMessage("Could not connect to the API. Is the backend running?");
    } finally {
      isAuthenticatingRef.current = false;
    }
  }, [repository]);

  useEffect(() => {
    if (!loadToken()) {
      void authenticate();
    }
    // Intentionally run only once per mount - re-running this on every
    // `authenticate` identity change would defeat the point of checking for
    // an existing token first.
  }, []);

  useEffect(() => {
    const handleUnauthorized = () => {
      void authenticate();
    };
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
  }, [authenticate]);

  const retry = useCallback(() => {
    clearToken();
    void authenticate();
  }, [authenticate]);

  const value = useMemo(() => ({ status, errorMessage, retry }), [status, errorMessage, retry]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
