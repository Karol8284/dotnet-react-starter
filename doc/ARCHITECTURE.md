backend/
├── src/
│   ├── ECommerce.API/                          # Entry point aplikacji
│   │   ├── Controllers/
│   │   │   ├── ProductsController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── UsersController.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── AuthenticationMiddleware.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── ApplicationBuilderExtensions.cs
│   │   ├── Configurations/
│   │   │   ├── AppSettings.cs
│   │   │   └── JwtSettings.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Program.cs
│   │   └── ECommerce.API.csproj
│   │
│   ├── ECommerce.Application/                 # Business Logic (UseCase, Service)
│   │   ├── Services/
│   │   │   ├── IProductService.cs
│   │   │   ├── ProductService.cs
│   │   │   ├── IOrderService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── IUserService.cs
│   │   │   └── UserService.cs
│   │   ├── DTOs/
│   │   │   ├── Product/
│   │   │   │   ├── CreateProductDto.cs
│   │   │   │   ├── UpdateProductDto.cs
│   │   │   │   └── ProductDto.cs
│   │   │   ├── Order/
│   │   │   │   ├── CreateOrderDto.cs
│   │   │   │   └── OrderDto.cs
│   │   │   └── User/
│   │   │       ├── RegisterUserDto.cs
│   │   │       └── UserDto.cs
│   │   ├── Validators/
│   │   │   ├── ProductValidator.cs
│   │   │   ├── OrderValidator.cs
│   │   │   └── UserValidator.cs
│   │   ├── Mappers/
│   │   │   ├── ProductMapper.cs
│   │   │   ├── OrderMapper.cs
│   │   │   └── UserMapper.cs
│   │   └── ECommerce.Application.csproj
│   │
│   ├── ECommerce.Domain/                      # Entities, Interfaces, Business Rules
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── User.cs
│   │   │   ├── Category.cs
│   │   │   └── BaseEntity.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── IService.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   └── Price.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── ProductNotFoundException.cs
│   │   │   ├── OrderNotFoundException.cs
│   │   │   └── InvalidOperationException.cs
│   │   ├── Enums/
│   │   │   ├── OrderStatus.cs
│   │   │   ├── PaymentStatus.cs
│   │   │   └── UserRole.cs
│   │   └── ECommerce.Domain.csproj
│   │
│   ├── ECommerce.Infrastructure/              # Database, External Services
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Migrations/
│   │   │   │   ├── Migration001_InitialCreate.cs
│   │   │   │   └── ApplicationDbContextModelSnapshot.cs
│   │   │   └── Configuration/
│   │   │       ├── ProductConfiguration.cs
│   │   │       ├── OrderConfiguration.cs
│   │   │       └── UserConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── Repository.cs
│   │   │   ├── ProductRepository.cs
│   │   │   ├── OrderRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── PaymentService.cs
│   │   │   ├── FileStorageService.cs
│   │   │   └── CacheService.cs
│   │   ├── Seeders/
│   │   │   ├── DatabaseSeeder.cs
│   │   │   └── InitialDataSeeder.cs
│   │   └── ECommerce.Infrastructure.csproj
│   │
│   └── ECommerce.Shared/                      # Shared Utilities, Constants
│       ├── Constants/
│       │   ├── AppConstants.cs
│       │   ├── ErrorMessages.cs
│       │   └── ValidationMessages.cs
│       ├── Utils/
│       │   ├── DateTimeHelper.cs
│       │   ├── StringHelper.cs
│       │   └── ValidationHelper.cs
│       ├── Responses/
│       │   ├── ApiResponse.cs
│       │   ├── PaginatedResponse.cs
│       │   └── ErrorResponse.cs
│       └── ECommerce.Shared.csproj
│
├── tests/
│   ├── ECommerce.UnitTests/                   # Unit Tests
│   │   ├── Application/
│   │   │   ├── Application/
│   │   │   │   ├── ProductServiceTests.cs
│   │   │   │   ├── OrderServiceTests.cs
│   │   │   │   └── UserServiceTests.cs
│   │   │   ├── Validators/
│   │   │   │   ├── ProductValidatorTests.cs
│   │   │   │   └── OrderValidatorTests.cs
│   │   │   └── Mappers/
│   │   │       └── ProductMapperTests.cs
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── ProductTests.cs
│   │   │   │   ├── OrderTests.cs
│   │   │   │   └── UserTests.cs
│   │   │   └── ValueObjects/
│   │   │       └── MoneyTests.cs
│   │   ├── Infrastructure/
│   │   │   ├── Repositories/
│   │   │   │   ├── ProductRepositoryTests.cs
│   │   │   │   └── OrderRepositoryTests.cs
│   │   │   └── Services/
│   │   │       ├── EmailServiceTests.cs
│   │   │       └── PaymentServiceTests.cs
│   │   ├── Fixtures/
│   │   │   ├── ProductFixture.cs
│   │   │   ├── OrderFixture.cs
│   │   │   └── UserFixture.cs
│   │   ├── Mocks/
│   │   │   ├── MockProductRepository.cs
│   │   │   └── MockOrderRepository.cs
│   │   ├── appsettings.Test.json
│   │   └── ECommerce.UnitTests.csproj
│   │
│   ├── ECommerce.IntegrationTests/            # Integration Tests
│   │   ├── API/
│   │   │   ├── Controllers/
│   │   │   │   ├── ProductsControllerTests.cs
│   │   │   │   ├── OrdersControllerTests.cs
│   │   │   │   └── UsersControllerTests.cs
│   │   ├── Database/
│   │   │   ├── DatabaseFixture.cs
│   │   │   └── RepositoryIntegrationTests.cs
│   │   ├── Services/
│   │   │   └── ServiceIntegrationTests.cs
│   │   ├── appsettings.Test.json
│   │   └── ECommerce.IntegrationTests.csproj
│   │
│   └── ECommerce.E2ETests/                    # End-to-End Tests
│       ├── Scenarios/
│       │   ├── ProductScenarios.cs
│       │   ├── OrderScenarios.cs
│       │   └── UserScenarios.cs
│       ├── Steps/
│       │   ├── ProductSteps.cs
│       │   ├── OrderSteps.cs
│       │   └── UserSteps.cs
│       ├── Fixtures/
│       │   └── ApiClientFixture.cs
│       └── ECommerce.E2ETests.csproj
│
├── .github/
│   └── workflows/
│       ├── ci-build.yml
│       ├── ci-tests.yml
│       └── ci-deploy.yml
│
├── docker/
│   ├── Dockerfile
│   └── Dockerfile.dev
│
├── docker-compose.yml
├── docker-compose.dev.yml
│
├── ECommerce.sln
├── .gitignore
├── .editorconfig
├── README.md
└── ARCHITECTURE.md