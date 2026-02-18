import { useNavigate } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'
import { useMe } from '../hooks/useMe'

const IconDashboard = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
    <path d="M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z" />
  </svg>
)
const IconTenants = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
    <path d="M12 7V3H2v18h20V7H12zM6 19H4v-2h2v2zm0-4H4v-2h2v2zm0-4H4V9h2v2zm0-4H4V5h2v2zm4 12H8v-2h2v2zm0-4H8v-2h2v2zm0-4H8V9h2v2zm0-4H8V5h2v2zm10 12h-8v-2h2v-2h-2v-2h2v-2h-2V9h8v10zm-2-8h-2v2h2v-2zm0 4h-2v2h2v-2z" />
  </svg>
)
const IconUsers = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
    <path d="M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z" />
  </svg>
)
const IconAdmin = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
    <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z" />
  </svg>
)
const IconLogout = () => (
  <svg viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" strokeWidth="1.5">
    <path d="M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z" />
  </svg>
)

type AppLayoutProps = {
  title: string
  children: React.ReactNode
  /** 'dashboard' | 'admin' | 'admin-tenants' | 'admin-users' */
  activeNav?: string
}

export default function AppLayout({ title, children, activeNav = 'dashboard' }: AppLayoutProps) {
  const navigate = useNavigate()
  const { logout } = useKeycloak()
  const { me } = useMe()

  const getInitials = (sub: string | undefined) => {
    if (!sub) return '?'
    return sub.slice(0, 2).toUpperCase()
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">SaaS Admin</div>
        {me && (
          <div className="sidebar-user">
            <div className="sidebar-user-avatar">{getInitials(me.sub)}</div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontWeight: 600, color: 'var(--sidebar-text)' }}>
                {me.name ?? me.givenName ?? 'Usuário'}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--sidebar-text-muted)' }}>
                {me.isAdmin ? 'Administrador' : 'Usuário'}
              </div>
            </div>
          </div>
        )}
        <nav className="sidebar-nav">
          <div className="sidebar-nav-section">
            <div className="sidebar-nav-section-title">Páginas</div>
            <button
              type="button"
              className={`sidebar-nav-item ${activeNav === 'dashboard' ? 'active' : ''}`}
              onClick={() => navigate('/dashboard')}
            >
              <IconDashboard />
              Dashboard
            </button>
            {me?.isAdmin && (
              <>
                <button
                  type="button"
                  className={`sidebar-nav-item ${activeNav === 'admin' ? 'active' : ''}`}
                  onClick={() => navigate('/admin/tenants')}
                >
                  <IconAdmin />
                  Administração
                </button>
                <button
                  type="button"
                  className={`sidebar-nav-item sidebar-nav-item-sub ${activeNav === 'admin-tenants' ? 'active' : ''}`}
                  onClick={() => navigate('/admin/tenants')}
                >
                  <IconTenants />
                  Tenants
                </button>
                <button
                  type="button"
                  className={`sidebar-nav-item sidebar-nav-item-sub ${activeNav === 'admin-users' ? 'active' : ''}`}
                  onClick={() => navigate('/admin/users')}
                >
                  <IconUsers />
                  Usuários
                </button>
              </>
            )}
          </div>
        </nav>
        <div style={{ padding: '0.75rem 1.5rem', borderTop: '1px solid rgba(255,255,255,0.08)' }}>
          <button
            type="button"
            className="sidebar-nav-item"
            onClick={() => logout()}
            style={{ width: '100%', justifyContent: 'flex-start' }}
          >
            <IconLogout />
            Sair
          </button>
        </div>
      </aside>
      <div className="main-wrapper">
        <header className="top-header">
          <div className="top-header-left">
            <h1 className="top-header-title">{title}</h1>
            <input type="search" className="header-search" placeholder="Buscar..." aria-label="Buscar" />
          </div>
          <div className="top-header-right">
            <button
              type="button"
              onClick={() => logout()}
              className="btn-header-logout"
            >
              Sair
            </button>
          </div>
        </header>
        <main className="main-content">{children}</main>
      </div>
    </div>
  )
}
