import { FormEvent, useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { ApiError } from "../api/client";
import { login } from "../api/auth";
import { useAuth } from "../auth/AuthContext";

interface LocationState {
  from?: {
    pathname?: string;
  };
}

export function LoginPage() {
  const { isAuthenticated, signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const locationState = location.state as LocationState | null;
  const destination = locationState?.from?.pathname ?? "/dashboard";

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const session = await login(email.trim(), password);
      signIn(session);
      navigate(destination, { replace: true });
    } catch (caughtError) {
      setError(
        caughtError instanceof ApiError
          ? caughtError.message
          : "Não foi possível conectar ao servidor. Tente novamente.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand-panel" aria-label="Valgor Studios">
        <div className="login-brand">
          <div className="brand-lockup">
            <span className="brand-mark" aria-hidden="true">V</span>
            <span>VALGOR</span>
          </div>
          <p className="brand-kicker">STUDIOS · CONTROL ROOM</p>
        </div>

        <div className="brand-message">
          <p className="section-label">Administrar é construir mundos.</p>
          <h1>Seu estúdio, em foco.</h1>
          <p>
            Acesso reservado à operação que transforma ideias em experiências
            memoráveis.
          </p>
        </div>

        <div className="brand-signature">
          <span />
          Valgor Studios
        </div>
      </section>

      <section className="login-form-panel">
        <form className="login-form" onSubmit={handleSubmit}>
          <div className="form-intro">
            <p className="section-label">Acesso restrito</p>
            <h2>Entrar no painel</h2>
            <p>Use suas credenciais para continuar.</p>
          </div>

          <label htmlFor="email">
            E-mail
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              disabled={isSubmitting}
            />
          </label>

          <label htmlFor="password">
            Senha
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              disabled={isSubmitting}
            />
          </label>

          {error && (
            <p className="form-error" role="alert">
              {error}
            </p>
          )}

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Autenticando..." : "Entrar"}
            <span aria-hidden="true">→</span>
          </button>
        </form>
      </section>
    </main>
  );
}
