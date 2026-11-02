## 1. Core (Entidades y Contratos)

- [x] 1.1 Crear el Enum `QuestionType` en `Core/Entities/QuestionType.cs` (`Text`, `YesNo`, `Scale`, `Emoji`).
- [x] 1.2 Crear modelos de dominio `Question` y `QuestionOption` en `Core/Entities`.
- [x] 1.3 Crear modelos de dominio `SurveyResponse` y `Answer` en `Core/Entities`.
- [x] 1.4 Definir contratos de persistencia `IQuestionRepository` e `IResponseRepository`.
- [x] 1.5 Definir contratos de lógica `IQuestionService` e `IResponseService`.

## 2. Infrastructure (Base de Datos)

- [x] 2.1 Extender `Schema.sql` agregando las tablas de `Question`, `QuestionOption`, `SurveyResponse` y `Answer` con cascada de borrado.
- [x] 2.2 Implementar `QuestionRepository`.
- [x] 2.3 Implementar `ResponseRepository`.

## 3. Application (Lógica de Negocio)

- [ ] 3.1 Actualizar `SurveyService` para habilitar el Update (editar) y Delete completo (borrado físico en cascada).
- [ ] 3.2 Implementar `QuestionService` manejando la generación de opciones por defecto (por ej. opciones de Sí/No o emojis).
- [ ] 3.3 Implementar `ResponseService` incluyendo funciones para el cálculo de estadísticas agrupadas por tipo de pregunta.

## 4. Web (MVC) - Backend Administrativo

- [ ] 4.1 Registrar nuevos repositorios y servicios en `Program.cs`.
- [ ] 4.2 Actualizar `SurveyController` con acciones HTTP GET/POST para `Edit` y `Delete`.
- [ ] 4.3 Crear `QuestionController` enfocado en listar y crear preguntas para una encuesta seleccionada.
- [ ] 4.4 Crear `AnalyticsController` que devuelva a la vista el `AnalyticsViewModel` procesado.
- [ ] 4.5 Construir vistas Razor para la gestión de preguntas y el Dashboard de Analíticas.

## 5. Web (MVC) - Vista Pública de Respondentes

- [ ] 5.1 Crear `PublicSurveyController` con un método GET que reciba el ID de encuesta y construya un formulario dinámico a través de `PublicExecutionViewModel`.
- [ ] 5.2 Desarrollar el método POST en `PublicSurveyController` que procese la lista de respuestas enviadas por el formulario y las guarde mediante `IResponseService`.
- [ ] 5.3 Implementar vista dinámica en `Views/PublicSurvey/Execute.cshtml` capaz de renderizar `inputs`, `radios`, y `sliders` dependiendo del `QuestionType`.
