# SaaS Architecture (Multi-tenant)

Arquitetura de referência para aplicações SaaS multi-tenant: backend em .NET, frontend em React, PostgreSQL, Keycloak, RabbitMQ e observabilidade com OpenTelemetry + Grafana Cloud.

## Índice

- [Funcionalidades](#funcionalidades)
- [Stack](#stack)
- [Pré-requisitos](#pré-requisitos)
- [Quick start](#quick-start)
- [Desenvolvimento local](#desenvolvimento-local)
- [Configuração](#configuração)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Documentação](#documentação)
- [Contribuição](#contribuição)
- [Licença](#licença)

## Funcionalidades

- **Multi-tenancy** com isolamento por tenant (TenantId, contexto por request)
- **Autenticação e autorização** via Keycloak (OAuth2/OIDC, JWT)
- **Backend** em arquitetura hexagonal + CQRS (MediatR)
- **Frontend** SPA em React com roteamento e área administrativa
- **Mensageria** com RabbitMQ para eventos entre serviços
- **Observabilidade** com OpenTelemetry (OTLP) e integração Grafana Cloud / Grafana Alloy

## Stack

| Camada         | Tecnologia                    |
|----------------|-------------------------------|
| Banco de dados | PostgreSQL 16                 |
| Backend        | C# (.NET 9), ASP.NET Core     |
| Frontend       | React 19, Vite 7              |
| Autenticação   | Keycloak (OIDC/JWT)          |
| Mensageria     | RabbitMQ                     |
| Observabilidade| OpenTelemetry, Grafana Alloy  |
| Infraestrutura | Docker, Docker Compose       |

Versões detalhadas em [docs/versions.md](docs/versions.md).

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) LTS (para o frontend)
- [Docker](https://docs.docker.com/get-docker/) e [Docker Compose](https://docs.docker.com/compose/install/)

## Quick start

Com Docker e Docker Compose instalados:

```bash
git clone <url-do-repositorio>
cd SaaS-Architecture
docker compose up -d
```

Após subir os containers:

| Serviço    | URL                      |
|------------|--------------------------|
| Frontend   | http://localhost:3000     |
| API        | http://localhost:5000     |
| Keycloak   | http://localhost:8080     |
| RabbitMQ   | http://localhost:15672    |
| Alloy (OTLP)| portas 4317 (gRPC), 4318 (HTTP) |

Para enviar telemetria ao Grafana Cloud, configure as variáveis do Alloy conforme [docs/grafana-cloud-setup.md](docs/grafana-cloud-setup.md).

## Desenvolvimento local

### Backend

```bash
cd backend
dotnet restore
dotnet run --project src/SaaS.API
```

Requer PostgreSQL e Keycloak (por exemplo via `docker compose up -d postgres keycloak rabbitmq alloy`).

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Defina as variáveis de ambiente (ex.: `VITE_API_URL`, `VITE_KEYCLOAK_*`). Use `.env.example` como base se existir.

## Configuração

- **Backend:** `backend/src/SaaS.API/appsettings.json` e `appsettings.Development.json` (connection strings, Keycloak, RabbitMQ, OpenTelemetry/TelemetryProxy).
- **Frontend:** variáveis `VITE_*` (API URL, Keycloak, opcionalmente `VITE_OTEL_EXPORTER_OTLP_ENDPOINT`).
- **Docker:** variáveis no `docker-compose.yml` ou em arquivo `.env` na raiz (ex.: credenciais Grafana Cloud para o Alloy).

## Estrutura do projeto

```
SaaS-Architecture/
├── backend/                 # Solução .NET (API, Application, Domain, Infrastructure)
├── frontend/                # Aplicação React (Vite)
├── alloy/                   # Config do Grafana Alloy (OTLP → Grafana Cloud)
├── docs/                    # Documentação (setup Keycloak, Grafana Cloud, versões)
├── docker-compose.yml
└── README.md
```

## Documentação

- [Configuração do Keycloak](docs/keycloak-setup.md)
- [Grafana Cloud e OpenTelemetry](docs/grafana-cloud-setup.md)
- [Versões da stack](docs/versions.md)

## Contribuição

Contribuições são bem-vindas. Sugestões:

1. Abra uma [issue](https://github.com/seu-usuario/SaaS-Architecture/issues) para discussão.
2. Faça um fork, crie um branch para sua alteração e envie um pull request.
3. Mantenha o código alinhado ao estilo do projeto e aos testes existentes.

## Licença

Este projeto está sob a licença que constar no arquivo [LICENSE](LICENSE) na raiz do repositório. Se não houver arquivo LICENSE, entre em contato com os mantenedores para condições de uso.
