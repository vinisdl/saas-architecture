/**
 * OpenTelemetry tracing para o frontend (padrão OTLP).
 * Envia traces para o proxy do backend (/api/telemetry), que reenvia ao Alloy/Grafana Cloud.
 */

import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http'
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch'
import { registerInstrumentations } from '@opentelemetry/instrumentation'
import { Resource } from '@opentelemetry/resources'
import { BatchSpanProcessor, WebTracerProvider } from '@opentelemetry/sdk-trace-web'

function getOtlpTracesUrl(): string {
  const env = import.meta.env.VITE_OTEL_EXPORTER_OTLP_ENDPOINT
  if (env && typeof env === 'string') {
    const base = env.replace(/\/v1\/traces\/?$/, '')
    return `${base}/v1/traces`
  }
  const apiBase = import.meta.env.VITE_API_URL ?? '/api'
  const origin = typeof window !== 'undefined' ? window.location.origin : ''
  const base = `${origin}${apiBase}/telemetry`
  return `${base}/v1/traces`
}

export function initTelemetry(): void {
  const url = getOtlpTracesUrl()
  const resource = new Resource({
    'service.name': 'saas-frontend',
  })
  const exporter = new OTLPTraceExporter({ url })
  const provider = new WebTracerProvider({
    resource,
    spanProcessors: [new BatchSpanProcessor(exporter)],
  })
  provider.register()

  registerInstrumentations({
    instrumentations: [new FetchInstrumentation()],
  })
}
