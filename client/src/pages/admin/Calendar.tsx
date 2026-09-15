import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import AdminLayout from '../../components/AdminLayout'

interface Match {
  id: string
  title: string
  matchDate: string
  acceptedCount: number
  photographersNeeded: number
  status: string
}

function getMonthGrid(year: number, month: number): (Date | null)[][] {
  const firstDay = new Date(year, month, 1)
  const startWeekday = firstDay.getDay() // 0 = Sunday
  const daysInMonth = new Date(year, month + 1, 0).getDate()

  const cells: (Date | null)[] = []
  for (let i = 0; i < startWeekday; i++) cells.push(null)
  for (let d = 1; d <= daysInMonth; d++) cells.push(new Date(year, month, d))
  while (cells.length % 7 !== 0) cells.push(null)

  const weeks: (Date | null)[][] = []
  for (let i = 0; i < cells.length; i += 7) weeks.push(cells.slice(i, i + 7))
  return weeks
}

export default function Calendar() {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [cursor, setCursor] = useState(() => {
    const now = new Date()
    return new Date(now.getFullYear(), now.getMonth(), 1)
  })

  useEffect(() => {
    apiFetch('/api/matches').then(setMatches).finally(() => setLoading(false))
  }, [])

  const year = cursor.getFullYear()
  const month = cursor.getMonth()
  const weeks = getMonthGrid(year, month)

  const matchesByDay = new Map<string, Match[]>()
  matches.forEach((m) => {
    const key = new Date(m.matchDate).toDateString()
    if (!matchesByDay.has(key)) matchesByDay.set(key, [])
    matchesByDay.get(key)!.push(m)
  })

  const today = new Date()

  return (
    <AdminLayout>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Calendar</h2>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <button onClick={() => setCursor(new Date(year, month - 1, 1))}>←</button>
          <span style={{ fontWeight: 600, minWidth: 140, textAlign: 'center' }}>
            {cursor.toLocaleString('en-US', { month: 'long', year: 'numeric' })}
          </span>
          <button onClick={() => setCursor(new Date(year, month + 1, 1))}>→</button>
        </div>
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : (
        <div className="card" style={{ padding: 12 }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 4, marginBottom: 4 }}>
            {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map((d) => (
              <div key={d} style={{ textAlign: 'center', fontSize: 12, color: 'var(--text-muted)', padding: 6 }}>{d}</div>
            ))}
          </div>

          {weeks.map((week, wi) => (
            <div key={wi} style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 4, marginBottom: 4 }}>
              {week.map((day, di) => {
                const dayMatches = day ? matchesByDay.get(day.toDateString()) || [] : []
                const isToday = day && day.toDateString() === today.toDateString()
                return (
                  <div key={di} style={{
                    minHeight: 90,
                    border: '1px solid var(--border)',
                    borderRadius: 8,
                    padding: 6,
                    background: isToday ? 'var(--accent-soft)' : 'transparent',
                    opacity: day ? 1 : 0.3,
                  }}>
                    {day && (
                      <>
                        <div style={{ fontSize: 12, fontWeight: isToday ? 700 : 500, marginBottom: 4 }}>
                          {day.getDate()}
                        </div>
                        {dayMatches.map((m) => (
                          <div key={m.id} style={{
                            fontSize: 10.5,
                            padding: '2px 5px',
                            borderRadius: 4,
                            marginBottom: 2,
                            background: m.status === 'Open' ? 'var(--success-bg)' : 'var(--danger-bg)',
                            color: m.status === 'Open' ? 'var(--success-text)' : 'var(--danger-text)',
                            whiteSpace: 'nowrap',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                          }}>
                            {m.title}
                          </div>
                        ))}
                      </>
                    )}
                  </div>
                )
              })}
            </div>
          ))}
        </div>
      )}
    </AdminLayout>
  )
}