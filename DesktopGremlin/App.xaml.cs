using Microsoft.Extensions.DependencyInjection;

namespace DesktopGremlin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // try resolve ChatViewModel from DI and set MainPage directly to ensure content is visible
            try
            {
                var vm = this.Handler?.MauiContext?.Services.GetService<ChatViewModel>();
                if (vm != null)
                    return new Window(new MainPage(vm));
            }
            catch { }

            return new Window(new AppShell());
        }
    }
}