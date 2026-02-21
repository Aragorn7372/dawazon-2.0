# 🛒 Dawazon 2.0

> Plataforma de e-commerce inspirada en Amazon, construida con una arquitectura moderna de microservicios utilizando **.NET 10**, **Blazor** y **Docker**.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Playwright](https://img.shields.io/badge/E2E-Playwright-45BA4B?style=flat&logo=playwright)](https://playwright.dev/)
[![NUnit](https://img.shields.io/badge/Tests-NUnit-brightgreen?style=flat)](https://nunit.org/)

---

## 📋 Tabla de Contenidos

- [Descripción](#-descripción)
- [Tecnologías](#-tecnologías)
- [Arquitectura](#-arquitectura)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Ejecución](#-instalación-y-ejecución)
    - [Con Docker Compose](#con-docker-compose-recomendado)
    - [Ejecución Local](#ejecución-local)
- [Testing](#-testing)
- [API Reference](#-api-reference)
- [Licencia](#-licencia)

---

## 📖 Descripción

**Dawazon 2.0** es la segunda iteración de una plataforma de comercio electrónico full-stack. Cuenta con un frontend en **Blazor**, un backend **ASP.NET Core Web API** con soporte **OpenAPI**, pruebas unitarias con **NUnit** y pruebas end-to-end (E2E) con **Playwright**. Todo el entorno está completamente containerizado con **Docker**.

---

## 🛠 Tecnologías

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Frontend | ASP.NET Core / Blazor | .NET 10 |
| Backend | ASP.NET Core Web API | .NET 10 |
| API Docs | Microsoft.AspNetCore.OpenApi | 10.0.2 |
| Pruebas unitarias | NUnit + NUnit3TestAdapter | 4.3.2 |
| Pruebas E2E | Microsoft.Playwright.NUnit | 1.52.0 |
| Cobertura | coverlet.collector | 6.0.4 |
| Containerización | Docker + Docker Compose | — |

---

## 🏗 Arquitectura

El proyecto sigue una arquitectura de **microservicios** con dos servicios principales containerizados:

```
┌─────────────────────────────────────────────────────┐
│                   Docker Compose                    │
│                                                     │
│  ┌──────────────────┐    ┌───────────────────────┐  │
│  │   dawazon2.0     │    │   dawazonbackend      │  │
│  │  (Frontend)      │◄──►│   (REST API)          │  │
│  │  Blazor / .NET   │    │  ASP.NET Core Web API │  │
│  │  Port: 8080/8081 │    │  Port: 8080/8081      │  │
│  └──────────────────┘    └───────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

---

## 📁 Estructura del Proyecto

```
dawazon-2.0/
│
├── 📄 compose.yaml                    # Orquestación Docker Compose
├── 📄 dawazon2.0.slnx                 # Solution file de Visual Studio
├── 📄 .dockerignore                   # Exclusiones para Docker
├── 📄 .gitignore                      # Exclusiones para Git
├── 📄 LICENSE                         # Licencia MIT
│
├── 📁 dawazon2.0/                     # Proyecto Frontend (Blazor)
│   ├── 📄 Dockerfile                  # Imagen Docker del frontend
│   ├── 📄 Program.cs                  # Punto de entrada de la aplicación
│   ├── 📄 dawazon2.0.csproj           # Configuración del proyecto
│   ├── 📄 appsettings.json            # Configuración de la aplicación
│   ├── 📄 appsettings.Development.json# Configuración de desarrollo
│   ├── 📁 Pages/                      # Páginas Razor/Blazor
│   ├── 📁 Properties/                 # Propiedades del proyecto (launchSettings)
│   └── 📁 wwwroot/                    # Archivos estáticos (CSS, JS, imágenes)
│
���── 📁 dawazonBackend/                 # Proyecto Backend (Web API)
│   ├── 📄 Dockerfile                  # Imagen Docker del backend
│   ├── 📄 Program.cs                  # Punto de entrada y configuración de servicios
│   ├── 📄 dawazonBackend.csproj       # Configuración del proyecto
│   ├── 📄 dawazonBackend.http         # Archivo de pruebas HTTP (REST Client)
│   ├── 📄 appsettings.json            # Configuración de la aplicación
│   ├── 📄 appsettings.Development.json# Configuración de desarrollo
│   └── 📁 Controllers/                # Controladores de la API
│
├── 📁 dawazonTest/                    # Pruebas Unitarias (NUnit)
│   ├── 📄 UnitTest1.cs                # Pruebas unitarias
│   └── 📄 dawazonTest.csproj          # Configuración del proyecto de pruebas
│
└── 📁 dawazonPlayWrite/               # Pruebas E2E (Playwright + NUnit)
    ├── 📄 UnitTest1.cs                # Pruebas end-to-end
    └── 📄 dawazonPlayWrite.csproj     # Configuración del proyecto E2E
```

---

## ✅ Requisitos Previos

Asegúrate de tener instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para ejecución con Docker)
- [Git](https://git-scm.com/)

---

## 🚀 Instalación y Ejecución

### Clonar el repositorio

```bash
git clone https://github.com/Aragorn7372/dawazon-2.0.git
cd dawazon-2.0
```

---

### Con Docker Compose (Recomendado)

Levanta todos los servicios con un solo comando:

```bash
docker compose up --build
```

Esto construirá y ejecutará:
- **Frontend** → `http://localhost:8080`
- **Backend API** → `http://localhost:5080`

Para detener los servicios:

```bash
docker compose down
```

---

### Ejecución Local

#### Backend

```bash
cd dawazonBackend
dotnet restore
dotnet run
```

El backend estará disponible en: `http://localhost:5080`

#### Frontend

```bash
cd dawazon2.0
dotnet restore
dotnet run
```

El frontend estará disponible en: `http://localhost:PORT` (ver `Properties/launchSettings.json`)

---

## 🧪 Testing

### Pruebas Unitarias (NUnit)

```bash
cd dawazonTest
dotnet test
```

### Pruebas E2E (Playwright)

Antes de ejecutar las pruebas E2E, instala los navegadores de Playwright:

```bash
cd dawazonPlayWrite
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install
dotnet test
```

> **Nota:** Las pruebas E2E requieren PowerShell (`pwsh`) para la instalación de navegadores.

---

## 📡 API Reference

El backend expone una API REST documentada con **OpenAPI**. En entorno de desarrollo, la documentación interactiva está disponible en:

```
http://localhost:5080/openapi
```

### Endpoints disponibles

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/weatherforecast` | Endpoint de ejemplo (placeholder) |

> Los endpoints de productos, usuarios y pedidos se irán añadiendo en futuras iteraciones.

---

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:

1. Haz un **fork** del proyecto.
2. Crea una rama con tu feature: `git checkout -b feature/nueva-funcionalidad`
3. Haz commit de tus cambios: `git commit -m 'feat: añadir nueva funcionalidad'`
4. Haz push a la rama: `git push origin feature/nueva-funcionalidad`
5. Abre un **Pull Request**.

---

## 📄 Licencia

Este proyecto está licenciado bajo la [MIT License](./LICENSE).

---

<div align="center">
  <sub>Desarrollado con ❤️ por <a href="https://github.com/Aragorn7372">Aragorn7372</a></sub>
</div>