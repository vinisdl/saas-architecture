const API_BASE = import.meta.env.VITE_API_URL ?? '/api'

export type MeResponse = {
  sub: string
  isAdmin: boolean
}

export async function fetchMe(token: string | undefined): Promise<MeResponse | null> {
  if (!token) return null
  const res = await fetch(`${API_BASE}/me`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (!res.ok) return null
  return res.json()
}
