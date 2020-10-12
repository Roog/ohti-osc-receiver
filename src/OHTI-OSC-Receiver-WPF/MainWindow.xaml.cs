using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OHTI_OSC_Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using OpenSoundControlClient;

namespace OHTI_OSC_Receiver_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private IServiceProvider _serviceProvider;
        private readonly ApplicationSettings _configuration;
        private readonly UDPBroadcastReceiver _oscListener;

        public MainWindow(
            ILogger<MainWindow> logger,
            IServiceProvider serviceProvider,
            IOptions<ApplicationSettings> options)
        {
            InitializeComponent();

            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = options.Value;

            _oscListener = _serviceProvider.GetService<UDPBroadcastReceiver>();

            //_oscListener.HeadtrackingDeviceEvent += (OpenSoundControlState state, string error) =>
            //{
            //    AddConsoleRow(state.ToString());

            //    if (state.Connected)
            //    {
            //        ButtonConnectOsc.Background = Brushes.Green;
            //        ButtonConnectOsc.Content = "Disconnect";
            //        ButtonSaveOscSettings.IsEnabled = false;
            //    }
            //    else
            //    {
            //        ButtonConnectOsc.Background = Brushes.Red;
            //        ButtonConnectOsc.Content = "Connect";
            //        ButtonSaveOscSettings.IsEnabled = true;
            //    }

            //    if (!String.IsNullOrEmpty(error))
            //    {
            //        AddConsoleRow(error);
            //    }
            //};

            _oscListener.HeadtrackingDataEvent += (string address, float x, float y, float z, float theta) =>
            {
                try {
                    string mess = $"{address} {x}, {y}, {z}, {theta}".ToString();
                    AddConsoleRow(mess);
                } catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };

            // Set up GUI listeners
            //ButtonSaveOscSettings.Click += SaveOscSettings_Click;
            ButtonConnectOsc.IsEnabled = true;
            ButtonConnectOsc.Click += ConnectOsc_Click;

            CheckBoxUseUnicast.IsChecked = false;
            CheckBoxUseUnicast.Click += CheckBoxUseUnicast_Click;
        }

        private void CheckBoxUseUnicast_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxUseUnicast.IsChecked == true)
            {
                InputOscIpAddress.IsEnabled = false;
            }
            else
            {
                InputOscIpAddress.IsEnabled = true;
            }
        }

        private void ConnectOsc_Click(object sender, RoutedEventArgs e)
        {
            _oscListener.StartService(new UdpClientOptions());
            //if (_oscListener.State.Connected)
            //{
            //    _oscListener.Disconnect();
            //}
            //else
            //{
            //    _oscListener.Connect();
            //}
        }

        //private void SaveOscSettings_Click(object sender, RoutedEventArgs e)
        //{
        //    bool success = _oscListener.SaveConfiguration(InputOscIpAddress.Text, int.Parse(InputOscPort.Text), CheckBoxUseUnicast.IsEnabled);
        //    if (success)
        //    {
        //        ButtonConnectOsc.IsEnabled = true;
        //        AddConsoleRow($"Saved configuration successfully {_oscListener.State}");
        //    }
        //    else
        //    {
        //        AddConsoleRow($"Could not save configuration {_oscListener.State}");
        //    }
        //}

        private void AddConsoleRow(string text)
        {
            try {
                TextConsole.Text = "> " + text; // + Environment.NewLine + TextConsole.Text;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                TextConsole.Text = "...";
            }
        }

        #region WPF GUI Helpers
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //private void IPAddressValidationTextBox(object sender, TextCompositionEventArgs e)
        //{
        //    Regex regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        //    e.Handled = regex.IsMatch(e.Text);
        //}
        #endregion
    }
}
