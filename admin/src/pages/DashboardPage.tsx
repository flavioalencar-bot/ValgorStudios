import { useAuth } from "../auth/AuthContext";

export function DashboardPage() {
  const { user } = useAuth();

  return (
    <section className="dashboard">
      <div className="dashboard-eyebrow">
        <span aria-hidden="true" />
        Ambiente administrativo
      </div>
      <h1>Bem-vindo, {user?.displayName}.</h1>
      <p>
        Selecione uma área no menu para gerenciar a operação da Valgor Studios.
      </p>
    </section>
  );
}
