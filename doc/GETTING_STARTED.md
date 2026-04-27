# 🚀 Getting Started - Uruchom projekt w 5 minut!

## Prerequisites

Przed uruchomieniem upewnij się że masz zainstalowane:

- ✅ **Docker** (with Docker Compose) - [Download](https://www.docker.com/products/docker-desktop)
- ✅ **Git** - [Download](https://git-scm.com/)
- ✅ **Node.js** 20+ (opcjonalnie, dla local development) - [Download](https://nodejs.org/)
- ✅ **.NET 8 SDK** (opcjonalnie, dla local development) - [Download](https://dotnet.microsoft.com/)

---

## 🐳 Opcja 1: Docker (REKOMENDOWANE - najłatwiejsze!)

### 1. Clone projektu
```bash
git clone https://github.com/Karol8284/dotnet-react-starter.git
cd dotnet-react-starter
```

### 2. Uruchom Docker Compose
```bash
docker-compose up
```

Pierwszych minutę będzie budować obrazy... ⏳

### 3. Czekaj na:
```
frontend       | > Compiled successfully!
backend        | Now listening on: http://0.0.0.0:5000
```

### 4. Otwórz przeglądarke
- 🌐 Frontend: **http://localhost:3000**
- 🔌 API: **http://localhost:5000**
- 📚 Swagger (API docs): **http://localhost:5000/openapi/v1.json**

### 5. Stop
```bash
docker-compose down
```

---

## 💻 Opcja 2: Local Development (bez Docker)

### Backend (.NET)

**1. Przejdź do folderu backend**
```bash
cd backend
```

**2. Przywróć zależności NuGet**
```bash
dotnet restore
```

**3. Uruchom API**
```bash
dotnet run --project API/API.csproj
```

API będzie dostępne na: `http://localhost:5000`

---

### Frontend (React)

**1. Przejdź do folderu frontend**
```bash
cd frontend
```

**2. Zainstaluj zależności npm**
```bash
npm install
```

**3. Uruchom dev server**
```bash
npm start
```

Frontend będzie dostępny na: `http://localhost:3000`

---

## 📖 Dokumentacja

Po zapoznaniu się z projektem, przeczytaj:

1. **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Ogólna struktura projektu
2. **[BACKEND_SETUP.md](./BACKEND_SETUP.md)** - Jak pisać backend (Domain, Application, Infrastructure)
3. **[FRONTEND_SETUP.md](./FRONTEND_SETUP.md)** - Jak pisać frontend (Components, Hooks, Services)
4. **[backend/DOCKER.md](../backend/DOCKER.md)** - Wyjaśnienie Dockerfile dla backendu
5. **[frontend/DOCKER.md](../frontend/DOCKER.md)** - Wyjaśnienie Dockerfile dla frontendu
6. **[docker/DOCKER_COMPOSE.md](../docker/DOCKER_COMPOSE.md)** - Docker Compose orchestracja

---

## 🏗️ Struktura Projektu

```
dotnet-react-starter/
│
├── backend/                    # .NET 8 API
│   ├── API/                    # Kontrolery, Middleware
│   ├── Application/            # Business logic, Services, DTOs
│   ├── Domain/                 # Encje, Interfejsy, Reguły biznesowe
│   ├── Infrastructure/         # Baza danych, Repositories
│   ├── Shared/                 # Wspólne klasy (Responses, Constants)
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── E2ETests/
│   ├── Dockerfile              # Instrukcje budowania kontenerów
│   └── DOCKER.md               # Wyjaśnienie
│
├── frontend/                   # React + TypeScript
│   ├── public/                 # Statyczne pliki
│   ├── src/
│   │   ├── components/         # Reusable komponenty React
│   │   ├── pages/              # Full page components
│   │   ├── services/           # API calls (Axios)
│   │   ├── hooks/              # Custom React hooks
│   │   ├── context/            # State management (Zustand)
│   │   ├── types/              # TypeScript interfaces
│   │   ├── utils/              # Helper functions
│   │   ├── App.tsx             # Main component with routing
│   │   └── index.tsx           # Entry point
│   ├── Dockerfile              # Instrukcje budowania
│   └── DOCKER.md               # Wyjaśnienie
│
├── docker/                     # Docker configs
│   ├── nginx.conf              # Nginx reverse proxy
│   └── DOCKER_COMPOSE.md       # Wyjaśnienie docker-compose.yml
│
├── doc/                        # Dokumentacja
│   ├── ARCHITECTURE.md         # Ogólna architektura
│   ├── BACKEND_SETUP.md        # Backend best practices
│   └── FRONTEND_SETUP.md       # Frontend best practices
│
├── docker-compose.yml          # Orkestracja Production
├── docker-compose.dev.yml      # Orkestracja Development (TODO)
├── README.md
└── LICENSE
```

---

## 🎯 Szybkie Komendy

### Docker
```bash
# Uruchom wszystko
docker-compose up

# Uruchom w tle
docker-compose up -d

# Przebuduj (po zmianie kodu)
docker-compose up --build

# Logi
docker-compose logs -f frontend
docker-compose logs -f backend

# Stop
docker-compose down

# Stop + usuń dane
docker-compose down -v
```

### Backend (.NET)
```bash
# Uruchom
dotnet run --project backend/API/API.csproj

# Testy
dotnet test backend/UnitTests/UnitTests.csproj

# Buduj
dotnet build backend/
```

### Frontend (React)
```bash
# Uruchom dev server
npm start

# Build do produkcji
npm run build

# Testy
npm test

# Lint
npm run lint
```

---

## 🐛 Troubleshooting

### ❌ Port 3000/5000 już w użyciu
```bash
# Zmień port w docker-compose.yml
ports:
  - "3001:3000"  # zamiast 3000:3000
```

### ❌ Docker nie znaleziony
```bash
# Zainstaluj Docker Desktop:
# https://www.docker.com/products/docker-desktop

# Lub na Linux:
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

### ❌ Frontend nie widzi backendu
```bash
# Sprawdź czy backend działa
docker-compose ps

# Sprawdź logi backendu
docker-compose logs backend

# Jeśli chodzi o localhost:5000 w dev, zmień na http://backend:5000
# (bo w docker-compose kontenerami się mówią po imionach)
```

### ❌ "package.json not found"
```bash
# Frontend folder musi mieć package.json
cd frontend
npm install zustand axios @tanstack/react-query react-router-dom @mui/material @emotion/react @emotion/styled react-hook-form zod @hookform/resolvers lodash-es date-fns classnames
```

### ❌ .NET projekty się nie budują
```bash
# Usuń cache i przebuduj
rm -r backend/**/bin
rm -r backend/**/obj
dotnet build backend/
```

---

## 📝 Pierwszy Commit

Projekt jest już na GitHub z v0.1 tagiem! 🎉

```bash
# Sprawdź git status
git status

# Commity już są wysłane
git log --oneline

# Tagi
git tag
```

---

## 🚀 Następne Kroki

1. **Przeczytaj dokumentację** - [BACKEND_SETUP.md](./BACKEND_SETUP.md)
2. **Dodaj pierwszą encję** - np. `Product` w Domain
3. **Dodaj test** - UnitTest dla nowej encji
4. **Dodaj endpoint** - Controller dla API
5. **Dodaj frontend** - React component do wyświetlenia danych
6. **Test całości** - Otwórz `http://localhost:3000`

---

## 📚 Usefull Resources

- **[Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)**
- **[React Documentation](https://react.dev/)**
- **[.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)**
- **[Docker Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)**
- **[TypeScript Handbook](https://www.typescriptlang.org/docs/)**

---

## ❓ FAQ

### P: Czy muszę używać Docker?
**O:** Nie! Możesz korzystać z opcji 2 (Local Development). Ale Docker jest MEGA polecany, szczególnie w zespołach.

### P: Którą bazę danych używamy?
**O:** Aktualnie brak (TODO). Backend jest przygotowany dla EF Core, możesz użyć SQL Server, PostgreSQL, czy SQLite.

### P: Gdzie mam pisać nowy kod?
**O:** Czytaj [BACKEND_SETUP.md](./BACKEND_SETUP.md) i [FRONTEND_SETUP.md](./FRONTEND_SETUP.md) - tam są dokładne instrukcje!

### P: Jak deployować na produkcję?
**O:** Docker images są już gotowe! Możesz deployować do:
- Docker Hub
- AWS ECR
- Azure Container Registry
- Kubernetes
- Heroku, Railway, Render, itd.

---

## 💬 Support

Jeśli masz pytania:
1. Czytaj dokumentację w `/doc` folder
2. Sprawdź [GitHub Issues](https://github.com/Karol8284/dotnet-react-starter/issues)
3. Stwórz nowe Issue jeśli problem się powtarza

---

**Gotowy?** 🎉

```bash
docker-compose up
```

Otwórz http://localhost:3000 i zacznij kodować! 🚀
