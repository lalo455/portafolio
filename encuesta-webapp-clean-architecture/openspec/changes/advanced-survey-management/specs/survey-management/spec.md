## ADDED Requirements

### Requirement: Edición de encuesta
El sistema SHALL permitir al administrador modificar los datos generales de una encuesta existente.

#### Scenario: Edición exitosa
- **WHEN** el administrador actualiza el título o descripción y guarda
- **THEN** el sistema actualiza el registro en la base de datos

### Requirement: Eliminación de encuesta
El sistema SHALL permitir la eliminación completa (borrado físico) de una encuesta y todos sus datos relacionados.

#### Scenario: Borrado en cascada
- **WHEN** el administrador invoca la acción de eliminar
- **THEN** el sistema borra la encuesta, sus preguntas, opciones y respuestas asociadas de la base de datos
