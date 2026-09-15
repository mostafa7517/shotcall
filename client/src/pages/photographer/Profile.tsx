import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import PhotographerLayout from '../../components/PhotographerLayout'

interface Profile {
  fullName: string
  email: string
  profilePictureUrl: string | null
  phoneNumber: string | null
  bio: string | null
  reliabilityScore: number
}

export default function Profile() {
  const [profile, setProfile] = useState<Profile | null>(null)
  const [phoneNumber, setPhoneNumber] = useState('')
  const [bio, setBio] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    apiFetch('/api/profile')
      .then((data: Profile) => {
        setProfile(data)
        setPhoneNumber(data.phoneNumber || '')
        setBio(data.bio || '')
      })
      .catch((err) => setError(String(err)))
      .finally(() => setLoading(false))
  }, [])

  async function handleSave(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setSaved(false)
    setError(null)
    try {
      const updated = await apiFetch('/api/profile', {
        method: 'PUT',
        body: JSON.stringify({ phoneNumber: phoneNumber || null, bio: bio || null }),
      })
      setProfile(updated)
      setSaved(true)
      setTimeout(() => setSaved(false), 2500)
    } catch (err) {
      setError(String(err))
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <PhotographerLayout><p>Loading...</p></PhotographerLayout>
  if (!profile) return <PhotographerLayout><p style={{ color: 'var(--danger-text)' }}>{error}</p></PhotographerLayout>

  const isIncomplete = !profile.phoneNumber || !profile.bio

  return (
    <PhotographerLayout>
      <h2 style={{ marginBottom: 20 }}>Profile</h2>

      {isIncomplete && (
        <div className="card" style={{
          padding: '12px 16px', marginBottom: 20,
          background: 'var(--warning-bg)', border: 'none',
        }}>
          <span style={{ fontSize: 13, color: 'var(--warning-text)' }}>
            Complete your profile so admins know more about you.
          </span>
        </div>
      )}

      <div className="card" style={{ padding: 24, maxWidth: 480 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14, marginBottom: 20 }}>
          {profile.profilePictureUrl ? (
            <img src={profile.profilePictureUrl} alt="" style={{ width: 56, height: 56, borderRadius: '50%' }} />
          ) : (
            <div style={{
              width: 56, height: 56, borderRadius: '50%', background: 'var(--accent-soft)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 20, fontWeight: 600, color: 'var(--accent)',
            }}>
              {profile.fullName.charAt(0)}
            </div>
          )}
          <div>
            <p style={{ margin: 0, fontWeight: 600 }}>{profile.fullName}</p>
            <p style={{ margin: '2px 0 0', fontSize: 13, color: 'var(--text-muted)' }}>{profile.email}</p>
          </div>
        </div>

        <div style={{ marginBottom: 20, fontSize: 13, color: 'var(--text-muted)' }}>
          Reliability score: <strong style={{ color: 'var(--text)' }}>{profile.reliabilityScore}</strong>
        </div>

        <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <label style={{ fontSize: 13, fontWeight: 500 }}>
            Phone number
            <input
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              placeholder="e.g. +20 100 000 0000"
              style={{ display: 'block', marginTop: 4, width: '100%' }}
            />
          </label>

          <label style={{ fontSize: 13, fontWeight: 500 }}>
            Bio
            <textarea
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              placeholder="Tell admins a bit about yourself"
              rows={3}
              style={{ display: 'block', marginTop: 4, width: '100%' }}
            />
          </label>

          {error && <p style={{ color: 'var(--danger-text)', fontSize: 13 }}>{error}</p>}
          {saved && <p style={{ color: 'var(--success-text)', fontSize: 13 }}>Saved!</p>}

          <button type="submit" className="primary" disabled={saving} style={{ alignSelf: 'flex-start' }}>
            {saving ? 'Saving...' : 'Save changes'}
          </button>
        </form>
      </div>
    </PhotographerLayout>
  )
}