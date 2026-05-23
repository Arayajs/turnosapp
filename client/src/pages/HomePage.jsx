import { Link } from 'react-router-dom'

export default function HomePage() {
  return (
    <div>
      {/* Hero */}
      <section style={{
        background: 'linear-gradient(135deg, #1e293b 0%, #2563eb 100%)',
        color: '#fff',
        padding: '5rem 0',
        textAlign: 'center',
      }}>
        <div className="container">
          <h1 style={{ fontSize: '2.75rem', fontWeight: 800, marginBottom: '1rem', lineHeight: 1.2 }}>
            Agenda tus citas<br />en segundos
          </h1>
          <p style={{ fontSize: '1.15rem', color: '#cbd5e1', maxWidth: 520, margin: '0 auto 2rem' }}>
            TurnOS conecta clientes con barberías, clínicas y talleres. Elige horario, confirma al instante.
          </p>
          <div className="flex gap-4" style={{ justifyContent: 'center' }}>
            <Link to="/businesses">
              <button className="btn-primary" style={{ padding: '.75rem 2rem', fontSize: '1rem' }}>
                Ver negocios
              </button>
            </Link>
            <Link to="/register">
              <button className="btn-outline" style={{ padding: '.75rem 2rem', fontSize: '1rem', color: '#fff', borderColor: '#94a3b8' }}>
                Registrar mi negocio
              </button>
            </Link>
          </div>
        </div>
      </section>

      {/* Features */}
      <section style={{ padding: '4rem 0' }}>
        <div className="container">
          <h2 style={{ textAlign: 'center', fontSize: '1.75rem', fontWeight: 700, marginBottom: '2.5rem' }}>
            ¿Por qué TurnOS?
          </h2>
          <div className="grid-3">
            {[
              { icon: '📅', title: 'Agenda 24/7', desc: 'Tus clientes pueden reservar en cualquier momento, desde cualquier dispositivo.' },
              { icon: '🔔', title: 'Confirmación al instante', desc: 'Email automático al confirmar o cancelar una cita.' },
              { icon: '📊', title: 'Panel para negocios', desc: 'Visualiza todas tus citas del día, confirma o cancela con un clic.' },
            ].map(f => (
              <div key={f.title} className="card" style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '2.5rem', marginBottom: '.75rem' }}>{f.icon}</div>
                <h3 style={{ fontWeight: 700, marginBottom: '.5rem' }}>{f.title}</h3>
                <p className="text-muted">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  )
}
