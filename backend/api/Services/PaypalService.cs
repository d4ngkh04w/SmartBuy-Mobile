using System.Net.Http.Headers;
using System.Text;
using api.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace api.Services
{
    public class PaypalService : IPaypalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaypalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["Paypal:ClientId"];
            var secret = _configuration["Paypal:Secret"];
            var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.PostAsync("https://api-m.sandbox.paypal.com/v1/oauth2/token", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get PayPal access token");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody)!;

            return result.access_token;
        }
    }
}
