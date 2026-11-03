import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiFetch } from '../api/client'
import { useAuth } from '../context/AuthContext'
import StaffTypePicker from '../components/StaffTypePicker'

export default function PendingApproval() {
  const { user, logout, updateUser } = useAuth()
  const navigate = useNavigate()
  const [saving, setSaving] = useState(false)
  const [checking, setChecking] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  async function handleChooseRole(value: string) {
    setSaving(true)
    setError(null)
    try {
      const profile = await apiFetch('/api/profile/staff-type', {
        method: 'PUT',
        body: JSON.stringify({ staffType: value }),
      })
      updateUser({ staffType: profile.staffType })
    } catch (err) {
      setError(String(err))
    } finally {
      setSaving(false)
    }
  }

  // The saved login data doesn't know when an admin approves the account,
  // so let the person re-check instead of having to log out and back in.
  async function handleCheckStatus() {
    setChecking(true)
    setError(null)
    setMessage(null)
    try {
      const profile = await apiFetch('/api/profile')
      updateUser({ accountStatus: profile.accountStatus, staffType: profile.staffType })
      if (profile.accountStatus === 'Active') {
        navigate('/', { replace: true })
      } else if (profile.accountStatus === 'PendingApproval') {
        setMessage('Still waiting for an admin to approve your account.')
      } else {
        logout()
        navigate('/login', { replace: true })
      }
    } catch (err) {
      setError(String(err))
    } finally {
      setChecking(false)
    }
  }

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: 20,
    }}>
      <div className="card" style={{ width: 420, maxWidth: '100%', padding: '1.75rem' }}>
        <h2 style={{ marginBottom: 6 }}>Hi {user?.fullName} 👋</h2>
        <p style={{ color: 'var(--text-muted)', fontSize: 14, margin: '0 0 20px' }}>
          Your account is under review. An admin needs to approve it before you can browse matches.
        </p>

        <p style={{ fontSize: 13, fontWeight: 600, margin: '0 0 8px' }}>What is your role?</p>
        <StaffTypePicker value={user?.staffType ?? null} onChange={handleChooseRole} disabled={saving} />
        <p style={{ fontSize: 12, color: 'var(--text-muted)', margin: '8px 0 0' }}>
          {user?.staffType
            ? 'You can still change it until you are approved. After that only an admin can change it.'
            : 'Pick one so the admin can approve you.'}
        </p>

        {error && <p style={{ color: 'var(--danger-text)', fontSize: 13 }}>{error}</p>}
        {message && <p style={{ color: 'var(--text-muted)', fontSize: 13 }}>{message}</p>}

        <div style={{ display: 'flex', gap: 8, marginTop: 20 }}>
          <button className="primary" onClick={handleCheckStatus} disabled={checking}>
            {checking ? 'Checking...' : 'Check approval status'}
          </button>
          <button onClick={logout}>Log out</button>
        </div>
      </div>
    </div>
  )
}
