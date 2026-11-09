
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services
{
    public class RaceDayApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _accessor;

        private const string CookieKey = "RaceDayApiCookie";

        public RaceDayApiClient(
            IHttpClientFactory factory,
            IHttpContextAccessor accessor)
        {
            _factory = factory;
            _accessor = accessor;
        }

        private HttpRequestMessage CreateRequest(
            HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            string? cookie = _accessor.HttpContext?
                .Session.GetString(CookieKey);

            if (!string.IsNullOrWhiteSpace(cookie))
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie);

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request)
        {
            var client = _factory.CreateClient("RaceDayApi");
            return await client.SendAsync(request);
        }

        public async Task<JsonDocument?> LoginAsync(
            string email, string password)
        {
            using var request = CreateRequest(
                HttpMethod.Post, "api/auth/login");

            request.Content = JsonContent.Create(new
            {
                Email = email,
                Password = password
            });

            using var response = await SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            if (response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
            {
                var sessionCookie = cookies
                    .Select(c => c.Split(';')[0])
                    .FirstOrDefault(c =>
                        c.StartsWith(".AspNetCore.Session="));

                if (sessionCookie != null)
                {
                    _accessor.HttpContext?.Session.SetString(
                        CookieKey, sessionCookie);
                }
            }

            string json = await response.Content
                .ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        public async Task<bool> RegisterAsync(object model)
        {
            return await PostJsonAsync("api/auth/register", model);
        }

        public async Task<JsonDocument?> GetJsonAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Get, url);
            using var response = await SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(json);
        }

        public async Task<bool> PostJsonAsync(
            string url, object data)
        {
            using var request = CreateRequest(HttpMethod.Post, url);
            request.Content = JsonContent.Create(data);

            using var response = await SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutJsonAsync(
            string url, object data)
        {
            using var request = CreateRequest(HttpMethod.Put, url);
            request.Content = JsonContent.Create(data);

            using var response = await SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Delete, url);
            using var response = await SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            using var request = CreateRequest(
                HttpMethod.Post, "api/auth/logout");

            using var response = await SendAsync(request);

            _accessor.HttpContext?.Session.Remove(CookieKey);
        }

        // This endpoint is added during Azure integration.
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            using var request = CreateRequest(
                HttpMethod.Post, "api/images/upload");

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType);

            content.Add(fileContent, "file", file.FileName);
            request.Content = content;

            using var response = await SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            return json.RootElement
                .GetProperty("imageUrl").GetString();
        }
    }
}