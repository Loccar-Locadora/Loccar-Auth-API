# Loccar Auth API

[![.NET CI/CD Pipeline](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/dotnet.yml)
[![Pull Request Quality](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/pr-quality.yml/badge.svg)](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/pr-quality.yml)
[![Code Health](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/code-health.yml/badge.svg)](https://github.com/Loccar-Locadora/Loccar-Auth-API/actions/workflows/code-health.yml)

A robust authentication API built with .NET 8, featuring comprehensive code quality tools and automated CI/CD pipelines.

## 🚀 Features

- **JWT Authentication** - Secure token-based authentication
- **User Management** - Registration, login, and profile management
- **Role-based Authorization** - Flexible permission system
- **Refresh Tokens** - Secure token rotation
- **PostgreSQL Integration** - Reliable data persistence
- **Comprehensive Testing** - 85+ automated tests
- **Code Quality Enforcement** - Automated linting and analysis

## 🛠️ Technology Stack

- **.NET 8** - Latest LTS framework
- **ASP.NET Core Web API** - REST API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Primary database
- **JWT Bearer Authentication** - Token-based security
- **BCrypt** - Password hashing
- **xUnit** - Testing framework

## 📋 Code Quality & Standards

This project enforces strict code quality standards through:

### 🔍 Automated Linting & Analysis
- **StyleCop Analyzers** - C# style and consistency rules
- **Microsoft Code Analysis** - Built-in .NET analyzers
- **SonarAnalyzer** - Security and code quality analysis
- **EditorConfig** - Consistent formatting across IDEs

### 🏃‍♂️ Quick Start with Linting

```bash
# Windows
.\lint.ps1

# Linux/macOS  
./lint.sh
```

The linting script will:
- ✅ Restore NuGet packages
- ✅ Build with code analysis
- ✅ Format code automatically  
- ✅ Run all tests
- ✅ Report any issues

### 📊 CI/CD Pipelines

We have three automated workflows:

1. **Main CI/CD** (`.github/workflows/dotnet.yml`)
   - Code quality checks
   - Full test suite (85+ tests)
   - Security scanning
   - Production builds

2. **Pull Request Quality** (`.github/workflows/pr-quality.yml`)
   - Formatting verification
   - StyleCop analysis
   - Changed files linting
   - Automated PR comments

3. **Code Health** (`.github/workflows/code-health.yml`)
   - Weekly dependency audits
   - Security vulnerability scanning
   - Code metrics reporting
   - Automated issue creation

## 🏗️ Project Structure

```
Loccar-Auth-API/
├── LoccarAuth/           # Web API layer
├── LoccarApplication/    # Application services  
├── LoccarDomain/         # Domain models
├── LoccarInfra/          # Infrastructure & data access
├── LoccarTests/          # Test project (85+ tests)
├── scripts/              # Database initialization
├── .github/workflows/    # CI/CD pipelines
├── lint.ps1             # Windows linting script
├── lint.sh              # Linux/macOS linting script
├── .editorconfig         # Code formatting rules
├── stylecop.json         # StyleCop configuration
└── LINTING-README.md     # Detailed linting guide
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 16
- Docker (optional)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/Loccar-Locadora/Loccar-Auth-API.git
   cd Loccar-Auth-API
   ```

2. **Setup database**
   ```bash
   # Using Docker
   docker-compose up -d
   
   # Or manually setup PostgreSQL and run:
   psql -h localhost -U postgres -d LoccarAuth -f scripts/init-db.sql
   ```

3. **Run the application**
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project LoccarAuth
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

### Docker Development

See [README.Docker.md](README.Docker.md) for detailed Docker setup instructions.

## 🧪 Testing

The project includes comprehensive test coverage:

| Test Category | Count | Description |
|---------------|-------|-------------|
| Parameterized Tests | 21 | Data-driven test scenarios |
| Repository Tests | 19 | Data access layer tests |
| Controller Tests | 15 | API endpoint tests |
| Integration Tests | 14 | End-to-end workflows |
| Application Tests | 11 | Business logic tests |
| Legacy Tests | 5 | Compatibility tests |
| **TOTAL** | **85** | **Complete test coverage** |

Run specific test categories:
```bash
# All tests
dotnet test

# Specific category
dotnet test --filter "AuthControllerUnitTests"
dotnet test --filter "AuthRepositoryIntegrationTests"
```

## 📖 API Documentation

The API includes Swagger documentation available at:
- Development: `http://localhost:5000/swagger`
- API endpoints documented with OpenAPI specifications

## 🔒 Security

- **JWT tokens** with configurable expiration
- **Refresh token rotation** for enhanced security
- **BCrypt password hashing** with salt
- **CORS configuration** for cross-origin requests
- **Regular security audits** via automated workflows

## 📚 Documentation

- **[Linting Guide](LINTING-README.md)** - Comprehensive code quality setup
- **[Docker Guide](README.Docker.md)** - Container deployment instructions  
- **[Testing Guide](LoccarTests/README-Testes.md)** - Test documentation

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**
3. **Run linting**: `./lint.ps1` or `./lint.sh`
4. **Ensure all tests pass**: `dotnet test`
5. **Submit a pull request**

### Code Standards

- Follow the established linting rules
- Write tests for new functionality
- Update documentation as needed
- Use meaningful commit messages

Pull requests automatically run:
- ✅ Code formatting checks
- ✅ StyleCop analysis
- ✅ Security scanning
- ✅ Full test suite

## 📊 Code Quality Metrics

- **85+ automated tests** with comprehensive coverage
- **4 code analysis tools** active
- **Zero tolerance** for security vulnerabilities  
- **Consistent formatting** enforced automatically
- **Weekly health checks** and dependency audits

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🏢 Loccar Locadora

Part of the Loccar ecosystem - building reliable, secure, and maintainable software solutions.

---

## 🎯 Quick Actions

| Action | Command | Description |
|--------|---------|-------------|
| 🔍 **Lint Code** | `./lint.ps1` or `./lint.sh` | Full code quality check |
| 🧪 **Run Tests** | `dotnet test` | Execute test suite |
| 🐳 **Start Docker** | `./start-docker.bat` or `./start-docker.sh` | Launch with Docker |
| 🔧 **Build** | `dotnet build` | Compile application |
| 📝 **Format** | `dotnet format` | Auto-format code |

---

*Built with ❤️ by the Loccar team using industry best practices for code quality and maintainability.*
