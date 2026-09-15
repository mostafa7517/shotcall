import { createContext, useContext, useState, useEffect } from 'react'
import type { ReactNode } from 'react'
import { setTokens, clearTokens } from '../api/client'

interface User {
  userId: string
  fullName: string
  email: string
  role: 'Admin' | 'Photographer'
  accountStatus: 'PendingApproval' | 'Active' | 'Rejected' | 'Disabled'
}

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  login: (data: { accessToken: string; refreshToken: string } & User) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const stored = localStorage.getItem('user')
    if (stored) {
      try {
        setUser(JSON.parse(stored))
      } catch {
        clearTokens()
      }
    }
    setIsLoading(false)
  }, [])

  const login = (data: { accessToken: string; refreshToken: string } & User) => {
    setTokens(data.accessToken, data.refreshToken)
    const userData: User = {
      userId: data.userId,
      fullName: data.fullName,
      email: data.email,
      role: data.role,
      accountStatus: data.accountStatus,
    }
    localStorage.setItem('user', JSON.stringify(userData))
    setUser(userData)
  }

  const logout = () => {
    clearTokens()
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}