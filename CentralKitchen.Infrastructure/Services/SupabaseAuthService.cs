using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CentralKitchen.Application.Interfaces;

namespace CentralKitchen.Infrastructure.Services;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _anonKey;

    public SupabaseAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new ArgumentNullException("Supabase:Url configuration is missing");
        _anonKey = configuration["Supabase:AnonKey"] ?? throw new ArgumentNullException("Supabase:AnonKey configuration is missing");
    }

    public async Task<(string? AccessToken, Guid? UserId, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var requestUrl = $"{_supabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=password";
            
            var requestBody = new
            {
                email = email,
                password = password
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = content
            };

            request.Headers.Add("apikey", _anonKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _anonKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                var errorDesc = root.TryGetProperty("error_description", out var descProp) ? descProp.GetString() : null;
                var errorMsg = root.TryGetProperty("msg", out var msgProp) ? msgProp.GetString() : null;
                var errorType = root.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null;

                var displayError = errorDesc ?? errorMsg ?? errorType ?? "Invalid login attempt.";
                return (null, null, displayError);
            }

            var authResponse = JsonSerializer.Deserialize<SupabaseTokenResponse>(responseContent);
            if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                return (null, null, "Authentication response was empty.");
            }

            if (Guid.TryParse(authResponse.User?.Id, out var parsedGuid))
            {
                return (authResponse.AccessToken, parsedGuid, null);
            }

            return (authResponse.AccessToken, null, null);
        }
        catch (Exception ex)
        {
            return (null, null, $"Internal Auth Error: {ex.Message}");
        }
    }

    private class SupabaseTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("user")]
        public SupabaseUser? User { get; set; }
    }

    private class SupabaseUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }
}
