using DataGen_v1.Models;
using System.Text;

namespace DataGen_v1.Services
{
    public class HttpSenderService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpSenderService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // UPDATED: Returns a Tuple (Success, StatusCode, Body)
        public async Task<(bool Success, int StatusCode, string Body)> SendPayloadAsync(ApiConfig api, string jsonPayload)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient(); // Use Factory for performance
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Add headers if exists
                if (!string.IsNullOrEmpty(api.HeaderName) && !string.IsNullOrEmpty(api.HeaderValue))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(api.HeaderName, api.HeaderValue);
                }

                var response = await client.PostAsync(api.TargetUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Return raw data separately
                return (response.IsSuccessStatusCode, (int)response.StatusCode, responseBody);
            }
            catch (Exception ex)
            {
                return (false, 0, $"EXCEPTION: {ex.Message}");
            }
        }
    }
}