using System.Net.Http;
using System.Diagnostics;
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
            Debug.WriteLine("AI content: " + content);
            if (string.IsNullOrWhiteSpace(content)) return null;

            try
            {
                var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // try direct deserialization first
                try
                {
                    var direct = System.Text.Json.JsonSerializer.Deserialize<AiResponse>(content, opts);
                    if (direct != null)
                    {
                        Debug.WriteLine($"Direct parsed: emotion={direct.Emotion} message={direct.Message}");
                        if (!string.IsNullOrWhiteSpace(direct.Emotion) || !string.IsNullOrWhiteSpace(direct.Message))
                            return direct;
                    }
                }
                catch { }

                // Some backends wrap the payload inside choices[0].message.content - try to extract and parse.
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    var root = doc.RootElement;
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object && root.TryGetProperty("choices", out var choices) && choices.ValueKind == System.Text.Json.JsonValueKind.Array && choices.GetArrayLength() > 0)
                    {
                        var first = choices[0];
                        if (first.ValueKind == System.Text.Json.JsonValueKind.Object && first.TryGetProperty("message", out var messageEl) && messageEl.ValueKind == System.Text.Json.JsonValueKind.Object && messageEl.TryGetProperty("content", out var contentEl))
                        {
                            string? inner = null;
                            if (contentEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                inner = contentEl.GetString();
                            else
                                inner = contentEl.GetRawText();

                            if (!string.IsNullOrWhiteSpace(inner))
                            {
                                // unwrap quoted JSON strings repeatedly
                                var trimmed = inner.Trim();
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

                                // try deserialize trimmed into AiResponse
                                try
                                {
                                    var parsed = System.Text.Json.JsonSerializer.Deserialize<AiResponse>(trimmed, opts);
                                    if (parsed != null)
                                    {
                                        Debug.WriteLine($"Inner parsed: emotion={parsed.Emotion} message={parsed.Message}");
                                        if (!string.IsNullOrWhiteSpace(parsed.Emotion) || !string.IsNullOrWhiteSpace(parsed.Message))
                                            return parsed;
                                    }
                                }
                                catch { }

                                // if trimmed is an object, extract fields manually
                                if (trimmed.TrimStart().StartsWith("{"))
                                {
                                    try
                                    {
                                        using var innerDoc = System.Text.Json.JsonDocument.Parse(trimmed);
                                        var r = innerDoc.RootElement;
                                        if (r.ValueKind == System.Text.Json.JsonValueKind.Object)
                                        {
                                            var outp = new AiResponse();
                                            if (r.TryGetProperty("emotion", out var ie) && ie.ValueKind == System.Text.Json.JsonValueKind.String)
                                                outp.Emotion = ie.GetString();
                                            if (r.TryGetProperty("message", out var im))
                                            {
                                                if (im.ValueKind == System.Text.Json.JsonValueKind.String)
                                                    outp.Message = im.GetString();
                                                else
                                                    outp.Message = im.GetRawText();
                                            }
                                            Debug.WriteLine($"Manual inner parsed: emotion={outp.Emotion} message={outp.Message}");
                                            if (!string.IsNullOrWhiteSpace(outp.Emotion) || !string.IsNullOrWhiteSpace(outp.Message))
                                                return outp;
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    // plain string content -> return as message
                                    return new AiResponse { Message = trimmed };
                                }
                            }
                        }
                    }
                }
                catch { }

                // fallback: recursive search for emotion/message anywhere in the JSON
                try
                {
                    using var docAll = System.Text.Json.JsonDocument.Parse(content);
                    System.Func<System.Text.Json.JsonElement, (string? emotion, string? message)> find = null!;
                    find = (el) =>
                    {
                        try
                        {
                            if (el.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                string? e = null; string? m = null;
                                foreach (var p in el.EnumerateObject())
                                {
                                    var name = p.Name?.ToLowerInvariant() ?? string.Empty;
                                    if (name == "emotion" && p.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        e = p.Value.GetString();
                                    }
                                    else if (name == "message" && p.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        m = p.Value.GetString();
                                    }

                                    if (e != null && m != null) break;

                                    var child = find(p.Value);
                                    if (e == null) e = child.emotion;
                                    if (m == null) m = child.message;
                                    if (e != null && m != null) break;
                                }
                                return (e, m);
                            }
                            else if (el.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                string? e = null; string? m = null;
                                foreach (var item in el.EnumerateArray())
                                {
                                    var child = find(item);
                                    if (e == null) e = child.emotion;
                                    if (m == null) m = child.message;
                                    if (e != null && m != null) break;
                                }
                                return (e, m);
                            }
                            else if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var s = el.GetString() ?? string.Empty;
                                var t = s.Trim();
                                if (t.StartsWith("{"))
                                {
                                    try
                                    {
                                        using var innerDoc = System.Text.Json.JsonDocument.Parse(t);
                                        return find(innerDoc.RootElement);
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                        return (null, null);
                    };

                    var res = find(docAll.RootElement);
                    if (!string.IsNullOrWhiteSpace(res.emotion) || !string.IsNullOrWhiteSpace(res.message))
                    {
                        var ai = new AiResponse { Emotion = res.emotion, Message = res.message };
                        Debug.WriteLine($"Recursive found: emotion={ai.Emotion} message={ai.Message}");
                        return ai;
                    }
                }
                catch { }

                return null;
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
