using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DesktopGremlin.Services
{
    public class AiService
    {
        private static readonly HttpClient _http = new HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };

        public AiService() { }

        public async Task<string?> GenerateAsync(string message)
        {
            var resp = await _http.PostAsJsonAsync("/ai/chat", new { message });
            resp.EnsureSuccessStatusCode();
            var result = await resp.Content.ReadFromJsonAsync<ChatApiResponse>();
            return result?.Reply;
        }

        private class ChatApiResponse
        {
            public string? Reply { get; set; }
        }
    }
}
