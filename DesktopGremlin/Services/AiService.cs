using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DesktopGremlin.Services
{
    public class AiService
    {
        private static readonly HttpClient _http = new HttpClient { BaseAddress = new System.Uri("http://localhost:8080") };

        public AiService() { }

        public async Task<AiResponse?> GenerateAsync(string message)
        {
            var resp = await _http.PostAsJsonAsync("/ai/chat", new { message });
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return null;

            try
            {
                var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var outp = System.Text.Json.JsonSerializer.Deserialize<AiResponse>(content, opts);
                if (outp == null) return null;

                // unwrap quoted JSON strings repeatedly
                var msg = outp.Message ?? string.Empty;
                try
                {
                    var trimmed = msg.Trim();
                    bool unwrapped = true;
                    while (unwrapped)
                    {
                        unwrapped = false;
                        if (trimmed.Length >= 2 && (trimmed[0] == '"' || trimmed[0] == '\''))
                        {
                            try
                            {
                                using var d = System.Text.Json.JsonDocument.Parse(trimmed);
                                if (d.RootElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    trimmed = d.RootElement.GetString() ?? trimmed;
                                    unwrapped = true;
                                }
                            }
                            catch { }
                        }
                    }

                    // if trimmed is an object, parse and extract inner fields
                    if (trimmed.TrimStart().StartsWith("{"))
                    {
                        try
                        {
                            using var inner = System.Text.Json.JsonDocument.Parse(trimmed);
                            var root = inner.RootElement;
                            if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                if (string.IsNullOrWhiteSpace(outp.Emotion) && root.TryGetProperty("emotion", out var ie) && ie.ValueKind == System.Text.Json.JsonValueKind.String)
                                    outp.Emotion = ie.GetString();
                                if (root.TryGetProperty("message", out var im))
                                {
                                    if (im.ValueKind == System.Text.Json.JsonValueKind.String)
                                        outp.Message = im.GetString();
                                    else
                                        outp.Message = im.GetRawText();
                                }
                            }
                        }
                        catch { outp.Message = msg; }
                    }
                    else
                    {
                        outp.Message = trimmed;
                    }
                }
                catch { outp.Message = msg; }

                return outp;
            }
            catch { return null; }
        } 

       


        public class AiResponse
        {
            public string? Emotion { get; set; }
            public string? Message { get; set; }
        }




      
    }
}
