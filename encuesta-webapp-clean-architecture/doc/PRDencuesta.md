## Documento de Requisitos del Producto (PRD): Sistema de Encuestas de Satisfacción

### 1. Visión General del Producto

El objetivo es desarrollar una aplicación web para la creación, distribución y análisis de encuestas de satisfacción. El sistema permitirá a los administradores diseñar cuestionarios dinámicos y a los usuarios finales enviar sus respuestas de manera rápida e intuitiva.

### 2. Arquitectura y Estándares de Desarrollo

* **Framework:** ASP.NET MVC.
* **Base de Datos:** SQL Server.
* **Patrones y Principios:**
* **SOLID:** Aplicación estricta de Inyección de Dependencias (DI) en controladores para depender de interfaces en lugar de implementaciones concretas (SRP y DIP). Separación clara entre la capa de acceso a datos (Patrón Repositorio), la capa de lógica de negocio (Servicios) y la capa de presentación (Controladores/Vistas).
* **DRY:** Centralización de la lógica de validación (por ejemplo, usando FluentValidation o Data Annotations de manera transversal). Uso de *Partial Views* y *ViewComponents* para elementos de interfaz repetitivos (como la renderización de distintos tipos de preguntas), y abstracción de consultas comunes a la base de datos para evitar código duplicado.



### 3. Requisitos Funcionales (Borrador Base)

* **Módulo de Administración:**
* Crear, editar, clonar y desactivar encuestas.
* Configurar diferentes tipos de preguntas (opción múltiple, texto libre, escala de Likert).
* Gestión de vigencia (fecha de inicio y fin de la encuesta).


* **Módulo de Respondentes:**
* Interfaz de usuario responsiva para contestar las encuestas.
* Validación de respuestas obligatorias tanto en cliente (JavaScript) como en servidor.


* **Módulo de Resultados:**
* Visualización de respuestas tabuladas y estadísticas básicas.



### 4. Modelo de Datos Propuesto (SQL Server)

Una estructura relacional normalizada:

* `Survey` (Encuesta): ID, Título, Descripción, FechaInicio, FechaFin, Activa.
* `Question` (Pregunta): ID, SurveyID, Texto, TipoPregunta (Enum), EsObligatoria, Orden.
* `QuestionOption` (Opciones): ID, QuestionID, TextoOpcion, Valor.
* `Response` (Respuesta Global): ID, SurveyID, FechaRespuesta, IdentificadorUsuario (si aplica).
* `Answer` (Respuesta Detalle): ID, ResponseID, QuestionID, ValorTexto, QuestionOptionID.

