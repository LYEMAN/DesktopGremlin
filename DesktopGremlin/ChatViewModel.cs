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
            var result = await _ai.GenerateAsync(userMessage);
            if (string.IsNullOrEmpty(result)) return string.Empty;

            var lower = result.ToLower();
            if (lower.Contains("happy")) BotImage = "happy.png";
            else if (lower.Contains("sad")) BotImage = "sad.png";
            else if (lower.Contains("angry")) BotImage = "angry.png";
            else BotImage = "neutral.png";

            return result;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
