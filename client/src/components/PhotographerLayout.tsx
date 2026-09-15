import type { ReactNode } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import NotificationBell from './NotificationBell'

const navItems = [
  { path: '/matches', label: 'Matches', icon: '\ud83c\udfdf\ufe0f' },
  { path: '/my-requests', label: 'My requests', icon: '\ud83d\udccb' },
  { path: '/profile', label: 'Profile', icon: '\ud83d\udc64' },
]

export default function PhotographerLayout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const location = useLocation()

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <div style={{
        width: 220,
        borderRight: '1px solid var(--border)',
        background: 'var(--surface)',
        padding: '1.5rem 1rem',
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        position: 'sticky',
        top: 0,
        height: '100vh',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '0 8px', marginBottom: 24 }}>
          <div style={{
            width: 28, height: 28, borderRadius: 8, background: 'var(--accent)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: '#fff', fontSize: 14, fontWeight: 700,
          }}>S</div>
          <span style={{ fontWeight: 700, fontSize: 15 }}>ShotCall</span>
        </div>

        {navItems.map((item) => {
          const active = location.pathname === item.path
          return (
            <Link
              key={item.path}
              to={item.path}
              style={{
                display: 'flex', alignItems: 'center', gap: 10,
                padding: '9px 12px',
                borderRadius: 8,
                textDecoration: 'none',
                fontSize: 13.5,
                fontWeight: active ? 600 : 500,
                color: active ? 'var(--accent)' : 'var(--text-muted)',
                background: active ? 'var(--accent-soft)' : 'transparent',
              }}
            >
              <span style={{ fontSize: 15 }}>{item.icon}</span>
              {item.label}
            </Link>
          )
        })}

        <div style={{ marginTop: 'auto', paddingTop: 16, borderTop: '1px solid var(--border)' }}>
          <div style={{ fontSize: 13, fontWeight: 500, marginBottom: 8 }}>{user?.fullName}</div>
          <button onClick={logout} style={{ width: '100%' }}>Log out</button>
        </div>
      </div>

      <div style={{ flex: 1, padding: '2rem 2.5rem', maxWidth: 1200 }}>
        <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 12 }}>
          <NotificationBell />
        </div>
        {children}
      </div>
    </div>
  )
}
