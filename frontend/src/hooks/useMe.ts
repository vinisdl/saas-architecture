import { useState, useEffect } from 'react'
import { useKeycloak } from '../auth/KeycloakContext'
import { fetchMe, type MeResponse } from '../api/me'

export function useMe(): { me: MeResponse | null; loading: boolean } {
  const { token } = useKeycloak()
  const [me, setMe] = useState<MeResponse | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    fetchMe(token)
      .then((data) => {
        if (!cancelled) setMe(data)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [token])

  return { me, loading }
}
