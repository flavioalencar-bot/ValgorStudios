import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function AdminLayout() {
  const { signOut, user } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    signOut();
    navigate("/login", { replace: true });
  }

  return (
    <div className="admin-shell">
      <aside className="sidebar" aria-label="Navegação principal">
        <NavLink to="/dashboard" className="sidebar-brand" aria-label="Valgor Admin">
          <span className="brand-mark" aria-hidden="true">V</span>
          <span>
            <strong>VALGOR</strong>
            <small>STUDIOS · ADMIN</small>
          </span>
        </NavLink>

        <nav className="sidebar-nav">
          <NavLink
            to="/dashboard"
            className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
          >
            <span aria-hidden="true">▦</span>
            Dashboard
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <div className="account-summary">
            <span className="account-initial" aria-hidden="true">
              {user?.displayName.slice(0, 1).toUpperCase()}
            </span>
            <span>
              <strong>{user?.displayName}</strong>
              <small>{user?.role}</small>
            </span>
          </div>
          <button className="logout-button" type="button" onClick={handleLogout}>
            <span aria-hidden="true">↗</span>
            Sair
          </button>
        </div>
      </aside>

      <main className="admin-main">
        <Outlet />
      </main>
    </div>
  );
}
