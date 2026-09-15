import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import PhotographerLayout from '../../components/PhotographerLayout'

interface MatchRequest {
  id: string
  matchId: string
  matchTitle: string
  status: string
  requestedAt: string
}

const statusColors: Record<string, { bg: string; text: string }> = {
  Pending: { bg: '#fff4e0', text: '#c67c00' },
  Accepted: { bg: '#e8f6ee', text: '#1e8e5a' },
  Rejected: { bg: '#fdeaea', text: '#c0392b' },
  Withdrawn: { bg: '#eee', text: '#666' },
}

export default function MyRequests() {
  const [requests, setRequests] = useState<MatchRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actioningId, setActioningId] = useState<string | null>(null)

  async function loadRequests() {
    setLoading(true)
    try {
      const data = await apiFetch('/api/requests/my')
      setRequests(data)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRequests()
  }, [])

  async function handleWithdraw(id: string) {
    setActioningId(id)
    try {
      await apiFetch(`/api/requests/${id}/withdraw`, { method: 'PUT' })
      await loadRequests()
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  async function handleCancellationRequest(matchRequestId: string) {
    const reason = prompt('Reason for cancelling (optional):') || undefined
    setActioningId(matchRequestId)
    try {
      await apiFetch('/api/cancellations', {
        method: 'POST',
        body: JSON.stringify({ matchRequestId, reason }),
      })
      alert('Cancellation request submitted. Waiting for admin decision.')
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  return (
    <PhotographerLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>My requests</h2>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: '#c0392b' }}>{error}</p>}

      {!loading && !error && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {requests.length === 0 && <p style={{ color: '#666' }}>You haven't requested any matches yet.</p>}

          {requests.map((r) => {
            const colors = statusColors[r.status] || statusColors.Withdrawn
            return (
              <div key={r.id} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                border: '1px solid #e5e5e5',
                borderRadius: 12,
                padding: '14px 16px',
              }}>
                <div>
                  <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{r.matchTitle}</p>
                  <p style={{ margin: '4px 0 0', fontSize: 12, color: '#666' }}>
                    requested {new Date(r.requestedAt).toLocaleDateString()}
                  </p>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <span style={{
                    fontSize: 12,
                    padding: '4px 10px',
                    borderRadius: 6,
                    background: colors.bg,
                    color: colors.text,
                  }}>
                    {r.status}
                  </span>

                  {r.status === 'Pending' && (
                    <button
                      onClick={() => handleWithdraw(r.id)}
                      disabled={actioningId === r.id}
                      style={{ padding: '5px 12px', fontSize: 12 }}
                    >
                      Withdraw
                    </button>
                  )}

                  {r.status === 'Accepted' && (
                    <button
                      onClick={() => handleCancellationRequest(r.id)}
                      disabled={actioningId === r.id}
                      style={{ padding: '5px 12px', fontSize: 12, color: '#c0392b' }}
                    >
                      Request cancellation
                    </button>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </PhotographerLayout>
  )
}