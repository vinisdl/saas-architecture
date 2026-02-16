import {
  Button,
  Card,
  Spinner,
  Title3,
  MessageBar,
  MessageBarBody,
  Table,
  TableBody,
  TableCell,
  TableHeader,
  TableHeaderCell,
  TableRow,
} from '@fluentui/react-components'
import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useKeycloak } from '../auth/KeycloakContext'
import { useMe } from '../hooks/useMe'

const API_BASE = import.meta.env.VITE_API_URL ?? '/api'

type TenantDto = {
  id: string
  name: string
  slug: string
  isActive: boolean
}

export default function Dashboard() {
  const { token, logout } = useKeycloak()
  const { me } = useMe()
  const navigate = useNavigate()
  const [tenants, setTenants] = useState<TenantDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    }
    if (token) {
      (headers as Record<string, string>)['Authorization'] = `Bearer ${token}`
    }

    fetch(`${API_BASE}/tenants`, { headers })
      .then((res) => {
        if (!res.ok) throw new Error(`API ${res.status}`)
        return res.json()
      })
      .then((data) => {
        setTenants(Array.isArray(data) ? data : [])
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [token])

  return (
    <div style={{ padding: 24, maxWidth: 800 }}>
      <Card>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
          <Title3>Dashboard</Title3>
          <div style={{ display: 'flex', gap: 8 }}>
            <Button onClick={() => navigate('/')}>Voltar</Button>
            {me?.isAdmin && (
              <Button appearance="primary" onClick={() => navigate('/admin')}>
                Administração
              </Button>
            )}
            <Button appearance="secondary" onClick={() => logout()}>
              Sair
            </Button>
          </div>
        </div>
        <p>Chamada autenticada à API (lista de tenants).</p>

        {loading && <Spinner label="Carregando..." />}
        {error && (
          <MessageBar intent="error">
            <MessageBarBody>Erro: {error}</MessageBarBody>
          </MessageBar>
        )}
        {!loading && !error && tenants.length === 0 && (
          <p>Nenhum tenant cadastrado. Acesse a área Administração para criar um.</p>
        )}
        {!loading && !error && tenants.length > 0 && (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Nome</TableHeaderCell>
                <TableHeaderCell>Slug</TableHeaderCell>
                <TableHeaderCell>Ativo</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {tenants.map((t) => (
                <TableRow key={t.id}>
                  <TableCell>{t.name}</TableCell>
                  <TableCell>{t.slug}</TableCell>
                  <TableCell>{t.isActive ? 'Sim' : 'Não'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Card>
    </div>
  )
}
