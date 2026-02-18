import { Link, useLocation } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'

export default function Home() {
  const { login } = useKeycloak()
  const location = useLocation()
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/dashboard'

  const handleEntrar = () => {
    const redirectUri =
      typeof window !== 'undefined' ? `${window.location.origin}${from}` : undefined
    login({ redirectUri })
  }

  return (
    <div className="home">
      <header className="home-header">
        <div className="home-header__inner">
          <Link to="/" className="home-header__logo" aria-label="Início">
            SaaS
          </Link>
          <nav className="home-header__nav">
            <button
              type="button"
              className="home-header__btn home-header__btn--secondary"
              onClick={handleEntrar}
            >
              Entrar
            </button>
            <Link to="/signup" className="home-header__btn home-header__btn--primary">
              Cadastrar
            </Link>
          </nav>
        </div>
      </header>
      <main className="home-main">
        <div className="home-hero">
          <h1 className="home-hero__title">Bem-vindo</h1>
          <p className="home-hero__subtitle">
            Entre ou crie uma conta para acessar a plataforma.
          </p>
        </div>
      </main>
    </div>
  )
}
