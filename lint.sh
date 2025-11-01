#!/bin/bash

# Loccar Lint Script
# This script runs various linting and code analysis tools

echo "🔍 Running Loccar Code Analysis and Linting"
echo "============================================="

# Restore packages
echo -e "\n📦 Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Package restore failed!"
    exit 1
fi

# Build with warnings as errors for analyzers
echo -e "\n🔨 Building with code analysis..."
dotnet build --no-restore --verbosity quiet
if [ $? -ne 0 ]; then
    echo "❌ Build with analysis failed!"
    exit 1
fi

# Format code
echo -e "\n🎨 Formatting code..."
dotnet format --no-restore --verbosity diagnostic
if [ $? -ne 0 ]; then
    echo "⚠️  Code formatting completed with warnings"
else
    echo "✅ Code formatting completed successfully"
fi

# Run tests
echo -e "\n🧪 Running tests..."
dotnet test --no-build --verbosity quiet
if [ $? -ne 0 ]; then
    echo "❌ Tests failed!"
    exit 1
fi

echo -e "\n✅ All linting and analysis checks completed successfully!"
echo "🎉 Your code is ready for commit!"
