import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'

export default function Login() {
  const { authenticated, login } = useKeycloak()
  const navigate = useNavigate()

  useEffect(() => {
    if (authenticated) {
      navigate('/dashboard', { replace: true })
    } else {
      login()
    }
  }, [authenticated, login, navigate])

  return <div className="page">Redirecionando para login...</div>
}
