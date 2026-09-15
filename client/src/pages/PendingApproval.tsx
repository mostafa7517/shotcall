import { useAuth } from '../context/AuthContext'

export default function PendingApproval() {
  const { user, logout } = useAuth()

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontFamily: 'sans-serif',
      textAlign: 'center',
      padding: 20,
    }}>
      <div>
        <h2>Hi {user?.fullName} 👋</h2>
        <p style={{ color: '#666', maxWidth: 320 }}>
          Your account is under review. An admin needs to approve it before you can browse matches.
        </p>
        <button onClick={logout} style={{ marginTop: 20 }}>Log out</button>
      </div>
    </div>
  )
}
