import { useEffect, useState } from "react"
import { apiFetch } from "../../api/client"
import AdminLayout from "../../components/AdminLayout"

interface Match {
  id: string
  title: string
  teamA: string | null
  teamB: string | null
  category: string
  matchDate: string
  location: string
  description: string | null
  photographersNeeded: number
  acceptedCount: number
  requestDeadline: string | null
  status: string
}

export default function Matches() {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingMatch, setEditingMatch] = useState<Match | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)

  async function loadMatches() {
    setLoading(true)
    try {
      const data = await apiFetch("/api/matches")
      setMatches(data)
    } catch (err) {
      setError(String(err))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadMatches()
  }, [])

  async function handleDelete(id: string) {
    if (!confirm("Delete this match? This cannot be undone.")) return
    setDeletingId(id)
    try {
      await apiFetch(`/api/matches/${id}`, { method: "DELETE" })
      await loadMatches()
    } catch (err) {
      alert(String(err))
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <AdminLayout>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 20 }}>
        <h2 style={{ margin: 0 }}>Matches</h2>
        <button className="primary" onClick={() => setShowForm(true)}>+ New match</button>
      </div>

      {loading && <p>Loading...</p>}
      {error && <p style={{ color: "var(--danger-text)" }}>{error}</p>}

      {!loading && !error && (
        <div className="card">
          <div style={{
            display: "grid",
            gridTemplateColumns: "2fr 1fr 1fr 1fr 1fr 1.2fr",
            padding: "10px 16px",
            fontSize: 12,
            color: "var(--text-muted)",
            borderBottom: "1px solid var(--border)",
          }}>
            <span>Match</span><span>Date</span><span>Category</span><span>Slots</span><span>Status</span><span></span>
          </div>

          {matches.length === 0 && (
            <div style={{ padding: 20, color: "var(--text-muted)", fontSize: 14 }}>No matches yet.</div>
          )}

          {matches.map((m) => (
            <div key={m.id} className="row-hover" style={{
              display: "grid",
              gridTemplateColumns: "2fr 1fr 1fr 1fr 1fr 1.2fr",
              padding: "12px 16px",
              alignItems: "center",
              borderBottom: "1px solid #f0f0f0",
            }}>
              <div>
                <p style={{ margin: 0, fontSize: 13, fontWeight: 500 }}>{m.title}</p>
                <p style={{ margin: "2px 0 0", fontSize: 12, color: "var(--text-muted)" }}>{m.location}</p>
              </div>
              <span style={{ fontSize: 13 }}>{new Date(m.matchDate).toLocaleDateString()}</span>
              <span style={{ fontSize: 13 }}>{m.category}</span>
              <span style={{ fontSize: 13 }}>{m.acceptedCount}/{m.photographersNeeded}</span>
              <span className={m.status === "Open" ? "badge badge-success" : "badge badge-danger"}>
                {m.status}
              </span>
              <div style={{ display: "flex", gap: 6, justifyContent: "flex-end" }}>
                <button onClick={() => setEditingMatch(m)} style={{ padding: "4px 10px", fontSize: 12 }}>Edit</button>
                <button
                  onClick={() => handleDelete(m.id)}
                  disabled={deletingId === m.id}
                  className="danger-text"
                  style={{ padding: "4px 10px", fontSize: 12 }}
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <MatchFormModal
          onClose={() => setShowForm(false)}
          onSaved={() => {
            setShowForm(false)
            loadMatches()
          }}
        />
      )}

      {editingMatch && (
        <MatchFormModal
          existing={editingMatch}
          onClose={() => setEditingMatch(null)}
          onSaved={() => {
            setEditingMatch(null)
            loadMatches()
          }}
        />
      )}
    </AdminLayout>
  )
}

function MatchFormModal({
  existing,
  onClose,
  onSaved,
}: {
  existing?: Match
  onClose: () => void
  onSaved: () => void
}) {
  const isEditing = !!existing

  const [title, setTitle] = useState(existing?.title || "")
  const [teamA, setTeamA] = useState(existing?.teamA || "")
  const [teamB, setTeamB] = useState(existing?.teamB || "")
  const [category, setCategory] = useState(existing?.category || "Football")
  const [matchDate, setMatchDate] = useState(
    existing ? new Date(existing.matchDate).toISOString().slice(0, 16) : ""
  )
  const [location, setLocation] = useState(existing?.location || "")
  const [description, setDescription] = useState(existing?.description || "")
  const [photographersNeeded, setPhotographersNeeded] = useState(existing?.photographersNeeded || 1)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const payload = {
        title,
        teamA: teamA || null,
        teamB: teamB || null,
        category,
        matchDate: new Date(matchDate).toISOString(),
        location,
        description: description || null,
        photographersNeeded,
      }

      if (isEditing) {
        await apiFetch(`/api/matches/${existing!.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        })
      } else {
        await apiFetch("/api/matches", {
          method: "POST",
          body: JSON.stringify(payload),
        })
      }
      onSaved()
    } catch (err) {
      setError(String(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div style={{
      position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)",
      display: "flex", alignItems: "center", justifyContent: "center", zIndex: 50,
    }}>
      <form onSubmit={handleSubmit} className="card" style={{
        padding: "1.5rem", width: 420,
        display: "flex", flexDirection: "column", gap: 10,
      }}>
        <h3 style={{ margin: "0 0 8px" }}>{isEditing ? "Edit match" : "New match"}</h3>

        <input placeholder="Title" value={title} onChange={(e) => setTitle(e.target.value)} required />
        <div style={{ display: "flex", gap: 8 }}>
          <input placeholder="Team A (optional)" value={teamA} onChange={(e) => setTeamA(e.target.value)} style={{ flex: 1 }} />
          <input placeholder="Team B (optional)" value={teamB} onChange={(e) => setTeamB(e.target.value)} style={{ flex: 1 }} />
        </div>
        <select value={category} onChange={(e) => setCategory(e.target.value)}>
          <option>Football</option>
          <option>Padel</option>
          <option>Beach Football</option>
        </select>
        <input
          type="datetime-local"
          value={matchDate}
          onChange={(e) => setMatchDate(e.target.value)}
          required
        />
        <input placeholder="Location" value={location} onChange={(e) => setLocation(e.target.value)} required />
        <textarea placeholder="Description (optional)" value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
        <label style={{ fontSize: 13, color: "var(--text-muted)" }}>
          Photographers needed
          <input
            type="number"
            min={1}
            value={photographersNeeded}
            onChange={(e) => setPhotographersNeeded(Number(e.target.value))}
            style={{ display: "block", marginTop: 4, width: 80 }}
          />
        </label>

        {error && <p style={{ color: "var(--danger-text)", fontSize: 13 }}>{error}</p>}

        <div style={{ display: "flex", gap: 8, marginTop: 8 }}>
          <button type="submit" className="primary" disabled={submitting} style={{ flex: 1 }}>
            {submitting ? "Saving..." : isEditing ? "Save changes" : "Create match"}
          </button>
          <button type="button" onClick={onClose}>Cancel</button>
        </div>
      </form>
    </div>
  )
}
