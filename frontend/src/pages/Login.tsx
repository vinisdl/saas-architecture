import { useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'

export default function Login() {
  const { initialized, authenticated, login } = useKeycloak()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (!initialized) return
    if (authenticated) {
      const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/dashboard'
      navigate(from, { replace: true })
      return
    }
    // Evita loop: não chamar login() se a URL já tem o callback do Keycloak (hash com code=).
    // O init vai processar o hash e setar authenticated.
    if (typeof window !== 'undefined' && window.location.hash.includes('code=')) return
    login()
  }, [initialized, authenticated, login, navigate, location.state])

  if (!initialized) {
    return <div className="page-redirect">Carregando...</div>
  }

  return <div className="page-redirect">Redirecionando para login...</div>
}
