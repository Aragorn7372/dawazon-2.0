# Dawazon Playwright Tests

## Configuration

You can configure the test execution using environment variables. Create a `.env` file in the root directory or set the variables directly.

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DAWAZON_BASE_URL` | Base URL of the application | `http://localhost:5080` |
| `DAWAZON_HEADLESS` | Run browser in headless mode (`true`/`false`) | `true` |
| `DAWAZON_BROWSER` | Browser to use (`chromium`/`firefox`/`webkit`) | `chromium` |
| `DAWAZON_TIMEOUT` | Timeout in milliseconds | `30000` |
| `DAWAZON_SLOWMO` | Enable slow motion for debugging (`true`/`false`) | `false` |
| `DAWAZON_SLOWMO_DELAY` | Delay in milliseconds between actions | `0` |

### Example .env file

```bash
# Development - Show browser
DAWAZON_HEADLESS=false
DAWAZON_SLOWMO=true
DAWAZON_SLOWMO_DELAY=500

# Browser selection
DAWAZON_BROWSER=chromium

# Timeout
DAWAZON_TIMEOUT=60000
```

## Running Tests

### Prerequisites

1. Install Playwright browsers:
```bash
cd dawazonPlayWrite
pwsh bin/Debug/net10.0/playwright.ps1 install
```

Or on Windows with PowerShell:
```powershell
.\bin\Debug\net10.0\playwright.ps1 install
```

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Category

```bash
# Auth tests only
dotnet test --filter "FullyQualifiedName~AuthTests"

# Products tests only
dotnet test --filter "FullyQualifiedName~ProductsTests"

# Cart tests only
dotnet test --filter "FullyQualifiedName~CartTests"

# User tests only
dotnet test --filter "FullyQualifiedName~UserTests"

# Admin tests only
dotnet test --filter "FullyQualifiedName~AdminTests"

# Manager tests only
dotnet test --filter "FullyQualifiedName~ManagerTests"
```

### Run with Browser Visible (Headed Mode)

Set the environment variable before running:

**Windows (PowerShell):**
```powershell
$env:DAWAZON_HEADLESS="false"
dotnet test
```

**Windows (CMD):**
```cmd
set DAWAZON_HEADLESS=false
dotnet test
```

**Linux/Mac:**
```bash
DAWAZON_HEADLESS=false dotnet test
```

Or create a `.env` file:
```bash
echo "DAWAZON_HEADLESS=false" > .env
dotnet test
```

## Test Structure

```
dawazonPlayWrite/
├── Tests/
│   ├── AuthTests.cs       # Login, Register, Logout tests
│   ├── ProductsTests.cs   # Products list, detail, search tests
│   ├── CartTests.cs      # Cart, checkout, orders tests
│   ├── UserTests.cs      # User profile, favorites tests
│   ├── AdminTests.cs     # Admin panel tests
│   └── ManagerTests.cs   # Manager panel tests
├── BaseTest.cs           # Base test class with common methods
├── TestConfig.cs         # Configuration management
└── dawazonPlayWrite.csproj
```

## Test Coverage

### AuthTests
- [ ] Login with valid credentials
- [ ] Login with empty fields (validation)
- [ ] Login with invalid credentials (error handling)
- [ ] Register page load
- [ ] Register with empty form (validation)
- [ ] Logout functionality
- [ ] Access restricted page without login (redirect)

### ProductsTests
- [ ] Products index page loads
- [ ] Search products by name
- [ ] Filter by category
- [ ] Pagination navigation
- [ ] View product detail
- [ ] Product not found (404)

### CartTests
- [ ] Empty cart display
- [ ] Checkout without login (redirect)
- [ ] My orders without login (redirect)
- [ ] Order detail without login (redirect)

### UserTests
- [ ] Profile without login (redirect)
- [ ] Favorites without login (redirect)
- [ ] Edit profile without login (redirect)

### AdminTests
- [ ] Admin users page without login (redirect)
- [ ] Admin sales page without login (redirect)
- [ ] Admin stats page without login (redirect)
- [ ] Admin page as regular user (403/redirect)

### ManagerTests
- [ ] Manager sales page without login (redirect)
- [ ] Manager sales edit without login (redirect)
- [ ] Manager create product without login (redirect)

## Notes

- Tests are designed to work with the Dawazon e-commerce application
- Some tests require specific user roles (Admin, Manager, User) to be present in the database
- Tests will automatically redirect to login pages when accessing restricted areas
- The base URL defaults to `http://localhost:5080` which is the default Docker Compose port
