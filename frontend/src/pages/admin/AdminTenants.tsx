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
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Input,
  Label,
  useId,
} from '@fluentui/react-components'
import { useState, useEffect, useCallback } from 'react'
import { useKeycloak } from '../../auth/KeycloakContext'

const API_BASE = import.meta.env.VITE_API_URL ?? '/api'

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

export default function AdminTenants() {
  const { token } = useKeycloak()
  const [tenants, setTenants] = useState<TenantDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [editingTenant, setEditingTenant] = useState<TenantDto | null>(null)
  const [createName, setCreateName] = useState('')
  const [createSlug, setCreateSlug] = useState('')
  const [editName, setEditName] = useState('')
  const [editIsActive, setEditIsActive] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  const loadTenants = useCallback(() => {
    setLoading(true)
    fetch(`${API_BASE}/tenants`, { headers: authHeaders(token) })
      .then((res) => {
        if (!res.ok) throw new Error(`API ${res.status}`)
        return res.json()
      })
      .then((data) => setTenants(Array.isArray(data) ? data : []))
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false))
  }, [token])

  useEffect(() => {
    loadTenants()
  }, [loadTenants])

  const handleCreate = async () => {
    if (!createName.trim() || !createSlug.trim()) return
    setSubmitting(true)
    try {
      const res = await fetch(`${API_BASE}/tenants`, {
        method: 'POST',
        headers: authHeaders(token),
        body: JSON.stringify({ name: createName.trim(), slug: createSlug.trim().toLowerCase() }),
      })
      if (!res.ok) throw new Error(await res.text())
      setCreateOpen(false)
      setCreateName('')
      setCreateSlug('')
      loadTenants()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao criar tenant')
    } finally {
      setSubmitting(false)
    }
  }

  const openEdit = (t: TenantDto) => {
    setEditingTenant(t)
    setEditName(t.name)
    setEditIsActive(t.isActive)
    setEditOpen(true)
  }

  const handleUpdate = async () => {
    if (!editingTenant) return
    setSubmitting(true)
    try {
      const res = await fetch(`${API_BASE}/tenants/${editingTenant.id}`, {
        method: 'PATCH',
        headers: authHeaders(token),
        body: JSON.stringify({ name: editName.trim(), isActive: editIsActive }),
      })
      if (!res.ok) throw new Error(await res.text())
      setEditOpen(false)
      setEditingTenant(null)
      loadTenants()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erro ao atualizar tenant')
    } finally {
      setSubmitting(false)
    }
  }

  const createId = useId('create-dialog')
  const editId = useId('edit-dialog')

  return (
    <Card>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title3>Tenants</Title3>
        <Dialog open={createOpen} onOpenChange={(_, d) => setCreateOpen(d.open)}>
          <DialogTrigger disableButtonEnhancement>
            <Button appearance="primary">Novo tenant</Button>
          </DialogTrigger>
          <DialogSurface id={createId}>
            <DialogBody>
              <DialogTitle>Novo tenant</DialogTitle>
              <DialogContent>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 8 }}>
                  <div>
                    <Label htmlFor="create-name">Nome</Label>
                    <Input
                      id="create-name"
                      value={createName}
                      onChange={(_, d) => setCreateName(d.value)}
                      placeholder="Nome do tenant"
                    />
                  </div>
                  <div>
                    <Label htmlFor="create-slug">Slug</Label>
                    <Input
                      id="create-slug"
                      value={createSlug}
                      onChange={(_, d) => setCreateSlug(d.value)}
                      placeholder="slug-tenant"
                    />
                  </div>
                </div>
              </DialogContent>
              <DialogActions>
                <DialogTrigger disableButtonEnhancement>
                  <Button appearance="secondary">Cancelar</Button>
                </DialogTrigger>
                <Button appearance="primary" onClick={handleCreate} disabled={submitting || !createName.trim() || !createSlug.trim()}>
                  Criar
                </Button>
              </DialogActions>
            </DialogBody>
          </DialogSurface>
        </Dialog>
      </div>

      {error && (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}
      {loading && <Spinner label="Carregando tenants..." />}
      {!loading && tenants.length > 0 && (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Nome</TableHeaderCell>
              <TableHeaderCell>Slug</TableHeaderCell>
              <TableHeaderCell>Ativo</TableHeaderCell>
              <TableHeaderCell>Ações</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {tenants.map((t) => (
              <TableRow key={t.id}>
                <TableCell>{t.name}</TableCell>
                <TableCell>{t.slug}</TableCell>
                <TableCell>{t.isActive ? 'Sim' : 'Não'}</TableCell>
                <TableCell>
                  <Button appearance="subtle" size="small" onClick={() => openEdit(t)}>
                    Editar
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {!loading && tenants.length === 0 && <p>Nenhum tenant. Crie um usando o botão acima.</p>}

      <Dialog open={editOpen} onOpenChange={(_, d) => setEditOpen(d.open)}>
        <DialogSurface id={editId}>
          <DialogBody>
            <DialogTitle>Editar tenant</DialogTitle>
            <DialogContent>
              {editingTenant && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginTop: 8 }}>
                  <div>
                    <Label htmlFor="edit-name">Nome</Label>
                    <Input id="edit-name" value={editName} onChange={(_, d) => setEditName(d.value)} />
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                    <input
                      type="checkbox"
                      id="edit-active"
                      checked={editIsActive}
                      onChange={(e) => setEditIsActive(e.target.checked)}
                    />
                    <Label htmlFor="edit-active">Ativo</Label>
                  </div>
                </div>
              )}
            </DialogContent>
            <DialogActions>
              <Button appearance="secondary" onClick={() => setEditOpen(false)}>
                Cancelar
              </Button>
              <Button appearance="primary" onClick={handleUpdate} disabled={submitting}>
                Salvar
              </Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </Card>
  )
}
