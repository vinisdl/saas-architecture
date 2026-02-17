const API_BASE = import.meta.env.VITE_API_URL ?? '/api'

export type RegisterRequest = {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
  acceptTerms: boolean
}

export type RegisterError = {
  error?: string
}

export async function registerUser(
  body: RegisterRequest
): Promise<{ message?: string }> {
  const res = await fetch(`${API_BASE}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  const data = await res.json().catch(() => ({}))
  if (!res.ok) {
    const err = data as RegisterError
    throw new Error(err.error ?? `Registration failed (${res.status})`)
  }
  return data as { message?: string }
}
