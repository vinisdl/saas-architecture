import { Outlet, Navigate, useLocation } from 'react-router-dom'
import { useKeycloak } from '../../auth/KeycloakContext'
import { useMe } from '../../hooks/useMe'
import AppLayout from '../../components/AppLayout'

export default function AdminLayout() {
  const { authenticated } = useKeycloak()
  const { me, loading } = useMe()
  const location = useLocation()

  if (!authenticated) {
    return <Navigate to="/" replace state={{ from: { pathname: location.pathname || '/admin' } }} />
  }
  if (loading) {
    return (
      <div className="loading">
        Carregando...
      </div>
    )
  }
  if (!me?.isAdmin) {
    return <Navigate to="/dashboard" replace />
  }

  const activeNav = location.pathname.includes('/users') ? 'admin-users' : 'admin-tenants'
  const title = location.pathname.includes('/users') ? 'Usuários' : 'Tenants'

  return (
    <AppLayout title={title} activeNav={activeNav}>
      <Outlet />
    </AppLayout>
  )
}
