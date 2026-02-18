import { useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'

/**
 * Quando o usuário não está autenticado, redireciona imediatamente para o Keycloak (tela de login).
 * Não exibe formulário no frontend; o login (usuário/senha e Google) fica só no Keycloak.
 */
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
    const from = (location.state as { from?: { pathname: string } })?.from?.pathname
    const redirectUri =
      typeof window !== 'undefined'
        ? `${window.location.origin}${from ?? '/dashboard'}`
        : undefined
    login({ redirectUri })
  }, [initialized, authenticated, login, navigate, location.state])

  if (!initialized) {
    return <div className="loading">Carregando...</div>
  }

  if (authenticated) {
    return null
  }

  return <div className="loading">Redirecionando para login...</div>
}
