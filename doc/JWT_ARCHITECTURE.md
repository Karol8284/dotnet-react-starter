# 🏗️ Pełna Struktura JWT w Projekcie - Szczegółowe Wyjaśnienie

## 1️⃣ ARCHITEKTURA WARSTWOWA (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────────┐
│                         API LAYER                               │
│  (Prezentacja - Controllers, Middleware, Konfiguracja)         │
│  📁 API/Controllers/AuthController.cs                           │
│  📁 API/Program.cs (DI + konfiguracja JWT)                      │
│  📁 API/Filters/ValidationFilterAttribute.cs                    │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                            │
│  (Logika biznesowa - Serwisy, DTOs, Interfejsy)               │
│  📁 Application/Services/MockAuthService.cs                     │
│  📁 Application/DTOs/Auth/LoginUserDto.cs                       │
│  📁 Application/DTOs/Auth/RegisterUserDto.cs                    │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER                                │
│  (Logika domeny - Encje, Interfejsy, Enums - BEZ frameworków) │
│  📁 Domain/Entities/User.cs (zawiera JwtTokens!)               │
│  📁 Domain/Interfaces/IJwtTokenService.cs                       │
│  📁 Domain/Interfaces/IAuthService.cs                           │
│  📁 Domain/Enums/UserRole.cs                                    │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                  INFRASTRUCTURE LAYER                           │
│  (Implementacje - Serwisy, DB, Pakiety zewnętrzne)            │
│  📁 Infrastructure/Services/JwtTokenService.cs                  │
│  📁 Infrastructure/Data/ApplicationDbContext.cs                 │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SHARED LAYER                                │
│  (Wspólne - DTOs, Ustawienia, Responses)                       │
│  📁 Shared/Settings/JwtSettings.cs                              │
│  📁 Shared/Responses/ApiResponse.cs                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2️⃣ ZALEŻNOŚCI I PRZEPŁYW DANYCH

### **KIERUNEK ZALEŻNOŚCI** (co zależy od czego):

```
API ───────────────────────────────────────┐
│                                           ▼
│  ┌────► Application ────► Domain ◄────┐
│  │                                      │
│  ▼                                      ▼
Infrastructure ◄─────────────────────────┘
│
└─────► Shared (wszystkie warstwy mogą z niego korzystać)
```

### **PRZEPŁYW HTTP - LOGIN:**

```
1. FRONTEND wysyła:
   POST /api/auth/login
   { email: "test@example.com", password: "password123" }
   
   ↓
   
2. API LAYER - AuthController
   📁 API/Controllers/AuthController.cs:53
   ├─ Odbiera żądanie
   ├─ Waliduje ModelState
   └─ Wywołuje: _authService.AuthenticateAsync(email, password)
   
   ↓
   
3. APPLICATION LAYER - MockAuthService
   📁 Application/Services/MockAuthService.cs:31
   ├─ Implementuje IAuthService
   ├─ Szuka użytkownika (mock: testUser)
   ├─ Weryfikuje hasło
   └─ Zwraca: User (jeśli OK) lub null (jeśli ERROR)
   
   ↓
   
4. DOMAIN LAYER - User Entity
   📁 Domain/Entities/User.cs
   ├─ Encja User (20 linii)
   ├─ Encja JwtTokens (5 linii)
   └─ Zwracana do AuthController
   
   ↓
   
5. API LAYER - AuthController (znowu)
   📁 API/Controllers/AuthController.cs:54
   ├─ Jeśli user == null → return Unauthorized
   └─ Jeśli user != null → idź do kroku 6
   
   ↓
   
6. INFRASTRUCTURE LAYER - JwtTokenService
   📁 Infrastructure/Services/JwtTokenService.cs:36
   ├─ Implementuje IJwtTokenService
   ├─ Czyta ustawienia: _jwtSettings (wstrzyknięte przez DI)
   ├─ Tworzy Access Token (15 minut)
   ├─ Tworzy Refresh Token (256-bit random)
   └─ Zwraca: JwtTokens (Access + Refresh + ExpiresIn)
   
   ↓
   
7. SHARED LAYER - Formatowanie Odpowiedzi
   📁 Shared/Responses/ApiResponse.cs
   └─ Wrappuje wynik w ApiResponse<JwtTokens>
   
   ↓
   
8. API LAYER - Zwrot do Frontendu
   📁 API/Controllers/AuthController.cs:58
   └─ return Ok(ApiResponse<JwtTokens>.Success(...))
   
   ↓
   
9. FRONTEND odbiera:
   {
     "statusCode": 200,
     "message": "Login successful",
     "data": {
       "accessToken": "eyJhbGc...",
       "refreshToken": "jA2p8...",
       "expiresIn": 900
     },
     "timestamp": "2026-05-04T13:55:19Z"
   }
```

---

## 3️⃣ SZCZEGÓŁOWY BREAKDOWN KAŻDEGO KOMPONENTU

### **🔴 DOMAIN LAYER** (Warstwa Domeny - Najmniej zależna)

#### **Plik 1: Domain/Entities/User.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Domain\Entities\User.cs
Namespace: Domain.Entities

Zawiera:
├─ class User
│  └─ 9 właściwości (Id, Email, PasswordHash, DisplayName, AvatarUrl, Role, IsActive, IsEmailConfirmed, CreatedAt)
│
└─ class JwtTokens (wewnątrz tego samego pliku!)
   └─ 3 właściwości (AccessToken, RefreshToken, ExpiresIn)

Dlaczego razem?
✓ JwtTokens to DTO używany przez JWT
✓ Logicznie powiązane z User
✓ Unika cyklicznych zależności
✓ Domain nie ma zależności od Infrastructure
```

#### **Plik 2: Domain/Interfaces/IJwtTokenService.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Domain\Interfaces\IJwtTokenService.cs
Namespace: Domain.Interfaces

Zawiera:
interface IJwtTokenService
├─ Task<JwtTokens> GenerateTokensAsync(User user)
├─ Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
├─ Task RevokeTokenAsync(string refreshToken)
└─ Task<bool> IsTokenRevokedAsync(string refreshToken)

Dlaczego w Domain?
✓ To interfejs (bez implementacji)
✓ Nie zawiera zależności od bibliotek JWT
✓ Dostępny dla wszystkich warstw
✓ Application i Infrastructure mogą go implementować
```

#### **Plik 3: Domain/Interfaces/IAuthService.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Domain\Interfaces\IAuthService.cs
Namespace: Domain.Interfaces

Zawiera:
interface IAuthService
├─ Task<User?> AuthenticateAsync(email, password)
├─ Task<User?> RegisterAsync(email, password, displayName)
├─ Task<bool> LogoutAsync(userId)
├─ Task<bool> UserExistsAsync(email)
└─ ... (8 inne metody)

Dlaczego w Domain?
✓ Definiuje kontrakt logowania
✓ Niezależny od implementacji (mock, real, etc.)
✓ Używany przez API do DI
```

#### **Plik 4: Domain/Enums/UserRole.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Domain\Enums\UserRole.cs

Zawiera:
enum UserRole
├─ User
└─ Admin

Dlaczego w Domain?
✓ Część logiki domenowej (role-based authorization)
✓ Brak zależności od frameworków
```

---

### **🟠 SHARED LAYER** (Warstwa Wspólna)

#### **Plik 1: Shared/Settings/JwtSettings.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Shared\Settings\JwtSettings.cs
Namespace: Shared.Settings

Zawiera:
class JwtSettings
├─ string Secret
├─ string Issuer
├─ string Audience
└─ int ExpiresInMinutes

Dlaczego w Shared?
✓ Używany przez Infrastructure (JwtTokenService)
✓ Używany przez API (Program.cs)
✓ Wspólny dla wielu warstw
✓ Musi być dostępny wszędzie
```

#### **Plik 2: Shared/Responses/ApiResponse.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Shared\Responses\ApiResponse.cs
Namespace: Shared.Responses

Zawiera:
class ApiResponse<T>
├─ int StatusCode
├─ string Message
├─ T? Data
├─ List<ErrorDetail>? Errors
├─ DateTime Timestamp
└─ static methods: Success(), Error()

class ApiResponse (non-generic)
└─ Bez Data (dla operacji bez zwracanego obiektu)

class ErrorDetail
├─ string? Field
├─ string Message
└─ string? Code

Dlaczego w Shared?
✓ Ustandaryzowana odpowiedź API (wszędzie)
✓ Używana w AuthController
✓ Współdzielona między warstwami
```

---

### **🟡 APPLICATION LAYER** (Warstwa Aplikacji)

#### **Plik 1: Application/Services/MockAuthService.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Application\Services\MockAuthService.cs
Namespace: Application.Services

Zawiera:
class MockAuthService : IAuthService
├─ private static readonly User TestUser (hardcoded)
├─ ILogger<MockAuthService> _logger
├─ async Task<User?> AuthenticateAsync(email, password)
│  └─ Mock: if (email == "test@example.com" && password == "password123") → return TestUser
├─ async Task<User?> RegisterAsync(email, password, displayName)
│  └─ Mock: zawsze akceptuje, zwraca nowego User z losowym ID
└─ (8 innych metod: logout, verify, reset password, etc.)

Dlaczego w Application?
✓ Tymczasowa implementacja IAuthService
✓ Nie wymaga bazy danych (do testowania JWT)
✓ Zastęp dla rzeczywistego AuthService
✓ Będzie usunięty, gdy będzie rzeczywista implementacja w Infrastructure
```

#### **Plik 2: Application/DTOs/Auth/LoginUserDto.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Application\DTOs\Auth\LoginUserDto.cs
Namespace: Application.DTOs.Auth

Zawiera:
class LoginUserDto
├─ string Email
└─ string Password

Dlaczego w Application?
✓ DTO - transfer danych między warstwami
✓ Mapuje żądanie HTTP na domainę
✓ Zawiera tylko to co potrzebne do logowania
```

#### **Plik 3: Application/DTOs/Auth/RegisterUserDto.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Application\DTOs\Auth\RegisterUserDto.cs

Zawiera:
class RegisterUserDto
├─ string FirstName
├─ string LastName
├─ string Email
├─ string PhoneNumber
├─ string Address
└─ DateTime CreatedAt

Dlaczego w Application?
✓ DTO do rejestracji
✓ Zawiera więcej danych niż Login
```

---

### **🔵 INFRASTRUCTURE LAYER** (Warstwa Infrastruktury)

#### **Plik 1: Infrastructure/Services/JwtTokenService.cs** ⭐ NAJWAŻNIEJSZY
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Infrastructure\Services\JwtTokenService.cs
Namespace: Infrastructure.Services

Zawiera:
class JwtTokenService : IJwtTokenService
├─ private readonly JwtSettings _jwtSettings (wstrzyknięte przez IOptions<JwtSettings>)
├─ private readonly ILogger<JwtTokenService> _logger
├─ private static readonly HashSet<string> RevokedTokens (in-memory blacklist)
├─
├─ PUBLIC METHOD 1: GenerateTokensAsync(User user)
│  ├─ Czyta _jwtSettings (Secret, Issuer, Audience)
│  ├─ Tworzy SymmetricSecurityKey z Secret
│  ├─ Tworzy SigningCredentials (HS256)
│  ├─ Buduje Claims (sub, email, name, role, IsEmailConfirmed)
│  ├─ Tworzy AccessTokenDescriptor (15 minut)
│  ├─ Tworzy RefreshToken (256-bit random)
│  ├─ Zwraca JwtTokens (Access + Refresh + ExpiresIn)
│  └─ Loguje: "✓ Generated tokens for user {UserId}"
│
├─ PUBLIC METHOD 2: ValidateTokenAsync(string token)
│  ├─ Czyta Secret z _jwtSettings
│  ├─ Tworzy SymmetricSecurityKey
│  ├─ Waliduje: IssuerSigningKey, Issuer, Audience, Lifetime
│  ├─ Zwraca ClaimsPrincipal (jeśli OK) lub null (jeśli ERROR)
│  └─ Loguje: "⚠️ Token validation failed"
│
├─ PUBLIC METHOD 3: RevokeTokenAsync(string refreshToken)
│  ├─ Dodaje token do RevokedTokens (HashSet)
│  └─ Loguje: "🔐 Refresh token revoked"
│
├─ PUBLIC METHOD 4: IsTokenRevokedAsync(string refreshToken)
│  └─ Zwraca: RevokedTokens.Contains(refreshToken)
│
└─ PRIVATE METHOD: GenerateRefreshToken()
   ├─ Tworzy 32-byte random number (RandomNumberGenerator)
   └─ Zwraca Base64-encoded string

Zależności:
├─ System.IdentityModel.Tokens.Jwt (Microsoft)
├─ Microsoft.IdentityModel.Tokens (Microsoft)
├─ Microsoft.Extensions.Options (DI)
├─ Microsoft.Extensions.Logging (Logging)
└─ Domain.Entities (User, JwtTokens)

Dlaczego w Infrastructure?
✓ To implementacja (nie interfejs)
✓ Korzysta z bibliotek JWT (system dependency)
✓ Zawiera logikę techniczną (nie domenową)
✓ Testuje się z real JWT library
✓ Można ją łatwo zamienić na inną implementację
```

#### **Plik 2: Infrastructure/Data/ApplicationDbContext.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\Infrastructure\Data\ApplicationDbContext.cs
Namespace: Infrastructure.Data

Zawiera:
class ApplicationDbContext : DbContext
├─ public DbSet<User> Users (będzie dodane!)
├─ public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
└─ protected override void OnModelCreating(ModelBuilder modelBuilder)
   └─ Konfiguracja tabeli User

Dlaczego w Infrastructure?
✓ Bezpośrednia zależność od EF Core
✓ Zawiera konfigurację bazy danych
✓ Tworzy mapowanie Entity → Tabela
```

---

### **🟢 API LAYER** (Warstwa Prezentacji)

#### **Plik 1: API/Controllers/AuthController.cs** ⭐ NAJWAŻNIEJSZY
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\API\Controllers\AuthController.cs
Namespace: API.Controllers

Zawiera:
class AuthController : ControllerBase
├─ private readonly IJwtTokenService _jwtTokenService (wstrzyknięte)
├─ private readonly IAuthService _authService (wstrzyknięte)
├─ private readonly ILogger<AuthController> _logger
│
├─ [HttpPost("login")] ← POST /api/auth/login
│  └─ Logika:
│     1. Waliduje LoginUserDto
│     2. Wywołuje _authService.AuthenticateAsync(email, password)
│     3. Jeśli null → return Unauthorized
│     4. Jeśli User → _jwtTokenService.GenerateTokensAsync(user)
│     5. Zwraca ApiResponse<JwtTokens>.Success(...)
│
├─ [HttpPost("register")] ← POST /api/auth/register
│  └─ Logika: Podobna do login, ale:
│     1. Sprawdza czy user istnieje
│     2. Rejestruje nowego użytkownika
│     3. Zwraca Created(201)
│
├─ [HttpPost("refresh-token")] ← POST /api/auth/refresh-token
│  └─ Logika:
│     1. Sprawdza czy token nie jest revoked
│     2. TODO: Sprawdzić w DB
│     3. Zwraca nowe tokeny
│
├─ [HttpPost("logout")] ← POST /api/auth/logout [Authorize]
│  └─ Logika:
│     1. Pobiera userId z ClaimsPrincipal (from JWT)
│     2. Wywołuje _jwtTokenService.RevokeTokenAsync(refreshToken)
│     3. Zwraca ApiResponse.Success(200)
│
├─ [HttpGet("me")] ← GET /api/auth/me [Authorize]
│  └─ Logika:
│     1. Pobiera claims z JWT
│     2. Zwraca dane użytkownika (sub, email, name, role)
│     3. Zwraca ApiResponse<object>.Success(...)
│
└─ [HttpPost("verify-token")] ← POST /api/auth/verify-token
   └─ Logika:
      1. Pobiera token z żądania
      2. Wywołuje _jwtTokenService.ValidateTokenAsync(token)
      3. Zwraca { isValid: true/false }

Request DTOs (wewnątrz kontrolera):
├─ RefreshTokenRequest { RefreshToken: string }
├─ LogoutRequest { RefreshToken: string }
└─ VerifyTokenRequest { Token: string }

Dlaczego w API?
✓ To HTTP endpoints
✓ Odbiera żądania HTTP, zwraca odpowiedzi
✓ Orkestruje przepływ między warstwami
✓ Zawiera business logic (walidacja, logowanie)
```

#### **Plik 2: API/Program.cs** ⭐ KONFIGURACJA
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\API\Program.cs

Zawiera:
┌─ SERILOG CONFIGURATION
│  └─ Log.Logger = new LoggerConfiguration()...
│
├─ DEPENDENCY INJECTION (DI)
│  ├─ builder.Services.AddDbContext<ApplicationDbContext>(...) [EF Core]
│  ├─ builder.Services.Configure<JwtSettings>(...)  [Ustawienia]
│  ├─ builder.Services.AddScoped<IJwtTokenService, JwtTokenService>() [JWT]
│  ├─ builder.Services.AddScoped<IAuthService, MockAuthService>() [Auth - Mock]
│  └─ builder.Services.AddScoped<ValidationFilterAttribute>() [Walidacja]
│
├─ AUTHENTICATION CONFIGURATION
│  ├─ builder.Services.AddAuthentication(options => ...)
│  └─ builder.Services.AddAuthentication().AddJwtBearer(options => ...)
│     ├─ ValidateIssuerSigningKey = true
│     ├─ IssuerSigningKey = new SymmetricSecurityKey(...)
│     ├─ ValidateIssuer = true
│     ├─ ValidateAudience = true
│     ├─ ValidateLifetime = true
│     └─ ClockSkew = TimeSpan.Zero
│
├─ MIDDLEWARE PIPELINE
│  ├─ app.UseMiddleware<ExceptionHandlingMiddleware>()
│  ├─ app.UseSwagger() [OpenAPI]
│  ├─ app.UseAuthentication() ← WAŻNE!
│  ├─ app.UseAuthorization() ← WAŻNE!
│  ├─ app.MapControllers()
│  └─ app.UseHttpsRedirection()
│
└─ DATABASE INITIALIZATION
   ├─ var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
   ├─ await dbContext.Database.EnsureCreatedAsync()
   └─ Tworzy tabelę User (jeśli nie istnieje)

Dlaczego takie rozmieszczenie?
✓ DI registruje serwisy PRZED ich użyciem
✓ Middleware pipeline: Auth musi być PRZED Authorization
✓ JwtSettings musi być Configure() PRZED AddJwtBearer()
✓ JwtTokenService musi być Scoped (nowy na każde żądanie)
```

#### **Plik 3: API/Filters/ValidationFilterAttribute.cs**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\API\Filters\ValidationFilterAttribute.cs
Namespace: API.Filters

Zawiera:
class ValidationFilterAttribute : ActionFilterAttribute
├─ public override void OnActionExecuting(ActionExecutingContext context)
│  ├─ if (!context.ModelState.IsValid)
│  │  ├─ Pobiera błędy walidacji
│  │  ├─ Mapuje na słownik
│  │  └─ Zwraca BadRequest z błędami
│  └─ base.OnActionExecuting(context)

Dlaczego tutaj?
✓ Globalna walidacja dla wszystkich endpointów
✓ Sprawdza ModelState (Data Annotations)
✓ Ustandaryzowany format błędów
```

#### **Plik 4: API/appsettings.json**
```
Ścieżka: C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\API\appsettings.json

Zawiera:
{
  "DefaultConnection": "Host=localhost;Port=5432;Database=...",
  "Jwt": {
    "Secret": "your-super-secret-key-...",
    "Issuer": "Dotnet-React-Starter-API",
    "Audience": "Dotnet-React-Starter-App"
  },
  "Logging": {...}
}

Dlaczego tutaj?
✓ Ustawienia aplikacji (environment-specific)
✓ Łatwe do zmiany bez recompile
✓ JwtSettings mapuje się z tej sekcji [Jwt]
```

#### **Plik 5: API/secrets.json** (lokalne, nie w Git!)
```
Ścieżka: C:\Users\<user>\AppData\Roaming\Microsoft\UserSecrets\<guid>\secrets.json

Zawiera:
{
  "DefaultConnection": "Host=db;Port=5432;Username=postgres;Password=actual_password",
  "Jwt": {
    "Secret": "actual-super-secret-key-123456789012345"
  }
}

Dlaczego tutaj?
✓ Sensitive data (nie commituj!)
✓ local override dla appsettings.json
✓ Inna konfiguracja na dev vs prod
```

---

## 4️⃣ PRZEPŁYW ZALEŻNOŚCI - DEPENDENCY INJECTION

```
┌─────────────────────────────────────────────────────────────────┐
│ API/Program.cs - CONFIGURATION                                  │
│                                                                  │
│ var jwtSettings = Configuration.GetSection("Jwt").Get<...>()   │
│ builder.Services.Configure<JwtSettings>(...)                    │
│ builder.Services.AddScoped<IJwtTokenService, JwtTokenService>() │
│ builder.Services.AddScoped<IAuthService, MockAuthService>()     │
│                                                                  │
└──────────────────────┬──────────────────────────────────────────┘
                       │
          ↓ DI Container Builds ↓
                       │
    ┌──────────────────┴──────────────────┐
    │                                      │
    ▼                                      ▼
┌─────────────────────────────┐  ┌─────────────────────────────┐
│ JwtTokenService             │  │ MockAuthService             │
│ ────────────────            │  │ ───────────────             │
│ ctor(IOptions<JwtSettings>  │  │ ctor(ILogger<...>)          │
│      ILogger<...>)          │  │                             │
│                             │  │ Implementuje:               │
│ Implementuje:               │  │ IAuthService                │
│ IJwtTokenService            │  │                             │
└─────────────────────────────┘  └─────────────────────────────┘
    ▲                                   ▲
    │                                   │
    └───────────────┬───────────────────┘
                    │
                    │ Injected via Constructor
                    │
                    ▼
          ┌──────────────────────┐
          │ AuthController       │
          │ ───────────────      │
          │ ctor(IJwtTokenService│
          │      IAuthService    │
          │      ILogger<...>)   │
          └──────────────────────┘
                    ▲
                    │ HTTP Request
                    │
                    ▼
          ┌──────────────────────┐
          │ Frontend/Postman      │
          │ POST /api/auth/login  │
          └──────────────────────┘
```

---

## 5️⃣ REFERENCJE MIĘDZY PROJEKTAMI

```
API.csproj
├─ ProjectReference: Application (Application.csproj)
│  └─ Używa: MockAuthService, DTOs
├─ ProjectReference: Domain (Domain.csproj)
│  └─ Używa: IJwtTokenService, IAuthService, User, Entities
├─ ProjectReference: Infrastructure (Infrastructure.csproj)
│  └─ Używa: JwtTokenService, ApplicationDbContext
├─ ProjectReference: Shared (Shared.csproj)
│  └─ Używa: JwtSettings, ApiResponse
└─ PackageReference: FluentValidation.AspNetCore, Swashbuckle, etc.

Application.csproj
├─ ProjectReference: Domain (Domain.csproj)
│  └─ Używa: IJwtTokenService, IAuthService, User, Entities
├─ ProjectReference: Shared (Shared.csproj)
│  └─ Używa: JwtSettings (jeśli potrzebne)
└─ PackageReference: Microsoft.Extensions.Logging

Infrastructure.csproj
├─ ProjectReference: Domain (Domain.csproj)
│  └─ Używa: IJwtTokenService, User, Entities
├─ ProjectReference: Shared (Shared.csproj)
│  └─ Używa: JwtSettings
├─ PackageReference: System.IdentityModel.Tokens.Jwt
├─ PackageReference: Microsoft.IdentityModel.Tokens
├─ PackageReference: Microsoft.EntityFrameworkCore
└─ PackageReference: Npgsql.EntityFrameworkCore.PostgreSQL

Domain.csproj
├─ ProjectReference: Shared (Shared.csproj)
│  └─ (NIE implementuje, to by była cykliczna zależność)
└─ NO external dependencies ✅

Shared.csproj
└─ NO project dependencies
   (Shared nie zależy od niczego w projekcie)

⚠️ WAŻNE REGUŁY:
✓ API → Application, Domain, Infrastructure, Shared (wszystkie)
✓ Application → Domain, Shared (nie Infrastructure!)
✓ Infrastructure → Domain, Shared (nie Application!)
✓ Domain → Shared (MOŻE, tylko jeśli potrzebne)
✓ Shared → NIC (niezależny)
✗ NIE WOLNO: Application → Infrastructure (wracanie w górę!)
✗ NIE WOLNO: Domain → Application, API (łamanie Clean Architecture)
```

---

## 6️⃣ DLACZEGO TAK A NIE INACZEJ - WYJAŚNIENIA

### ❓ Dlaczego JwtTokens w Domain.Entities, a nie Shared.DTOs?

**ODPOWIEDŹ:**
- JwtTokens to DTO zwracane z generowania tokena
- Ale jest tak ściśle powiązane z User, że umieszczenie go w Domain.Entities ma sens
- Unika cyklicznych zależności (Domain → Shared → Domain)
- Wszystkie modele zwracane przez IJwtTokenService powinny być w Domain

### ❓ Dlaczego IJwtTokenService w Domain.Interfaces, a JwtTokenService w Infrastructure?

**ODPOWIEDŹ:**
```
Interfejs (IJwtTokenService) w Domain:
✓ Niezależny od implementacji
✓ Brak zależności od System.IdentityModel.Tokens.Jwt
✓ Dostępny dla wszystkich warstw
✓ Definiuje kontrakt

Implementacja (JwtTokenService) w Infrastructure:
✓ Zawiera biblioteki JWT (system dependency)
✓ Techniczna, a nie domenowa
✓ Łatwo zamienić na inną implementację (np. OAuth, OpenIdConnect)
```

### ❓ Dlaczego MockAuthService w Application, a nie Infrastructure?

**ODPOWIEDŹ:**
```
MockAuthService w Application:
✓ Tymczasowy (do testowania bez bazy)
✓ Nie wymaga bibliotek External (tylko User entity)
✓ Łatwo usunąć, gdy będzie real implementacja
✓ Domain → Application jest OK

Rzeczywisty AuthService będzie w Infrastructure:
✓ Będzie wymagać EF Core
✓ Będzie haszować hasła (BCrypt - external library)
✓ Będzie wchodzić w interakcję z DB
```

### ❓ Dlaczego Shared/Settings/JwtSettings, a nie infrastruktura?

**ODPOWIEDŹ:**
```
JwtSettings musi być w Shared, bo:
✓ Używany w API/Program.cs (konfiguracja globalna)
✓ Używany w Infrastructure/JwtTokenService
✓ Wstrzykiwany przez IOptions<JwtSettings>
✓ Musi być dostępny dla DI w API

Jeśli byłby w Infrastructure:
✗ API nie mogłaby go importować (wracanie w górę!)
✗ Łamałoby Clean Architecture
```

### ❓ Dlaczego use app.UseAuthentication() PRZED app.UseAuthorization()?

**ODPOWIEDŹ:**
```
Middleware pipeline order:
1. UseAuthentication() → Buduje ClaimsPrincipal z JWT
2. UseAuthorization() → Sprawdza czy ma [Authorize]

Jeśli byłoby odwrotnie:
✗ UseAuthorization() nie wiedziałoby, kto to jest
✗ [Authorize] zawsze by failowało
```

---

## 7️⃣ PODSUMOWANIE - MAPA PROJEKTU

```
📁 C:\My Private programs\Programing\MAIN\dotnet-react-starter\backend\

├─ 📁 Domain (Logika domeny)
│  ├─ 📁 Entities
│  │  ├─ User.cs (zawiera też JwtTokens!)
│  │  └─ ...
│  ├─ 📁 Interfaces
│  │  ├─ IJwtTokenService.cs ⭐ (4 metody JWT)
│  │  ├─ IAuthService.cs ⭐ (11 metod auth)
│  │  └─ ...
│  └─ 📁 Enums
│     └─ UserRole.cs (User, Admin)
│
├─ 📁 Application (Logika biznesowa)
│  ├─ 📁 Services
│  │  └─ MockAuthService.cs ⭐ (Mock dla testów)
│  ├─ 📁 DTOs
│  │  └─ Auth
│  │     ├─ LoginUserDto.cs
│  │     └─ RegisterUserDto.cs
│  └─ ...
│
├─ 📁 Infrastructure (Implementacje)
│  ├─ 📁 Services
│  │  └─ JwtTokenService.cs ⭐ (Implementacja JWT)
│  ├─ 📁 Data
│  │  └─ ApplicationDbContext.cs (EF Core DbContext)
│  └─ ...
│
├─ 📁 API (Prezentacja)
│  ├─ 📁 Controllers
│  │  └─ AuthController.cs ⭐ (6 endpointów JWT)
│  ├─ 📁 Filters
│  │  └─ ValidationFilterAttribute.cs
│  ├─ 📁 Middleware
│  │  └─ ...
│  ├─ Program.cs ⭐ (DI + Konfiguracja)
│  ├─ appsettings.json (Ustawienia publiczne)
│  └─ secrets.json (Dane wrażliwe - local)
│
├─ 📁 Shared (Wspólne)
│  ├─ 📁 Settings
│  │  └─ JwtSettings.cs ⭐ (Konfiguracja JWT)
│  ├─ 📁 Responses
│  │  ├─ ApiResponse.cs (Generic wrapper)
│  │  └─ ErrorDetail.cs
│  └─ ...
│
└─ 📁 Tests (Unit Tests - nieobowiązkowe)
   ├─ UnitTests
   ├─ IntegrationTests
   └─ E2ETests
```

---

## 🎯 PODSUMOWANIE

| Komponent | Lokalizacja | Rola | Zależności |
|-----------|-------------|------|-----------|
| **IJwtTokenService** | Domain/Interfaces | Definiuje interfejs JWT | Brak |
| **JwtTokenService** | Infrastructure/Services | Implementuje JWT | System.IdentityModel.Tokens.Jwt |
| **IAuthService** | Domain/Interfaces | Definiuje logowanie | Brak |
| **MockAuthService** | Application/Services | Mock logowania | ILogger, User |
| **JwtTokens** | Domain/Entities | DTO tokeny | Brak |
| **User** | Domain/Entities | Encja użytkownika | Brak |
| **JwtSettings** | Shared/Settings | Config JWT | Brak |
| **AuthController** | API/Controllers | Endpointy HTTP | IJwtTokenService, IAuthService |
| **ApiResponse<T>** | Shared/Responses | Standardowa odpowiedź | Brak |
| **Program.cs** | API/ | Konfiguracja DI | Wszystkie |

---

**Teraz mogłeś zobaczyć CAŁĄ strukturę JWT!** 🚀 Każdy komponent ma swoje miejsce, każda warstwa ma swoją rolę. Clean Architecture w akcji!