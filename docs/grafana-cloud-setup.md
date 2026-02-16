# Configuração Grafana Cloud e OpenTelemetry

Este documento descreve como obter as credenciais e variáveis de ambiente para enviar telemetria (traces, métricas e logs) ao Grafana Cloud usando o protocolo OTLP (OpenTelemetry).

## 1. Obter endpoint e autenticação no Grafana Cloud

1. Acesse o [Grafana Cloud Portal](https://grafana.com/auth/sign-in/) e faça login.
2. Na **Overview** da organização, selecione o stack (ou crie um) e clique em **Launch** para abrir o stack.
3. No stack, vá em **Connections** (ou expanda e clique em **Add new connection**).
4. Procure por **OpenTelemetry** e pressione Enter para filtrar.
5. Clique no tile **OpenTelemetry (OTLP)** e depois em **Configure**.
6. Na tela de configuração do OTLP você encontrará:
   - **OTLP Endpoint URL** (ex.: `https://otlp-gateway-<region>-<stack>.grafana.net/otlp`)
   - **Instance ID** e **API Key** (ou instruções para gerar um token)
   - Protocolo recomendado: **http/protobuf**

### Montar o header de autenticação

O Grafana Cloud usa autenticação HTTP Basic:

- Formato: `Authorization: Basic <base64(instanceId:apiKey)>`
- Em um shell (Linux/macOS):  
  `echo -n "INSTANCE_ID:API_KEY" | base64`
- Use o resultado em variáveis de ambiente ou no config do Alloy (sem colocar a API key no frontend).

## 2. Variáveis de ambiente

### Envio direto (quickstart, sem Alloy)

Para desenvolvimento ou testes, as aplicações podem enviar OTLP direto para o Grafana Cloud:

```bash
# Backend (.NET)
export OTEL_SERVICE_NAME=saas-backend
export OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp-gateway-<region>-<stack>.grafana.net/otlp
export OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
export OTEL_EXPORTER_OTLP_HEADERS="Authorization=Basic <base64(instanceId:apiKey)>"
export OTEL_RESOURCE_ATTRIBUTES=deployment.environment=development,service.namespace=saas
```

### Envio via Grafana Alloy (recomendado para produção)

Com o Alloy no Docker Compose, as aplicações enviam para o Alloy (sem precisar da API key). Apenas o container do Alloy usa as credenciais do Grafana Cloud.

**Backend e frontend (proxy):** apontam para o Alloy:

- Backend: `OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy:4317` (gRPC) ou `http://alloy:4318` (HTTP)
- Frontend (via proxy no backend): envia para o backend; o backend reenvia para o Alloy. O frontend usa a mesma base da API (`VITE_API_URL` ou `/api`) para o endpoint de telemetria; opcionalmente defina `VITE_OTEL_EXPORTER_OTLP_ENDPOINT` com a URL completa do proxy (ex.: `http://localhost:5000/api/telemetry`).

**Container Alloy:** use no `docker-compose` (ou em `.env`):

```bash
GRAFANA_CLOUD_OTLP_ENDPOINT=https://otlp-gateway-<region>-<stack>.grafana.net/otlp
GRAFANA_CLOUD_OTLP_AUTH=<base64(instanceId:apiKey)>
```

No config do Alloy, o endpoint e o header `Authorization: Basic <valor>` são lidos dessas variáveis. Para o Alloy subir sem erros, defina essas variáveis no `.env` ou no `docker-compose` ao usar Grafana Cloud; caso contrário o Alloy pode falhar ao iniciar.

## 3. Resumo: quickstart vs produção

| Cenário        | Quem envia telemetria | Onde enviam       | Quem tem API key |
|----------------|------------------------|-------------------|-------------------|
| Quickstart     | Backend e frontend     | Direto Grafana Cloud | Backend (env vars) |
| Produção       | Backend e frontend     | Alloy (local)     | Apenas Alloy     |

- **Quickstart:** mais simples; cada app precisa de `OTEL_EXPORTER_OTLP_*` e da API key (nunca no frontend; use proxy no backend para traces do browser).
- **Produção (Alloy):** um único ponto (Alloy) com credenciais; suporta sampling, retry e enriquecimento; aplicações só precisam do endpoint do Alloy.

## 4. Validar no Grafana Cloud

- **Traces:** Grafana → Explore → data source **Tempo** (ou Application Observability).
- **Métricas:** Explore → **Prometheus** / **Mimir**.
- **Logs:** Explore → **Loki**.

Ajuste `service.name` e `OTEL_RESOURCE_ATTRIBUTES` conforme necessário para filtrar por serviço e ambiente.
