import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'
import { RoleBadge } from '../../components/RoleIcon'
import { STAFF_TYPES } from '../../constants/staffTypes'

interface Account {
  id: string
  fullName: string
  email: string
  phoneNumber: string | null
  role: string
  staffType: string | null
  accountStatus: string
  reliabilityScore: number
  createdAt: string
}

const statusColors: Record<string, { bg: string; text: string }> = {
  PendingApproval: { bg: 'var(--warning-bg)', text: 'var(--warning-text)' },
  Active: { bg: 'var(--success-bg)', text: 'var(--success-text)' },
  Rejected: { bg: 'var(--danger-bg)', text: 'var(--danger-text)' },
  Disabled: { bg: '#eee', text: 'var(--text-muted)' },
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

  async function handlePromote(id: string) {
    if (!confirm('Promote this person to Admin? This cannot be undone from here.')) return
    setActioningId(id)
    try {
      await apiFetch(`/api/accounts/${id}/promote`, { method: 'PUT' })
      await loadAccounts()
    } catch (err) {
      alert(String(err))
    } finally {
      setActioningId(null)
    }
  }

  async function handleSetStaffType(id: string, staffType: string) {
    if (!staffType) return
    setActioningId(id)
    try {
      await apiFetch(`/api/accounts/${id}/staff-type`, {
        method: 'PUT',
        body: JSON.stringify({ staffType }),
      })
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
            className={filter === s ? 'tab-btn active' : 'tab-btn'}
          >
            {s}
          </button>
        ))}
      </div>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: 'var(--danger-text)' }}>{error}</p>}

      {!loading && !error && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {accounts.length === 0 && <p style={{ color: 'var(--text-muted)' }}>No accounts with this status.</p>}

          {accounts.map((a) => {
            const colors = statusColors[a.accountStatus] || statusColors.Disabled
            const busy = actioningId === a.id
            return (
              <div key={a.id} className="card row-hover" style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                padding: '14px 16px',
                gap: 12,
              }}>
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{a.fullName}</p>
                    <RoleBadge type={a.staffType} />
                  </div>
                  <p style={{ margin: '4px 0 0', fontSize: 12, color: 'var(--text-muted)' }}>
                    {a.email} · Reliability {a.reliabilityScore} · joined {new Date(a.createdAt).toLocaleDateString()}
                  </p>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap', justifyContent: 'flex-end' }}>
                  <select
                    value={a.staffType ?? ''}
                    onChange={(e) => handleSetStaffType(a.id, e.target.value)}
                    disabled={busy}
                    title="Change this person's role"
                    style={{ padding: '4px 8px', fontSize: 12 }}
                  >
                    {!a.staffType && <option value="">Set role…</option>}
                    {STAFF_TYPES.map((t) => (
                      <option key={t.value} value={t.value}>{t.label}</option>
                    ))}
                  </select>

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
                      <button
                        onClick={() => handleAction(a.id, 'approve')}
                        disabled={busy || !a.staffType}
                        title={a.staffType ? undefined : 'Set a role first'}
                        style={{ padding: '5px 12px', fontSize: 12 }}
                      >
                        Approve
                      </button>
                      <button onClick={() => handleAction(a.id, 'reject')} disabled={busy} style={{ padding: '5px 12px', fontSize: 12 }}>
                        Reject
                      </button>
                    </>
                  )}

                  {a.accountStatus === 'Active' && (
                    <>
                      <button onClick={() => handleAction(a.id, 'disable')} disabled={busy} style={{ padding: '5px 12px', fontSize: 12 }}>
                        Disable
                      </button>
                      <button onClick={() => handlePromote(a.id)} disabled={busy} className="primary" style={{ padding: '5px 12px', fontSize: 12 }}>
                        Promote to Admin
                      </button>
                    </>
                  )}

                  {a.accountStatus === 'Disabled' && (
                    <button onClick={() => handleAction(a.id, 'enable')} disabled={busy} style={{ padding: '5px 12px', fontSize: 12 }}>
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
