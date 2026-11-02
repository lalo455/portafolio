# RFID Inventory App (Xamarin.Forms / Android)

App móvil para lectores RFID UHF de mano: inventario por lectura de etiquetas EPC, administración de ítems y personas, importación masiva, reportes y almacenamiento local SQLite. Arquitectura MVVM (Models, ViewModels, Views, Services) con servicio en primer plano para lectura continua.

> **Versión de portafolio (saneada).** Se removieron las librerías propietarias del fabricante del lector (`DeviceAPI*.dll`), bases locales, direcciones de servidores, nombres de clientes y el historial `.git`. Para compilar necesitas el SDK de tu lector.
