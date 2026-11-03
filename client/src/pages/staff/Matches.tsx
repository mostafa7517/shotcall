import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import { useAuth } from '../../context/AuthContext'
import StaffLayout from '../../components/StaffLayout'
import { RoleIcon } from '../../components/RoleIcon'

interface MatchSlot {
  staffType: string
  needed: number
  accepted: number
  invited: number
}

interface Match {
  id: string
  title: string
  isEvent?: boolean
  hostName?: string | null
  category: string
  matchDate: string
  location: string
  slots: MatchSlot[]
  status: string
}

export default function StaffMatches() {
  const { user } = useAuth()
  const myType = user?.staffType ?? null

  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [requestedIds, setRequestedIds] = useState<Set<string>>(new Set())
  const [invitedIds, setInvitedIds] = useState<Set<string>>(new Set())
  const [waitingIds, setWaitingIds] = useState<Set<string>>(new Set())

  async function loadMatches() {
    if (!myType || !user) {
      setLoading(false)
      return
    }

    setLoading(true)
    try {
      // Only matches that actually need my role
      const [openMatches, myRequests]: [Match[], any[]] = await Promise.all([
        apiFetch(`/api/matches?status=Open&staffType=${myType}`),
        apiFetch('/api/requests/my'),
      ])
      setMatches(openMatches)

      const activeMatchIds = myRequests
        .filter((r) => r.status === 'Pending' || r.status === 'Accepted' || r.status === 'Invited')
        .map((r) => r.matchId)
      setRequestedIds(new Set(activeMatchIds))
      setInvitedIds(new Set(myRequests.filter((r) => r.status === 'Invited').map((r) => r.matchId)))

      // For matches that are full for my role, check whether I'm already waiting
      const fullForMe = openMatches.filter((m) => isFullFor(m, myType))
      const lists = await Promise.all(
        fullForMe.map((m) => apiFetch(`/api/matches/${m.id}/waitinglist`).catch(() => []))
      )
      const waiting = new Set<string>()
      fullForMe.forEach((m, i) => {
        if (lists[i].some((w: any) => w.photographerId === user.userId)) waiting.add(m.id)
      })
      setWaitingIds(waiting)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadMatches()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [myType])

  async function handleRequest(matchId: string) {
    setBusyId(matchId)
    try {
      await apiFetch('/api/requests', {
        method: 'POST',
        body: JSON.stringify({ matchId }),
      })
      await loadMatches()
    } catch (err) {
      alert(String(err))
    } finally {
      setBusyId(null)
    }
  }

  async function handleJoinWaitingList(matchId: string) {
    setBusyId(matchId)
    try {
      const entry = await apiFetch(`/api/matches/${matchId}/waitinglist`, { method: 'POST' })
      alert(`You are number ${entry.position} on the waiting list.`)
      await loadMatches()
    } catch (err) {
      alert(String(err))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <StaffLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>Open matches</h2>

      {!myType && (
        <p style={{ color: 'var(--text-muted)' }}>Choose your role in your profile to see matches for you.</p>
      )}
      {loading && myType && <p>Loading...</p>}
      {error && <p style={{ color: '#c0392b' }}>{error}</p>}

      {!loading && !error && myType && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          {matches.length === 0 && <p style={{ color: '#666' }}>No open matches for your role right now.</p>}

          {matches.map((m) => {
            const alreadyRequested = requestedIds.has(m.id)
            const full = isFullFor(m, myType)
            const waiting = waitingIds.has(m.id)
            return (
              <div key={m.id} className="card" style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                gap: 12,
                padding: '14px 16px',
              }}>
                <div>
                  <p style={{ margin: 0, fontSize: 14, fontWeight: 500 }}>{m.title}</p>
                  <p style={{ margin: '4px 0 0', fontSize: 12, color: '#666' }}>
                    {new Date(m.matchDate).toLocaleString()} · {m.location} · {m.category}{m.isEvent ? ' · Event' : ''}{m.isEvent && m.hostName ? ` · MC: ${m.hostName}` : ''}
                  </p>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
                  <div style={{ display: 'flex', gap: 10 }}>
                    {m.slots.map((s) => (
                      <span
                        key={s.staffType}
                        title={s.staffType === myType ? 'Your role' : undefined}
                        style={{
                          display: 'inline-flex', alignItems: 'center', gap: 4, fontSize: 12,
                          color: s.staffType === myType ? 'var(--text)' : 'var(--text-muted)',
                          fontWeight: s.staffType === myType ? 600 : 400,
                        }}
                      >
                        <RoleIcon type={s.staffType} size={22} />
                        {s.accepted}/{s.needed}
                      </span>
                    ))}
                  </div>

                  {alreadyRequested ? (
                    <button disabled style={{ padding: '6px 14px', fontSize: 12 }}>
                      {invitedIds.has(m.id) ? 'Invited - see My requests' : 'Requested'}
                    </button>
                  ) : full ? (
                    <button
                      onClick={() => handleJoinWaitingList(m.id)}
                      disabled={waiting || busyId === m.id}
                      style={{ padding: '6px 14px', fontSize: 12 }}
                    >
                      {waiting ? 'On waiting list' : 'Join waiting list'}
                    </button>
                  ) : (
                    <button
                      className="primary"
                      onClick={() => handleRequest(m.id)}
                      disabled={busyId === m.id}
                      style={{ padding: '6px 14px', fontSize: 12 }}
                    >
                      Request
                    </button>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </StaffLayout>
  )
}

function isFullFor(match: Match, staffType: string): boolean {
  const slot = match.slots.find((s) => s.staffType === staffType)
  // Spots held by pending invitations count as taken
  return !slot || slot.accepted + slot.invited >= slot.needed
}
