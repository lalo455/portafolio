## Context

Actualmente el sistema cuenta con un CRUD básico para la entidad `Survey`. Para que la plataforma sea funcional, se necesita habilitar un constructor dinámico de cuestionarios (preguntas de múltiples tipos) y una interfaz pública donde el encuestado pueda resolverlas. Además, es esencial contar con analíticas de las respuestas de cada encuesta para el Administrador.

## Goals / Non-Goals

**Goals:**
- Extender el modelo de base de datos para soportar `Question`, `QuestionOption`, `SurveyResponse` y `Answer`.
- Soportar 4 tipos de pregunta: `Text`, `YesNo`, `Scale`, `Emoji`.
- Implementar la vista del Administrador para editar/borrar encuestas y gestionar sus preguntas.
- Implementar la vista pública (flujo de ejecución) para Respondentes.
- Implementar dashboard analítico básico de respuestas.

**Non-Goals:**
- Autenticación o control de sesión por usuario (se usará sesión anónima o cookie para los respondentes por ahora).
- Exportación de métricas a Excel/PDF.

## Decisions

- **Modelado de Preguntas (Tabla Única):** Se utilizará una sola tabla `Question` con una columna `Type` (Enum). Las opciones para preguntas cerradas (YesNo, Emoji) se pueden modelar pre-poblando una tabla relacional `QuestionOption` o manejándolas a nivel de presentación si son fijas, pero por flexibilidad usaremos `QuestionOption`.
- **Registro de Respuestas:** Las respuestas se agruparán bajo una entidad `SurveyResponse` (representando un intento). Cada respuesta individual será un registro en la tabla `Answer`, que apuntará a un `QuestionId` y contendrá un valor de texto (para preguntas abiertas) o una referencia a un `OptionId`.
- **Arquitectura:** Se crearán contratos específicos (`IQuestionRepository`, `IResponseRepository`) para respetar la separación de responsabilidades exigida en `AGENTS.md`.

## Risks / Trade-offs

- [Vistas complejas con mucha lógica] → Mitigación: El `@backend-dev` deberá construir ViewModels muy robustos que ya contengan la información pre-procesada, evitando que el `@frontend-dev` tenga que escribir lógica compleja en Razor.
