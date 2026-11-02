## ADDED Requirements

### Requirement: Vista pública de encuesta
El sistema SHALL proveer una URL pública para que los encuestados puedan ver y resolver una encuesta.

#### Scenario: Encuesta activa
- **WHEN** un usuario navega a la URL pública de una encuesta activa
- **THEN** el sistema muestra todas las preguntas renderizadas según su tipo en un formulario interactivo

### Requirement: Envío de respuestas
El sistema SHALL capturar y almacenar todas las respuestas enviadas por el respondente.

#### Scenario: Envío exitoso
- **WHEN** el usuario completa el formulario y lo envía
- **THEN** el sistema crea un registro de `SurveyResponse` y las respectivas `Answer` en la base de datos, redireccionando a un mensaje de agradecimiento
