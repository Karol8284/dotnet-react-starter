# Aktualny poziom realizacji projektu: 
Oto szacunkowa ocena postępu do wersji V1.0 (MVP, produkcyjny JWT + Users CRUD, bez zaawansowanych ficzerów):

**Cały projekt (V1.0): ok. 70%**

### Backend
- JWT login/register/me/logout: 100%
- Refresh-token: 95% (działa, ale brak testów integracyjnych i panelu do zarządzania tokenami)
- UsersController CRUD: 80% (endpointy są, wymaga pełnych testów i walidacji edge-case)
- Testy jednostkowe/integracyjne: 30% (brak pokrycia, tylko szkielet)
- Obsługa błędów/logowanie: 80%
- Migracje/DB setup: 90% (brakuje migracji startowej dla RefreshToken)
- Bezpieczeństwo (hash refresh tokenów, rotacja): 90%

### Frontend
- TokenManager, HttpClient, AuthApi, UserApi: 100%
- AuthContext, ProtectedRoute, Navbar: 100%
- Strony: login, register, dashboard, profile, users CRUD: 90% (UI minimalistyczne, ale działa)
- Obsługa automatycznego odświeżania tokena: 90% (wymaga testu end-to-end)
- Walidacja formularzy/UI/UX: 70%
- Testy frontend: 20% (brak testów automatycznych)
- Build/produkcyjny deploy: 100%

### DevOps/Integracja
- Docker/compose: 80% (pliki są, wymaga testu całościowego)
- README/setup: 80% (dokumentacja jest, ale niepełna dla nowych ficzerów)
- CI/CD: 0% (brak pipeline)

---

**Podsumowanie:**
- Backend: ~85% (główne funkcje są, brakuje testów i drobnych szlifów)
- Frontend: ~90% (wszystko działa, UI do dopracowania, testy do napisania)
- Testy automatyczne: ~25%
- Dokumentacja/devops: ~60%

Do pełnego V1.0 brakuje: testów (unit/integration), migracji startowej dla RefreshToken, pełnej dokumentacji, CI/CD oraz szlifów UI/UX.

Chcesz szczegółowy plan „co zrobić do 100%”?

# 🚀 .NET React Starter

A modern, production-ready starter template for building full-stack web applications with **.NET 8 API** and **React + TypeScript**.

Perfect for teams who want a clean architecture, Docker support, and best practices out of the box.

## ✨ Features

- ✅ **Clean Architecture** - Domain, Application, Infrastructure layers
- ✅ **Docker Support** - Docker & Docker Compose for easy deployment
- ✅ **TypeScript** - Full type safety on frontend
- ✅ **Modern Stack** - React 18, .NET 8, Material-UI, React Query, Zustand
- ✅ **Ready for Testing** - Unit, Integration, E2E test projects
- ✅ **API Documentation** - Swagger/OpenAPI included
- ✅ **Form Validation** - Zod + React Hook Form
- ✅ **State Management** - Zustand for client state, React Query for server state

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose (recommended)
- Or: Node.js 20+ and .NET 8 SDK

### Run with Docker (Recommended)
```bash
git clone https://github.com/Karol8284/dotnet-react-starter.git
cd dotnet-react-starter
docker-compose up
```

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/openapi/v1.json

## 📚 Documentation

- **[Getting Started](./doc/GETTING_STARTED.md)** - Detailed setup instructions
- **[Architecture](./doc/ARCHITECTURE.md)** - Project structure overview
- **[Backend Setup](./doc/BACKEND_SETUP.md)** - How to develop on backend
- **[Frontend Setup](./doc/FRONTEND_SETUP.md)** - How to develop on frontend
- **[Docker Guide](./docker/DOCKER_COMPOSE.md)** - Docker orchestration

## 📁 Project Structure

```
├── backend/              # .NET 8 API (Clean Architecture)
│   ├── API/              # Controllers, Middleware
│   ├── Application/      # Services, DTOs, Validators
│   ├── Domain/           # Entities, Interfaces, Business Rules
│   ├── Infrastructure/   # Database, Repositories
│   ├── Shared/           # Common Classes
│   └── Tests/            # Unit, Integration, E2E Tests
│
├── frontend/             # React + TypeScript
│   └── src/
│       ├── components/   # Reusable React Components
│       ├── pages/        # Full Page Components
│       ├── services/     # API Communication (Axios)
│       ├── hooks/        # Custom React Hooks
│       ├── context/      # State Management (Zustand)
│       ├── types/        # TypeScript Interfaces
│       └── utils/        # Helper Functions
│
└── docker/               # Docker Configs
    ├── nginx.conf        # Reverse Proxy
    └── DOCKER_COMPOSE.md # Documentation
```

## 🛠️ Tech Stack

### Backend
- **.NET 8** - Modern C# framework
- **Entity Framework Core** - ORM
- **Clean Architecture** - Separation of concerns
- **AutoMapper** - Object mapping
- **xUnit/Moq** - Testing

### Frontend
- **React 18** - UI Library
- **TypeScript** - Type Safety
- **React Router** - Routing
- **React Query** - Server State Management
- **Zustand** - Client State Management
- **Material-UI** - Component Library
- **React Hook Form + Zod** - Form Validation

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Orchestration
- **GitHub Actions** - CI/CD (ready for setup)

## 📝 Development Workflow

### Adding a New Feature - Backend
1. Create Entity in `Domain/Entities`
2. Create DTO in `Application/DTOs`
3. Create Service in `Application/Services`
4. Create Repository in `Infrastructure/Repositories`
5. Create Controller in `API/Controllers`
6. Register in `Program.cs`

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
```

### Frontend
```bash
npm test
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

