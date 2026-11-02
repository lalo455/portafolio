using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WmsMovil.Models;

namespace WmsMovil.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // URL Base de tu documentación
        private const string BASE_URL = "https://api.example.com/wms/";

        // Tenant fijo por ahora (después podríamos ponerlo en una pantalla de configuración)
        private const string TENANT_ID = "demo";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromSeconds(15) // Regla de la API: 15 segundos máximo
            };

            // Regla de la API: Header Obligatorio (Multi-Tenancy)
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", TENANT_ID);
        }

        public async Task<WmsResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
        {
            try
            {
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // PropertyNameCaseInsensitive = true hace que "codigo" en JSON haga match con "Codigo" en C#
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<WmsResponse<TResponse>>(responseString, options);
                }

                // Manejo de Errores HTTP según la Documentación
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) // 400
                    return new WmsResponse<TResponse> { Codigo = -1, Mensaje = "JSON mal formado (HTTP 400)" };

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) // 403
                    return new WmsResponse<TResponse> { Codigo = -1, Mensaje = "Tenant suspendido por adeudo (HTTP 403)" };

                if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented) // 501
                    return new WmsResponse<TResponse> { Codigo = -1, Mensaje = "Acción no existe en los plugins de la API (HTTP 501)" };

                return new WmsResponse<TResponse> { Codigo = -1, Mensaje = $"Error del servidor: {response.StatusCode}" };
            }
            catch (TaskCanceledException)
            {
                // Si pasan los 15 segundos
                return new WmsResponse<TResponse> { Codigo = -1, Mensaje = "Tiempo de espera agotado (Timeout)." };
            }
            catch (Exception ex)
            {
                return new WmsResponse<TResponse> { Codigo = -1, Mensaje = $"Sin conexión: {ex.Message}" };
            }
        }
        // Método para hacer consultas GET (Catálogos, Tickets, etc.)
        public async Task<WmsResponse<TResponse>> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<WmsResponse<TResponse>>(responseString, options);
                }

                return new WmsResponse<TResponse> { Codigo = -1, Mensaje = $"Error HTTP: {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new WmsResponse<TResponse> { Codigo = -1, Mensaje = $"Sin conexión: {ex.Message}" };
            }
        }
    }
}