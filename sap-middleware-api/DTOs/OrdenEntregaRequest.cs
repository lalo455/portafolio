// DTOs/OrdenEntregaRequest.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SapMiddleware.DTOs;

public class OrdenEntregaRequest
{
    [Required(ErrorMessage = "El DocEntry del pedido es obligatorio.")]
    public int DocEntryPedido { get; set; }

    [Required(ErrorMessage = "La Sociedad es obligatoria.")]
    public string Sociedad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del repartidor es obligatorio.")]
    public string NombreRepartidor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe incluir al menos un detalle de entrega.")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle de entrega.")]
    public List<DetalleEntregaRequest> Detalles { get; set; } = new();
}

public class DetalleEntregaRequest
{
    [Required]
    public int Linea { get; set; } // BaseLine de SAP (0, 1, 2...)

    [Required]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el repartidor marcó este artículo como entregado.
    /// Solo se procesan en SAP las líneas donde Entregado = true y CantidadEditada > 0.
    /// Equivale a detalle.Entregado del sistema Rastreo original.
    /// </summary>
    public bool Entregado { get; set; }

    /// <summary>
    /// Cantidad real que entregó el repartidor (puede ser menor al pedido original).
    /// </summary>
    public double CantidadEditada { get; set; }

    /// <summary>
    /// Cantidad original del pedido (referencia para determinar si fue entrega completa o parcial).
    /// </summary>
    public double Cantidad { get; set; }

    /// <summary>
    /// Cantidad pendiente tras esta entrega. Si == 0 para todos los detalles, el pedido se cierra en SAP.
    /// </summary>
    public double CantidadPendiente { get; set; }
}
