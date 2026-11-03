import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { setTokens, clearTokens } from '../api/client'

export interface User {
  userId: string
  fullName: string
  email: string
  role: 'Admin' | 'Staff'
  staffType: string | null
  accountStatus: 'PendingApproval' | 'Active' | 'Rejected' | 'Disabled'
}

interface LoginData extends Omit<User, 'staffType'> {
  accessToken: string
  refreshToken: string
  staffType?: string | null
}

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  login: (data: LoginData) => void
  logout: () => void
  updateUser: (changes: Partial<User>) => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const stored = localStorage.getItem('user')
    if (stored) {
      try {
        const parsed = JSON.parse(stored)
        if (parsed.role === 'Admin' || parsed.role === 'Staff') {
          setUser({
            userId: parsed.userId,
            fullName: parsed.fullName,
            email: parsed.email,
            role: parsed.role,
            staffType: parsed.staffType ?? null,
            accountStatus: parsed.accountStatus,
          })
        } else {
          // A session saved before roles were renamed: sign in again
          clearTokens()
        }
      } catch {
        clearTokens()
      }
    }
    setIsLoading(false)
  }, [])

  // Keep the saved copy in sync whenever the user changes
  useEffect(() => {
    if (user) localStorage.setItem('user', JSON.stringify(user))
  }, [user])

  const login = useCallback((data: LoginData) => {
    setTokens(data.accessToken, data.refreshToken)
    setUser({
      userId: data.userId,
      fullName: data.fullName,
      email: data.email,
      role: data.role,
      staffType: data.staffType ?? null,
      accountStatus: data.accountStatus,
    })
  }, [])

  const logout = useCallback(() => {
    clearTokens()
    setUser(null)
  }, [])

  const updateUser = useCallback((changes: Partial<User>) => {
    setUser((current) => (current ? { ...current, ...changes } : current))
  }, [])

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}