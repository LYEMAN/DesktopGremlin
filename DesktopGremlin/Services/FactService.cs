using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DesktopGremlin.Services
{
    public class FactService
    {
        private static readonly HttpClient _http = new HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };

        public FactService() { }

        public async Task<FactResponse?> GetFact()
        {
            var resp = await _http.GetAsync("/facts/random");
            resp.EnsureSuccessStatusCode();
            var result = await resp.Content.ReadFromJsonAsync<FactResponse>();
            return result;
        }

        public class FactResponse
        {
            public string? Fact { get; set; }
            public string? Source { get; set; }
        }
    }
}
