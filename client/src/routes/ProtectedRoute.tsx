import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

interface Props {
  allowedRoles?: ('Admin' | 'Photographer')[]
}

export default function ProtectedRoute({ allowedRoles }: Props) {
  const { user, isLoading } = useAuth()

  if (isLoading) return null

  if (!user) return <Navigate to="/login" replace />

  if (user.accountStatus === 'PendingApproval') {
    return <Navigate to="/pending-approval" replace />
  }

  if (user.accountStatus === 'Rejected' || user.accountStatus === 'Disabled') {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
