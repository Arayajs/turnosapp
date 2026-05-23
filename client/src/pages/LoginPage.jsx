import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleChange = e => setForm(f => ({ ...f, [e.target.name]: e.target.value }))

  const handleSubmit = async e => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const user = await login(form.email, form.password)
      navigate(user.role === 'BusinessOwner' ? '/dashboard' : '/businesses')
    } catch (err) {
      setError(err.response?.data?.error || 'Credenciales inválidas.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page" style={{ display: 'flex', justifyContent: 'center' }}>
      <div className="card" style={{ width: '100%', maxWidth: 420 }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '.25rem' }}>Iniciar sesión</h1>
        <p className="text-muted" style={{ marginBottom: '1.5rem' }}>
          ¿No tienes cuenta? <Link to="/register">Regístrate</Link>
        </p>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email" name="email" value={form.email}
              onChange={handleChange} required autoFocus
              placeholder="tu@email.com"
            />
          </div>
          <div className="form-group">
            <label>Contraseña</label>
            <input
              type="password" name="password" value={form.password}
              onChange={handleChange} required
              placeholder="••••••••"
            />
          </div>
          <button type="submit" className="btn-primary btn-full" disabled={loading}>
            {loading ? 'Ingresando...' : 'Ingresar'}
          </button>
        </form>

        <p className="text-muted text-center mt-4" style={{ fontSize: '.8rem' }}>
          <Link to="/forgot-password">¿Olvidaste tu contraseña?</Link>
        </p>
      </div>
    </div>
  )
}
