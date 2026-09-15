import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'

interface Match {
  id: string
  title: string
  matchDate: string
  location: string
  acceptedCount: number
  photographersNeeded: number
  status: string
}

interface MatchRequest {
  id: string
  matchTitle: string
  photographerName: string
  status: string
}

export default function Dashboard() {
  const [openMatchesCount, setOpenMatchesCount] = useState(0)
  const [pendingRequests, setPendingRequests] = useState<MatchRequest[]>([])
  const [pendingAccountsCount, setPendingAccountsCount] = useState(0)
  const [activePhotographersCount, setActivePhotographersCount] = useState(0)
  const [upcomingMatches, setUpcomingMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadDashboard() {
      try {
        const [openMatches, allMatches, requests, pendingAccounts, activeAccounts] = await Promise.all([
          apiFetch('/api/matches?status=Open'),
          apiFetch('/api/matches'),
          apiFetch('/api/requests?status=Pending'),
          apiFetch('/api/accounts?status=PendingApproval'),
          apiFetch('/api/accounts?status=Active'),
        ])

        setOpenMatchesCount(openMatches.length)
        setPendingRequests(requests)
        setPendingAccountsCount(pendingAccounts.length)
        setActivePhotographersCount(activeAccounts.length)

        const upcoming = allMatches
          .filter((m: Match) => new Date(m.matchDate) >= new Date())
          .sort((a: Match, b: Match) => new Date(a.matchDate).getTime() - new Date(b.matchDate).getTime())
          .slice(0, 5)
        setUpcomingMatches(upcoming)
      } catch (err) {
        setError(String(err))
      } finally {
        setLoading(false)
      }
    }

    loadDashboard()
  }, [])

  if (loading) return <AdminLayout><p>Loading...</p></AdminLayout>
  if (error) return <AdminLayout><p style={{ color: 'red' }}>{error}</p></AdminLayout>

  return (
    <AdminLayout>
      <h2 style={{ marginTop: 0, marginBottom: 20 }}>Dashboard</h2>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 12, marginBottom: 24 }}>
        <StatCard label="Open matches" value={openMatchesCount} />
        <StatCard label="Pending requests" value={pendingRequests.length} color="#c67c00" />
        <StatCard label="Pending accounts" value={pendingAccountsCount} color="#2563eb" />
        <StatCard label="Active photographers" value={activePhotographersCount} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1.3fr 1fr', gap: 16 }}>
        <div style={{ border: '1px solid #e5e5e5', borderRadius: 12, padding: '1rem 1.25rem' }}>
          <h3 style={{ marginTop: 0 }}>Requests awaiting review</h3>
          {pendingRequests.length === 0 && <p style={{ color: '#666', fontSize: 13 }}>Nothing pending.</p>}
          {pendingRequests.map((r) => (
            <div key={r.id} style={{ padding: '10px 0', borderTop: '1px solid #eee', fontSize: 13 }}>
              <strong>{r.photographerName}</strong> — {r.matchTitle}
            </div>
          ))}
        </div>

        <div style={{ border: '1px solid #e5e5e5', borderRadius: 12, padding: '1rem 1.25rem' }}>
          <h3 style={{ marginTop: 0 }}>Upcoming matches</h3>
          {upcomingMatches.length === 0 && <p style={{ color: '#666', fontSize: 13 }}>No upcoming matches.</p>}
          {upcomingMatches.map((m) => (
            <div key={m.id} style={{ padding: '10px 0', borderTop: '1px solid #eee', fontSize: 13 }}>
              <strong>{m.title}</strong>
              <div style={{ color: '#666' }}>
                {new Date(m.matchDate).toLocaleDateString()} · {m.acceptedCount}/{m.photographersNeeded} assigned
              </div>
            </div>
          ))}
        </div>
      </div>
    </AdminLayout>
  )
}

function StatCard({ label, value, color }: { label: string; value: number; color?: string }) {
  return (
    <div style={{ border: '1px solid #e5e5e5', borderRadius: 8, padding: '1rem' }}>
      <p style={{ fontSize: 13, color: '#666', margin: '0 0 6px' }}>{label}</p>
      <p style={{ fontSize: 24, fontWeight: 600, margin: 0, color: color || '#111' }}>{value}</p>
    </div>
  )
}