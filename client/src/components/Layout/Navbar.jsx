import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

export default function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <nav style={{
      background: '#1e293b',
      color: '#fff',
      padding: '.75rem 0',
      boxShadow: '0 2px 8px rgba(0,0,0,.2)',
    }}>
      <div className="container flex-between">
        <Link to="/" style={{ color: '#fff', fontWeight: 700, fontSize: '1.25rem', textDecoration: 'none' }}>
          TurnApp
        </Link>

        <div className="flex gap-4" style={{ alignItems: 'center' }}>
          <Link to="/businesses" style={{ color: '#cbd5e1', fontSize: '.9rem', textDecoration: 'none' }}>
            Negocios
          </Link>

          {user ? (
            <>
              {user.role === 'BusinessOwner' && (
                <Link to="/dashboard" style={{ color: '#cbd5e1', fontSize: '.9rem', textDecoration: 'none' }}>
                  Mi Negocio
                </Link>
              )}
              <Link to="/my-appointments" style={{ color: '#cbd5e1', fontSize: '.9rem', textDecoration: 'none' }}>
                Mis Citas
              </Link>
              <span style={{ color: '#94a3b8', fontSize: '.875rem' }}>
                {user.fullName}
              </span>
              <button onClick={handleLogout} className="btn-outline btn-sm" style={{ color: '#cbd5e1', borderColor: '#475569' }}>
                Salir
              </button>
            </>
          ) : (
            <>
              <Link to="/login" style={{ color: '#cbd5e1', fontSize: '.9rem', textDecoration: 'none' }}>
                Iniciar sesión
              </Link>
              <Link to="/register">
                <button className="btn-primary btn-sm">Registrarse</button>
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  )
}
