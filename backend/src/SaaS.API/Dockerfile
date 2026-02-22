FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/SaaS.Domain/SaaS.Domain.csproj", "src/SaaS.Domain/"]
COPY ["src/SaaS.Application/SaaS.Application.csproj", "src/SaaS.Application/"]
COPY ["src/SaaS.Infrastructure/SaaS.Infrastructure.csproj", "src/SaaS.Infrastructure/"]
COPY ["src/SaaS.API/SaaS.API.csproj", "src/SaaS.API/"]

RUN dotnet restore "src/SaaS.API/SaaS.API.csproj"
COPY . .
RUN dotnet build "src/SaaS.API/SaaS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/SaaS.API/SaaS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SaaS.API.dll"]
