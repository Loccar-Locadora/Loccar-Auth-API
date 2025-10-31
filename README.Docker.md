# Loccar Auth API - Docker Setup

Este projeto está configurado para rodar completamente em Docker com PostgreSQL.

## Pré-requisitos

- Docker
- Docker Compose

## Como executar

1. Clone o repositório
2. Navegue até a pasta raiz do projeto
3. Execute o comando:

```bash
docker-compose up --build
```

## Serviços

### PostgreSQL Database
- **Container**: `loccar-db`
- **Porta**: `5433:5432`
- **Database**: `loccardb_auth`
- **Usuário**: `postgres`
- **Senha**: `postgres`

### API .NET
- **Container**: `loccar-locadora-auth`
- **Porta**: `5002:8081`
- **URL**: http://localhost:5002
- **Swagger**: http://localhost:5002/swagger

## Estrutura do Banco de Dados

O banco de dados é inicializado automaticamente com as seguintes tabelas:
- `customer` - Dados dos clientes
- `vehicle` - Dados dos veículos
- `cargo_vehicle` - Veículos de carga
- `leisure_vehicle` - Veículos de lazer
- `motorcycle` - Motocicletas
- `passenger_vehicle` - Veículos de passageiros
- `reservation` - Reservas

## Comandos Úteis

### Parar os serviços
```bash
docker-compose down
```

### Reconstruir apenas a API
```bash
docker-compose up --build loccar-locadora
```

### Ver logs da API
```bash
docker-compose logs -f loccar-locadora
```

### Ver logs do banco
```bash
docker-compose logs -f db
```

### Conectar ao PostgreSQL
```bash
docker exec -it loccar-db psql -U postgres -d loccardb_auth
```

## Variáveis de Ambiente

As seguintes variáveis estão configuradas no docker-compose.yml:
- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection` - String de conexão com o PostgreSQL
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key` - Configurações JWT

## Troubleshooting

### Problema de conexão com banco
- Verifique se o container do PostgreSQL está rodando: `docker-compose ps`
- Verifique os logs: `docker-compose logs db`

### Rebuild completo
```bash
docker-compose down
docker system prune -f
docker-compose up --build
```
