import {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from 'react'
import type Keycloak from 'keycloak-js'

type KeycloakContextValue = {
  keycloak: Keycloak
  initialized: boolean
  authenticated: boolean
  login: () => void
  logout: () => void
  token: string | undefined
}

const KeycloakContext = createContext<KeycloakContextValue | null>(null)

export function KeycloakProvider({
  keycloak,
  children,
}: {
  keycloak: Keycloak
  children: ReactNode
}) {
  const [initialized, setInitialized] = useState(false)
  const [authenticated, setAuthenticated] = useState(false)

  useEffect(() => {
    keycloak
      .init({ onLoad: 'check-sso' })
      .then((auth) => {
        setAuthenticated(auth ?? false)
        setInitialized(true)
      })
      .catch(() => setInitialized(true))
  }, [keycloak])

  const login = () => keycloak.login()
  const logout = () => keycloak.logout()
  const token = keycloak.token

  return (
    <KeycloakContext.Provider
      value={{
        keycloak,
        initialized,
        authenticated,
        login,
        logout,
        token,
      }}
    >
      {children}
    </KeycloakContext.Provider>
  )
}

export function useKeycloak() {
  const ctx = useContext(KeycloakContext)
  if (!ctx) throw new Error('useKeycloak must be used within KeycloakProvider')
  return ctx
}
