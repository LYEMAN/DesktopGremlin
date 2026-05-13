namespace DesktopGremlin
{
    public partial class MainPage : ContentPage
    {
        private ChatViewModel vm;

        public MainPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            vm = viewModel;
            BindingContext = vm;
        }

        private async void OnSendClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInput.Text)) return;

            var parsed = Services.InputParser.Parse(UserInput.Text);
            UserInput.Text = string.Empty;

            switch (parsed.Type)
            {
                case Services.InputType.Empty:
                    return;

                case Services.InputType.Command:
                    HandleCommand(parsed.Command);
                    return;

                case Services.InputType.Message:
                    MessagesContainer.Children.Add(new Label { Text = "You: " + parsed.Message });
                    var reply = await vm.SendMessage(parsed.Message);
                    MessagesContainer.Children.Add(new Label { Text = "Gremlin: " + reply });
                    break;
            }
        }

        private void HandleCommand(string command)
        {
            switch (command)
            {
                case "help":
                    MessagesContainer.Children.Add(new Label
                    {
                        Text = "Gremlin: Commands: /help, /clear, /quit"
                    });
                    break;

                case "clear":
                    MessagesContainer.Children.Clear();
                    break;

                case "quit":
                    Application.Current?.Quit();
                    break;

                default:
                    MessagesContainer.Children.Add(new Label
                    {
                        Text = "Gremlin: Unknown command. Type /help for a list."
                    });
                    break;
            }
        }
    }
}
