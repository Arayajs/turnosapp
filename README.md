# TurnOS

> Cloud-based appointment management for small businesses — barbershops, clinics, workshops, and more.

![CI/CD](https://github.com/your-org/turnosapp/actions/workflows/deploy.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![React](https://img.shields.io/badge/React-18-61DAFB)
![License](https://img.shields.io/badge/license-MIT-green)

---

## Features

- **Multi-role auth** — Admin, BusinessOwner, Client with JWT + opaque refresh tokens
- **Business management** — create and manage services (name, duration, price)
- **Smart slot booking** — real-time availability from 09:00 to 19:00
- **Appointment lifecycle** — Pending → Confirmed → Completed / Cancelled
- **Email notifications** — confirmations and cancellations via SendGrid
- **Owner dashboard** — view and action daily appointments by date
- **Client history** — see all upcoming and past bookings in one place

---

## Architecture

```
TurnOS/
├── src/
│   ├── TurnOS.Domain/          # Entities, enums, repository interfaces
│   ├── TurnOS.Application/     # DTOs, service interfaces, business logic
│   ├── TurnOS.Infrastructure/  # EF Core, repos, JWT, BCrypt, SendGrid
│   └── TurnOS.API/             # ASP.NET Core controllers, middleware, DI
├── tests/
│   └── TurnOS.UnitTests/       # xUnit + Moq + FluentAssertions (19 tests)
├── client/                     # React 18 + Vite 5 + React Router 6
├── docker-compose.yml
└── .github/workflows/deploy.yml
```

**Dependency rule:** Domain ← Application ← Infrastructure ← API  
No outer layer may be imported by an inner layer.

---

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 9, Entity Framework Core 9 |
| Database | SQL Server 2022 |
| Auth | JWT Bearer (HS256) + BCrypt |
| Email | SendGrid |
| Tests | xUnit, Moq, FluentAssertions |
| Frontend | React 18, Vite 5, React Router 6, Axios |
| Container | Docker, docker-compose |
| CI/CD | GitHub Actions → Azure App Service |

---

## API Endpoints

### Auth  `/api/auth`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/register` | — | Register new user |
| POST | `/login` | — | Login, returns JWT + refresh token |
| POST | `/refresh` | — | Exchange refresh token for new JWT |
| POST | `/forgot-password` | — | Send password-reset email |
| POST | `/reset-password` | — | Reset password with token |

### Businesses  `/api/businesses`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/` | — | List all businesses |
| GET | `/{id}` | — | Get business by ID |
| POST | `/` | BusinessOwner, Admin | Create business |
| PUT | `/{id}` | BusinessOwner (owner), Admin | Update business |
| DELETE | `/{id}` | BusinessOwner (owner), Admin | Delete business |

### Services  `/api/services`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/businesses/{businessId}/services` | — | List services for a business |
| POST | `/` | BusinessOwner, Admin | Create service |
| PUT | `/{id}` | BusinessOwner (owner), Admin | Update service |
| DELETE | `/{id}` | BusinessOwner (owner), Admin | Delete service |

### Appointments  `/api/appointments`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/available-slots` | — | `?serviceId=&date=YYYY-MM-DD` |
| POST | `/` | Client, Admin | Book appointment |
| GET | `/my` | Client, Admin | Client's own appointments |
| GET | `/business/{businessId}` | BusinessOwner, Admin | `?date=YYYY-MM-DD` |
| PATCH | `/{id}/status` | BusinessOwner (owner), Admin | Update status |
| DELETE | `/{id}` | Client (own), Admin | Cancel appointment |

---

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- SQL Server 2022 (or Docker)
- Node.js 20+ (for the frontend dev server)

### Run with Docker Compose

```bash
cd TurnOS
docker-compose up --build
```

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

The database is created and migrated automatically on first startup.

### Run locally (without Docker)

**Backend:**

```bash
cd TurnOS
# Edit src/TurnOS.API/appsettings.Development.json with your SQL Server details
dotnet run --project src/TurnOS.API
```

**Frontend:**

```bash
cd TurnOS/client
npm install
npm run dev
# → http://localhost:5173
```

### Run tests

```bash
cd TurnOS
dotnet test --verbosity normal
```

19 unit tests covering Auth, Business, and Appointment services.

---

## Deployment

### Azure App Service

1. Create an **Azure SQL Database** and note the connection string
2. Create an **Azure App Service** (Linux, .NET 9)
3. Download the **Publish Profile** from the Azure Portal
4. Add it as the `AZURE_WEBAPP_PUBLISH_PROFILE` secret in GitHub (Settings → Secrets → Actions)
5. Update `env.AZURE_WEBAPP_NAME` in `.github/workflows/deploy.yml` to match your App Service name
6. Set all required application settings — see [ENVIRONMENT.md](ENVIRONMENT.md)
7. Push to `main` — the workflow builds, tests, and deploys automatically

### CI/CD pipeline

```
push to main
  └─ build-and-test   ← dotnet build + dotnet test (all branches)
       └─ publish      ← dotnet publish --configuration Release
            └─ deploy  ← azure/webapps-deploy@v3 (main branch only)
```

Pull requests run `build-and-test` only — no deployment.

---

## Project structure (detailed)

```
src/TurnOS.Domain/
  Entities/         User, Business, Service, Appointment
  Enums/            UserRole, AppointmentStatus
  Interfaces/       IUserRepository, IBusinessRepository,
                    IServiceRepository, IAppointmentRepository,
                    IEmailService

src/TurnOS.Application/
  DTOs/             Auth/, Business/, Service/, Appointment/
  Interfaces/       IAuthService, IBusinessService, IServiceService,
                    IAppointmentService, IJwtService, IPasswordHasher
  Services/         AuthService, BusinessService, ServiceService,
                    AppointmentService

src/TurnOS.Infrastructure/
  Data/             AppDbContext, Migrations/
  Repositories/     UserRepository, BusinessRepository,
                    ServiceRepository, AppointmentRepository
  Services/         JwtService, PasswordHasher, EmailService
  DependencyInjection.cs

src/TurnOS.API/
  Controllers/      AuthController, BusinessController,
                    ServiceController, AppointmentController
  Middleware/       ExceptionMiddleware
  Extensions/       ClaimsPrincipalExtensions
  Program.cs
  Dockerfile

tests/TurnOS.UnitTests/
  Services/         AuthServiceTests (6), AppointmentServiceTests (7),
                    BusinessServiceTests (6)

client/
  src/
    context/        AuthContext.jsx
    services/       api.js (axios + JWT interceptor)
    components/     Layout/Navbar, Common/ProtectedRoute
    pages/          HomePage, LoginPage, RegisterPage,
                    BusinessesPage, BookingPage,
                    MyAppointmentsPage, DashboardPage
  vite.config.js    proxy /api → localhost:8080
```

---

## License

MIT
