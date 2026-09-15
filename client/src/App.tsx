import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import ProtectedRoute from './routes/ProtectedRoute'
import Login from './pages/Login'
import PendingApproval from './pages/PendingApproval'
import Dashboard from './pages/admin/Dashboard'
import Matches from './pages/admin/Matches'
import Calendar from './pages/admin/Calendar'
import Requests from './pages/admin/Requests'
import Accounts from './pages/admin/Accounts'
import Cancellations from './pages/admin/Cancellations'
import PhotographerMatches from './pages/photographer/Matches'
import MyRequests from './pages/photographer/MyRequests'
import Profile from './pages/photographer/Profile'

function HomeRedirect() {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  return user.role === 'Admin'
    ? <Navigate to="/admin/dashboard" replace />
    : <Navigate to="/matches" replace />
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/pending-approval" element={<PendingApproval />} />

          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<HomeRedirect />} />
          </Route>

          <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
            <Route path="/admin/dashboard" element={<Dashboard />} />
            <Route path="/admin/matches" element={<Matches />} />
            <Route path="/admin/calendar" element={<Calendar />} />
            <Route path="/admin/requests" element={<Requests />} />
            <Route path="/admin/accounts" element={<Accounts />} />
            <Route path="/admin/cancellations" element={<Cancellations />} />
          </Route>

          <Route element={<ProtectedRoute allowedRoles={['Photographer']} />}>
            <Route path="/matches" element={<PhotographerMatches />} />
            <Route path="/my-requests" element={<MyRequests />} />
            <Route path="/profile" element={<Profile />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
