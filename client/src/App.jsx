import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import Navbar from './components/Layout/Navbar'
import ProtectedRoute from './components/Common/ProtectedRoute'

import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import BusinessesPage from './pages/BusinessesPage'
import BookingPage from './pages/BookingPage'
import MyAppointmentsPage from './pages/MyAppointmentsPage'
import DashboardPage from './pages/DashboardPage'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Navbar />
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/businesses" element={<BusinessesPage />} />
          <Route path="/businesses/:businessId" element={<BookingPage />} />
          <Route
            path="/my-appointments"
            element={
              <ProtectedRoute roles={['Client', 'Admin']}>
                <MyAppointmentsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute roles={['BusinessOwner', 'Admin']}>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={
            <div className="container page text-center">
              <h1 style={{ fontSize: '3rem', fontWeight: 800, color: 'var(--primary)' }}>404</h1>
              <p className="text-muted">Página no encontrada.</p>
            </div>
          } />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
