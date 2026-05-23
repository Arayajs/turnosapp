import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const ROLES = [
  { value: 'Client', label: 'Cliente — quiero agendar citas' },
  { value: 'BusinessOwner', label: 'Dueño de negocio — quiero gestionar citas' },
]

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    fullName: '', email: '', password: '', phone: '', role: 'Client',
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleChange = e => setForm(f => ({ ...f, [e.target.name]: e.target.value }))

  const handleSubmit = async e => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const user = await register(form)
      navigate(user.role === 'BusinessOwner' ? '/dashboard' : '/businesses')
    } catch (err) {
      setError(err.response?.data?.error || 'Error al registrarse.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page" style={{ display: 'flex', justifyContent: 'center' }}>
      <div className="card" style={{ width: '100%', maxWidth: 460 }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '.25rem' }}>Crear cuenta</h1>
        <p className="text-muted" style={{ marginBottom: '1.5rem' }}>
          ¿Ya tienes cuenta? <Link to="/login">Inicia sesión</Link>
        </p>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Nombre completo</label>
            <input name="fullName" value={form.fullName} onChange={handleChange} required placeholder="Juan Pérez" />
          </div>
          <div className="form-group">
            <label>Email</label>
            <input type="email" name="email" value={form.email} onChange={handleChange} required placeholder="tu@email.com" />
          </div>
          <div className="form-group">
            <label>Contraseña</label>
            <input type="password" name="password" value={form.password} onChange={handleChange} required minLength={6} placeholder="Mínimo 6 caracteres" />
          </div>
          <div className="form-group">
            <label>Teléfono <span className="text-muted">(opcional)</span></label>
            <input name="phone" value={form.phone} onChange={handleChange} placeholder="+1 555 0001" />
          </div>
          <div className="form-group">
            <label>Tipo de cuenta</label>
            <select name="role" value={form.role} onChange={handleChange}>
              {ROLES.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
            </select>
          </div>
          <button type="submit" className="btn-primary btn-full" disabled={loading}>
            {loading ? 'Creando cuenta...' : 'Crear cuenta'}
          </button>
        </form>
      </div>
    </div>
  )
}
