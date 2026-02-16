# Versões fixadas

Registro das versões utilizadas para reprodutibilidade.

## Backend (.NET)

- **SDK / Runtime**: .NET 9 (9.0.x) — utilizada na implementação (SDK 10 não instalado). Plano prevê upgrade para .NET 10.
- **Target framework**: `net9.0`
- **global.json**: em `backend/global.json` (rollForward: latestFeature)
- **Pacotes principais**: ASP.NET Core, EF Core, MediatR, RabbitMQ.Client, JWT Bearer — últimas estáveis compatíveis com .NET 10

## Frontend

- **React**: 19.x
- **Vite**: última estável
- **Node**: LTS (para build)
- **React Router**, **keycloak-js** ou **react-oidc-context**: últimas compatíveis com React 19

## Infraestrutura (Docker)

- **PostgreSQL**: 16 (imagem oficial)
- **Keycloak**: imagem oficial (latest ou tag estável)
- **RabbitMQ**: imagem oficial (latest ou tag estável)
- **Backend runtime**: `mcr.microsoft.com/dotnet/aspnet:10.0`
- **Backend build**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Frontend build**: Node LTS
- **Frontend serve**: Nginx Alpine

Atualizado na data de início do projeto. Consultar [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) e [react.dev](https://react.dev) para versões atuais.
