using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Pages
{
    public class RegisterApiClient
    {
        private readonly HttpClient _httpClient;
        public RegisterApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", dto);
            if (response.IsSuccessStatusCode)
                return (true, null);
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
    }
}
