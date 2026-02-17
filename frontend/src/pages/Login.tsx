import { useEffect } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { Button, Title3 } from '@fluentui/react-components'
import { useKeycloak } from '../auth/KeycloakContext'

export default function Login() {
  const { initialized, authenticated, login, loginWithGoogle } = useKeycloak()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (!initialized) return
    if (authenticated) {
      const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/dashboard'
      navigate(from, { replace: true })
    }
  }, [initialized, authenticated, navigate, location.state])

  if (!initialized) {
    return <div className="page-redirect">Carregando...</div>
  }

  if (authenticated) {
    return null
  }

  return (
    <div className="page-center">
      <div className="card-admin">
        <Title3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.5rem' }}>Entrar</Title3>
        <p style={{ marginBottom: 16 }}>Escolha como deseja acessar.</p>
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
      </div>
    </div>
  )
}
