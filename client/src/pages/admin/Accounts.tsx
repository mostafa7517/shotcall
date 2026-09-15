import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'

interface Account {
  id: string
  fullName: string
  email: string
  phoneNumber: string | null
  role: string
  accountStatus: string
  reliabilityScore: number
  createdAt: string
}

const statusColors: Record<string, { bg: string; text: string }> = {
  PendingApproval: { bg: '#fff4e0', text: '#c67c00' },
  Active: { bg: '#e8f6ee', text: '#1e8e5a' },
  Rejected: { bg: '#fdeaea', text: '#c0392b' },
  Disabled: { bg: '#eee', text: '#666' },
}

export default function Accounts() {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [filter, setFilter] = useState('PendingApproval')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actioningId, setActioningId] = useState<string | null>(null)

  async function loadAccounts() {
    setLoading(true)
    setError(null)
    try {
      const data = await apiFetch(`/api/accounts?status=${filter}`)
      setAccounts(data)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadAccounts()
  }, [filter])

  async function handleAction(id: string, action: 'approve' | 'reject' | 'disable' | 'enable') {
    setActioningId(id)
    try {
      await apiFetch(`/api/accounts/${id}/${action}`, { method: 'PUT' })
      await loadAccounts()
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  return (
    <AdminLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>Accounts</h2>

      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        {['PendingApproval', 'Active', 'Rejected', 'Disabled'].map((s) => (
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
          {accounts.length === 0 && <p style={{ color: '#666' }}>No accounts with this status.</p>}

          {accounts.map((a) => {
            const colors = statusColors[a.accountStatus] || statusColors.Disabled
            return (
              <div key={a.id} style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                border: '1px solid #e5e5e5',
                borderRadius: 12,
                padding: '14px 16px',
              }}>
                <div>
                  <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{a.fullName}</p>
                  <p style={{ margin: '4px 0 0', fontSize: 12, color: '#666' }}>
                    {a.email} · Reliability {a.reliabilityScore} · joined {new Date(a.createdAt).toLocaleDateString()}
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
                    {a.accountStatus}
                  </span>

                  {a.accountStatus === 'PendingApproval' && (
                    <>
                      <button onClick={() => handleAction(a.id, 'approve')} disabled={actioningId === a.id} style={{ padding: '5px 12px', fontSize: 12 }}>
                        Approve
                      </button>
                      <button onClick={() => handleAction(a.id, 'reject')} disabled={actioningId === a.id} style={{ padding: '5px 12px', fontSize: 12 }}>
                        Reject
                      </button>
                    </>
                  )}

                  {a.accountStatus === 'Active' && (
                    <button onClick={() => handleAction(a.id, 'disable')} disabled={actioningId === a.id} style={{ padding: '5px 12px', fontSize: 12 }}>
                      Disable
                    </button>
                  )}

                  {a.accountStatus === 'Disabled' && (
                    <button onClick={() => handleAction(a.id, 'enable')} disabled={actioningId === a.id} style={{ padding: '5px 12px', fontSize: 12 }}>
                      Enable
                    </button>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </AdminLayout>
  )
}