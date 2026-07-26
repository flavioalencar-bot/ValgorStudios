import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import type { AuthenticatedUser } from "../api/auth";

const STORAGE_KEY = "valgor.admin.session";

interface AuthSession {
  accessToken: string;
  user: AuthenticatedUser;
}

interface AuthContextValue {
  accessToken: string | null;
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  signIn: (session: AuthSession) => void;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function readSession(): AuthSession | null {
  try {
    const value = localStorage.getItem(STORAGE_KEY);
    if (!value) {
      return null;
    }

    const session: unknown = JSON.parse(value);
    if (
      typeof session === "object" &&
      session !== null &&
      "accessToken" in session &&
      typeof session.accessToken === "string" &&
      "user" in session &&
      typeof session.user === "object" &&
      session.user !== null
    ) {
      const { user } = session;
      if (
        "email" in user &&
        typeof user.email === "string" &&
        "displayName" in user &&
        typeof user.displayName === "string" &&
        "role" in user &&
        typeof user.role === "string"
      ) {
        return {
          accessToken: session.accessToken,
          user: {
            email: user.email,
            displayName: user.displayName,
            role: user.role,
          },
        };
      }
    }
  } catch {
    localStorage.removeItem(STORAGE_KEY);
  }

  return null;
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(readSession);

  const signIn = useCallback((nextSession: AuthSession) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(nextSession));
    setSession(nextSession);
  }, []);

  const signOut = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY);
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      accessToken: session?.accessToken ?? null,
      user: session?.user ?? null,
      isAuthenticated: session !== null,
      signIn,
      signOut,
    }),
    [session, signIn, signOut],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth deve ser usado dentro de AuthProvider.");
  }
  return context;
}
