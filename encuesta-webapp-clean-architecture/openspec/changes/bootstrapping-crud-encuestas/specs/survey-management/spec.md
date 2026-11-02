## ADDED Requirements

### Requirement: Crear una nueva encuesta
El sistema SHALL permitir la creación de una nueva encuesta capturando al menos un título y descripción.

#### Scenario: Creación exitosa con datos válidos
- **WHEN** un usuario envía el formulario de nueva encuesta con un título válido
- **THEN** el sistema guarda la encuesta en base de datos y redirige a la vista de listado

#### Scenario: Validación fallida por falta de título
- **WHEN** un usuario envía el formulario sin especificar el título
- **THEN** el sistema intercepta la acción (frontend/backend) mostrando un error de campo requerido y previniendo el guardado

### Requirement: Listado de encuestas
El sistema SHALL proveer una vista donde se Listen las encuestas existentes.

#### Scenario: Visualización del índice de encuestas
- **WHEN** un usuario accede a la ruta base de encuestas (Index)
- **THEN** el sistema obtiene y muestra la lista de encuestas a través del `ISurveyService`

### Requirement: Desactivación de encuesta
El sistema SHALL permitir cambiar el estado de una encuesta para desactivarla sin eliminar físicamente el registro (Soft Delete o Flag).

#### Scenario: Desactivar una encuesta activa
- **WHEN** un usuario invoca la acción de desactivar sobre una encuesta listada
- **THEN** el sistema actualiza su estado en la base de datos y actualiza la vista para reflejar el cambio de estado
