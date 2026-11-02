## Why

El proyecto necesita establecer su arquitectura base (Core, Infrastructure, Application, Web) e implementar el flujo CRUD fundacional para la entidad principal `Survey`. Esto definirá el estándar de desarrollo (inyección de dependencias, repositorios, thin controllers, y validaciones bidireccionales) para los futuros módulos del sistema, asegurando el cumplimiento de las directivas arquitectónicas del equipo de agentes.

## What Changes

- Establecimiento de la estructura de proyecto en capas (Core, Infrastructure, Application, Web).
- Creación del modelo de dominio `Survey`.
- Definición de contratos base de persistencia y negocio (`ISurveyRepository`, `ISurveyService`).
- Implementación de infraestructura de acceso a datos para SQL Server.
- Implementación de la lógica de negocio en `SurveyService`.
- Creación del DTO/ViewModel `SurveyViewModel` con Data Annotations correspondientes.
- Implementación del `SurveyController` (Thin Controller) para exponer endpoints MVC.
- Creación de vistas Razor `Index.cshtml` (Listado) y `Create.cshtml` (Formulario) responsivas.

## Capabilities

### New Capabilities
- `survey-management`: Capacidad para gestionar el ciclo de vida de una encuesta (Crear, Leer, Desactivar) sentando las bases arquitectónicas del proyecto.

### Modified Capabilities

## Impact

- Define los estándares de codificación y arquitectura para todo el proyecto.
- Generación de la tabla `Survey` en la base de datos SQL Server.
- Se configurará la inyección de dependencias en el contenedor de servicios de la aplicación web.
- Impacta la estructura de carpetas y el flujo de trabajo de frontend y backend.
