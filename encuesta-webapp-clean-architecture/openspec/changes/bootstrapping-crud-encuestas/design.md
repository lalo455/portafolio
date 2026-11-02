## Context

El proyecto "Encuesta" necesita establecer su arquitectura base y flujos iniciales. Los agentes tienen directivas estrictas en `AGENTS.md` sobre cómo se debe estructurar el código: el `@arquitecto` define interfaces y esquemas SQL, el `@backend-dev` implementa lógica de negocio y controladores delgados, y el `@frontend-dev` conecta vistas Razor usando ViewModels robustos. Implementar el CRUD de `Survey` servirá como modelo base arquitectónico (bootstrapping).

## Goals / Non-Goals

**Goals:**
- Establecer una arquitectura de capas lógicas (Core, Infrastructure, Application, Web) en el repositorio.
- Configurar inyección de dependencias en el proyecto web.
- Implementar el CRUD básico de la entidad `Survey` aislando el acceso a datos.
- Implementar validaciones DRY centralizadas en ViewModels usando Data Annotations.

**Non-Goals:**
- Autenticación o Autorización (fuera del alcance inicial).
- Implementación de la vista pública de respondentes (se centrará en la gestión de administración).
- Estilización visual compleja de UI (se utilizará un scaffolding básico funcional por ahora).

## Decisions

- **Arquitectura en Capas Logicas:** El proyecto se estructurará internamente separando carpetas `Core` (interfaces y entidades), `Infrastructure` (repositorios a BD), `Application` (servicios) y `Web` (MVC).
- **Patrón Repositorio y Servicio:** `ISurveyRepository` definirá contratos de BD y `ISurveyService` lógica de negocio. `SurveyController` dependerá exclusivamente de `ISurveyService`.
- **ViewModels como Contratos UI:** Las vistas no consumirán la entidad `Survey` directamente, sino `SurveyViewModel`, garantizando que la vista solo renderice lo que el backend provee, con validaciones no obstructivas activadas por atributos.
- **Acceso a Datos:** Se implementará el repositorio inicial con enfoque directo a SQL Server (via Dapper/ADO.NET o EF) según defina la infraestructura final, manteniendo el controlador completamente ajeno al método subyacente.

## Risks / Trade-offs

- [Acoplamiento Temprano] → Mitigación: Uso estricto de contratos (`interfaces`) para todos los servicios y repositorios inyectados.
- [Duplicidad de Validaciones] → Mitigación: Utilizar Data Annotations en `SurveyViewModel` que alimenten tanto el `ModelState.IsValid` del backend como el unobtrusive JavaScript del frontend.
