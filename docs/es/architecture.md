# Arquitectura de UtilityMeter

## Visión general

UtilityMeter sigue la estructura de cuatro proyectos del repositorio:

- **Presentation** recibe solicitudes HTTP y expone Swagger/OpenAPI.
- **Application** contiene la lógica de dominio, commands y queries con MediatR, validadores y mapeos a DTOs.
- **Infrastructure** implementa persistencia con SQLite/EF Core, object storage y la cola de trabajos en segundo plano.
- **Worker** consume trabajos encolados y los vuelve a despachar a handlers de Application.

## Arquitectura de alto nivel

```mermaid
flowchart LR
    Client[Usuario o integración] --> API[Presentation API]
    API --> App[Application]
    App --> Infra[Infrastructure]
    Infra --> Db[(Base de datos vía EF Core)]
    Infra --> Storage[(MinIO o almacenamiento en memoria)]
    API --> Queue[IBackgroundJobQueue]
    Queue --> Worker[Worker service]
    Worker --> App
```

## Flujo del dominio

```mermaid
flowchart LR
    Measurement[Measurement / Reading] --> Consumption[Cálculo de consumo]
    Consumption --> Billing[Comparación con facturación]
    Billing --> Proration[Prorrata / conciliación]
    Proration --> Audit[Auditoría y análisis]
    Evidence[Archivos de evidencia] --> Measurement
```

## Componentes principales en ejecución

### API

La API expone endpoints agrupados para:
- lecturas
- evidencia
- reportes

Se mantiene delgada: los endpoints delegan commands y queries a `ISender`.

### Application

El proyecto Application contiene:
- readings
- evidence
- reports
- alerts
- contratos de background jobs
- abstracciones de storage

Los resultados de negocio, como alertas, se registran dentro del flujo de lectura y reportes, no como si fueran errores de formato de entrada.

### Infrastructure

Infrastructure hoy aporta:
- acceso a datos con EF Core
- persistencia configurable a base de datos mediante EF Core
- fallback de base en memoria para desarrollo cuando no hay connection string
- object storage con MinIO cuando está configurado
- despacho mediante `Channel` con persistencia de trabajos en JSON bajo `BACKGROUND_JOB_QUEUE_PATH`

### Worker

El Worker es un host separado que procesa trabajos para:
- extracción OCR
- análisis de anomalías
- generación de reportes
- exportaciones

## Secuencia: crear lectura con evidencia y procesamiento asíncrono

```mermaid
sequenceDiagram
    actor User as Usuario
    participant API as Presentation API
    participant App as Application
    participant Repo as Repositorio de lecturas
    participant Storage as Object Storage
    participant Queue as Cola de trabajos
    participant Worker as Worker

    User->>API: POST /api/evidence/upload
    API->>App: UploadEvidenceCommand
    App->>Storage: Guardar archivo
    App-->>API: Id de evidencia

    User->>API: POST /api/readings
    API->>App: CreateReadingCommand
    App->>Repo: Cargar lectura previa
    App->>Repo: Guardar lectura + alertas
    App-->>API: Lectura creada

    User->>API: POST /api/evidence/attach
    API->>App: AttachEvidenceToReadingCommand
    App->>Repo: Vincular evidencia a la lectura
    App-->>API: Asociación confirmada

    User->>API: POST /api/reports/abnormal-readings/{billingPeriodId}/analyze
    API->>Queue: Encolar análisis de anomalías
    API-->>User: 202 Accepted
    Worker->>Queue: Tomar trabajo
    Worker->>App: ProcessAnomalyAnalysisJobCommand
    App-->>Worker: Análisis completado
```

## Secuencia: conciliación de condominio

```mermaid
sequenceDiagram
    actor User as Usuario
    participant API as Presentation API
    participant App as Application
    participant Repo as Repositorio de conciliación

    User->>API: GET /api/reports/condominiums/{condominiumId}/reconciliation/{billingPeriodId}
    API->>App: GetCondominiumReconciliationQuery
    App->>Repo: Cargar condominio, propiedades, medidores y lecturas
    Repo-->>App: Datos de conciliación
    App->>App: Comparar medidor principal vs submedidores
    App->>App: Marcar faltantes, alto consumo y diferencias con factura
    App-->>API: DTO de conciliación
    API-->>User: % de diferencia, propiedades sospechosas y explicación
```

## Vista de despliegue local

```mermaid
flowchart TB
    subgraph DockerCompose[Docker Compose]
        API[servicio api]
        Worker[servicio worker]
        MinIO[servicio minio]
        Volume[(volumen compartido /data)]
    end

    API --> Volume
    Worker --> Volume
    API --> MinIO
    Worker --> MinIO
```

## Notas para contribuidores

- Ejecuta **Presentation** y **Worker** si quieres probar flujos asíncronos.
- Docker Compose es la forma más simple de tener base compartida, ruta de cola y object storage en local.
- La implementación actual ya sigue la separación esperada entre escrituras rápidas por API y trabajo pesado en segundo plano.
