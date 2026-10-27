using System;
using System.Threading.Tasks;
using System.Windows;
using Checkers.Tools.Logging;
using Checkers.Tools.Engine;
using Checkers.WPF.Online;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows.Threading;

namespace Checkers.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ServiceProvider? _services;

        protected override void OnStartup(StartupEventArgs e)
        {
            LoggingSetup.Initialize("Checkers.WPF", c => c.WriteTo.Debug());
            Log.Information("WPF application started");
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                Log.Fatal(args.ExceptionObject as Exception, "Unhandled domain exception (terminating: {IsTerminating})", args.IsTerminating);
            TaskScheduler.UnobservedTaskException += (_, args) =>
                Log.Error(args.Exception, "Unobserved task exception");
            base.OnStartup(e);

            _services = new ServiceCollection()
                .AddSingleton<IGameSaveService>(_ => new GameSaveService())
                .AddSingleton<ISettingsService>(_ => new SettingsService())
                .AddSingleton<IOnlineGameClient>(_ => new SignalRGameClient())
                .AddSingleton<MainViewModel>()
                .AddSingleton<MainWindow>()
                .BuildServiceProvider();
            _services.GetRequiredService<MainWindow>().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("WPF application exiting with code {ExitCode}", e.ApplicationExitCode);
            _services?.Dispose();
            LoggingSetup.Shutdown();
            base.OnExit(e);
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled dispatcher exception");
            var exceptionMessage = "An unhandled exception just occurred: " + e.Exception.Message;
            MessageBox.Show(exceptionMessage, "Unhandled Application Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}