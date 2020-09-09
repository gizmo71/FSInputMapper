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
    public partial class MainWindow : Window
    {
        const int WM_USER_SIMCONNECT = 0x0402;

        private readonly FSIMViewModel _viewModel;

        private SimConnect simconnect = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new FSIMViewModel();
        }

        private void Push_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude = 10000;
        }

        private void Pull_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude = 66600;
        }

        private void Up1000_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude += 1000;
        }

        private void Up100_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude += 100;
        }

        private void Down1000_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude -= 1000;
        }

        private void Down100_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Altitude -= 100;
        }
    }
}
