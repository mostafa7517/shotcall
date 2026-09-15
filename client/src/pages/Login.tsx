import { useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const API_BASE = 'https://localhost:7102'
const GOOGLE_CLIENT_ID = '128996488757-0t0m9t9s7oni3mfpot9n1u2pf3ee8arq.apps.googleusercontent.com'

declare global {
  interface Window {
    google: any
  }
}

export default function Login() {
  const buttonRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const { login } = useAuth()

  useEffect(() => {
    const handleCredentialResponse = async (response: any) => {
      try {
        const res = await fetch(`${API_BASE}/api/auth/google-login`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ idToken: response.credential }),
        })
        const data = await res.json()
        if (!res.ok) {
          alert(data.error || 'Login failed')
          return
        }

        login(data)
        navigate('/')
      } catch (err) {
        alert(String(err))
      }
    }

    const initGoogle = () => {
      if (!window.google) return
      window.google.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: handleCredentialResponse,
      })
      if (buttonRef.current) {
        window.google.accounts.id.renderButton(buttonRef.current, {
          theme: 'outline',
          size: 'large',
        })
      }
    }

    const interval = setInterval(() => {
      if (window.google) {
        clearInterval(interval)
        initGoogle()
      }
    }, 100)

    return () => clearInterval(interval)
  }, [login, navigate])

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontFamily: 'sans-serif',
    }}>
      <div style={{
        width: 320,
        background: '#fff',
        border: '1px solid #e5e5e5',
        borderRadius: 12,
        padding: '2rem 1.75rem',
        textAlign: 'center',
      }}>
        <h2 style={{ margin: '0 0 6px' }}>ShotCall</h2>
        <p style={{ fontSize: 13, color: '#666', margin: '0 0 28px' }}>
          Match coverage, organized.
        </p>

        <div ref={buttonRef} style={{ display: 'flex', justifyContent: 'center' }}></div>

        <p style={{ fontSize: 11, color: '#999', margin: '20px 0 0', lineHeight: 1.5 }}>
          New accounts are reviewed by an admin before you can access matches.
        </p>
      </div>
    </div>
  )
}
