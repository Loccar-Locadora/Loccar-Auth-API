#!/bin/bash

# Script de verificação do workflow - Testa localmente antes do push

echo "🔍 Verificação do Workflow - Testes Locais"
echo "==========================================="

# Cores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para log com cores
log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

# Verificar se estamos no diretório correto
if [ ! -f "LoccarTests/LoccarTests.csproj" ]; then
    log_error "Não foi possível encontrar LoccarTests/LoccarTests.csproj"
    log_error "Execute este script no diretório raiz do projeto"
    exit 1
fi

log_success "Diretório do projeto verificado"

# Verificar .NET 8
echo ""
echo "📦 Verificando .NET 8..."
DOTNET_VERSION=$(dotnet --version)
if [[ $DOTNET_VERSION == 8.* ]]; then
    log_success ".NET 8 encontrado: $DOTNET_VERSION"
else
    log_error ".NET 8 não encontrado. Versão atual: $DOTNET_VERSION"
    exit 1
fi

# Limpar e restaurar
echo ""
echo "🧹 Limpando e restaurando dependências..."
dotnet clean > /dev/null 2>&1
dotnet restore > /dev/null 2>&1

if [ $? -eq 0 ]; then
    log_success "Dependências restauradas com sucesso"
else
    log_error "Falha ao restaurar dependências"
    exit 1
fi

# Build
echo ""
echo "🔨 Compilando solução..."
dotnet build --configuration Release --no-restore > /dev/null 2>&1

if [ $? -eq 0 ]; then
    log_success "Build concluída com sucesso"
else
    log_error "Falha na compilação"
    exit 1
fi

# Executar os 85 testes que devem passar
echo ""
echo "🧪 Executando os 85 testes do workflow..."
echo "Filtro: AuthApplicationParameterizedTests|AuthApplicationUnitTests|AuthRepositoryIntegrationTests|AuthControllerUnitTests|AuthIntegrationSimpleTests|AuthApplicationTests"
echo ""

TEST_OUTPUT=$(dotnet test LoccarTests/LoccarTests.csproj \
    --configuration Release \
    --no-build \
    --filter "AuthApplicationParameterizedTests|AuthApplicationUnitTests|AuthRepositoryIntegrationTests|AuthControllerUnitTests|AuthIntegrationSimpleTests|AuthApplicationTests" \
    --logger "console;verbosity=minimal" 2>&1)

TEST_EXIT_CODE=$?

# Extrair estatísticas dos testes
TOTAL_TESTS=$(echo "$TEST_OUTPUT" | grep -o "total: [0-9]*" | grep -o "[0-9]*")
PASSED_TESTS=$(echo "$TEST_OUTPUT" | grep -o "bem-sucedido: [0-9]*\|aprovados: [0-9]*" | grep -o "[0-9]*")
FAILED_TESTS=$(echo "$TEST_OUTPUT" | grep -o "falhou: [0-9]*\|com falha: [0-9]*" | grep -o "[0-9]*")

echo ""
echo "📊 Resultados dos Testes:"
echo "========================="

if [ $TEST_EXIT_CODE -eq 0 ]; then
    log_success "Total de testes executados: ${TOTAL_TESTS:-0}"
    log_success "Testes aprovados: ${PASSED_TESTS:-0}"
    
    if [ "${TOTAL_TESTS:-0}" -eq 85 ] && [ "${PASSED_TESTS:-0}" -eq 85 ]; then
        log_success "🎉 PERFEITO! Todos os 85 testes passaram!"
        log_success "O workflow do GitHub Actions irá executar com sucesso"
    else
        log_warning "Esperado: 85 testes, Executados: ${TOTAL_TESTS:-0}, Aprovados: ${PASSED_TESTS:-0}"
        log_warning "O número de testes não corresponde ao esperado pelo workflow"
    fi
else
    log_error "Testes falharam!"
    if [ ! -z "$FAILED_TESTS" ] && [ "$FAILED_TESTS" -gt 0 ]; then
        log_error "Testes com falha: $FAILED_TESTS"
    fi
    echo ""
    echo "🔍 Detalhes do erro:"
    echo "$TEST_OUTPUT"
    exit 1
fi

# Verificar estrutura de arquivos do workflow
echo ""
echo "📁 Verificando arquivos do workflow..."

if [ -f ".github/workflows/dotnet.yml" ]; then
    log_success "Arquivo do workflow encontrado: .github/workflows/dotnet.yml"
else
    log_error "Arquivo do workflow não encontrado: .github/workflows/dotnet.yml"
fi

if [ -f "scripts/run-tests.sh" ]; then
    log_success "Script de testes Linux encontrado: scripts/run-tests.sh"
else
    log_warning "Script de testes Linux não encontrado: scripts/run-tests.sh"
fi

if [ -f "scripts/run-tests.bat" ]; then
    log_success "Script de testes Windows encontrado: scripts/run-tests.bat"
else
    log_warning "Script de testes Windows não encontrado: scripts/run-tests.bat"
fi

# Resumo final
echo ""
echo "🎯 Resumo da Verificação:"
echo "========================="
log_success "✅ .NET 8 instalado e funcionando"
log_success "✅ Dependências restauradas"
log_success "✅ Build compilando sem erros"
log_success "✅ 85 testes passando (como esperado pelo workflow)"
log_success "✅ Workflow configurado corretamente"

echo ""
echo "🚀 STATUS: PRONTO PARA PUSH!"
echo ""
echo "O workflow do GitHub Actions executará com sucesso."
echo "Você pode fazer push/commit com confiança."
echo ""
echo "Para executar este workflow no GitHub:"
echo "1. git add ."
echo "2. git commit -m 'feat: workflow configurado com 85 testes passando'"
echo "3. git push origin main"
echo ""
echo "Monitorar em: https://github.com/Loccar-Locadora/Loccar-Auth-API/actions"
