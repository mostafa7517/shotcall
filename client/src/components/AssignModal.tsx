import { useEffect, useState } from 'react'
import { apiFetch } from '../api/client'
import { RoleIcon } from './RoleIcon'
import { STAFF_TYPES } from '../constants/staffTypes'

interface MatchSlot {
  staffType: string
  needed: number
  accepted: number
  invited: number
}

interface AssignMatch {
  id: string
  title: string
  slots: MatchSlot[]
}

interface Candidate {
  id: string
  fullName: string
  email: string
  profilePictureUrl: string | null
  staffType: string
  reliabilityScore: number
  requestStatus: string | null
  conflicts: string[]
}

interface Props {
  match: AssignMatch
  onClose: () => void
  onDone: () => void
}

const STATUS_TEXT: Record<string, string> = {
  Pending: 'Already requested - picking them accepts the request',
  Accepted: 'Already on this match',
  Invited: 'Invitation sent, waiting for an answer',
}

// The admin picks specific people for a match. They get a notification + email and have to confirm.
export default function AssignModal({ match, onClose, onDone }: Props) {
  const [candidates, setCandidates] = useState<Candidate[]>([])
  const [selected, setSelected] = useState<Set<string>>(new Set())
  const [loading, setLoading] = useState(true)
  const [sending, setSending] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    apiFetch(`/api/matches/${match.id}/candidates`)
      .then(setCandidates)
      .catch((err) => setError(String(err)))
      .finally(() => setLoading(false))
  }, [match.id])

  function freeSpots(staffType: string): number {
    const slot = match.slots.find((s) => s.staffType === staffType)
    return slot ? Math.max(0, slot.needed - slot.accepted - slot.invited) : 0
  }

  function selectedCount(staffType: string): number {
    return candidates.filter((c) => c.staffType === staffType && selected.has(c.id)).length
  }

  function toggle(id: string) {
    setSelected((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  async function handleSend() {
    const chosen = candidates.filter((c) => selected.has(c.id))
    const withConflicts = chosen.filter((c) => c.conflicts.length > 0)

    if (withConflicts.length > 0) {
      const names = withConflicts.map((c) => c.fullName).join(', ')
      if (!confirm(`${names} already ${withConflicts.length === 1 ? 'has' : 'have'} another match around the same time. Send anyway?`)) {
        return
      }
    }

    setSending(true)
    setError(null)
    try {
      await apiFetch(`/api/matches/${match.id}/invitations`, {
        method: 'POST',
        body: JSON.stringify({ userIds: chosen.map((c) => c.id) }),
      })
      onDone()
    } catch (err) {
      setError(String(err))
    } finally {
      setSending(false)
    }
  }

  const roles = STAFF_TYPES.filter((t) => match.slots.some((s) => s.staffType === t.value))

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.4)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
    }}>
      <div className="card" style={{
        padding: '1.5rem', width: 560, maxWidth: '94vw', maxHeight: '90vh',
        display: 'flex', flexDirection: 'column',
      }}>
        <h3 style={{ margin: '0 0 4px' }}>Assign people</h3>
        <p style={{ margin: '0 0 16px', fontSize: 13, color: 'var(--text-muted)' }}>
          {match.title} - they get a notification and an email, and need to confirm.
        </p>

        <div style={{ overflowY: 'auto', flex: 1, paddingRight: 4 }}>
          {loading && <p>Loading...</p>}

          {!loading && roles.map((role) => {
            const people = candidates.filter((c) => c.staffType === role.value)
            const free = freeSpots(role.value)
            const picked = selectedCount(role.value)

            return (
              <div key={role.value} style={{ marginBottom: 18 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
                  <RoleIcon type={role.value} size={28} />
                  <strong style={{ fontSize: 14 }}>{role.label}</strong>
                  <span style={{ fontSize: 12, color: free === 0 ? 'var(--danger-text)' : 'var(--text-muted)' }}>
                    {free === 0 ? 'no free spots' : `${free - picked} free spot${free - picked === 1 ? '' : 's'} left`}
                  </span>
                </div>

                {people.length === 0 && (
                  <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
                    No active {role.label} accounts.
                  </p>
                )}

                <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                  {people.map((c) => {
                    const isSelected = selected.has(c.id)
                    const blocked = c.requestStatus === 'Accepted' || c.requestStatus === 'Invited'
                    const noRoom = !isSelected && picked >= free
                    const disabled = blocked || noRoom

                    return (
                      <label
                        key={c.id}
                        style={{
                          display: 'flex', alignItems: 'flex-start', gap: 10,
                          padding: '10px 12px', borderRadius: 8,
                          border: isSelected ? `1.5px solid ${role.color}` : '1px solid var(--border)',
                          background: isSelected ? role.bg : 'var(--surface)',
                          opacity: disabled ? 0.55 : 1,
                          cursor: disabled ? 'not-allowed' : 'pointer',
                        }}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          disabled={disabled}
                          onChange={() => toggle(c.id)}
                          style={{ width: 16, height: 16, padding: 0, margin: '2px 0 0', accentColor: role.color }}
                        />
                        <span style={{ flex: 1, minWidth: 0 }}>
                          <span style={{ display: 'block', fontSize: 13.5, fontWeight: 600 }}>{c.fullName}</span>
                          <span style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)' }}>
                            {c.email} · Reliability {c.reliabilityScore}
                          </span>
                          {c.requestStatus && (
                            <span style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginTop: 2 }}>
                              {STATUS_TEXT[c.requestStatus] ?? c.requestStatus}
                            </span>
                          )}
                          {c.conflicts.length > 0 && (
                            <span style={{ display: 'block', fontSize: 12, color: 'var(--warning-text)', marginTop: 2 }}>
                              ⚠ Also booked around the same time: {c.conflicts.join(', ')}
                            </span>
                          )}
                        </span>
                      </label>
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>

        {error && <p style={{ color: 'var(--danger-text)', fontSize: 13, margin: '8px 0 0' }}>{error}</p>}

        <div style={{ display: 'flex', gap: 8, marginTop: 14 }}>
          <button
            className="primary"
            onClick={handleSend}
            disabled={sending || selected.size === 0}
            style={{ flex: 1 }}
          >
            {sending ? 'Sending...' : `Send invitation${selected.size > 0 ? ` (${selected.size})` : ''}`}
          </button>
          <button onClick={onClose}>Cancel</button>
        </div>
      </div>
    </div>
  )
}
