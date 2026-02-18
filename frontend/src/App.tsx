import { FluentProvider, webLightTheme } from '@fluentui/react-components'
import { Routes, Route, Navigate } from 'react-router-dom'
import Keycloak from 'keycloak-js'
import { KeycloakProvider, useKeycloak } from './auth/KeycloakContext'
import Dashboard from './pages/Dashboard'
import Home from './pages/Home'
import Login from './pages/Login'
import Signup from './pages/Signup'
import AdminLayout from './pages/admin/AdminLayout'
import AdminTenants from './pages/admin/AdminTenants'
import AdminUsers from './pages/admin/AdminUsers'

const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
  realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'saas',
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'saas-frontend',
})

function AppRoutes() {
  const { initialized, authenticated } = useKeycloak()

  if (!initialized) {
    return (
      <FluentProvider theme={webLightTheme}>
        <div className="loading">Carregando...</div>
      </FluentProvider>
    )
  }

  return (
    <Routes>
      <Route
        path="/"
        element={authenticated ? <Navigate to="/dashboard" replace /> : <Home />}
      />
      <Route path="/login" element={<Login />} />
      <Route path="/signup" element={<Signup />} />
      <Route
        path="/dashboard"
        element={
          authenticated ? (
            <Dashboard />
          ) : (
            <Navigate to="/" replace state={{ from: { pathname: '/dashboard' } }} />
          )
        }
      />
      <Route
        path="/admin"
        element={
          authenticated ? (
            <AdminLayout />
          ) : (
            <Navigate to="/" replace state={{ from: { pathname: '/admin' } }} />
          )
        }
      >
        <Route index element={<Navigate to="tenants" replace />} />
        <Route path="tenants" element={<AdminTenants />} />
        <Route path="users" element={<AdminUsers />} />
      </Route>
    </Routes>
  )
}

export default function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <KeycloakProvider keycloak={keycloak}>
        <AppRoutes />
      </KeycloakProvider>
    </FluentProvider>
  )
}
