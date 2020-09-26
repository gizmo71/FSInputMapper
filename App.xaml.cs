using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace FSInputMapper
{

    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            this.Dispatcher.UnhandledException += (sender, args) => OnUnhandledException(args);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // https://stackoverflow.com/a/58476347/1892057
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection?view=dotnet-plat-ext-3.1
            services.AddSingleton<MainWindow>();
            foreach (var t in Assembly.GetEntryAssembly()!.DefinedTypes.Where(t => typeof(FSIMViewModel).IsAssignableFrom(t)))
            {
                services.AddScoped(/*TODO: more general*/t, t);
            }
            //TODO: above is long winded version of services.AddSingleton<FSIMViewModel>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _serviceProvider.GetService<MainWindow>().Show();
        }

        private void OnUnhandledException(DispatcherUnhandledExceptionEventArgs details)
        {
            _ = MessageBox.Show(details.Exception.ToString(), "Unhandled Exception",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            details.Handled = true;
        }
    }
}
