## ADDED Requirements

### Requirement: Dashboard de métricas
El sistema SHALL proveer un panel administrativo para visualizar el resumen de los resultados por encuesta.

#### Scenario: Visualización de métricas
- **WHEN** el administrador accede a la sección de analíticas de una encuesta
- **THEN** el sistema muestra el total de participaciones (`SurveyResponse`) y totalizaciones de respuestas por cada pregunta cerrada (conteo de opciones, promedio para escala numérica)
