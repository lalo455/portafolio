// Interfaces/ISapOrderProcessor.cs
// Definimos la interfaz aquí para que el controlador compile. La implementaremos después.
namespace SapMiddleware
{
    public interface ISapOrderProcessor
    {
        Task ProcessOrderAsync(Guid ticketId);
    }
}