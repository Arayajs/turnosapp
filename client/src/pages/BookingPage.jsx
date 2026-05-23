import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../services/api'
import { useAuth } from '../context/AuthContext'

function toLocalISO(date) {
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
    .toISOString().slice(0, 10)
}

export default function BookingPage() {
  const { businessId } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()

  const [business, setBusiness] = useState(null)
  const [services, setServices] = useState([])
  const [selectedService, setSelectedService] = useState(null)
  const [date, setDate] = useState(() => toLocalISO(new Date()))
  const [slots, setSlots] = useState([])
  const [selectedSlot, setSelectedSlot] = useState(null)
  const [notes, setNotes] = useState('')
  const [loading, setLoading] = useState(true)
  const [slotsLoading, setSlotsLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    Promise.all([
      api.get(`/businesses/${businessId}`),
      api.get(`/businesses/${businessId}/services`),
    ]).then(([bRes, sRes]) => {
      setBusiness(bRes.data)
      setServices(sRes.data)
      if (sRes.data.length > 0) setSelectedService(sRes.data[0])
    }).catch(() => setError('Error al cargar el negocio.'))
      .finally(() => setLoading(false))
  }, [businessId])

  useEffect(() => {
    if (!selectedService || !date) return
    setSlotsLoading(true)
    setSelectedSlot(null)
    api.get('/appointments/available-slots', {
      params: { businessId, serviceId: selectedService.id, date },
    }).then(r => setSlots(r.data.slots))
      .catch(() => setSlots([]))
      .finally(() => setSlotsLoading(false))
  }, [selectedService, date, businessId])

  const handleBook = async () => {
    if (!user) { navigate('/login'); return }
    if (!selectedSlot) return
    setSubmitting(true)
    setError('')
    try {
      await api.post('/appointments', {
        serviceId: selectedService.id,
        businessId,
        startTime: selectedSlot.startTime,
        notes: notes || undefined,
      })
      setSuccess('¡Cita agendada! Revisa tu email para la confirmación.')
      setSelectedSlot(null)
      setNotes('')
    } catch (err) {
      setError(err.response?.data?.error || 'Error al agendar la cita.')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) return <div className="container page"><div className="spinner" /></div>
  if (!business) return <div className="container page"><div className="alert alert-error">{error}</div></div>

  const minDate = toLocalISO(new Date())

  return (
    <div className="container page">
      <h1 className="page-title">{business.name}</h1>
      {business.address && <p className="text-muted" style={{ marginBottom: '1.5rem' }}>📍 {business.address}</p>}

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
        {/* Left — service + date */}
        <div>
          <div className="card" style={{ marginBottom: '1rem' }}>
            <h2 style={{ fontWeight: 700, marginBottom: '1rem' }}>1. Elige el servicio</h2>
            {services.length === 0
              ? <p className="text-muted">Este negocio no tiene servicios aún.</p>
              : services.map(s => (
                <div
                  key={s.id}
                  onClick={() => setSelectedService(s)}
                  style={{
                    padding: '.75rem',
                    border: `2px solid ${selectedService?.id === s.id ? 'var(--primary)' : 'var(--border)'}`,
                    borderRadius: 'var(--radius)',
                    marginBottom: '.5rem',
                    cursor: 'pointer',
                    background: selectedService?.id === s.id ? '#eff6ff' : 'var(--surface)',
                  }}
                >
                  <div className="flex-between">
                    <strong>{s.name}</strong>
                    <span style={{ color: 'var(--primary)', fontWeight: 600 }}>${s.price}</span>
                  </div>
                  <p className="text-muted" style={{ fontSize: '.8rem' }}>⏱ {s.durationMinutes} min</p>
                </div>
              ))
            }
          </div>

          <div className="card">
            <h2 style={{ fontWeight: 700, marginBottom: '1rem' }}>2. Elige la fecha</h2>
            <input type="date" value={date} min={minDate} onChange={e => setDate(e.target.value)} />
          </div>
        </div>

        {/* Right — slots + confirm */}
        <div>
          <div className="card" style={{ marginBottom: '1rem' }}>
            <h2 style={{ fontWeight: 700, marginBottom: '.5rem' }}>3. Elige el horario</h2>
            {slotsLoading
              ? <div className="spinner" style={{ width: '1.5rem', height: '1.5rem', margin: '1rem auto' }} />
              : slots.length === 0
                ? <p className="text-muted mt-2">No hay horarios disponibles para esta fecha.</p>
                : (
                  <div className="slot-grid">
                    {slots.map(slot => {
                      const time = new Date(slot.startTime).toLocaleTimeString('es', { hour: '2-digit', minute: '2-digit' })
                      const isSelected = selectedSlot?.startTime === slot.startTime
                      return (
                        <button
                          key={slot.startTime}
                          className={`slot-btn ${!slot.isAvailable ? 'taken' : isSelected ? 'selected' : 'available'}`}
                          disabled={!slot.isAvailable}
                          onClick={() => slot.isAvailable && setSelectedSlot(slot)}
                        >
                          {time}
                        </button>
                      )
                    })}
                  </div>
                )
            }
          </div>

          {selectedSlot && (
            <div className="card">
              <h2 style={{ fontWeight: 700, marginBottom: '1rem' }}>4. Confirmar cita</h2>
              <div style={{ background: 'var(--bg)', borderRadius: 'var(--radius)', padding: '.75rem', marginBottom: '1rem', fontSize: '.875rem' }}>
                <p><strong>Servicio:</strong> {selectedService?.name}</p>
                <p><strong>Fecha:</strong> {new Date(selectedSlot.startTime).toLocaleDateString('es', { weekday: 'long', day: 'numeric', month: 'long' })}</p>
                <p><strong>Hora:</strong> {new Date(selectedSlot.startTime).toLocaleTimeString('es', { hour: '2-digit', minute: '2-digit' })}</p>
                <p><strong>Precio:</strong> ${selectedService?.price}</p>
              </div>
              <div className="form-group">
                <label>Notas adicionales <span className="text-muted">(opcional)</span></label>
                <textarea rows={2} value={notes} onChange={e => setNotes(e.target.value)} placeholder="Ej: alergia al latex..." />
              </div>
              {!user && (
                <p className="text-muted" style={{ marginBottom: '.75rem', fontSize: '.875rem' }}>
                  Debes <a href="/login">iniciar sesión</a> para confirmar la cita.
                </p>
              )}
              <button className="btn-primary btn-full" onClick={handleBook} disabled={submitting || !user}>
                {submitting ? 'Agendando...' : 'Confirmar cita'}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
