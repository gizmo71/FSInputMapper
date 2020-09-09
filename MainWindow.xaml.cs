using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/getting-started/walkthrough-my-first-wpf-desktop-application?view=netframeworkdesktop-4.8

namespace FSInputMapper
{
    public class FSIMModel
    {
        public string text { get; set; }
    }

    public class FSIMViewModel : INotifyPropertyChanged
    {
        private FSIMModel model = new FSIMModel { text = "-----", };

        public string Text
        {
            get { return model.text; }
            set
            {
                if (model.text != value)
                {
                    model.text = value;
                    OnPropertyChange();
                }
            }
        }

        //TODO: is there not a superclass or something to do this stuff for us?
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        const int WM_USER_SIMCONNECT = 0x0402;

        private readonly FSIMViewModel _viewModel;

        private SimConnect simconnect = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new FSIMViewModel();

            try
            {
                //simconnect = new SimConnect("Managed Datazmo", new WindowInteropHelper(this).EnsureHandle(), WM_USER_SIMCONNECT, null, 0);
            }
            catch (COMException ex)
            {
                MessageBox.Show("Boo hiss " + ex);
            }
            //if (simconnect != null) { simconnect.Dispose(); simconnect = null; }
        }

        private void TestTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Push_Clicked(object sender, RoutedEventArgs e)
        {
            _viewModel.Text = "M";
        }

        private void Pull_Clicked(object sender, RoutedEventArgs e)
        {
            _viewModel.Text = "S";
        }
    }
}
