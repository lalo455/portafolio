// DTOs/OrderRequestDto.cs
namespace SapMiddleware.DTOs;

public class OrderRequestDto
{
    public string CardCode { get; set; } = string.Empty;
    public DateTime DocDueDate { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string U_NumPed { get; set; } = string.Empty;

    // NOTA: En tu código original obtenías la Ruta de una base de datos externa.
    // En una arquitectura limpia, el portal web debe mandar este dato en el JSON.
    public string Uruta { get; set; } = string.Empty;

    public List<OrderLineDto> DocumentLines { get; set; } = new();
}

public class OrderLineDto
{
    public string ItemCode { get; set; } = string.Empty;
    public double PrecioUnitario { get; set; }
    public double Discount { get; set; } // U_Desc1
    public double PrtgDiscount { get; set; } // U_Desc2
    public double Quantity { get; set; }
    public string VatGroup { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}