using System.ComponentModel.DataAnnotations;

namespace SapMiddleware.DTOs;

public class PedidoIntegracionDto
{
    [Required(ErrorMessage = "El CardCode es obligatorio.")]
    public string CardCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Sociedad es obligatoria.")]
    public string Sociedad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    public DateTime DocDueDate { get; set; }

    public string Comments { get; set; } = string.Empty;

    public double Discount { get; set; }

    [Required(ErrorMessage = "El número de pedido web (U_NumPed) es obligatorio.")]
    public string U_NumPed { get; set; } = string.Empty;

    // Agregado de tu código anterior
    public string Uruta { get; set; } = string.Empty;

    [Required(ErrorMessage = "El pedido debe contener al menos una línea.")]
    [MinLength(1, ErrorMessage = "El pedido debe contener al menos una línea.")]
    public List<LineaPedidoDto> DocumentLines { get; set; } = new();
}

public class LineaPedidoDto
{
    [Required(ErrorMessage = "El ItemCode es obligatorio.")]
    public string ItemCode { get; set; } = string.Empty;

    public double PrtgDiscount { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public double PrecioUnitario { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public double Quantity { get; set; }

    public string VatGroup { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}