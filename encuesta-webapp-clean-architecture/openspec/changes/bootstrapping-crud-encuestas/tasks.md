## 1. Configuración Inicial y Estructura

- [x] 1.1 Crear la estructura de carpetas lógica (`Core`, `Infrastructure`, `Application`, `Web/Controllers`, `Web/ViewModels`, `Web/Views/Survey`).
- [x] 1.2 Configurar el entorno (si aplica) con los paquetes necesarios para acceso a datos.

## 2. Core (Modelos y Contratos)

- [x] 2.1 Crear el modelo de dominio `Survey` en `Core/Entities/Survey.cs`.
- [x] 2.2 Definir el contrato `ISurveyRepository` en `Core/Interfaces/ISurveyRepository.cs`.
- [x] 2.3 Definir el contrato `ISurveyService` en `Core/Interfaces/ISurveyService.cs`.

## 3. Infrastructure (Acceso a Datos)

- [x] 3.1 Implementar `SurveyRepository` en `Infrastructure/Data/SurveyRepository.cs` cumpliendo con `ISurveyRepository`.
- [x] 3.2 Proveer el script SQL o esquema necesario para generar la tabla `Survey` en la base de datos SQL Server.

## 4. Application (Lógica de Negocio)

- [x] 4.1 Implementar `SurveyService` en `Application/Services/SurveyService.cs` para manejar la lógica CRUD llamando al repositorio.

## 5. Web (Presentación MVC)

- [x] 5.1 Registrar `ISurveyRepository` y `ISurveyService` en el contenedor de Inyección de Dependencias del proyecto MVC.
- [x] 5.2 Crear `SurveyViewModel` en `Web/ViewModels/SurveyViewModel.cs` con sus Data Annotations.
- [x] 5.3 Crear `SurveyController` (Thin Controller) en `Web/Controllers/SurveyController.cs` con métodos GET/POST para el CRUD.
- [x] 5.4 Crear la vista de listado en `Web/Views/Survey/Index.cshtml`.
- [x] 5.5 Crear la vista de formulario en `Web/Views/Survey/Create.cshtml` soportando validación de cliente.
- [x] 5.6 Implementar endpoint POST en `SurveyController` para Desactivar encuestas, según la especificación.
