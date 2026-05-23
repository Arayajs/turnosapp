import { useEffect, useState } from 'react'
import api from '../services/api'

const STATUS_LABEL = {
  Pending: 'Pendiente',
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
  Completed: 'Completada',
}

export default function MyAppointmentsPage() {
  const [appointments, setAppointments] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = () => {
    setLoading(true)
    api.get('/appointments/my')
      .then(r => setAppointments(r.data))
      .catch(() => setError('Error al cargar tus citas.'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleCancel = async (id) => {
    if (!confirm('¿Cancelar esta cita?')) return
    try {
      await api.delete(`/appointments/${id}`)
      load()
    } catch (err) {
      alert(err.response?.data?.error || 'Error al cancelar.')
    }
  }

  if (loading) return <div className="container page"><div className="spinner" /></div>

  return (
    <div className="container page">
      <h1 className="page-title">Mis citas</h1>
      {error && <div className="alert alert-error">{error}</div>}

      {appointments.length === 0 ? (
        <div className="card text-center" style={{ padding: '3rem' }}>
          <p className="text-muted">No tienes citas registradas.</p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '.75rem' }}>
          {appointments.map(a => {
            const start = new Date(a.startTime)
            const canCancel = a.status !== 'Cancelled' && a.status !== 'Completed' && start > new Date()
            return (
              <div key={a.id} className="card flex-between">
                <div>
                  <div className="flex gap-2" style={{ alignItems: 'center', marginBottom: '.35rem' }}>
                    <strong>{a.serviceName}</strong>
                    <span className={`badge badge-${a.status.toLowerCase()}`}>{STATUS_LABEL[a.status]}</span>
                  </div>
                  <p className="text-muted" style={{ fontSize: '.85rem' }}>
                    📅 {start.toLocaleDateString('es', { weekday: 'long', day: 'numeric', month: 'long' })}
                    {' · '}
                    🕐 {start.toLocaleTimeString('es', { hour: '2-digit', minute: '2-digit' })}
                  </p>
                  <p className="text-muted" style={{ fontSize: '.85rem' }}>🏢 {a.businessName} · ${a.price}</p>
                  {a.notes && <p className="text-muted" style={{ fontSize: '.8rem' }}>📝 {a.notes}</p>}
                </div>
                {canCancel && (
                  <button className="btn-danger btn-sm" onClick={() => handleCancel(a.id)}>
                    Cancelar
                  </button>
                )}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
