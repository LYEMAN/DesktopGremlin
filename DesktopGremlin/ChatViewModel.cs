using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DesktopGremlin.Services;

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
                OnPropertyChanged();
            }
        }

        public async Task<string> SendMessage(string userMessage)
        {
            var resp = await _ai.GenerateAsync(userMessage);
            if (resp == null) return string.Empty;

            var emotion = resp.Emotion?.ToLower() ?? string.Empty;
            if (emotion == "happy") BotImage = "happy.png";
            else if (emotion == "sad") BotImage = "sad.png";
            else if (emotion == "angry") BotImage = "angry.png";
            else BotImage = "neutral.png";

            var raw = resp.Message ?? string.Empty;
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

            return raw;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
