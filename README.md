# 🛒 Dawazon 2.0

> **Plataforma de comercio electrónico full-stack** construida con arquitectura moderna, containerización completa y testing automatizado.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
[![Playwright](https://img.shields.io/badge/Playwright-45BA4B?style=for-the-badge&logo=playwright)](https://playwright.dev/)
[![NUnit](https://img.shields.io/badge/Tests_NUnit-8CBE2D?style=for-the-badge)](https://nunit.org/)
[![GitHub Actions](https://img.shields.io/badge/CI/CD-GitHub_Actions-2088FF?style=for-the-badge&logo=github-actions)](https://github.com/features/actions)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](./LICENSE)
[![Render](https://img.shields.io/badge/Deployed-Render-46E3B7?style=for-the-badge&logo=render)](https://render.com/)

---

## 📋 Tabla de Contenidos

- [🎯 Descripción](#-descripción)
- [🛠 Tecnologías](#-tecnologías)
- [🏗 Arquitectura](#-arquitectura)
- [✨ Características](#-características)
- [📁 Estructura del Proyecto](#-estructura-del-proyecto)
- [🚀 Instalación y Ejecución](#-instalación-y-ejecución)
  - [Docker Compose (Recomendado)](#docker-compose-recomendado)
  - [Ejecución Local](#ejecución-local)
  - [Variables de Entorno](#variables-de-entorno)
- [🧪 Testing](#-testing)
  - [Pruebas Unitarias](#pruebas-unitarias-nunit)
  - [Pruebas API (Bruno)](#pruebas-api-bruno)
  - [Pruebas E2E (Playwright)](#pruebas-e2e-playwright)
- [🔄 CI/CD](#-cicd)
- [📦 Despliegue](#-despliegue)
- [📡 API](#-api)
- [🤝 Contribución](#-contribución)
- [📄 Licencia](#-licencia)

---

## 🎯 Descripción

**Dawazon 2.0** es una plataforma de comercio electrónico de siguiente generación que combina un **frontend moderno en Blazor** con un **backend robusto en ASP.NET Core**. El proyecto implementa las mejores prácticas de desarrollo de software:

- ✅ **Arquitectura limpia** con separación de responsabilidades
- ✅ **Containerización completa** con Docker
- ✅ **Testing automatizado** en 3 niveles (unitario, API, E2E)
- ✅ **Integración continua** con GitHub Actions
- ✅ **Documentación automática** con Doxygen
- ✅ **Despliegue automático** a producción

---

## 🛠 Tecnologías

### Backend
| Tecnología | Propósito |
|------------|-----------|
| ASP.NET Core 10 | Framework web moderno |
| Entity Framework Core | ORM para base de datos |
| PostgreSQL | Base de datos relacional |
| Redis | Caché de alto rendimiento |
| JWT | Autenticación segura |
| Stripe | Procesamiento de pagos |
| GraphQL (HotChocolate) | Consultas flexibles |
| Serilog | Logging estructurado |
| MailKit | Envío de emails |

### Frontend
| Tecnología | Propósito |
|------------|-----------|
| Blazor WebAssembly | UI interactiva |
| Razor Pages | Renderizado server-side |
| ASP.NET Core Identity | Gestión de usuarios |
| SignalR | Tiempo real |

### Testing & DevOps
| Tecnología | Propósito |
|------------|-----------|
| NUnit | Pruebas unitarias |
| Playwright | Pruebas E2E |
| Bruno | Pruebas de API |
| Doxygen | Documentación de código |
| GitHub Actions | CI/CD |
| Docker | Containerización |
| Render | Despliegue en la nube |

---

## 🏗 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DAWAZON 2.0                                    │
│                         ARQUITECTURA DEL SISTEMA                            │
└─────────────────────────────────────────────────────────────────────────────┘

                                    │
                    ┌───────────────┴───────────────┐
                    │         CLIENTE (Browser)       │
                    │    ┌───────────────────────┐   │
                    │    │   Frontend Blazor     │   │
                    │    │   Puerto: 8080/8081  │   │
                    │    └───────────────────────┘   │
                    └───────────────┬───────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DOCKER COMPOSE                                 │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                         CONTENEDOR NGINX                              │   │
│  │                      (Reverse Proxy / SSL)                          │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│           ┌────────────────────────┼────────────────────────┐              │
│           ▼                        ▼                        ▼              │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐       │
│  │   dawazon2.0    │    │ dawazonbackend  │    │   PostgreSQL    │       │
│  │   (Frontend)    │◄──►│   (API REST)    │◄─►│   (Database)    │       │
│  │   Blazor/.NET   │    │  ASP.NET Core   │    │    Puerto 5432  │       │
│  │   Puerto 5041   │    │   Puerto 5041   │    └─────────────────┘       │
│  └─────────────────┘    └─────────────────┘             │                  │
│                                                          ▼                  │
│                                                 ┌─────────────────┐        │
│                                                 │     Redis       │        │
│                                                 │    (Cache)      │        │
│                                                 │   Puerto 6379  │        │
│                                                 └─────────────────┘        │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                           CAPAS DE LA APLICACIÓN                            │
└─────────────────────────────────────────────────────────────────────────────┘

    ┌─────────────────────────────────────────────────────────────┐
    │                    PRESENTACIÓN (Blazor)                     │
    │   Pages/  Components/  Models/  Mapper/  Controllers/       │
    └─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
    ┌─────────────────────────────────────────────────────────────┐
    │                  API REST (ASP.NET Core)                     │
    │   RestControllers/  Middleware/  Filters/  Validators/       │
    └─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
    ┌─────────────────────────────────────────────────────────────┐
    │                    SERVICIOS (Domain)                        │
    │   Users/  Products/  Cart/  Stripe/  Email/  Storage/       │
    └─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
    ┌─────────────────────────────────────────────────────────────┐
    │                 REPOSITORIOS (Data Access)                    │
    │   Entity Framework Core  PostgreSQL  Redis Cache            │
    └─────────────────────────────────────────────────────────────┘
```

---

## ✨ Características

### 🔐 Autenticación y Autorización
- Registro y login de usuarios
- Autenticación JWT con refresh tokens
- Roles: Usuario, Manager, Administrador
- Protección de rutas y endpoints

### 🛒 Carrito de Compras
- Añadir/eliminar productos
- Gestión de cantidades
- Persistencia en base de datos
- Checkout con Stripe

### 📦 Productos
- Catálogo de productos con categorías
- Búsqueda y filtrado
- Gestión de inventario (solo managers)
- Sistema de comentarios y valoraciones

### 📊 Administración
- Panel de administración
- Gestión de usuarios (banear, editar)
- Estadísticas de ventas
- Gestión de pedidos

### 📨 Notificaciones
- Emails transaccionales (registro, pedido)
- Notificaciones en tiempo real (SignalR)
- Recordatorios de carrito abandonado

### 📄 Documentación
- API documentada con OpenAPI/Swagger
- Pruebas de API con Bruno
- Documentación del código con Doxygen
- Informes de tests en GitHub Pages

---

## 📁 Estructura del Proyecto

```
dawazon-2.0/
│
├── 📄 ARCHIVOS DE CONFIGURACIÓN
│   ├── compose.yaml                  # Orquestación de contenedores
│   ├── Dockerfile                    # Imagen Docker principal
│   ├── Dockerfile-Render              # Imagen para Render
│   ├── Dockerfile-playWrite           # Imagen para tests E2E
│   ├── Dockerfile-bruno              # Imagen para tests API
│   ├── dawazon2.0.slnx              # Solución .NET
│   ├── .dockerignore                 # Exclusiones Docker
│   ├── .gitignore                    # Exclusiones Git
│   └── LICENSE                       # Licencia MIT
│
├── 📁 DAWAZON2.0 (Frontend - Blazor)
│   ├── Program.cs                    # Punto de entrada
│   ├── dawazon2.0.csproj             # Proyecto
│   ├── appsettings.json              # Configuración
│   │
│   ├── 🗂 Components/                 # Componentes Blazor
│   │   └── ...
│   │
│   ├── 🗂 Pages/                      # Páginas Razor
│   │   ├── Index.cshtml
│   │   ├── Auth/
│   │   │   ├── Login.cshtml
│   │   │   ├── Register.cshtml
│   │   │   └── Logout.cshtml
│   │   ├── Privacy.cshtml
│   │   └── Error.cshtml
│   │
│   ├── 🗂 RestControllers/            # API REST
│   │   ├── AuthController.cs
│   │   ├── ProductsCotroller.cs
│   │   ├── CartController.cs
│   │   └── UserAdminController.cs
│   │
│   ├── 🗂 MvcControllers/            # Controllers MVC
│   │   ├── UserMvcController.cs
│   │   ├── ProductsMvcController.cs
│   │   ├── CartMvcController.cs
│   │   ├── ManagerMvcController.cs
│   │   └── AdminMvcController.cs
│   │
│   ├── 🗂 Models/                     # ViewModels
│   │   ├── LoginModelView.cs
│   │   ├── RegisterModelView.cs
│   │   ├── ProductDetailViewModel.cs
│   │   ├── CartOrderSummaryViewModel.cs
│   │   ├── AdminUserListViewModel.cs
│   │   └── ... (20+ archivos)
│   │
│   ├── 🗂 Mapper/                     # Mapeadores
│   │   ├── UserMapper.cs
│   │   ├── ProductMvcMapper.cs
│   │   └── CartMvcMapper.cs
│   │
│   ├── 🗂 Pdf/                        # Generación PDF
│   │   ├── IOrderPdfService.cs
│   │   └── OrderPdfService.cs
│   │
│   ├── 🗂 Middleware/                 # Middleware personalizado
│   │   └── GlobalExceptionHandler.cs
│   │
│   ├── 🗂 Session/                    # Gestión de sesión
│   │   └── SessionExtensions.cs
│   │
│   ├── 🗂 Infrastructures/            # Configuraciones
│   │   ├── AuthenticationConfig.cs
│   │   ├── CorsConfig.cs
│   │   ├── DbConfig.cs
│   │   ├── CacheConfig.cs
│   │   ├── EmailConfig.cs
│   │   ├── SerilogConfig.cs
│   │   ├── StorageConfig.cs
│   │   ├── IdentitySeeder.cs
│   │   ├── CartCleanupBackgroundService.cs
│   │   └── ...
│   │
│   └── 🗂 wwwroot/                    # Archivos estáticos
│       ├── css/
│       ├── js/
│       └── uploads/
│
├── 📁 DAWAZONBACKEND (Lógica de Negocio)
│   ├── Program.cs
│   ├── dawazonBackend.csproj
│   │
│   ├── 🗂 Users/                      # Módulo de Usuarios
│   │   ├── Models/
│   │   │   ├── User.cs
│   │   │   └── UserRoles.cs
│   │   ├── Dto/
│   │   │   ├── UserDto.cs
│   │   │   ├── UserRequestDto.cs
│   │   │   ├── LoginDto.cs
│   │   │   ├── RegisterDto.cs
│   │   │   └── AuthResponseDto.cs
│   │   ├── Mapper/
│   │   │   └── UserMapper.cs
│   │   ├── Errors/
│   │   │   └── UserError.cs
│   │   └── Service/
│   │       ├── IUserService.cs
│   │       ├── UserService.cs
│   │       ├── Auth/
│   │       │   ├── IAuthService.cs
│   │       │   └── AuthService.cs
│   │       ├── Favs/
│   │       │   ├── IFavService.cs
│   │       │   └── FavService.cs
│   │       └── Jwt/
│   │           ├── IJwtService.cs
│   │           ├── JwtService.cs
│   │           ├── IJwtTokenExtractor.cs
│   │           └── JwtTokenExtractor.cs
│   │
│   ├── 🗂 Products/                   # Módulo de Productos
│   │   ├── Models/
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   └── Comment.cs
│   │   ├── Dto/
│   │   │   ├── ProductResponseDto.cs
│   │   │   ├── ProductRequestDto.cs
│   │   │   ├── ProductPatchRequestDto.cs
│   │   │   └── CommentDto.cs
│   │   ├── Mapper/
│   │   │   └── ProductMapper.cs
│   │   ├── Errors/
│   │   │   └── ProductError.cs
│   │   ├── Repository/
│   │   │   ├── Productos/
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   └── ProductRepository.cs
│   │   │   └── Categoria/
│   │   │       ├── ICategoriaRepository.cs
│   │   │       └── CategoryRepository.cs
│   │   └── Service/
│   │       ├── IProductService.cs
│   │       └── ProductService.cs
│   │
│   ├── 🗂 Cart/                      # Módulo de Carrito
│   │   ├── Models/
│   │   │   ├── Cart.cs
│   │   │   ├── CartLine.cs
│   │   │   ├── Client.cs
│   │   │   ├── Address.cs
│   │   │   └── Status.cs
│   │   ├── Dto/
│   │   │   ├── CartResponseDto.cs
│   │   │   ├── LineRequestDto.cs
│   │   │   ├── SaleLineDto.cs
│   │   │   └── ...
│   │   ├── Mapper/
│   │   │   └── CartMapper.cs
│   │   ├── Errors/
│   │   │   └── CartError.cs
│   │   ├── Exceptions/
│   │   │   └── CartException.cs
│   │   ├── Repository/
│   │   │   ├── ICartRepository.cs
│   │   │   └── CartRepository.cs
│   │   └── Service/
│   │       ├── ICartService.cs
│   │       └── CartService.cs
│   │
│   ├── 🗂 Common/                    # Componentes Compartidos
│   │   ├── Database/
│   │   │   └── DawazonDbContext.cs
│   │   ├── Dto/
│   │   │   ├── PageResponseDto.cs
│   │   │   ├── FilterDto.cs
│   │   │   └── AdminStatsDto.cs
│   │   ├── Cache/
│   │   │   ├── ICacheService.cs
│   │   │   └── CacheService.cs
│   │   ├── Mail/
│   │   │   ├── IEmailService.cs
│   │   │   ├── MailKitEmailService.cs
│   │   │   ├── EmailTemplates.cs
│   │   │   ├── EmailMessage.cs
│   │   │   └── EmailBackgroundService.cs
│   │   ├── Storage/
│   │   │   ├── IStorage.cs
│   │   │   └── Storage.cs
│   │   ├── Hub/
│   │   │   └── NotificationHub.cs
│   │   ├── Utils/
│   │   │   └── IdGenerator.cs
│   │   ├── Error/
│   │   │   └── DomainError.cs
│   │   └── Attribute/
│   │       └── GenerateCustomIdAtribute.cs
│   │
│   └── 🗂 Stripe/                     # Pago con Stripe
│       ├── IStripeService.cs
│       ├── StripeService.cs
│       └── Errors/
│           └── StripeError.cs
│
├── 📁 DAWAZONTEST (Pruebas Unitarias)
│   ├── dawazonTest.csproj
│   │
│   ├── 🗂 Users/                      # Tests de Usuarios
│   ├── 🗂 Products/                  # Tests de Productos
│   ├── 🗂 Cart/                       # Tests de Carrito
│   ├── 🗂 Common/                     # Tests Comunes
│   ├── 🗂 Container/                  # Tests con Docker
│   │
│   └── 📊 Coverage/                   # Informes de cobertura
│
├── 📁 DAWAZONPLAYWRITE (Pruebas E2E)
│   ├── dawazonPlayWrite.csproj
│   ├── playwright.runsettings
│   ├── TestConfig.cs
│   ├── BaseTest.cs
│   │
│   └── 🗂 Tests/
│       ├── AuthTests.cs
│       ├── UserTests.cs
│       ├── ProductsTests.cs
│       ├── CartTests.cs
│       ├── ManagerTests.cs
│       └── AdminTests.cs
│
├── 📁 DAWAZONBRUNOTEST (Pruebas API)
│   ├── environments/
│   │   ├── Local.bru
│   │   └── Local.json
│   │
│   ├── 🗂 ControladorAuth/
│   │   ├── 01-Register-OK.bru
│   │   ├── 02-Register-Conflict.bru
│   │   ├── 03-Login-OK.bru
│   │   ├── 04-Login-WrongPassword.bru
│   │   └── 05-Login-Admin.bru
│   │
│   ├── 🗂 ControladorProductos/
│   │   ├── 01-GetAll-Productos.bru
│   │   ├── 02-GetAll-FiltroNombre.bru
│   │   ├── 03-GetById-OK.bru
│   │   ├── 04-GetById-NotFound.bru
│   │   ├── 05-Post-Producto-SinAuth.bru
│   │   ├── 06-Post-Producto-ConManagerToken.bru
│   │   ├── 07-Put-Producto-OK.bru
│   │   ├── 08-Put-Producto-SinAuth.bru
│   │   ├── 09-Delete-Producto-SinAuth.bru
│   │   └── 10-Delete-Producto-OK.bru
│   │
│   ├── 🗂 ControladorCarrito/
│   │   └── ... (8 pruebas)
│   │
│   └── 🗂 ControladorAdmin/
│       └── ... (6 pruebas)
│
├── 📁 .GITHUB/
│   └── workflows/
│       ├── docs.yml                   # CI: Build, Tests, Docs, Deploy
│       └── teste2e.yml                # CI: Solo tests E2E
│
├── 📁 DOCKER/
│   ├── nginx/
│   │   └── conf/
│   │       └── nginx.conf
│   └── mysql/
│       └── conf/
│           └── my.cnf
│
├── 📁 DOCS/                          # Documentación
│   ├── Doxyfile                       # Configuración Doxygen
│   └── html/                         # Documentación generada
│
└── 📁 DOCUMENTACIÓN/
    ├── Dawazon2.0.pdf
    ├── casos de uso.drawio.png
    └── gitflow.png
```

---

## 🚀 Instalación y Ejecución

### Docker Compose (Recomendado)

La forma más rápida de levantar todo el entorno:

```bash
# Clonar el repositorio
git clone https://github.com/G-Corp-YA/dawazon-2.0.git
cd dawazon-2.0

# Ejecutar con Docker Compose
docker compose up --build

# Servicios disponibles:
# - Frontend:    http://localhost:8080
# - Backend API: http://localhost:5080
# - PostgreSQL:  localhost:5432
# - Redis:       localhost:6379
```

**Comandos útiles:**

```bash
# Detener servicios
docker compose down

# Ver logs
docker compose logs -f

# Reconstruir solo un servicio
docker compose build dawazon2.0
docker compose up -d dawazon2.0
```

---

### Ejecución Local

#### Requisitos Previos
- .NET 10 SDK
- PostgreSQL 15+
- Redis 7+
- Node.js 20+ (para Bruno CLI)

#### Configuración

1. **Crear base de datos PostgreSQL:**
```sql
CREATE DATABASE dawazon;
```

2. **Configurar variables de entorno:**
```bash
# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Database=dawazon;Username=postgres;Password=tu_password"
export Redis__Host=localhost
export Redis__Port=6379
export Jwt__Key="TuClaveSecretaMuyLarga12345678901234567890"
export Stripe__Key="sk_test_..."
export Smtp__Host="smtp.gmail.com"
export Smtp__Port=587
export Smtp__Username="tu_email@gmail.com"
export Smtp__Password="tu_app_password"
```

#### Ejecutar Backend
```bash
cd dawazonBackend
dotnet restore
dotnet run
# API disponible en: http://localhost:5041
```

#### Ejecutar Frontend
```bash
cd dawazon2.0
dotnet restore
dotnet run
# Frontend disponible en: http://localhost:5xxx
```

---

### Variables de Entorno

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development`, `Production` |
| `ConnectionStrings__DefaultConnection` | Conexión PostgreSQL | `Host=postgres;...` |
| `Redis__Host` | Servidor Redis | `localhost` |
| `Redis__Port` | Puerto Redis | `6379` |
| `Jwt__Key` | Clave JWT (min 32 chars) | `MiClaveSuperSegura12345678901234567890` |
| `Jwt__Issuer` | Emisor JWT | `dawazon2.0` |
| `Jwt__Audience` | Audiencia JWT | `dawazon2.0` |
| `Stripe__Key` | Clave Stripe | `sk_test_...` |
| `Server__Url` | URL del servidor | `https://dawazon.com` |
| `Smtp__Host` | Servidor SMTP | `smtp.gmail.com` |
| `Smtp__Port` | Puerto SMTP | `587` |
| `Smtp__Username` | Usuario SMTP | `email@gmail.com` |
| `Smtp__Password` | Password SMTP | `app_password` |
| `Storage__UploadPath` | Ruta de uploads | `wwwroot/uploads` |
| `Storage__MaxFileSize` | Tamaño máximo archivo | `10485760` (10MB) |

---

## 🧪 Testing

### Pruebas Unitarias (NUnit)

```bash
cd dawazonTest
dotnet test --configuration Release

# Con cobertura
dotnet test --configuration Release /p:CollectCoverage=true
```

**Resultado esperado:**
```
+----------------------------+
|     Unit Tests Results     |
+----------------------------+
| Total:    150+ tests      |
| Passed:   145 tests       |
| Failed:   5 tests         |
| Skipped:  0 tests         |
+----------------------------+
```

---

### Pruebas API (Bruno)

Ejecuta las pruebas de API con Bruno CLI:

```bash
# Instalar Bruno CLI
npm install -g @usebruno/cli

# Ejecutar pruebas
cd dawazonBrunoTest
bru run --env Local --format json
```

**Colecciones disponibles:**
- ✅ `ControladorAuth` - 5 pruebas (registro, login)
- ✅ `ControladorProductos` - 10 pruebas (CRUD productos)
- ✅ `ControladorCarrito` - 8 pruebas (gestión carrito)
- ✅ `ControladorAdmin` - 6 pruebas (administración)

---

### Pruebas E2E (Playwright)

```bash
cd dawazonPlayWrite

# Instalar navegadores
dotnet tool install --global Microsoft.Playwright.CLI
playwright install --with-deps chromium

# Ejecutar tests
dotnet test --configuration Release

# O con configuración específica
dotnet test --configuration Release --settings:playwright.runsettings
```

**Test suites:**
- ✅ `AuthTests` - Login, registro, logout
- ✅ `UserTests` - Perfil de usuario
- ✅ `ProductsTests` - Navegación y búsqueda
- ✅ `CartTests` - Añadir al carrito, checkout
- ✅ `ManagerTests` - Gestión de productos
- ✅ `AdminTests` - Panel de administración

---

## 🔄 CI/CD

El proyecto usa **GitHub Actions** para integración y despliegue continuo:

### Workflow: `docs.yml`
Se ejecuta en cada push a `main`:

1. **Build** - Compilación del proyecto
2. **Unit Tests** - Pruebas unitarias con NUnit
3. **Bruno Tests** - Pruebas de API
4. **Playwright Tests** - Pruebas E2E
5. **Generar Docs** - Documentación con Doxygen
6. **Deploy to GitHub Pages** - Publicación automática

### Workflow: `teste2e.yml`
Ejecuta solo los tests E2E (para testing rápido).

---

## 📦 Despliegue

### Render (Recomendado)

El proyecto está configurado para desplegar en **Render**:

1. Conectar repositorio GitHub a Render
2. Seleccionar "Web Service"
3. Usar el `Dockerfile-Render`
4. Configurar variables de entorno
5. Desplegar

**Build Command:**
```bash
# No necesario (Dockerfile lo hace)
```

**Start Command:**
```bash
# No necesario (entrypoint.sh lo maneja)
```

---

## 📡 API

### Documentación Interactiva

La API está documentada con **OpenAPI/Swagger**:

```
http://localhost:5041/openapi
http://localhost:5041/swagger/index.html
```

### Endpoints Principales

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Registrar usuario | ❌ |
| `POST` | `/api/auth/login` | Iniciar sesión | ❌ |
| `GET` | `/api/products` | Listar productos | ❌ |
| `GET` | `/api/products/{id}` | Obtener producto | ❌ |
| `POST` | `/api/products` | Crear producto | ✅ Manager |
| `PUT` | `/api/products/{id}` | Actualizar producto | ✅ Manager |
| `DELETE` | `/api/products/{id}` | Eliminar producto | ✅ Manager |
| `GET` | `/api/cart` | Obtener carrito | ✅ |
| `POST` | `/api/cart/add` | Añadir al carrito | ✅ |
| `DELETE` | `/api/cart/remove/{id}` | Quitar del carrito | ✅ |
| `POST` | `/api/cart/checkout` | Finalizar compra | ✅ |
| `GET` | `/api/admin/users` | Listar usuarios | ✅ Admin |
| `PUT` | `/api/admin/ban/{id}` | Banear usuario | ✅ Admin |

---

## 🤝 Contribución

¡Las contribuciones son bienvenidas! Por favor:

1. **Fork** el proyecto
2. Crea tu rama: `git checkout -b feature/nueva-funcionalidad`
3. Commits descriptivos: `git commit -m 'feat: añadir nueva funcionalidad'`
4. Push a la rama: `git push origin feature/nueva-funcionalidad`
5. Abre un **Pull Request**

### Convenciones de Commits

```
feat:     Nueva funcionalidad
fix:      Corrección de bug
docs:     Documentación
style:    Cambios de formato
refactor: Refactorización
test:     Tests
chore:    Tareas varias
```

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver [LICENSE](./LICENSE) para más detalles.

---

## 👥 Equipo de Desarrollo
<div align="center">
<table>
  <tr>
    <td align="center">
      <a href="https://github.com/Aragorn7372">
        <img src="https://github.com/Aragorn7372.png" width="120" height="120" style="border-radius: 50%; object-fit: cover;" alt="Víctor Marín Escribano"/>
        <br/>
        <sub><b>Víctor Marín</b></sub>
        <br/>
        <sub>Aragorn7372</sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/Sggz221">
        <img src="https://github.com/Sggz221.png" width="120" height="120" style="border-radius: 50%; object-fit: cover;" alt="Sggz221"/>
        <br/>
        <sub><b>Sggz221</b></sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/charlieecy">
        <img src="https://github.com/charlieecy.png" width="120" height="120" style="border-radius: 50%; object-fit: cover;" alt="Charlieecy"/>
        <br/>
        <sub><b>charlieecy</b></sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/AdrianHerSac">
        <img src="https://github.com/AdrianHerSac.png" width="120" height="120" style="border-radius: 50%; object-fit: cover;" alt="Adrián Hernández"/>
        <br/>
        <sub><b>Adrián Hernández</b></sub>
        <br/>
        <sub>AdrianHerSac</sub>
      </a>
    </td>
  </tr>
</table>
</div>
### 🏢 Organización

<div align="center">
  <a href="https://github.com/G-Corp-YA">
    <img src="https://github.com/G-Corp-YA.png" width="150" height="150" style="border-radius: 50%; object-fit: cover;" alt="G-Corp-YA"/>
    <br/>
    <sub><b>G-Corp-YA</b></sub>
  </a>
</div>

---

## 📚 Documentación del Proyecto

### Diagramas de Arquitectura

<p align="center">
  <img src="./documentacion/casos de uso.drawio.png" alt="Casos de Uso" width="45%"/>
  <img src="./documentacion/gitflow.png" alt="Gitflow" width="45%"/>
</p>

| Documento | Descripción |
|-----------|-------------|
| [Casos de Uso](./documentacion/casos%20de%20uso.drawio.png) | Diagrama de casos de uso del sistema |
| [Gitflow](./documentacion/gitflow.png) | Flujo de trabajo con Git |
| [Documentación PDF](./documentacion/Dawazon2.0.pdf) | Documentación técnica completa del proyecto |

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver [LICENSE](./LICENSE) para más detalles.

---

<div align="center">

### ⭐️ ¡Dale una estrella al proyecto si te fue útil!

*Construido con ❤️ por el equipo de G-Corp-YA usando .NET 10, Blazor y Docker*

</div>
