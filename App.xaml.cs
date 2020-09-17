using System.Windows;
using System.Windows.Threading;

//TODO: does https://stackoverflow.com/questions/54877352/dependency-injection-in-net-core-3-0-for-wpf make sense for us?

namespace FSInputMapper
{
    public partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += (sender, args) => OnUnhandledException(args);
        }

        private void OnUnhandledException(DispatcherUnhandledExceptionEventArgs details)
        {
            System.Windows.MessageBox.Show(details.Exception.ToString(),
                                  "Unhandled Exception",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
            details.Handled = true;
        }
    }
}
