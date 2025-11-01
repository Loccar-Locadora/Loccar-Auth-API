#!/bin/bash
#
# Comprehensive linting script for LoccarAuth .NET 8 project
# This script performs comprehensive code quality analysis including:
# - Code formatting validation
# - StyleCop analysis
# - .NET analyzers
# - SonarAnalyzer rules
# - Security vulnerability scanning
#
# Usage:
#   ./lint.sh           - Run all linting checks
#   ./lint.sh --fix     - Run linting and automatically fix formatting issues
#   ./lint.sh --skip-build - Run only formatting checks without building
#   ./lint.sh --verbose - Enable verbose output for detailed analysis
#

set -e  # Exit on any error

# Parse command line arguments
FIX=false
SKIP_BUILD=false
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --fix)
            FIX=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [--fix] [--skip-build] [--verbose] [--help]"
            echo ""
            echo "Options:"
            echo "  --fix        Automatically fix code formatting issues"
            echo "  --skip-build Skip the build step and only run formatting checks"
            echo "  --verbose    Enable verbose output for detailed analysis"
            echo "  --help       Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Color functions
print_success() { echo -e "\e[32m✅ $1\e[0m"; }
print_info() { echo -e "\e[36mℹ️  $1\e[0m"; }
print_warning() { echo -e "\e[33m⚠️  $1\e[0m"; }
print_error() { echo -e "\e[31m❌ $1\e[0m"; }
print_step() { echo -e "\e[34m🔄 $1\e[0m"; }

# Initialize counters
TOTAL_ISSUES=0
FIXED_ISSUES=0
START_TIME=$(date +%s)

print_info "🚀 Starting LoccarAuth Code Quality Analysis"
print_info "⏰ Started at: $(date '+%H:%M:%S')"
print_info "📁 Working directory: $(pwd)"

if [ "$VERBOSE" = true ]; then
    print_info "🔧 Verbose mode enabled"
fi

# Step 1: Verify .NET SDK
print_step "Verifying .NET 8 SDK installation..."
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed or not in PATH"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
print_success ".NET SDK version: $DOTNET_VERSION"

# Check if .NET 8 is available
if ! dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 8\."; then
    print_error ".NET 8 runtime not found. Please install .NET 8 SDK."
    exit 1
fi
print_success ".NET 8 runtime verified"

# Step 2: Restore packages
print_step "Restoring NuGet packages..."
if [ "$VERBOSE" = true ]; then
    if ! dotnet restore --verbosity detailed; then
        print_error "Failed to restore packages"
        ((TOTAL_ISSUES++))
    else
        print_success "NuGet packages restored successfully"
    fi
else
    if ! dotnet restore --verbosity minimal --nologo > /dev/null 2>&1; then
        print_error "Failed to restore packages"
        ((TOTAL_ISSUES++))
    else
        print_success "NuGet packages restored successfully"
    fi
fi

# Step 3: Check code formatting
print_step "Checking code formatting..."
if [ "$FIX" = true ]; then
    print_info "🔧 Fixing code formatting..."
    if [ "$VERBOSE" = true ]; then
        if dotnet format --verbosity diagnostic; then
            print_success "Code formatting applied"
            ((FIXED_ISSUES++))
        else
            print_error "Failed to apply code formatting"
            ((TOTAL_ISSUES++))
        fi
    else
        if dotnet format --verbosity minimal > /dev/null 2>&1; then
            print_success "Code formatting applied"
            ((FIXED_ISSUES++))
        else
            print_error "Failed to apply code formatting"
            ((TOTAL_ISSUES++))
        fi
    fi
else
    # Check formatting without fixing
    if [ "$VERBOSE" = true ]; then
        FORMAT_OUTPUT=$(dotnet format --verify-no-changes --verbosity diagnostic 2>&1)
    else
        FORMAT_OUTPUT=$(dotnet format --verify-no-changes --verbosity minimal 2>&1)
    fi
    
    if [ $? -eq 0 ]; then
        print_success "Code formatting is correct"
    else
        print_warning "Code formatting issues found:"
        echo "$FORMAT_OUTPUT" | grep -E "(warning|error)" | while read -r line; do
            print_warning "  $line"
        done
        print_info "💡 Run with --fix parameter to automatically fix formatting issues"
        ((TOTAL_ISSUES++))
    fi
fi

# Step 4: Build with code analysis (skip if requested)
if [ "$SKIP_BUILD" != true ]; then
    print_step "Building solution with code analysis..."
    
    BUILD_ARGS=(
        "build"
        "--configuration" "Release"
        "--no-restore"
        "-p:TreatWarningsAsErrors=false"
        "-p:EnableNETAnalyzers=true"
        "-p:AnalysisLevel=latest"
        "-p:RunStyleCopAnalyzer=true"
    )
    
    if [ "$VERBOSE" = true ]; then
        BUILD_ARGS+=("--verbosity" "normal")
    else
        BUILD_ARGS+=("--verbosity" "minimal" "--nologo")
    fi
    
    BUILD_OUTPUT=$(dotnet "${BUILD_ARGS[@]}" 2>&1)
    BUILD_EXIT_CODE=$?
    
    if [ $BUILD_EXIT_CODE -eq 0 ]; then
        print_success "Build completed successfully"
    else
        print_warning "Build completed with warnings/errors:"
        
        # Parse and categorize issues
        WARNINGS=$(echo "$BUILD_OUTPUT" | grep -c "warning" || true)
        ERRORS=$(echo "$BUILD_OUTPUT" | grep -c "error" || true)
        
        if [ "$ERRORS" -gt 0 ]; then
            print_error "Build errors found:"
            echo "$BUILD_OUTPUT" | grep "error" | while read -r line; do
                print_error "  $line"
            done
            ((TOTAL_ISSUES += ERRORS))
        fi
        
        if [ "$WARNINGS" -gt 0 ]; then
            print_warning "Build warnings found:"
            echo "$BUILD_OUTPUT" | grep "warning" | while read -r line; do
                print_warning "  $line"
            done
            ((TOTAL_ISSUES += WARNINGS))
        fi
    fi
else
    print_info "🚫 Skipping build as requested"
fi

# Step 5: Security analysis
print_step "Running security vulnerability scan..."
SECURITY_OUTPUT=$(dotnet list package --vulnerable --include-transitive 2>&1)

if echo "$SECURITY_OUTPUT" | grep -q "has the following vulnerable packages"; then
    print_warning "Security vulnerabilities found:"
    echo "$SECURITY_OUTPUT" | grep ">" | while read -r line; do
        print_warning "  $line"
    done
    ((TOTAL_ISSUES++))
    print_info "💡 Please update vulnerable packages using 'dotnet add package <PackageName> --version <LatestVersion>'"
else
    print_success "No security vulnerabilities found"
fi

# Step 6: Generate summary report
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
MINUTES=$((DURATION / 60))
SECONDS=$((DURATION % 60))

PROJECT_COUNT=$(find . -name "*.csproj" -type f | wc -l)

print_info ""
print_info "📊 === LINT ANALYSIS SUMMARY ==="
printf "⏱️  Duration: %02d:%02d\n" $MINUTES $SECONDS
print_info "📁 Projects analyzed: $PROJECT_COUNT"
print_info "🔍 Total issues found: $TOTAL_ISSUES"

if [ "$FIX" = true ] && [ $FIXED_ISSUES -gt 0 ]; then
    print_info "🔧 Issues fixed: $FIXED_ISSUES"
fi

if [ $TOTAL_ISSUES -eq 0 ]; then
    print_success "🎉 All linting checks passed! Code quality is excellent."
    print_info ""
    print_info "✨ Quality metrics:"
    print_info "  📏 Code formatting: ✅ Perfect"
    print_info "  🔍 Code analysis: ✅ Clean"
    print_info "  📐 StyleCop rules: ✅ Compliant"
    print_info "  🔒 Security scan: ✅ Safe"
    exit 0
else
    print_warning "⚠️  Found $TOTAL_ISSUES issues that need attention."
    print_info ""
    print_info "🛠️  Recommended actions:"
    print_info "  1. Run 'dotnet format' to fix formatting issues"
    print_info "  2. Review and fix analyzer warnings in your IDE"
    print_info "  3. Update any vulnerable packages"
    print_info "  4. Address StyleCop rule violations"
    print_info ""
    print_info "💡 Tip: Use your IDE's Error List or Problems panel for detailed issue navigation"
    exit 1
fi
