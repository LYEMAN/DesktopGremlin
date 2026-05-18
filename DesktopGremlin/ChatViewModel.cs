using DesktopGremlin.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DesktopGremlin
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly AiService _ai;
        private string _botImage = "neutral.png";

        public ChatViewModel(AiService ai)
        {
            _ai = ai;
        }

        public string BotImage
        {
            get => _botImage;
            set
            {
                _botImage = value;
                Debug.WriteLine("BotImage set to: " + _botImage);
                OnPropertyChanged();
            }
        }

        public async Task<string> SendMessage(string userMessage)
        {
            var resp = await _ai.GenerateAsync(userMessage);
            if (resp == null) return string.Empty;
            var raw = resp.Message ?? string.Empty;
            string? innerEmotion = null;
            // try to extract nested message if raw is JSON or quoted JSON
            try
            {
                var s = raw.Trim();
                if (s.StartsWith("\"") || s.StartsWith("'"))
                {
                    // attempt to parse as JSON string
                    try
                    {
                        using var d = System.Text.Json.JsonDocument.Parse(s);
                        if (d.RootElement.ValueKind == System.Text.Json.JsonValueKind.String)
                            s = d.RootElement.GetString() ?? s;
                    }
                    catch { }
                }

                if (s.TrimStart().StartsWith("{"))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(s);
                    var root = doc.RootElement;
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        // capture any embedded emotion field
                        if (root.TryGetProperty("emotion", out var em) && em.ValueKind == System.Text.Json.JsonValueKind.String)
                            innerEmotion = em.GetString();
                        if (root.TryGetProperty("message", out var m) && m.ValueKind == System.Text.Json.JsonValueKind.String)
                            raw = m.GetString() ?? raw;
                        else if (root.TryGetProperty("reply", out var r) && r.ValueKind == System.Text.Json.JsonValueKind.String)
                            raw = r.GetString() ?? raw;
                        else if (root.TryGetProperty("fact", out var f) && f.ValueKind == System.Text.Json.JsonValueKind.String)
                            raw = f.GetString() ?? raw;
                        else
                            raw = root.GetRawText();
                    }
                }
            }
            catch { }

            var emotion = resp.Emotion ?? innerEmotion ?? string.Empty;
            emotion = emotion.ToLowerInvariant();
            Debug.WriteLine("Emotion: " + emotion);
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (emotion)
                {
                    case "happy": BotImage = "happy.png"; break;
                    case "sad": BotImage = "sad.png"; break;
                    case "neutral": BotImage = "neutral.png"; break;
                    case "angry": BotImage = "angry.png"; break;
                    case "annoyed": BotImage = "annoyed.png"; break;
                    case "embarrased": BotImage = "embarrased.png"; break;
                    case "frustrated": BotImage = "frustrated.png"; break;
                    case "hurt": BotImage = "hurt.png"; break;
                    case "indignant": BotImage = "indignant.png"; break;
                    case "glad": BotImage = "glad.png"; break;
                    case "sadder": BotImage = "sadder.png"; break;
                    case "elated": BotImage = "elated.png"; break;
                    case "disconcerted": BotImage = "disconcerted.png"; break;
                    default: BotImage = "neutral.png"; break;
                }
            });

            return raw;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
