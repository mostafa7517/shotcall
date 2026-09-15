import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'

interface MatchRequest {
  id: string
  matchId: string
  matchTitle: string
  photographerId: string
  photographerName: string
  status: string
  requestedAt: string
}

export default function Requests() {
  const [requests, setRequests] = useState<MatchRequest[]>([])
  const [filter, setFilter] = useState('Pending')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actioningId, setActioningId] = useState<string | null>(null)

  async function loadRequests() {
    setLoading(true)
    setError(null)
    try {
      const data = await apiFetch(`/api/requests?status=${filter}`)
      setRequests(data)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRequests()
  }, [filter])

  async function handleAction(id: string, action: 'accept' | 'reject') {
    setActioningId(id)
    try {
      await apiFetch(`/api/requests/${id}/${action}`, { method: 'PUT' })
      await loadRequests()
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  return (
    <AdminLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>Requests</h2>

      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        {['Pending', 'Accepted', 'Rejected'].map((s) => (
          <button
            key={s}
            onClick={() => setFilter(s)}
            style={{
              padding: '6px 14px',
              fontSize: 13,
              background: filter === s ? '#111' : '#fff',
              color: filter === s ? '#fff' : '#111',
              border: '1px solid #ccc',
              borderRadius: 6,
            }}
          >
            {s}
          </button>
        ))}
      </div>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: '#c0392b' }}>{error}</p>}

      {!loading && !error && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {requests.length === 0 && <p style={{ color: '#666' }}>No {filter.toLowerCase()} requests.</p>}

          {requests.map((r) => (
            <div key={r.id} style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              border: '1px solid #e5e5e5',
              borderRadius: 12,
              padding: '14px 16px',
            }}>
              <div>
                <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{r.photographerName}</p>
                <p style={{ margin: '4px 0 0', fontSize: 12, color: '#666' }}>
                  {r.matchTitle} · requested {new Date(r.requestedAt).toLocaleDateString()}
                </p>
              </div>

              {filter === 'Pending' ? (
                <div style={{ display: 'flex', gap: 6 }}>
                  <button
                    onClick={() => handleAction(r.id, 'accept')}
                    disabled={actioningId === r.id}
                    style={{ padding: '5px 12px', fontSize: 12 }}
                  >
                    Accept
                  </button>
                  <button
                    onClick={() => handleAction(r.id, 'reject')}
                    disabled={actioningId === r.id}
                    style={{ padding: '5px 12px', fontSize: 12 }}
                  >
                    Reject
                  </button>
                </div>
              ) : (
                <span style={{
                  fontSize: 12,
                  padding: '4px 10px',
                  borderRadius: 6,
                  background: r.status === 'Accepted' ? '#e8f6ee' : '#fdeaea',
                  color: r.status === 'Accepted' ? '#1e8e5a' : '#c0392b',
                }}>
                  {r.status}
                </span>
              )}
            </div>
          ))}
        </div>
      )}
    </AdminLayout>
  )
}