using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopGremlin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // register AI service and ChatViewModel for DI
            builder.Services.AddSingleton<DesktopGremlin.Services.AiService>();
            builder.Services.AddSingleton<DesktopGremlin.ChatViewModel>();

#if WINDOWS
            // Windows-specific window size / resize behavior
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows =>
                {
                    windows.OnWindowCreated(winObj =>
                    {
                        // winObj is the native WinUI window
                        var nativeWindow = winObj as Microsoft.UI.Xaml.Window;
                        if (nativeWindow == null)
                            return;
                        if (nativeWindow == null)
                            return;

                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                        // Set initial size
                        appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 300, Height = 300 });

                        // Make title bar and window background transparent
                        try
                        {
                            // Extend content into title bar so the app content fills the title area
                            nativeWindow.ExtendsContentIntoTitleBar = true;

                            var titleBar = appWindow.TitleBar;
                            titleBar.ExtendsContentIntoTitleBar = true;

                            // Set title bar button and background colors to transparent
                            var transparent = Microsoft.UI.Colors.Transparent;
                            titleBar.BackgroundColor = transparent;
                            titleBar.ButtonBackgroundColor = transparent;
                            titleBar.ButtonInactiveBackgroundColor = transparent;
                            titleBar.ButtonHoverBackgroundColor = transparent;
                            titleBar.ButtonPressedBackgroundColor = transparent;
                            titleBar.ButtonForegroundColor = transparent;
                            titleBar.ButtonInactiveForegroundColor = transparent;

                            // Set the app window's presenter to allow transparent background if supported
                            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p)
                            {
                                // No direct API to make whole window transparent here; we at least clear title visuals
                                p.IsResizable = p.IsResizable; // no-op to avoid analyzer
                            }
                        }
                        catch { }

                        // Disable resizing and keep window always on top
                        if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                        {
                            presenter.IsResizable = false;
                            try { presenter.IsAlwaysOnTop = true; } catch { }
                        }
                    });
                });
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
