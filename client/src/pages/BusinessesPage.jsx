import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../services/api'

export default function BusinessesPage() {
  const [businesses, setBusinesses] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get('/businesses')
      .then(r => setBusinesses(r.data))
      .catch(() => setError('Error al cargar los negocios.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="container page"><div className="spinner" /></div>

  return (
    <div className="container page">
      <h1 className="page-title">Negocios disponibles</h1>
      {error && <div className="alert alert-error">{error}</div>}

      {businesses.length === 0 ? (
        <div className="card text-center" style={{ padding: '3rem' }}>
          <p className="text-muted">No hay negocios registrados aún.</p>
        </div>
      ) : (
        <div className="grid-2">
          {businesses.map(b => (
            <div key={b.id} className="card">
              <div className="flex-between" style={{ marginBottom: '.5rem' }}>
                <h2 style={{ fontSize: '1.1rem', fontWeight: 700 }}>{b.name}</h2>
              </div>
              {b.description && <p className="text-muted" style={{ marginBottom: '.75rem', fontSize: '.875rem' }}>{b.description}</p>}
              <div className="text-muted" style={{ fontSize: '.8rem', marginBottom: '1rem' }}>
                {b.address && <span>📍 {b.address}</span>}
                {b.phone && <span style={{ marginLeft: '1rem' }}>📞 {b.phone}</span>}
              </div>
              <Link to={`/businesses/${b.id}`}>
                <button className="btn-primary btn-sm">Ver servicios</button>
              </Link>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
