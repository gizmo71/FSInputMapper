using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace FSInputMapper
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SingletonAttribute : Attribute { }

    public static class AttributeExtensions
    {

        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name!)!.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

    }

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
            services.AddSingleton(typeof(MainWindow), typeof(MainWindow));
            foreach (var candidate in Assembly.GetEntryAssembly()!.DefinedTypes)
            {
                var singleton = candidate.GetCustomAttribute<SingletonAttribute>();
                if (singleton != null)
                {
                    services.AddSingleton(candidate, candidate);
// https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/
                }
            }
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
