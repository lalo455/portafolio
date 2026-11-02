namespace WmsMovil.Models
{
    // Respuesta genérica de la API
    public class WmsResponse<T>
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public T Data { get; set; }
    }

    // El objeto específico que devuelve el Login en "data"
    public class LoginResponseData
    {
        public string UserId { get; set; }
        public string U_NAME { get; set; }
        public string User_CODE { get; set; }
        public string Warehouse { get; set; }
    }
}