using Microsoft.Maui.Controls;
using DesktopGremlin.Services;

namespace DesktopGremlin
{
    public partial class MainPage : ContentPage
    {
        private ChatViewModel vm;
        private VerticalStackLayout _messagesContainer;
        private ScrollView _messagesPanel;
        private Label _latestMessageLabel;
        private Frame _messagesFrame;
        private Entry _userInput;
        private Image _botImageControl;

        public MainPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            vm = viewModel;
            BindingContext = vm;

            // resolve named controls from XAML
            _messagesContainer = this.FindByName<VerticalStackLayout>("MessagesContainer");
            _messagesPanel = this.FindByName<ScrollView>("MessagesPanel");
            _latestMessageLabel = this.FindByName<Label>("LatestMessageLabel");
            _messagesFrame = this.FindByName<Frame>("MessagesFrame");
            _userInput = this.FindByName<Entry>("UserInput");
            _botImageControl = this.FindByName<Image>("BotImage");

            // subscribe to fact poller event
            try
            {
                var poller = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(FactPoller)) as FactPoller;
                if (poller != null)
                {
                    poller.FactReceived += (s, fact) =>
                    {
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _messagesContainer?.Children.Add(new Label { Text = "BMO: " + fact });
                            if (_latestMessageLabel != null) _latestMessageLabel.Text = fact;
                        });
                    };
                }
            }
            catch { }
        }

        private void OnLatestMessageTapped(object? sender, EventArgs e)
        {
            try
            {
                if (_messagesPanel == null || _messagesFrame == null) return;
                _messagesPanel.IsVisible = !_messagesPanel.IsVisible;
                _messagesFrame.HeightRequest = _messagesPanel.IsVisible ? 200 : 32;
            }
            catch { }
        }

        private async void OnSendClicked(object? sender, EventArgs e)
        {
            if (_userInput == null || string.IsNullOrWhiteSpace(_userInput.Text)) return;

            var parsed = InputParser.Parse(_userInput.Text);
            _userInput.Text = string.Empty;

            switch (parsed.Type)
            {
                case InputType.Empty:
                    return;

                case InputType.Command:
                    HandleCommand(parsed.Command);
                    return;

                case InputType.Message:
                    _messagesContainer?.Children.Add(new Label { Text = "You: " + parsed.Message });
                    if (_latestMessageLabel != null) _latestMessageLabel.Text = "You: " + parsed.Message;
                    var reply = await vm.SendMessage(parsed.Message);
                    _messagesContainer?.Children.Add(new Label { Text = "BMO: " + reply });
                    if (_latestMessageLabel != null) _latestMessageLabel.Text = reply;
                    try { Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => _botImageControl.Source = vm.BotImage); } catch { }
                    break;
            }
        }

        private void HandleCommand(string command)
        {
            switch (command)
            {
                case "help":
                    _messagesContainer?.Children.Add(new Label { Text = "Gremlin: Commands: /help, /clear, /quit" });
                    break;

                case "clear":
                    _messagesContainer?.Children.Clear();
                    break;

                case "quit":
                    Application.Current?.Quit();
                    break;

                default:
                    _messagesContainer?.Children.Add(new Label { Text = "Gremlin: Unknown command. Type /help for a list." });
                    break;
            }
        }
    }
}
