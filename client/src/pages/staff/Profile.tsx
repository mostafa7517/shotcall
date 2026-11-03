import { useEffect, useState } from 'react'
import { apiFetch } from '../../api/client'
import { useAuth } from '../../context/AuthContext'
import StaffLayout from '../../components/StaffLayout'
import StaffTypePicker from '../../components/StaffTypePicker'
import { RoleBadge } from '../../components/RoleIcon'

interface Profile {
  fullName: string
  email: string
  profilePictureUrl: string | null
  phoneNumber: string | null
  bio: string | null
  reliabilityScore: number
  staffType: string | null
  canChangeStaffType: boolean
}

export default function Profile() {
  const { updateUser } = useAuth()
  const [profile, setProfile] = useState<Profile | null>(null)
  const [phoneNumber, setPhoneNumber] = useState('')
  const [bio, setBio] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [savingRole, setSavingRole] = useState(false)
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

  async function handleChooseRole(value: string) {
    setSavingRole(true)
    setError(null)
    try {
      const updated: Profile = await apiFetch('/api/profile/staff-type', {
        method: 'PUT',
        body: JSON.stringify({ staffType: value }),
      })
      setProfile(updated)
      updateUser({ staffType: updated.staffType })
    } catch (err) {
      setError(String(err))
    } finally {
      setSavingRole(false)
    }
  }

  if (loading) return <StaffLayout><p>Loading...</p></StaffLayout>
  if (!profile) return <StaffLayout><p style={{ color: 'var(--danger-text)' }}>{error}</p></StaffLayout>

  const isIncomplete = !profile.phoneNumber || !profile.bio

  return (
    <StaffLayout>
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
            <p style={{ margin: '2px 0 6px', fontSize: 13, color: 'var(--text-muted)' }}>{profile.email}</p>
            <RoleBadge type={profile.staffType} />
          </div>
        </div>

        <div style={{ marginBottom: 20, fontSize: 13, color: 'var(--text-muted)' }}>
          Reliability score: <strong style={{ color: 'var(--text)' }}>{profile.reliabilityScore}</strong>
        </div>

        {profile.canChangeStaffType ? (
          <div style={{ marginBottom: 20 }}>
            <p style={{ fontSize: 13, fontWeight: 600, margin: '0 0 8px' }}>Choose your role</p>
            <StaffTypePicker value={profile.staffType} onChange={handleChooseRole} disabled={savingRole} />
          </div>
        ) : (
          <p style={{ marginBottom: 20, fontSize: 12, color: 'var(--text-muted)' }}>
            Your role is fixed. Ask an admin if it needs to change.
          </p>
        )}

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
    </StaffLayout>
  )
}
