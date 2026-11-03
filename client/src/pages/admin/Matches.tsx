import { useEffect, useState } from "react"
import { apiFetch } from "../../api/client"
import AdminLayout from "../../components/AdminLayout"
import { RoleIcon } from "../../components/RoleIcon"
import AssignModal from "../../components/AssignModal"
import { STAFF_TYPES } from "../../constants/staffTypes"

interface MatchSlot {
  staffType: string
  needed: number
  accepted: number
  invited: number
}

interface Match {
  id: string
  title: string
  teamA: string | null
  teamB: string | null
  isEvent: boolean
  hostName: string | null
  category: string
  matchDate: string
  location: string
  description: string | null
  slots: MatchSlot[]
  neededTotal: number
  acceptedTotal: number
  requestDeadline: string | null
  status: string
}

export default function Matches() {
  const [matches, setMatches] = useState<Match[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [editingMatch, setEditingMatch] = useState<Match | null>(null)
  const [assigningMatch, setAssigningMatch] = useState<Match | null>(null)
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

  // Categories already used by matches, so a custom one only has to be typed once
  const knownCategories = Array.from(new Set(matches.map((m) => m.category).filter(Boolean)))

  const columns = "2fr 1fr 1fr 1.8fr 1fr 2fr"

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
            gridTemplateColumns: columns,
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
              gridTemplateColumns: columns,
              padding: "12px 16px",
              alignItems: "center",
              borderBottom: "1px solid #f0f0f0",
            }}>
              <div>
                <p style={{ margin: 0, fontSize: 13, fontWeight: 500 }}>
                  {m.title}
                  {m.isEvent && (
                    <span style={{ marginLeft: 8, fontSize: 11, padding: "1px 6px", borderRadius: 4, background: "#eef2ff", color: "#3730a3" }}>
                      Event
                    </span>
                  )}
                </p>
                <p style={{ margin: "2px 0 0", fontSize: 12, color: "var(--text-muted)" }}>
                  {m.location}{m.isEvent && m.hostName ? ` · MC: ${m.hostName}` : ""}
                </p>
              </div>
              <span style={{ fontSize: 13 }}>{new Date(m.matchDate).toLocaleDateString()}</span>
              <span style={{ fontSize: 13 }}>{m.category}</span>
              <div style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
                {m.slots.map((s) => (
                  <span key={s.staffType} style={{ display: "inline-flex", alignItems: "center", gap: 4, fontSize: 13 }}>
                    <RoleIcon type={s.staffType} size={22} />
                    {s.accepted}/{s.needed}
                    {s.invited > 0 && (
                      <span style={{ color: "var(--warning-text)", fontSize: 11 }} title="Waiting for confirmation">
                        +{s.invited}
                      </span>
                    )}
                  </span>
                ))}
              </div>
              <span className={m.status === "Open" ? "badge badge-success" : "badge badge-danger"} style={{ justifySelf: "start" }}>
                {m.status}
              </span>
              <div style={{ display: "flex", gap: 6, justifyContent: "flex-end" }}>
                <button onClick={() => setAssigningMatch(m)} style={{ padding: "4px 10px", fontSize: 12 }}>Assign people</button>
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
          knownCategories={knownCategories}
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
          knownCategories={knownCategories}
          onClose={() => setEditingMatch(null)}
          onSaved={() => {
            setEditingMatch(null)
            loadMatches()
          }}
        />
      )}

      {assigningMatch && (
        <AssignModal
          match={assigningMatch}
          onClose={() => setAssigningMatch(null)}
          onDone={() => {
            setAssigningMatch(null)
            loadMatches()
          }}
        />
      )}
    </AdminLayout>
  )
}

const DEFAULT_CATEGORIES = ["Football", "Padel", "Beach Football"]

function MatchFormModal({
  existing,
  knownCategories,
  onClose,
  onSaved,
}: {
  existing?: Match
  knownCategories: string[]
  onClose: () => void
  onSaved: () => void
}) {
  const isEditing = !!existing

  const [title, setTitle] = useState(existing?.title || "")
  const [teamA, setTeamA] = useState(existing?.teamA || "")
  const [teamB, setTeamB] = useState(existing?.teamB || "")
  const [hostName, setHostName] = useState(existing?.hostName || "")
  const [category, setCategory] = useState(existing?.category || "Football")
  // An event (press conference, tournament...) has no two teams
  const [isEvent, setIsEvent] = useState(existing?.isEvent ?? false)
  const [matchDate, setMatchDate] = useState(
    existing ? new Date(existing.matchDate).toISOString().slice(0, 16) : ""
  )
  const [location, setLocation] = useState(existing?.location || "")
  const [description, setDescription] = useState(existing?.description || "")

  // How many people are needed for each role (0 = this role is not needed)
  const [needed, setNeeded] = useState<Record<string, number>>(() => {
    const initial: Record<string, number> = {}
    STAFF_TYPES.forEach((t) => {
      const slot = existing?.slots.find((s) => s.staffType === t.value)
      initial[t.value] = slot ? slot.needed : t.value === "Photographer" && !existing ? 1 : 0
    })
    return initial
  })

  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // The usual sports first, then anything the admin typed before
  const categoryOptions = [...DEFAULT_CATEGORIES, ...knownCategories.filter((c) => !DEFAULT_CATEGORIES.includes(c))]

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)

    if (!category.trim()) {
      setError("Type a category (a sport or an event).")
      return
    }

    const slots = STAFF_TYPES
      .filter((t) => needed[t.value] > 0)
      .map((t) => ({ staffType: t.value, needed: needed[t.value] }))

    if (slots.length === 0) {
      setError("Pick at least one role and how many people you need for it.")
      return
    }

    setSubmitting(true)
    try {
      const payload = {
        title,
        teamA: isEvent ? null : teamA || null,
        teamB: isEvent ? null : teamB || null,
        category: category.trim(),
        isEvent,
        hostName: isEvent ? hostName.trim() || null : null,
        matchDate: new Date(matchDate).toISOString(),
        location,
        description: description || null,
        slots,
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
        padding: "1.5rem", width: 440, maxHeight: "92vh", overflowY: "auto",
        display: "flex", flexDirection: "column", gap: 10,
      }}>
        <h3 style={{ margin: "0 0 8px" }}>{isEditing ? "Edit match" : "New match"}</h3>

        <input placeholder="Title" value={title} onChange={(e) => setTitle(e.target.value)} required />
        <input
          list="category-options"
          placeholder="Category (pick one or type a new sport / event)"
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          maxLength={50}
          required
        />
        <datalist id="category-options">
          {categoryOptions.map((c) => <option key={c} value={c} />)}
        </datalist>

        <label style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 13, color: "var(--text-muted)" }}>
          <input
            type="checkbox"
            checked={isEvent}
            onChange={(e) => setIsEvent(e.target.checked)}
            style={{ width: 16, height: 16, padding: 0, margin: 0 }}
          />
          This is an event (no teams)
        </label>

        {isEvent && (
          <input placeholder="MC / host of the event (optional)" value={hostName} maxLength={150} onChange={(e) => setHostName(e.target.value)} />
        )}

        {!isEvent && (
          <div style={{ display: "flex", gap: 8 }}>
            <input placeholder="Team A (optional)" value={teamA} onChange={(e) => setTeamA(e.target.value)} style={{ flex: 1 }} />
            <input placeholder="Team B (optional)" value={teamB} onChange={(e) => setTeamB(e.target.value)} style={{ flex: 1 }} />
          </div>
        )}
        <input
          type="datetime-local"
          value={matchDate}
          onChange={(e) => setMatchDate(e.target.value)}
          required
        />
        <input placeholder="Location" value={location} onChange={(e) => setLocation(e.target.value)} required />
        <textarea placeholder="Description (optional)" value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />

        <div>
          <p style={{ fontSize: 13, color: "var(--text-muted)", margin: "4px 0 8px" }}>
            Who do you need? (0 = not needed)
          </p>
          <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
            {STAFF_TYPES.map((t) => (
              <div key={t.value} style={{ display: "flex", alignItems: "center", gap: 10 }}>
                <RoleIcon type={t.value} size={32} />
                <span style={{ flex: 1, fontSize: 13, fontWeight: 500 }}>{t.label}</span>
                <input
                  type="number"
                  min={0}
                  value={needed[t.value]}
                  onChange={(e) => setNeeded((n) => ({ ...n, [t.value]: Math.max(0, Number(e.target.value) || 0) }))}
                  style={{ width: 80 }}
                  aria-label={`${t.label} needed`}
                />
              </div>
            ))}
          </div>
        </div>

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
