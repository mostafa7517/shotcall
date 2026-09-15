import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'

interface CancellationRequest {
  id: string
  matchRequestId: string
  matchTitle: string
  photographerName: string
  reason: string | null
  isLateCancellation: boolean
  status: string
  createdAt: string
}

export default function Cancellations() {
  const [items, setItems] = useState<CancellationRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actioningId, setActioningId] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    try {
      const data = await apiFetch('/api/cancellations')
      setItems(data)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleAction(id: string, action: 'approve' | 'reject') {
    setActioningId(id)
    try {
      await apiFetch(`/api/cancellations/${id}/${action}`, { method: 'PUT' })
      await load()
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  return (
    <AdminLayout>
      <h2 style={{ marginBottom: 20 }}>Cancellation requests</h2>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: 'var(--danger-text)' }}>{error}</p>}

      {!loading && !error && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {items.length === 0 && <p style={{ color: 'var(--text-muted)' }}>No pending cancellation requests.</p>}

          {items.map((c) => (
            <div key={c.id} className="card row-hover" style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              padding: '14px 16px',
            }}>
              <div>
                <p style={{ margin: 0, fontSize: 14, fontWeight: 600 }}>
                  {c.photographerName} → {c.matchTitle}
                </p>
                <p style={{ margin: '4px 0 0', fontSize: 12, color: 'var(--text-muted)' }}>
                  {c.reason ? `"${c.reason}"` : 'No reason given'} · requested {new Date(c.createdAt).toLocaleDateString()}
                </p>
                {c.isLateCancellation && (
                  <span className="badge badge-warning" style={{ marginTop: 6, display: 'inline-block' }}>
                    Late cancellation — violation will be logged
                  </span>
                )}
              </div>

              <div style={{ display: 'flex', gap: 6 }}>
                <button
                  className="primary"
                  onClick={() => handleAction(c.id, 'approve')}
                  disabled={actioningId === c.id}
                >
                  Approve
                </button>
                <button
                  onClick={() => handleAction(c.id, 'reject')}
                  disabled={actioningId === c.id}
                >
                  Reject
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </AdminLayout>
  )
}