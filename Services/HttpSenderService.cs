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

        public async Task<string> SendPayloadAsync(ApiConfig api, string jsonPayload)
        {
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // Add headers if you have them
                // client.DefaultRequestHeaders.Add("Authorization", api.AuthKey);

                var response = await client.PostAsync(api.TargetUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return $"SUCCESS {(int)response.StatusCode}: {responseBody}";
                }
                else
                {
                    return $"FAILED {(int)response.StatusCode}: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}