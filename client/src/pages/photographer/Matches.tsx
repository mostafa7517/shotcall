import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import PhotographerLayout from '../../components/PhotographerLayout'

interface Match {
  id: string
  title: string
  category: string
  matchDate: string
  location: string
  acceptedCount: number
  photographersNeeded: number
  status: string
}

export default function PhotographerMatches() {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [requestingId, setRequestingId] = useState<string | null>(null)
  const [requestedIds, setRequestedIds] = useState<Set<string>>(new Set())

  async function loadMatches() {
    setLoading(true)
    try {
      const [openMatches, myRequests] = await Promise.all([
        apiFetch('/api/matches?status=Open'),
        apiFetch('/api/requests/my'),
      ])
      setMatches(openMatches)
      const activeMatchIds = myRequests
        .filter((r: any) => r.status === 'Pending' || r.status === 'Accepted')
        .map((r: any) => r.matchId)
      setRequestedIds(new Set(activeMatchIds))
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadMatches()
  }, [])

  async function handleRequest(matchId: string) {
    setRequestingId(matchId)
    try {
      await apiFetch('/api/requests', {
        method: 'POST',
        body: JSON.stringify({ matchId }),
      })
      await loadMatches()
    } catch (err) {
      alert(String(err))
    } finally {
      setRequestingId(null)
    }
  }

  return (
    <PhotographerLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>Open matches</h2>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: '#c0392b' }}>{error}</p>}

      {!loading && !error && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {matches.length === 0 && <p style={{ color: '#666' }}>No open matches right now.</p>}

          {matches.map((m) => {
            const alreadyRequested = requestedIds.has(m.id)
            return (
              <div key={m.id} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                border: '1px solid #e5e5e5',
                borderRadius: 12,
                padding: '14px 16px',
              }}>
                <div>
                  <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{m.title}</p>
                  <p style={{ margin: '4px 0 0', fontSize: 12, color: '#666' }}>
                    {new Date(m.matchDate).toLocaleString()} · {m.location} · {m.category}
                  </p>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <span style={{ fontSize: 12, color: '#666' }}>
                    {m.acceptedCount}/{m.photographersNeeded} filled
                  </span>
                  <button
                    onClick={() => handleRequest(m.id)}
                    disabled={alreadyRequested || requestingId === m.id}
                    style={{ padding: '6px 14px', fontSize: 12 }}
                  >
                    {alreadyRequested ? 'Requested' : 'Request'}
                  </button>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </PhotographerLayout>
  )
}