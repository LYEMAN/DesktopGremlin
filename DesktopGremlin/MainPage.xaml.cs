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

        private async void OnSendClicked(object sender, EventArgs e)
        {
            await vm.SendMessage(UserInput.Text);
        }




    }
}
