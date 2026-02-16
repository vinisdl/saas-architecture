import { Button, Title3 } from '@fluentui/react-components'
import { Outlet, useNavigate, Navigate } from 'react-router-dom'
import { useKeycloak } from '../../auth/KeycloakContext'
import { useMe } from '../../hooks/useMe'

export default function AdminLayout() {
  const { authenticated } = useKeycloak()
  const { me, loading } = useMe()
  const navigate = useNavigate()

  if (!authenticated) {
    return <Navigate to="/login" replace />
  }
  if (loading) {
    return <div style={{ padding: 24 }}>Carregando...</div>
  }
  if (!me?.isAdmin) {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <div style={{ padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, flexWrap: 'wrap', gap: 8 }}>
        <Title3>Administração</Title3>
        <div style={{ display: 'flex', gap: 8 }}>
          <Button onClick={() => navigate('/admin/tenants')}>Tenants</Button>
          <Button onClick={() => navigate('/admin/users')}>Usuários</Button>
          <Button appearance="secondary" onClick={() => navigate('/dashboard')}>
            Voltar ao Dashboard
          </Button>
        </div>
      </div>
      <Outlet />
    </div>
  )
}
