import { useEffect, useRef, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import { apiFetch, getAccessToken } from '../api/client'

export interface AppNotification {
  id: string
  type: string
  message: string
  isRead: boolean
  createdAt: string
}

export function useNotifications() {
  const [notifications, setNotifications] = useState<AppNotification[]>([])
  const connectionRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    apiFetch('/api/notifications/my')
      .then(setNotifications)
      .catch(() => {})

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7102/hubs/notifications', {
        accessTokenFactory: () => getAccessToken() || '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveNotification', (notification: AppNotification) => {
      setNotifications((prev) => [notification, ...prev])
    })

    connection.start().catch((err) => console.error('SignalR connection failed:', err))
    connectionRef.current = connection

    return () => {
      connection.stop()
    }
  }, [])

  async function markAsRead(id: string) {
    setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)))
    try {
      await apiFetch(`/api/notifications/${id}/read`, { method: 'PUT' })
    } catch {
      // ignore - not critical
    }
  }

  const unreadCount = notifications.filter((n) => !n.isRead).length

  return { notifications, unreadCount, markAsRead }
}