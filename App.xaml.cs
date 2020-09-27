using System.Collections.Generic;
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
            foreach (var serviceType in new [] {
                typeof(FSIMViewModel),
                typeof(MainWindow),
                typeof(SimConnectAdapter),
                typeof(FSIMTriggerBus),
            })
            {
                IEnumerable<TypeInfo> types = Assembly.GetEntryAssembly()!.DefinedTypes;
                types = types.Where(CanBeInstatiated);
                foreach (var t in types.Where(t => serviceType.IsAssignableFrom(t)))
                {
                    services.AddSingleton(serviceType, t);
                }
            }
        }

        private bool CanBeInstatiated(TypeInfo t) { return !t.IsInterface && !t.IsAbstract; }

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
