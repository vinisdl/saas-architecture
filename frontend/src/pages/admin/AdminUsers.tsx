import {
  Button,
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
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Label,
  useId,
} from '@fluentui/react-components'
import { useState, useEffect, useCallback } from 'react'
import { useKeycloak } from '../../auth/KeycloakContext'

const API_BASE = import.meta.env.VITE_API_URL ?? '/api'

type KeycloakUserDto = {
  id: string
  username: string | null
  email: string | null
  tenantId: string | null
}

type TenantDto = {
  id: string
  name: string
  slug: string
  isActive: boolean
}

function authHeaders(token: string | undefined): HeadersInit {
  const h: HeadersInit = { 'Content-Type': 'application/json' }
  if (token) (h as Record<string, string>)['Authorization'] = `Bearer ${token}`
  return h
}

export default function AdminUsers() {
  const { token } = useKeycloak()
  const [users, setUsers] = useState<KeycloakUserDto[]>([])
  const [tenants, setTenants] = useState<TenantDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [assignOpen, setAssignOpen] = useState(false)
  const [assigningUser, setAssigningUser] = useState<KeycloakUserDto | null>(null)
  const [selectedTenantId, setSelectedTenantId] = useState<string>('')
  const [submitting, setSubmitting] = useState(false)

  const loadUsers = useCallback(() => {
    fetch(`${API_BASE}/admin/users`, { headers: authHeaders(token) })
      .then((res) => {
        if (!res.ok) throw new Error(`API ${res.status}`)
        return res.json()
      })
      .then((data) => setUsers(Array.isArray(data) ? data : []))
      .catch((e) => setError(e.message))
  }, [token])

  useEffect(() => {
    setLoading(true)
    Promise.all([
      fetch(`${API_BASE}/admin/users`, { headers: authHeaders(token) }).then((res) => {
        if (!res.ok) throw new Error(`API ${res.status}`)
        return res.json()
      }),
      fetch(`${API_BASE}/tenants`, { headers: authHeaders(token) }).then((res) => {
        if (!res.ok) return []
        return res.json()
      }),
    ])
      .then(([usersData, tenantsData]) => {
        setUsers(Array.isArray(usersData) ? usersData : [])
        setTenants(Array.isArray(tenantsData) ? tenantsData : [])
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [token])

  const openAssign = (u: KeycloakUserDto) => {
    setAssigningUser(u)
    setSelectedTenantId(u.tenantId ?? '')
    setAssignOpen(true)
  }

  const handleAssign = async () => {
    if (!assigningUser || !selectedTenantId) return
    setSubmitting(true)
    try {
      const res = await fetch(
        `${API_BASE}/admin/tenants/${selectedTenantId}/users/${encodeURIComponent(assigningUser.id)}`,
        {
          method: 'POST',
          headers: authHeaders(token),
        }
      )
      if (!res.ok) throw new Error(await res.text())
      setAssignOpen(false)
      setAssigningUser(null)
      loadUsers()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao atribuir tenant')
    } finally {
      setSubmitting(false)
    }
  }

  const assignId = useId('assign-dialog')
  const tenantName = (id: string) => tenants.find((t) => t.id === id)?.name ?? id

  return (
    <div className="card-admin">
      <Title3 style={{ marginBottom: 16, marginTop: 0 }}>Usuários</Title3>
      <p>Lista de usuários do Keycloak. Atribua um tenant para dar acesso.</p>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}
      {loading && <Spinner label="Carregando usuários..." />}
      {!loading && users.length > 0 && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Username</TableHeaderCell>
              <TableHeaderCell>Email</TableHeaderCell>
              <TableHeaderCell>Tenant</TableHeaderCell>
              <TableHeaderCell>Ações</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {users.map((u) => (
              <TableRow key={u.id}>
                <TableCell>{u.username ?? '-'}</TableCell>
                <TableCell>{u.email ?? '-'}</TableCell>
                <TableCell>{u.tenantId ? tenantName(u.tenantId) : '-'}</TableCell>
                <TableCell>
                  <Button appearance="subtle" size="small" onClick={() => openAssign(u)}>
                    Atribuir tenant
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {!loading && users.length === 0 && <p>Nenhum usuário encontrado.</p>}

      <Dialog open={assignOpen} onOpenChange={(_, d) => setAssignOpen(d.open)}>
        <DialogSurface id={assignId}>
          <DialogBody>
            <DialogTitle>Atribuir usuário ao tenant</DialogTitle>
            <DialogContent>
              {assigningUser && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 8 }}>
                  <p>
                    Usuário: <strong>{assigningUser.username ?? assigningUser.email ?? assigningUser.id}</strong>
                  </p>
                  <div>
                    <Label htmlFor="tenant-select">Tenant</Label>
                    <select
                      id="tenant-select"
                      value={selectedTenantId}
                      onChange={(e) => setSelectedTenantId(e.target.value)}
                      style={{ padding: 6, minWidth: 200 }}
                    >
                      <option value="">-- Selecione um tenant --</option>
                      {tenants.filter((t) => t.isActive).map((t) => (
                        <option key={t.id} value={t.id}>
                          {t.name} ({t.slug})
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              )}
            </DialogContent>
            <DialogActions>
              <Button appearance="secondary" onClick={() => setAssignOpen(false)}>
                Cancelar
              </Button>
              <Button
                appearance="primary"
                onClick={handleAssign}
                disabled={submitting || !selectedTenantId}
              >
                Atribuir
              </Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </div>
  )
}
