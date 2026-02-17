import { Button, Title3 } from '@fluentui/react-components'
import { useNavigate, Link } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'
import { useMe } from '../hooks/useMe'

export default function Home() {
  const { authenticated, login, loginWithGoogle, logout } = useKeycloak()
  const { me } = useMe()
  const navigate = useNavigate()

  return (
    <div className="page-center">
      <div className="card-admin">
        <Title3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.5rem' }}>SaaS Multi-tenant</Title3>
        <p>Bem-vindo. Backend em .NET (Hexagonal + CQRS), Frontend em React, Keycloak.</p>
        {authenticated ? (
          <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap', justifyContent: 'center' }}>
            <Button appearance="primary" onClick={() => navigate('/dashboard')}>
              Ir para Dashboard
            </Button>
            {me?.isAdmin && (
              <Button appearance="primary" onClick={() => navigate('/admin/tenants')}>
                Administração
              </Button>
            )}
            <Button appearance="secondary" onClick={() => logout()}>
              Sair
            </Button>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10, alignItems: 'center' }}>
            <Button appearance="primary" onClick={() => login()}>
              Entrar com Keycloak
            </Button>
            <Button appearance="secondary" onClick={() => loginWithGoogle()}>
              Entrar com Google
            </Button>
            <p style={{ margin: 0 }}>
              <Link to="/signup">Cadastrar</Link>
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
