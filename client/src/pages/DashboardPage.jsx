import { useEffect, useState } from 'react'
import api from '../services/api'
import { useAuth } from '../context/AuthContext'

const STATUS_LABEL = {
  Pending: 'Pendiente',
  Confirmed: 'Confirmada',
  Cancelled: 'Cancelada',
  Completed: 'Completada',
}

const STATUS_NEXT = {
  Pending: [{ value: 'Confirmed', label: 'Confirmar' }, { value: 'Cancelled', label: 'Cancelar' }],
  Confirmed: [{ value: 'Completed', label: 'Completar' }, { value: 'Cancelled', label: 'Cancelar' }],
}

export default function DashboardPage() {
  const { user } = useAuth()
  const [business, setBusiness] = useState(null)
  const [appointments, setAppointments] = useState([])
  const [services, setServices] = useState([])
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showNewBiz, setShowNewBiz] = useState(false)
  const [bizForm, setBizForm] = useState({ name: '', description: '', address: '', phone: '' })
  const [serviceForm, setServiceForm] = useState({ name: '', description: '', durationMinutes: 30, price: 0 })
  const [showNewSvc, setShowNewSvc] = useState(false)
  const [saving, setSaving] = useState(false)

  const loadBusiness = async () => {
    try {
      const { data } = await api.get('/businesses')
      const mine = data.find(b => b.ownerId === user.id)
      if (mine) {
        setBusiness(mine)
        const [apptRes, svcRes] = await Promise.all([
          api.get(`/appointments/business/${mine.id}`, { params: { date } }),
          api.get(`/businesses/${mine.id}/services`),
        ])
        setAppointments(apptRes.data)
        setServices(svcRes.data)
      }
    } catch {
      setError('Error al cargar el dashboard.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { loadBusiness() }, [date])

  const handleCreateBusiness = async e => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post('/businesses', bizForm)
      setShowNewBiz(false)
      loadBusiness()
    } catch (err) {
      setError(err.response?.data?.error || 'Error al crear el negocio.')
    } finally {
      setSaving(false)
    }
  }

  const handleCreateService = async e => {
    e.preventDefault()
    setSaving(true)
    try {
      await api.post('/services', { ...serviceForm, businessId: business.id })
      setShowNewSvc(false)
      loadBusiness()
    } catch (err) {
      setError(err.response?.data?.error || 'Error al crear el servicio.')
    } finally {
      setSaving(false)
    }
  }

  const handleDeleteService = async (id) => {
    if (!confirm('¿Eliminar este servicio?')) return
    try {
      await api.delete(`/services/${id}`)
      loadBusiness()
    } catch (err) {
      alert(err.response?.data?.error || 'Error al eliminar.')
    }
  }

  const handleStatusChange = async (apptId, status) => {
    try {
      await api.patch(`/appointments/${apptId}/status`, { status })
      loadBusiness()
    } catch (err) {
      alert(err.response?.data?.error || 'Error al actualizar.')
    }
  }

  if (loading) return <div className="container page"><div className="spinner" /></div>

  return (
    <div className="container page">
      <div className="flex-between" style={{ marginBottom: '1.5rem' }}>
        <h1 className="page-title" style={{ margin: 0 }}>
          {business ? `Dashboard — ${business.name}` : 'Mi Negocio'}
        </h1>
        {!business && (
          <button className="btn-primary" onClick={() => setShowNewBiz(true)}>
            + Crear negocio
          </button>
        )}
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {/* Create business form */}
      {showNewBiz && (
        <div className="card" style={{ marginBottom: '1.5rem' }}>
          <h2 style={{ fontWeight: 700, marginBottom: '1rem' }}>Nuevo negocio</h2>
          <form onSubmit={handleCreateBusiness}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
              <div className="form-group"><label>Nombre *</label><input required value={bizForm.name} onChange={e => setBizForm(f => ({ ...f, name: e.target.value }))} /></div>
              <div className="form-group"><label>Teléfono</label><input value={bizForm.phone} onChange={e => setBizForm(f => ({ ...f, phone: e.target.value }))} /></div>
              <div className="form-group"><label>Dirección</label><input value={bizForm.address} onChange={e => setBizForm(f => ({ ...f, address: e.target.value }))} /></div>
              <div className="form-group"><label>Descripción</label><input value={bizForm.description} onChange={e => setBizForm(f => ({ ...f, description: e.target.value }))} /></div>
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn-primary" disabled={saving}>{saving ? 'Guardando...' : 'Crear negocio'}</button>
              <button type="button" className="btn-outline" onClick={() => setShowNewBiz(false)}>Cancelar</button>
            </div>
          </form>
        </div>
      )}

      {business && (
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
          {/* Services */}
          <div>
            <div className="flex-between" style={{ marginBottom: '.75rem' }}>
              <h2 style={{ fontWeight: 700 }}>Servicios</h2>
              <button className="btn-primary btn-sm" onClick={() => setShowNewSvc(v => !v)}>+ Agregar</button>
            </div>

            {showNewSvc && (
              <form onSubmit={handleCreateService} className="card" style={{ marginBottom: '1rem' }}>
                <div className="form-group"><label>Nombre *</label><input required value={serviceForm.name} onChange={e => setServiceForm(f => ({ ...f, name: e.target.value }))} /></div>
                <div className="form-group"><label>Descripción</label><input value={serviceForm.description} onChange={e => setServiceForm(f => ({ ...f, description: e.target.value }))} /></div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                  <div className="form-group"><label>Duración (min)</label><input type="number" min={5} max={480} value={serviceForm.durationMinutes} onChange={e => setServiceForm(f => ({ ...f, durationMinutes: +e.target.value }))} /></div>
                  <div className="form-group"><label>Precio ($)</label><input type="number" min={0} step="0.01" value={serviceForm.price} onChange={e => setServiceForm(f => ({ ...f, price: +e.target.value }))} /></div>
                </div>
                <div className="flex gap-2">
                  <button type="submit" className="btn-primary btn-sm" disabled={saving}>{saving ? '...' : 'Guardar'}</button>
                  <button type="button" className="btn-outline btn-sm" onClick={() => setShowNewSvc(false)}>Cancelar</button>
                </div>
              </form>
            )}

            {services.length === 0
              ? <p className="text-muted">Sin servicios. Agrega uno para empezar.</p>
              : services.map(s => (
                <div key={s.id} className="card flex-between" style={{ marginBottom: '.5rem', padding: '1rem' }}>
                  <div>
                    <strong>{s.name}</strong>
                    <p className="text-muted" style={{ fontSize: '.8rem' }}>⏱ {s.durationMinutes} min · ${s.price}</p>
                  </div>
                  <button className="btn-danger btn-sm" onClick={() => handleDeleteService(s.id)}>Eliminar</button>
                </div>
              ))
            }
          </div>

          {/* Appointments */}
          <div>
            <div className="flex-between" style={{ marginBottom: '.75rem' }}>
              <h2 style={{ fontWeight: 700 }}>Citas del día</h2>
              <input type="date" value={date} onChange={e => setDate(e.target.value)} style={{ width: 'auto' }} />
            </div>

            {appointments.length === 0
              ? <p className="text-muted">Sin citas para esta fecha.</p>
              : appointments.map(a => {
                const start = new Date(a.startTime)
                const actions = STATUS_NEXT[a.status] || []
                return (
                  <div key={a.id} className="card" style={{ marginBottom: '.5rem', padding: '1rem' }}>
                    <div className="flex-between" style={{ marginBottom: '.35rem' }}>
                      <strong>{a.clientName}</strong>
                      <span className={`badge badge-${a.status.toLowerCase()}`}>{STATUS_LABEL[a.status]}</span>
                    </div>
                    <p className="text-muted" style={{ fontSize: '.8rem' }}>
                      {a.serviceName} · {start.toLocaleTimeString('es', { hour: '2-digit', minute: '2-digit' })}
                    </p>
                    {actions.length > 0 && (
                      <div className="flex gap-2 mt-2">
                        {actions.map(act => (
                          <button
                            key={act.value}
                            className={act.value === 'Cancelled' ? 'btn-outline btn-sm' : 'btn-primary btn-sm'}
                            onClick={() => handleStatusChange(a.id, act.value)}
                          >
                            {act.label}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )
              })
            }
          </div>
        </div>
      )}
    </div>
  )
}
