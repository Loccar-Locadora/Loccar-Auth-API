# Linting and Code Analysis Setup

This project includes comprehensive linting and code analysis tools to ensure code quality and consistency.

## 🛠️ Tools Included

### 1. **EditorConfig** (`.editorconfig`)
- Defines consistent coding styles across different editors and IDEs
- Configures indentation, line endings, character encoding
- Sets up C# specific formatting rules
- Enforces naming conventions

### 2. **StyleCop Analyzers**
- Enforces C# style and consistency rules
- Configured via `stylecop.json`
- Integrates with Visual Studio and VS Code
- Provides real-time feedback while coding

### 3. **Microsoft Code Analysis**
- Built-in .NET analyzers for code quality
- Detects potential bugs, performance issues, and security vulnerabilities
- Enforces best practices

### 4. **SonarAnalyzer**
- Additional code quality and security analysis
- Detects code smells and maintainability issues
- Provides comprehensive rule coverage

### 5. **Custom Rule Set** (`code-analysis.ruleset`)
- Customized severity levels for different rule categories
- Balanced approach between strict and practical
- Allows for gradual adoption

## 🚀 Usage

### Running Linting Manually

#### Windows (PowerShell)
```powershell
.\lint.ps1
```

#### Linux/macOS (Bash)
```bash
./lint.sh
```

### What the Lint Scripts Do

1. **Restore packages** - Ensures all analyzer packages are available
2. **Build with analysis** - Compiles code and runs all analyzers
3. **Format code** - Automatically fixes formatting issues using `dotnet format`
4. **Run tests** - Executes unit tests to ensure nothing is broken

### IDE Integration

#### Visual Studio
- All analyzers work automatically
- Real-time squiggles and suggestions
- Code fixes available via Ctrl+. (Quick Actions)

#### Visual Studio Code
- Install C# extension
- Analyzers provide real-time feedback
- Use Ctrl+Shift+I for format document

#### JetBrains Rider
- Built-in support for EditorConfig and analyzers
- Additional inspections available
- Code cleanup profiles can be configured

## 📋 Configuration Files

### `Directory.Build.props`
Central MSBuild properties applied to all projects:
- Enables nullable reference types
- Configures analyzer packages
- Sets up documentation generation
- Links to rule set and StyleCop configuration

### `stylecop.json`
StyleCop Analyzers configuration:
- Company name and copyright settings
- Documentation rules preferences
- Layout and ordering rules
- Naming conventions

### `code-analysis.ruleset`
Custom rule severity configuration:
- StyleCop rules with practical severity levels
- Microsoft analyzer rules optimization
- SonarAnalyzer integration
- Balanced between strict and usable

### `global.json`
SDK and tooling version management:
- Specifies .NET SDK version
- Ensures consistent tooling across team
- MSBuild SDK versions

## 🎯 Recommended Workflow

### For Developers

1. **Before committing**:
   ```bash
   ./lint.ps1  # or ./lint.sh on Linux/macOS
   ```

2. **During development**:
   - Pay attention to IDE suggestions and warnings
   - Use Quick Actions (Ctrl+.) to apply suggested fixes
   - Format document regularly (Ctrl+K, Ctrl+D in VS)

3. **Code Review**:
   - Ensure all analyzer warnings are addressed
   - Check that code follows established patterns
   - Verify tests pass

### For CI/CD

Add to your pipeline:
```yaml
- name: Code Analysis
  run: |
    dotnet restore
    dotnet build --no-restore
    dotnet format --verify-no-changes --no-restore
    dotnet test --no-build
```

## 🔧 Customization

### Adjusting Rule Severity

Edit `code-analysis.ruleset` to change rule severity:
- `Action="Error"` - Fails build
- `Action="Warning"` - Shows as warning
- `Action="Info"` - Shows as suggestion
- `Action="None"` - Disabled

### StyleCop Configuration

Modify `stylecop.json` to adjust:
- Documentation requirements
- Naming conventions  
- Code organization preferences

### EditorConfig Updates

Update `.editorconfig` for:
- Formatting preferences
- Indentation styles
- Naming rules

## 📚 Additional Resources

- [EditorConfig Documentation](https://editorconfig.org/)
- [StyleCop Analyzers Rules](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)
- [.NET Code Analysis](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/)
- [SonarAnalyzer for C#](https://rules.sonarsource.com/csharp)

## 🐛 Troubleshooting

### Common Issues

1. **Build fails with analyzer errors**:
   - Run `./lint.ps1` to see specific issues
   - Use Quick Actions in IDE to fix automatically
   - Check rule set configuration if too strict

2. **Format command fails**:
   - Ensure you have latest .NET SDK
   - Try `dotnet format --include *.cs` for specific files

3. **IDE not showing analyzer warnings**:
   - Restart IDE
   - Check that analyzer packages are restored
   - Verify `.editorconfig` is in root directory

### Performance Tips

- Analyzers may slow down initial build
- Consider disabling some rules in `code-analysis.ruleset` if too restrictive
- Use `--verbosity quiet` for faster builds in CI

## 🎉 Benefits

- **Consistent code style** across the entire team
- **Early bug detection** through static analysis
- **Improved maintainability** with enforced best practices
- **Automated formatting** reduces manual work
- **Better code reviews** with consistent standards
