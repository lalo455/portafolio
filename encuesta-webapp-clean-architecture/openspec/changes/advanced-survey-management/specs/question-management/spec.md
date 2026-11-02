## ADDED Requirements

### Requirement: Añadir preguntas a una encuesta
El sistema SHALL permitir a los administradores agregar múltiples preguntas a una encuesta existente.

#### Scenario: Añadir pregunta de texto
- **WHEN** el administrador selecciona tipo "Texto" y define la pregunta
- **THEN** el sistema guarda la pregunta asociada a la encuesta sin opciones predefinidas

#### Scenario: Añadir pregunta de escala
- **WHEN** el administrador selecciona tipo "Escala"
- **THEN** el sistema guarda la pregunta y asume una configuración de escala del 1 al 10

### Requirement: Tipos de pregunta
El sistema SHALL soportar preguntas de tipo Texto, Sí/No/No sé, Escala (1-10) y Emojis.

#### Scenario: Selección de tipo y opciones
- **WHEN** se crea una pregunta de tipo cerrado (Sí/No/No sé o Emojis)
- **THEN** el sistema debe asociar automáticamente las opciones correspondientes a la pregunta para que el respondente pueda seleccionarlas
