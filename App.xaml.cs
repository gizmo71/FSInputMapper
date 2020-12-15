using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using HidSharp;
using System.Threading;

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
            return enumType.GetField(name!)!.GetCustomAttributes(false).OfType<TAttribute>().Single();
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
            _serviceProvider.GetService<HidSharpCoreTest>().Test();
        }

        private void OnUnhandledException(DispatcherUnhandledExceptionEventArgs details)
        {
            MessageBox.Show(details.Exception.ToString(), "Unhandled Exception",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            // Don't die: details.Handled = true;
        }
    }

    //https://docs.zer7.com/hidsharp/?topic=html/2794bd11-156f-04dd-db45-7978aaa249ca.htm
    [Singleton]
    public class HidSharpCoreTest
    {
        private readonly DebugConsole dc;

        public HidSharpCoreTest(DebugConsole dc)
        {
            this.dc = dc;
        }

        internal void Test()
        {
            var device = DeviceList.Local.GetHidDeviceOrNull(0x44f);
            if (device == null)
            {
                MessageBox.Show("Restart me after plugging it in", "GamePad not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            dc.Text = $"Found {device}";
            var rd = device.GetReportDescriptor();
            foreach (var r in rd.InputReports.SelectMany(ir => ir.DataItems).Select(di => di.Report))
            {
                dc.Text += $"\n{r} with";
                foreach (var di in r.DataItems)
                    dc.Text += $" {di.ExpectedUsageType}";
            }
return;
            DeviceList.Local.Changed += DevicesChanged;
            var stream = device.Open();
            var hidUpdateThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                using (stream)
                {
                    stream.ReadTimeout = 1000;
                    for (;;)
                    {
                        if (!Thread.CurrentThread.IsAlive)
                        {
                            dc.Text = $"Dead {DateTime.UtcNow}";
                            break;
                        }
                        try
                        {
                            var bytes = stream.Read();
                            dc.Text = $"Rx {string.Join(" ", bytes)} {DateTime.UtcNow.Millisecond}";
                        }
                        catch (TimeoutException)
                        {
//dc.Text = $"Alive {DateTime.Now} but timed out";
                            continue;
                        }
                        catch (Exception e)
                        {
                            dc.Text = $"Failed to read {e}";
                        }
                    }
                }
            });
            hidUpdateThread.Start();
        }

        private void DevicesChanged(object? sender, DeviceListChangedEventArgs e)
        {
            dc.Text = $"Detected change {e}";
        }
    }

}
