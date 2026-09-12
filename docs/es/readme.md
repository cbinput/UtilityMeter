# Resumen de UtilityMeter

## Qué es UtilityMeter

UtilityMeter es un sistema para monitorear consumo residencial de agua, electricidad, gas y otros recursos, pensado tanto para viviendas individuales como para condominios. Su objetivo es que cada lectura quede trazable, con evidencia asociada, y que luego pueda validarse, analizarse y conciliarse.

## Qué problema resuelve

En entornos con consumo compartido, una propiedad con consumo anormal, una lectura mal tomada por la empresa o un reparto manual mal hecho puede distorsionar toda la prorrata. UtilityMeter crea un registro estructurado para poder:

- conservar lecturas del residente y de la empresa por separado
- adjuntar evidencia como fotos y PDFs
- calcular consumo a partir del historial del medidor
- detectar anomalías y lecturas pendientes
- conciliar el medidor principal del condominio contra los submedidores

## Conceptos centrales

UtilityMeter mantiene separados estos conceptos para conservar la trazabilidad:

1. **Measurement**: la lectura capturada del medidor
2. **Consumption**: el delta entre lecturas
3. **Billing**: la información de facturación de la empresa
4. **Proration**: el reparto de costos dentro del condominio o comunidad
5. **Audit and analysis**: detección de diferencias, anomalías y reportes

## Capacidades actuales

- Crear y consultar lecturas por id, medidor, propiedad y período
- Subir archivos de evidencia y asociarlos a una lectura
- Detectar advertencias cuando una lectura es menor que la anterior
- Detectar consumo anormalmente alto respecto al promedio histórico
- Listar lecturas pendientes y lecturas anómalas
- Generar resúmenes mensuales y vistas de conciliación por condominio
- Encolar procesos más pesados para ejecución asíncrona

## Grupos principales de API

- `/api/readings`
- `/api/evidence`
- `/api/reports`

Swagger queda disponible al levantar la API.

## Cómo levantarlo localmente

### Docker Compose

Es la opción recomendada para correr el stack completo.

```bash
docker compose up --build
```

Servicios disponibles:
- API / Swagger: `http://localhost:8080/swagger`
- API de MinIO: `http://localhost:9000`
- Consola de MinIO: `http://localhost:9001`

Para detenerlo:

```bash
docker compose down
```

### .NET CLI

Para desarrollo local sin contenedores, ejecuta la API y el Worker por separado usando la misma base y la misma ruta de cola.

```bash
export ConnectionStrings__UtilityMeterDb='Data Source=/tmp/utilitymeter.db'
export BACKGROUND_JOB_QUEUE_PATH='/tmp/utilitymeter-background-jobs'
```

Levantar la API:

```bash
dotnet run --project ./src/Presentation
```

Levantar el Worker en otra terminal:

```bash
dotnet run --project ./src/Worker
```

Usa las URLs que imprime `dotnet run`, o revisa `src/Presentation/Properties/launchSettings.json` para ver los valores actuales del perfil local.

## Estructura del proyecto

```text
src/
  Application/      reglas de negocio, commands, queries, validadores
  Infrastructure/   persistencia, storage e implementación de colas
  Presentation/     endpoints minimal API y configuración OpenAPI
  Worker/           host de trabajos en segundo plano
tests/              pruebas unitarias e integración
docs/               documentación bilingüe del producto y arquitectura
```

## Más documentación

- [Arquitectura](./architecture.md)
