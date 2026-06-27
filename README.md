# 🚀 .NET 9 + React 18 Starter – Clean Architecture Template

**Production-ready full-stack starter** for building modern web applications with **.NET 9 Clean Architecture API** and **React 18 + TypeScript frontend**.

Perfect for teams, entrepreneurs, and developers who need a **clean, scalable, and battle-tested foundation** with best practices baked in from day one.

---

**Dostępne wersje:**
- 🇬🇧 [English](./README.md)     - English
- 🇵🇱 [Polski](./README.PL.md)   – Polska

---

## ✨ What's Included

### Core Features (V1.0)
- ✅ **Clean Architecture** – Domain → Application → Infrastructure, with clear separation of concerns
- ✅ **JWT Authentication** – Secure refresh token rotation, token validation, user sessions
- ✅ **Entity Framework Core 9** – PostgreSQL ORM with migrations and automatic schema management
- ✅ **Health Checks** – Built-in `/health` endpoint for container orchestration
- ✅ **Request Validation** – FluentValidation with automatic DTO validation
- ✅ **Structured Logging** – Serilog with console and file outputs
- ✅ **CORS Configuration** – Environment-based origin management
- ✅ **Docker & Docker Compose** – Single-command deployment with PostgreSQL database
- ✅ **API Documentation** – Swagger UI and OpenAPI v1 schema
- ✅ **TypeScript Frontend** – React 18 with type safety and modern hooks
- ✅ **Test Projects** – Unit, Integration, and E2E test scaffolding

### In the Box (Ready Now)
- Backend running on .NET 9
- Frontend with React 18 + TypeScript
- PostgreSQL database with migrations
- JWT auth flow (login, register, refresh token)
- User entity with role-based access control
- CORS configured from appsettings
- Health checks for orchestration
- Clean project structure ready for growth

---

## 🚀 Quick Start

### Prerequisites
**Option 1: Docker (Recommended)**
- Docker & Docker Compose installed

**Option 2: Local Development**
- .NET 9 SDK
- Node.js 20+ and npm
- PostgreSQL 16+

### Run with Docker (30 seconds)
```bash
git clone https://github.com/Karol8284/dotnet-react-starter.git
cd dotnet-react-starter
docker-compose up
```

**Then open:**
- 🎨 **Frontend:** http://localhost:3000
- 🔌 **API:** http://localhost:5000
- 📖 **Swagger Docs:** http://localhost:5000/swagger
- ✅ **Health Check:** http://localhost:5000/health

### Local Development

**Backend:**
```bash
cd backend
dotnet build
dotnet run --project API/API.csproj
```
Runs on `https://localhost:7021`

### Prerequisites
- Docker & Docker Compose (recommended)
- Or: Node.js 20+ and .NET 9 SDK

## Environment Configuration

This repository uses two configuration entry points:

1. Root `.env` for Docker Compose and deployment/runtime values.
2. `frontend/.env.*` files for local CRA frontend builds.

Start from the tracked examples:

```bash
copy .env.example .env
copy frontend/.env.example frontend/.env.development.local
```

Important rules:

- Backend secrets go to root `.env`, CI/CD secrets, or hosting configuration.
- Never put secrets in frontend `REACT_APP_*` variables. They are public at build time.
- For Docker/nginx deployments, set `FRONTEND_REACT_APP_API_URL=/api`.
- For local frontend to local backend development, use `REACT_APP_API_URL=http://localhost:5000`.

**Database (first time only):**
```bash
git clone https://github.com/Karol8284/dotnet-react-starter.git
cd dotnet-react-starter
copy .env.example .env
docker-compose up --build
```

---

## 📚 Documentation

| Guide | Purpose |
|-------|---------|
| [Getting Started](./doc/GETTING_STARTED.md) | Step-by-step setup & configuration |
| [Architecture](./doc/ARCHITECTURE.md) | Project organization & design patterns |
| [Backend Development](./doc/BACKEND_SETUP.md) | How to extend API and add features |
| [Frontend Development](./doc/FRONTEND_SETUP.md) | Components, hooks, state management |
| [Docker Guide](./docker/DOCKER_COMPOSE.md) | Container orchestration & networking |

---

## 📁 Project Structure

```
dotnet-react-starter/
│
├── backend/                          # .NET 9 API (Clean Architecture)
│   ├── API/
│   │   ├── Controllers/              # HTTP endpoints
│   │   ├── Filters/                  # Validation, exception handling
│   │   ├── Middleware/               # Auth, error handling pipelines
│   │   ├── Program.cs                # Startup configuration, DI
│   │   ├── appsettings.json          # Production config
│   │   ├── appsettings.Development.json
│   │   └── Dockerfile
│   │
│   ├── Application/
│   │   ├── DTOs/                     # Data transfer objects
│   │   │   └── Auth/                 # LoginUserDto, RegisterUserDto
│   │   ├── Services/                 # Business logic
│   │   ├── Validators/               # FluentValidation rules
│   │   └── Commands/                 # CQRS command patterns
│   │
│   ├── Domain/
│   │   ├── Entities/                 # User, core business models
│   │   ├── Enums/                    # UserRole, status types
│   │   ├── Interfaces/               # IAuthService, IJwtTokenService
│   │   └── ValueObjects/             # Custom value types
│   │
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Migrations/           # EF Core migrations
│   │   ├── Repositories/             # Repository pattern
│   │   ├── Services/                 # AuthService, JwtTokenService
│   │   └── Extensions/               # Dependency injection helpers
│   │
│   ├── Shared/
│   │   ├── Dtos/                     # Shared response models
│   │   ├── Settings/                 # Configuration classes (JwtSettings)
│   │   ├── Constants/                # Application constants
│   │   └── Responses/                # ApiResponse<T> patterns
│   │
│   ├── UnitTests/                    # xUnit + Moq tests
│   ├── IntegrationTests/             # Database + API tests
│   ├── E2ETests/                     # End-to-end scenarios
│   └── backend.slnx                  # Solution file
│
├── frontend/                         # React 18 + TypeScript
│   ├── src/
│   │   ├── components/               # Reusable UI components
│   │   ├── pages/                    # Full-page components
│   │   ├── services/                 # API client (axios)
│   │   ├── hooks/                    # Custom React hooks
│   │   ├── context/                  # Global state (Zustand)
│   │   ├── types/                    # TypeScript interfaces
│   │   ├── styles/                   # CSS modules, theming
│   │   ├── utils/                    # Helper functions
│   │   └── App.tsx                   # Root component
│   ├── public/                       # Static assets
│   ├── package.json
│   ├── tsconfig.json
│   ├── Dockerfile
│   └── README.md
│
├── docker/
│   ├── docker-compose.yml            # Multi-container orchestration
│   ├── nginx.conf                    # Reverse proxy config
│   └── DOCKER_COMPOSE.md             # Networking guide
│
├── doc/
│   ├── GETTING_STARTED.md
│   ├── ARCHITECTURE.md
│   ├── BACKEND_SETUP.md
│   ├── FRONTEND_SETUP.md
│   └── DOCKER_COMPOSE.md
│
└── README.md                         # This file
```

---

## 🛠️ Tech Stack

### Backend (.NET 9)
| Layer | Technology | Purpose |
|-------|-----------|---------|
| **API** | ASP.NET Core 9 | REST endpoints, middleware |
| **Auth** | JWT Bearer + BCrypt | Secure authentication & token management |
| **Database** | Entity Framework Core 9 | ORM & PostgreSQL integration |
| **Database** | PostgreSQL 16 | Production-grade SQL database |
| **Validation** | FluentValidation | Request validation, business rules |
| **Logging** | Serilog | Structured logs to console & file |
| **API Docs** | Swagger/OpenAPI | Interactive API documentation |
| **Testing** | xUnit + Moq | Unit & integration tests |

### Frontend (React 18)
| Package | Version | Purpose |
|---------|---------|---------|
| **React** | 18.x | UI library |
| **TypeScript** | 5.x | Type safety |
| **React Router** | 6.x | Client-side routing |
| **Axios** | 1.x | HTTP client for API calls |
| **Zustand** | Latest | Lightweight state management |
| **React Query** | Latest | Server state sync & caching |
| **React Hook Form** | Latest | Performant form handling |
| **Zod** | Latest | Runtime schema validation |

### DevOps & Infrastructure
- **Docker** – Containerization
- **Docker Compose** – Multi-container orchestration
- **PostgreSQL** – Primary data store
- **Nginx** – Reverse proxy
- **GitHub Actions** – CI/CD ready (configuration templates included)

---

## 🔐 Authentication & Security

### How Auth Works
1. **Register** – `POST /api/auth/register` creates user with BCrypt-hashed password
2. **Login** – `POST /api/auth/login` returns JWT access token + refresh token
3. **Access** – Frontend sends `Authorization: Bearer {accessToken}` on protected endpoints
4. **Refresh** – `POST /api/auth/refresh-token` rotates token pair before expiry
5. **Logout** – `POST /api/auth/logout` revokes refresh token

### Security Features ✅
- Passwords hashed with BCrypt (never stored plain)
- JWT tokens with configurable expiration (15 min access, 7 day refresh)
- CORS validation against whitelisted origins (configurable)
- Automatic token rotation on refresh
- Request validation before processing
- Role-based access control (RBAC) ready

### Configuration
Edit `appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "CHANGE_IN_PRODUCTION_minimum-32-characters",
    "Issuer": "Dotnet-React-Starter-API",
    "Audience": "Dotnet-React-Starter-App"
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:3000", "https://yourdomain.com"]
  }
}
```

---

## 📦 Environment Variables

### Docker (.env support)
```bash
# Database
POSTGRES_DB=dotnet_react_starter
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# API
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=...
```

### Local (.NET secrets)
```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "your-secret-key"
dotnet user-secrets set "Jwt:Issuer" "your-issuer"
```

---

## 📋 Development Workflow

### Adding a New API Feature

1. **Create Entity** (`Domain/Entities/YourEntity.cs`)
   ```csharp
   public class YourEntity
   {
       public Guid Id { get; set; }
       public string Name { get; set; }
   }
   ```

2. **Add DbSet** (`Infrastructure/Data/ApplicationDbContext.cs`)
   ```csharp
   public DbSet<YourEntity> YourEntities => Set<YourEntity>();
   ```

3. **Create DTO** (`Application/DTOs/YourEntityDto.cs`)
   ```csharp
   public class YourEntityDto
   {
       public Guid Id { get; set; }
       public string Name { get; set; }
   }
   ```

4. **Create Service Interface** (`Domain/Interfaces/IYourService.cs`)
   ```csharp
   public interface IYourService
   {
       Task<YourEntityDto> GetByIdAsync(Guid id);
       Task<YourEntityDto> CreateAsync(CreateYourEntityDto dto);
   }
   ```

5. **Implement Service** (`Infrastructure/Services/YourService.cs`)
   ```csharp
   public class YourService : IYourService
   {
       // Implementation
   }
   ```

6. **Create Controller** (`API/Controllers/YourController.cs`)
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class YourController : ControllerBase
   {
       private readonly IYourService _service;
       // Endpoints
   }
   ```

7. **Register in DI** (`API/Program.cs`)
   ```csharp
   builder.Services.AddScoped<IYourService, YourService>();
   ```

8. **Create Migration** (in `backend/API` directory)
   ```bash
   dotnet ef migrations add AddYourEntity --project ../Infrastructure --output-dir ../Infrastructure/Data/Migrations
   ```

### Testing Your Feature
```bash
# Unit tests
dotnet test --filter "FullyQualifiedName~YourFeature"

# Run full test suite
dotnet test
```

---

## 🗓️ Roadmap & Upcoming Features

### V1.0 (Current) ✅
- ✅ Clean Architecture baseline
- ✅ JWT authentication & refresh tokens
- ✅ PostgreSQL + EF migrations
- ✅ Request validation (FluentValidation)
- ✅ Health checks for orchestration
- ✅ CORS configuration from settings
- ✅ Docker containerization
- ✅ API documentation (Swagger)
- ✅ Logging (Serilog)

### V1.5 (Q3 2026) 📅
- **Database Enhancements**
  - [ ] Persist refresh tokens to database (instead of in-memory)
  - [ ] Audit logging (created/modified timestamps, change tracking)
  - [ ] Soft deletes support
  
- **Testing**
  - [ ] Complete auth flow unit tests
  - [ ] Integration tests for database operations
  - [ ] E2E test examples (login → protected endpoint)
  
- **Frontend Features**
  - [ ] Login/Register pages
  - [ ] Protected routes
  - [ ] Logout & token refresh logic
  - [ ] Error handling & retry logic
  - [ ] Loading states

- **DevOps**
  - [ ] GitHub Actions CI/CD pipeline
  - [ ] Automated testing on PR
  - [ ] Docker build optimization

### V2.0 (Q4 2026) 🎯
- **Advanced Auth**
  - [ ] Google/GitHub OAuth2 integration
  - [ ] Multi-factor authentication (MFA)
  - [ ] Email confirmation flow
  - [ ] Password reset functionality
  
- **API Features**
  - [ ] Pagination & filtering
  - [ ] Full-text search support
  - [ ] Bulk operations
  - [ ] API versioning (v1, v2 endpoints)
  - [ ] GraphQL alternative endpoint
  
- **Frontend**
  - [ ] Dashboard layout
  - [ ] User profile management
  - [ ] Settings page
  - [ ] Notification system
  - [ ] Dark mode support
  
- **Infrastructure**
  - [ ] Redis caching layer
  - [ ] Background jobs (Hangfire)
  - [ ] Real-time notifications (SignalR)
  - [ ] File upload service
  - [ ] Email service integration
  
- **Documentation**
  - [ ] API endpoint catalog
  - [ ] Video tutorials
  - [ ] Deployment guides (AWS, Azure, DigitalOcean)
  - [ ] Performance optimization guide

### V2.5+ (Future Possibilities)
- [ ] Admin dashboard
- [ ] Analytics & reporting
- [ ] Multi-tenancy support
- [ ] Mobile app (React Native)
- [ ] Microservices architecture transition
- [ ] Event sourcing & CQRS deep dive
- [ ] DDD principles advanced implementation

---

## 🏗️ Clean Architecture Principles

This starter enforces **Clean Architecture** by design:

- **Domain Layer** – Pure business logic, no dependencies
- **Application Layer** – Use cases, DTOs, validators (depends only on Domain)
- **Infrastructure Layer** – Database, external services (depends on Domain & Application)
- **API Layer** – Controllers, middleware (depends on all layers)

**Benefits:**
- Easy to test (dependencies injected, mockable)
- Business logic independent of frameworks
- High maintainability & extensibility
- Clear separation of concerns

---

## 📖 Common Tasks

### Change Database (PostgreSQL → SQL Server)
1. Remove `Npgsql.EntityFrameworkCore.PostgreSQL`
2. Install `Microsoft.EntityFrameworkCore.SqlServer`
3. Update connection string
4. Regenerate migrations: `dotnet ef migrations add MigrateToSqlServer`

### Add Role-Based Authorization
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly() => Ok("Admin only");
```

### Configure Logging Level
Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Your.Namespace": "Debug"
    }
  }
}
```

### Scale Backend Horizontally
1. Deploy multiple instances
2. Use load balancer (Nginx, AWS ALB)
3. Share database connection
4. Refresh tokens stay in-memory per instance (or use Redis in V1.5)

---

## 🤝 Contributing

Have suggestions? Found a bug? Want to add a feature?

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License – see [LICENSE](./LICENSE) file for details.

---

## 💬 Support & Questions

- 📖 Read [GETTING_STARTED.md](./doc/GETTING_STARTED.md) first
- 🐛 Report issues on GitHub Issues
- 💡 Discussions welcome in GitHub Discussions
- 📧 Email: support@yourdomain.com (add your contact)

---

## ⭐ Show Your Support

If this starter helped you, please consider giving it a ⭐ on GitHub!

**Made with ❤️ for developers who value clean code, scalability, and productivity.**

---

**Last Updated:** June 2026 | **Maintained by:** [Karol8284](https://github.com/Karol8284)

### Adding a New Feature - Frontend
1. Create types in `src/types`
2. Create service in `src/services`
3. Create hook in `src/hooks`
4. Create component in `src/components`
5. Create page in `src/pages`
6. Add route in `src/App.tsx`

See [BACKEND_SETUP.md](./doc/BACKEND_SETUP.md) and [FRONTEND_SETUP.md](./doc/FRONTEND_SETUP.md) for detailed examples!

## 🐳 Docker Commands

```bash
# Start everything
docker-compose up

# Start in background
docker-compose up -d

# Rebuild after code changes
docker-compose up --build

# View logs
docker-compose logs -f frontend
docker-compose logs -f backend

# Stop everything
docker-compose down

# Stop and remove data
docker-compose down -v
```

## 🧪 Running Tests

### Backend
```bash
dotnet test backend/UnitTests/UnitTests.csproj
dotnet test backend/IntegrationTests/IntegrationTests.csproj
dotnet test backend/E2ETests/E2ETests.csproj
```

Smoke tests are intended for a running application, for example after `docker compose up` or after deployment.

Optional environment overrides:

```bash
set SMOKE_API_URL=http://localhost:5000
set SMOKE_FRONTEND_URL=http://localhost:3000
dotnet test backend/E2ETests/E2ETests.csproj
```

### Frontend
```bash
npm run test:once
```

## 📦 Scripts

### Frontend
```bash
npm start         # Start development server
npm run build     # Build for production
npm test          # Run tests
npm run lint      # Check code quality
```

### Backend
```bash
dotnet run --project backend/API/API.csproj
dotnet test backend/
dotnet build backend/
```

## 🚢 Deployment

The Docker images are production-ready. Deploy to:
- Docker Hub + Docker Swarm
- Kubernetes
- AWS ECS
- Azure Container Instances
- Heroku, Railway, Render, etc.

## 📖 Learning Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [React Documentation](https://react.dev/)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Docker Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Support

For questions and issues:
1. Check the [documentation](./doc/)
2. Open a GitHub Issue
3. Create a Discussion

---

**Ready to build something amazing?** 🚀

```bash
docker-compose up
```

