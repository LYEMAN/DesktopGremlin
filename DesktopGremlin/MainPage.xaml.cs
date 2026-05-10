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

            var userText = UserInput.Text;
            UserInput.Text = string.Empty;

            MessagesContainer.Children.Add(new Label { Text = "You: " + userText });

            var reply = await vm.SendMessage(userText);
            MessagesContainer.Children.Add(new Label { Text = "Gremlin: " + reply });
        }
    }
}
