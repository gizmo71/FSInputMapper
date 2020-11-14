using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSInputMapper
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class SingletonAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ProvideDerivedAttribute : Attribute { }

    public static class AttributeExtensions
    {

        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name!)!.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

    }

    [Singleton]
    public class DebugConsole : INotifyPropertyChanged
    {

        private string text = "";
        public string Text
        {
            get { return text; }
            set { if (text != value) { text = value; OnPropertyChange(); } }
        }

        private string? connectionError = "Connection not yet attempted";
        public string? ConnectionError
        {
            get { return connectionError; }
            set { if (connectionError != value) { connectionError = value; OnPropertyChange(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection?view=dotnet-plat-ext-3.1
            services.AddSingleton<MainWindow>();
            var serviceInterfacesAndBaseClasses = Assembly.GetEntryAssembly()!.DefinedTypes
                .Where(candidate => candidate.GetCustomAttribute<ProvideDerivedAttribute>() != null);
            foreach (var candidate in Assembly.GetEntryAssembly()!.DefinedTypes)
            {
                var required = candidate.GetCustomAttribute<SingletonAttribute>() != null;
                foreach (var derviedFrom in serviceInterfacesAndBaseClasses
                    .Where(fromCandidate => fromCandidate.IsAssignableFrom(candidate) && !candidate.IsAbstract))
                {
                    services.AddSingleton(derviedFrom, x => x.GetRequiredService(candidate));
                    required = true;
                }
                if (required)
                {
                    if (candidate.IsValueType) // For structs.
                        services.AddSingleton(candidate, _ => Activator.CreateInstance(candidate));
                    else
                        services.AddSingleton(candidate, candidate);
                }
            }
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _serviceProvider.GetService<MainWindow>().Show();
        }

        private void OnUnhandledException(DispatcherUnhandledExceptionEventArgs details)
        {
            MessageBox.Show(details.Exception.ToString(), "Unhandled Exception",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            // Don't die: details.Handled = true;
        }
    }

}
