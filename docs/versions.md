# Versões fixadas

Registro das versões utilizadas para reprodutibilidade.

## Backend (.NET)

- **SDK / Runtime**: .NET 10 (10.0.x)
- **Linguagem**: C# 14
- **Target framework**: `net10.0`
- **global.json**: em `backend/global.json` (versão 10.0.100, rollForward: latestFeature)
- **Pacotes principais**: ASP.NET Core 10, EF Core 10, MediatR, Npgsql.EntityFrameworkCore.PostgreSQL 10, RabbitMQ.Client, JWT Bearer — compatíveis com .NET 10

## Frontend

- **React**: 19.x
- **Vite**: última estável
- **Node**: LTS (para build)
- **React Router**, **keycloak-js** ou **react-oidc-context**: últimas compatíveis com React 19

## Infraestrutura (Docker)

- **PostgreSQL**: 16 (imagem oficial)
- **Keycloak**: imagem oficial (latest ou tag estável)
- **RabbitMQ**: imagem oficial (latest ou tag estável)
- **Backend (produção)**: build `mcr.microsoft.com/dotnet/sdk:10.0`, runtime `mcr.microsoft.com/dotnet/aspnet:10.0` (ver `backend/src/SaaS.API/Dockerfile`)
- **Backend (desenvolvimento)**: `mcr.microsoft.com/dotnet/sdk:10.0-alpine` com hot reload (ver `backend/src/SaaS.API/Dockerfile.dev`)
- **Frontend build**: Node LTS
- **Frontend serve**: Nginx Alpine

Consultar [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) e [react.dev](https://react.dev) para versões atuais.
