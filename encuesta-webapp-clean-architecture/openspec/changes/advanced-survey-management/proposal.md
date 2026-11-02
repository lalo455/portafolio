## Why

El sistema base actual solo permite registrar encuestas genéricas y listarlas. Para que la plataforma sea funcional, necesitamos permitir al administrador construir el cuestionario (agregar distintos tipos de preguntas), habilitar a los usuarios finales a responder estas encuestas, y finalmente, proveer a la administración un panel para analizar las métricas de las respuestas. Esto convierte el esqueleto inicial en un motor de encuestas completo.

## What Changes

- Creación de entidades y tablas en la BD para `Question`, `QuestionOption`, `SurveyResponse` y `Answer`.
- Soporte para tipos de pregunta: Texto, Opción Múltiple (Sí/No/No sé), Escala (1-10) y Emojis.
- Nueva vista de edición para agregar y gestionar las preguntas de una encuesta existente (Administrador).
- Flujo público para que los encuestados vean el formulario de la encuesta y puedan enviar sus respuestas.
- Panel de métricas y analíticas por encuesta para el Administrador.

## Capabilities

### New Capabilities
- `question-management`: Gestión de preguntas por encuesta y sus opciones de respuesta, según el tipo de pregunta.
- `survey-execution`: Flujo de ejecución donde un encuestado responde una encuesta pública.
- `survey-analytics`: Visualización de métricas de respuestas por encuesta (dashboard).

### Modified Capabilities
- `survey-management`: Se actualizará para permitir la edición completa (nombre/descripción) y borrar (eliminar) la encuesta por completo, ya que en la versión anterior solo se permitía crear y desactivar.

## Impact

- **BD:** Cambios significativos en la base de datos añadiendo relaciones uno-a-muchos y mucho-a-muchos.
- **Frontend:** Nuevas vistas complejas (constructor de encuestas dinámico y visor público de encuestas dinámico).
- **Controladores:** Nuevos controladores para la sección pública y analíticas.
