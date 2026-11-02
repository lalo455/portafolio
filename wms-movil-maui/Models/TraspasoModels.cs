using CommunityToolkit.Mvvm.ComponentModel;

namespace WmsMovil.Models { 

    public class Cliente
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string FromWhs { get; set; }
        public string ToWhs { get; set; }

        // Para que el combo muestre "C003 - COLCHONES WENDY"
        public string DisplayName => $"{CardCode} - {CardName}";
    }

    public class Almacen
    {
        public string WhsCode { get; set; }
        public string WhsName { get; set; }

        // Para que el combo muestre "01 - Almacén general"
        public string DisplayName => $"{WhsCode} - {WhsName}";
    }

    public class CatalogosResponseData
    {
        public List<Cliente> Clientes { get; set; }
        public List<Almacen> Almacenes { get; set; }
    }

    // === ESTA ES LA CLASE QUE FALTABA ===
    public class SolicitudAbierta
    {
        public string CardCode { get; set; }
        public int DocNum { get; set; }
        public int TotalItem { get; set; }
    }
    // ====================================

    public class LineaST
    {
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string FromWhsCod { get; set; }
        public string ToWhsCode { get; set; }
        public double Quantity { get; set; }
        public double OpenQty { get; set; }
        public double QtyLeido { get; set; }
    }

    // === MODELOS PARA LA FASE 1 (VALIDACIÓN) ===
    public class ValidacionRequest
    {
        public string wmsOrigin { get; set; } = "MAUI_APP";
        public string userName { get; set; }
        public string accion { get; set; } = "ValidarLectura";
        public string solTr { get; set; }
        public string clienteSel { get; set; }
        public string whsCodeOrigen { get; set; }
        public string whsCodeDestino { get; set; }
        public string ubicacionDestino { get; set; }
        public int entryST { get; set; }
        public List<string> tagsEscaneados { get; set; }
    }

    public class ValidacionResponseData
    {
        public List<LoteEscaneado> Lotes { get; set; }
        public List<AlertaValidacion> Alertas { get; set; }
    }


public class AlertaValidacion
{
    public string Tipo { get; set; } // "e", "w", "i"
    public string Texto { get; set; }

    // Como la API no manda Titulo, lo creamos nosotros basándonos en el Tipo
    public string Titulo => Tipo == "e" ? "Error" : (Tipo == "w" ? "Advertencia" : "Información");

    public Color ColorAlerta => Tipo == "e" ? Colors.Red : (Tipo == "w" ? Colors.Orange : Colors.Blue);
}
// Heredamos de ObservableObject para que la UI reaccione a los cambios de color
public partial class LoteEscaneado : ObservableObject
{
    public string DistNumber { get; set; }
    public string ItemCode { get; set; }
    public double OnHandQty { get; set; }
    public string CodigoTarima { get; set; }
    public string UbQty { get; set; }
    public double QtySAP { get; set; }
    public int Etqs { get; set; } = 1;

    // Variable reactiva para el color
    [ObservableProperty]
    private Color _colorFondo = Colors.White;
}
    public class TraspasoRequest
    {
        public string wmsOrigin { get; set; } = "MAUI_APP";
        public string userName { get; set; }
        public string accion { get; set; } = "ValidarLectura";
        public string solTr { get; set; }
        public string clienteSel { get; set; }
        public string whsCodeOrigen { get; set; }
        public string whsCodeDestino { get; set; }
        public string ubicacionDestino { get; set; }
        public int entryST { get; set; }
        public List<string> tagsEscaneados { get; set; }
    }

    // 2. La Respuesta exacta que manda la API
    public class ValidacionLecturaResponse
    {
        public List<LoteValidado> Lotes { get; set; }
        public List<MensajeAlerta> Alertas { get; set; }
    }

    // 3. El modelo de Alertas de la API
    public class MensajeAlerta
    {
        public string Tipo { get; set; } // 'w', 'e', 'i', 's'
        public string Texto { get; set; }

        // Propiedades visuales para MAUI (La API no las manda, MAUI las calcula)
        public string Titulo => Tipo == "e" ? "Error" : (Tipo == "w" ? "Advertencia" : "Información");
        public Color ColorAlerta => Tipo == "e" ? Colors.Red : (Tipo == "w" ? Colors.Orange : Colors.Blue);
    }

    // 4. El modelo de Lote de la API (Hereda de ObservableObject para cambiar de color)
    public partial class LoteValidado : ObservableObject
    {
        public string ItemCode { get; set; }
        public string DistNumber { get; set; }
        public string WhsCode { get; set; }
        public string WhsDestino { get; set; }
        public double OnHandQty { get; set; }
        public double QtySAP { get; set; }
        public int AbsEntry { get; set; }
        public int BaseLine { get; set; }
        public string TipoEtiqueta { get; set; }
        public string ItemName { get; set; }
        public string Rack { get; set; }
        public string CodigoTarima { get; set; }
        public double Peso { get; set; }
        public double MetrosBrutos { get; set; }
        public double Longitud { get; set; }
        public double Ancho { get; set; }
        public string GrupoArticulo { get; set; }
        public string BinCode { get; set; }

        // --- PROPIEDADES VISUALES PARA MAUI ---
        public int Etqs { get; set; } = 1;

        [ObservableProperty]
        private Color _colorFondo = Colors.White;
    }
}