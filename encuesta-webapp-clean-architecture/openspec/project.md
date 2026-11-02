# Especificación del Proyecto: Sistema de Encuestas de Satisfacción

## 1. Stack Tecnológico
- **Backend/Framework:** ASP.NET MVC (C#).
- **Base de Datos:** SQL Server.
- **Frontend:** Vistas Razor, HTML/CSS responsivo, JavaScript puro/ligero para validaciones en cliente.

## 2. Arquitectura y Reglas de Negocio
Este proyecto se rige por los siguientes principios fundamentales:

### 2.1. SOLID
- **Single Responsibility Principle (SRP):** Las clases de los controladores solo gestionan el enrutamiento HTTP y las respuestas. La lógica reside en Servicios.
- **Dependency Inversion Principle (DIP):** Los Controladores dependen de interfaces de Servicios (`ISurveyService`), y los Servicios dependen de interfaces de Repositorio (`ISurveyRepository`). Inyección de dependencias configurada en el inicio de la aplicación.

### 2.2. DRY (Don't Repeat Yourself)
- **Validación transversal:** Uso de Data Annotations en los ViewModels (ej. `[Required]`, `[MaxLength]`) para validación automática tanto en cliente como en servidor.
- **Renderizado reutilizable:** Toda representación de tipo de pregunta (Opción múltiple, texto libre, Likert) debe extraerse a *Partial Views* (`_QuestionMultipleChoice.cshtml`, etc.) o *ViewComponents*.
- **Acceso a datos centralizado:** Uso de un patrón Repositorio Genérico base para operaciones CRUD estándar.

## 3. Modelo de Datos (SQL Server)
- `Survey`: `Id` (PK), `Title`, `Description`, `StartDate`, `EndDate`, `IsActive`.
- `Question`: `Id` (PK), `SurveyId` (FK), `Text`, `QuestionType` (Enum/Int), `IsRequired`, `OrderIndex`.
- `QuestionOption`: `Id` (PK), `QuestionId` (FK), `OptionText`, `Value`.
- `Response`: `Id` (PK), `SurveyId` (FK), `SubmittedAt`, `UserId` (Opcional).
- `Answer`: `Id` (PK), `ResponseId` (FK), `QuestionId` (FK), `TextValue`, `SelectedOptionId` (FK opcional).

## 4. Flujo de OpenSpec
- La carpeta `openspec/specs/` contiene el estado actual del sistema.
- Cualquier cambio o característica nueva debe proponerse en `openspec/changes/<nombre-cambio>/` antes de su implementación.
- Los tests deben ejecutarse para validar criterios de aceptación.
