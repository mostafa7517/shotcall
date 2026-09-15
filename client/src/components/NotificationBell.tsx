import { useState } from 'react'
import { useNotifications } from '../hooks/useNotifications'

export default function NotificationBell() {
  const { notifications, unreadCount, markAsRead } = useNotifications()
  const [open, setOpen] = useState(false)

  return (
    <div style={{ position: 'relative' }}>
      <button onClick={() => setOpen((o) => !o)} style={{ position: 'relative' }}>
        🔔
        {unreadCount > 0 && (
          <span style={{
            position: 'absolute', top: -4, right: -4,
            background: 'var(--danger-text)', color: '#fff',
            fontSize: 10, fontWeight: 700,
            borderRadius: '50%', width: 16, height: 16,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
          }}>
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div className="card" style={{
          position: 'absolute', top: 36, right: 0, width: 300,
          maxHeight: 360, overflowY: 'auto', zIndex: 30, padding: 8,
        }}>
          {notifications.length === 0 && (
            <p style={{ fontSize: 13, color: 'var(--text-muted)', padding: 12, margin: 0 }}>
              No notifications yet.
            </p>
          )}
          {notifications.map((n) => (
            <div
              key={n.id}
              onClick={() => !n.isRead && markAsRead(n.id)}
              style={{
                padding: '10px 12px',
                borderRadius: 8,
                fontSize: 13,
                cursor: n.isRead ? 'default' : 'pointer',
                background: n.isRead ? 'transparent' : 'var(--accent-soft)',
                marginBottom: 4,
              }}
            >
              <p style={{ margin: 0 }}>{n.message}</p>
              <p style={{ margin: '4px 0 0', fontSize: 11, color: 'var(--text-muted)' }}>
                {new Date(n.createdAt).toLocaleString()}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}