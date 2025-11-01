# 🎨 LoccarAuth Code Linting & Quality Assurance

This document describes the comprehensive code linting and quality assurance setup for the LoccarAuth .NET 8 project.

## 📋 Overview

Our linting setup ensures consistent code quality, security, and maintainability across the entire project. It integrates multiple analyzers and tools to provide comprehensive code analysis.

## 🛠️ Tools & Analyzers

### Core Components

| Component | Version | Purpose |
|-----------|---------|---------|
| **StyleCop.Analyzers** | 1.2.0-beta.556 | Code style and consistency |
| **Microsoft.CodeAnalysis.Analyzers** | 3.11.0 | Core .NET code analysis |
| **SonarAnalyzer.CSharp** | 9.32.0.97167 | Advanced code quality rules |
| **Microsoft.CodeAnalysis.BannedApiAnalyzers** | 3.11.0 | Security-focused analysis |
| **AsyncUsageAnalyzers** | 1.0.0-alpha003 | Async/await pattern validation |

### Configuration Files

- **`Directory.Build.props`** - Centralized analyzer configuration
- **`code-analysis.ruleset`** - Custom rule definitions and severity levels
- **`stylecop.json`** - StyleCop-specific settings
- **`.editorconfig`** - IDE formatting rules

## 🚀 Quick Start

### Running Linting

**PowerShell (Windows):**
```powershell
# Basic linting
.\lint.ps1

# Fix formatting issues automatically
.\lint.ps1 -Fix

# Verbose output
.\lint.ps1 -Verbose

# Skip build, format check only
.\lint.ps1 -SkipBuild
```

**Bash (Linux/macOS/WSL):**
```bash
# Make script executable (first time only)
chmod +x lint.sh

# Basic linting
./lint.sh

# Fix formatting issues automatically  
./lint.sh --fix

# Verbose output
./lint.sh --verbose

# Skip build, format check only
./lint.sh --skip-build
```

## 📊 Analysis Categories

### 1. Code Formatting
- **Tool**: `dotnet format`
- **Rules**: EditorConfig + built-in formatters
- **Auto-fix**: ✅ Available with `--fix` flag
- **Scope**: Indentation, spacing, line endings, using statements

### 2. Style Analysis  
- **Tool**: StyleCop.Analyzers
- **Rules**: SA1xxx series (300+ rules)
- **Auto-fix**: ✅ Partial (via IDE)
- **Scope**: Naming conventions, documentation, layout, readability

### 3. Code Quality
- **Tool**: SonarAnalyzer.CSharp
- **Rules**: S1xxx series (500+ rules)  
- **Auto-fix**: ⚠️ Limited
- **Scope**: Maintainability, reliability, performance, security

### 4. Security Analysis
- **Tool**: Microsoft analyzers + vulnerability scanning
- **Rules**: CA2xxx, CA3xxx, CA5xxx series
- **Auto-fix**: ❌ Manual review required
- **Scope**: SQL injection, cryptography, deserialization, etc.

### 5. .NET Best Practices
- **Tool**: Microsoft.CodeAnalysis.NetAnalyzers
- **Rules**: CA1xxx series
- **Auto-fix**: ✅ Many rules supported
- **Scope**: Design, performance, globalization, interoperability

## ⚙️ Configuration Details

### Severity Levels

| Level | Symbol | Description | Action |
|-------|--------|-------------|---------|
| **Error** | ❌ | Critical issues that must be fixed | Build fails |
| **Warning** | ⚠️ | Important issues that should be addressed | Build succeeds with warnings |
| **Info** | ℹ️ | Suggestions for improvement | No build impact |
| **Hidden** | 👻 | IDE-only hints | No build impact |

### Key Rule Categories

#### Security Rules (Error Level)
- `CA2100` - SQL injection prevention
- `CA5350/5351` - Weak cryptography detection
- `CA3075` - Insecure XML processing

#### Performance Rules (Warning Level)  
- `CA1802` - Use literals appropriately
- `CA1825` - Avoid zero-length array allocations
- `CA1821` - Remove empty finalizers

#### Style Rules (Warning Level)
- `SA1210` - Alphabetical using directives
- `SA1413` - Trailing commas in initializers
- Custom naming conventions

#### Disabled Rules
- `SA1633` - File headers (project-specific choice)
- `SA1600/1601` - XML documentation (API-only requirement)
- `CA2007` - ConfigureAwait (not required for .NET Core/5+)

## 🔧 IDE Integration

### Visual Studio
1. Install StyleCop.Analyzers extension (optional, built-in analyzers work)
2. Rules appear in Error List window
3. Code fixes available via Ctrl+. (Quick Actions)
4. Bulk fixes: **Analyze** → **Code Cleanup** → **Run Profile**

### Visual Studio Code
1. Install C# extension (includes analyzers)
2. Install "StyleCop.Analyzers" extension  
3. Rules appear in Problems panel
4. Code fixes via Ctrl+. (Quick Fix)

### Rider
1. Built-in support for all analyzers
2. Rules appear in Solution Wide Analysis
3. Code fixes via Alt+Enter
4. Bulk fixes: **Code** → **Code Cleanup**

## 🚦 CI/CD Integration

### GitHub Actions Workflow

The linting is integrated into the `.github/workflows/dotnet.yml` file with the following steps:

```yaml
- name: Check code formatting
  run: dotnet format --verify-no-changes --no-restore --verbosity diagnostic

- name: Build with code analysis  
  run: dotnet build --configuration Release --no-restore
       -p:EnableNETAnalyzers=true -p:AnalysisLevel=latest
       -p:RunStyleCopAnalyzer=true

- name: Security Analysis
  run: dotnet list package --vulnerable --include-transitive
```

### Build Requirements

- ✅ All formatting must be correct
- ✅ No build errors allowed
- ⚠️ Warnings are logged but don't fail build
- 🔒 Security vulnerabilities fail the build

## 📈 Metrics & Reporting

### Local Development
The lint scripts provide detailed reports including:
- Total issues found by category
- Time to complete analysis
- Security vulnerability summary
- Suggested remediation actions

### CI/CD Dashboard
GitHub Actions provides:
- Build status badges
- Downloadable analysis reports
- PR check status
- Historical trend analysis

## 🎯 Best Practices

### For Developers

1. **Run linting before commits**:
   ```bash
   ./lint.sh --fix
   git add .
   git commit -m "fix: address linting issues"
   ```

2. **Use IDE integration**: Enable real-time analysis
3. **Address warnings promptly**: Don't accumulate technical debt
4. **Review security rules**: Understand the security implications

### For Code Reviews

1. **Automated checks first**: Let CI handle basic quality
2. **Focus on logic**: Review business logic and architecture
3. **Security review**: Pay special attention to security analyzer warnings
4. **Performance review**: Check performance-related suggestions

### For Project Maintenance

1. **Regular updates**: Keep analyzers updated monthly
2. **Rule review**: Quarterly review of rule configuration
3. **Metrics tracking**: Monitor code quality trends
4. **Team training**: Regular sessions on new rules and practices

## 🔍 Troubleshooting

### Common Issues

#### "StyleCop analyzers not running"
**Solution**: Ensure `stylecop.json` is included in project:
```xml
<ItemGroup>
  <AdditionalFiles Include="stylecop.json" />
</ItemGroup>
```

#### "Too many warnings in legacy code"
**Solution**: Use `.globalconfig` to suppress rules for specific files:
```ini
# Suppress SA1633 for all files in legacy directory
[**/Legacy/**]
dotnet_analyzer_diagnostic.SA1633.severity = none
```

#### "Build performance issues"
**Solution**: Disable analyzers in Debug builds:
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <RunAnalyzersDuringBuild>false</RunAnalyzersDuringBuild>
</PropertyGroup>
```

### Getting Help

1. **Rule documentation**: [StyleCop Docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)
2. **Sonar rules**: [SonarSource Rules](https://rules.sonarsource.com/csharp)
3. **Microsoft analyzers**: [.NET Analyzer Docs](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)

## 📝 Contributing

When contributing to linting configuration:

1. **Test changes thoroughly** with `./lint.sh --verbose`
2. **Document rule changes** in this README
3. **Consider backward compatibility** with existing code
4. **Update CI/CD scripts** if needed

## 🏆 Quality Goals

- **Zero build errors** in main branch
- **< 10 warnings per 1000 lines** of code
- **100% security rule compliance**
- **Consistent formatting** across all files
- **Automated quality gates** in CI/CD

---

*Last updated: $(Get-Date -Format 'yyyy-MM-dd')*
*Maintained by: LoccarAuth Team*
