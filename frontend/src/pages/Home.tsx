import { Button, Card, Title3 } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'
import { useMe } from '../hooks/useMe'

export default function Home() {
  const { authenticated, login, logout } = useKeycloak()
  const { me } = useMe()
  const navigate = useNavigate()

  return (
    <div style={{ padding: 24, maxWidth: 640 }}>
      <Card>
        <Title3>SaaS Multi-tenant</Title3>
        <p>Bem-vindo. Backend em .NET (Hexagonal + CQRS), Frontend em React, Keycloak.</p>
        {authenticated ? (
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            <Button appearance="primary" onClick={() => navigate('/dashboard')}>
              Ir para Dashboard
            </Button>
            {me?.isAdmin && (
              <Button appearance="primary" onClick={() => navigate('/admin')}>
                Administração
              </Button>
            )}
            <Button appearance="secondary" onClick={() => logout()}>
              Sair
            </Button>
          </div>
        ) : (
          <Button appearance="primary" onClick={() => login()}>
            Entrar com Keycloak
          </Button>
        )}
      </Card>
    </div>
  )
}
