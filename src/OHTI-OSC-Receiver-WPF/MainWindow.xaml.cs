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

        public MainWindow(ILogger<MainWindow> logger, IServiceProvider serviceProvider, IOptions<ApplicationSettings> options)
        {
            InitializeComponent();

            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = options.Value;

            OpenSoundControlListener openSoundControlListener = _serviceProvider.GetService<OpenSoundControlListener>();

            openSoundControlListener.HeadtrackingDeviceEvent += (OpenSoundControlState state) =>
            {
                AddConsoleRow(state.ToString());
            };

            openSoundControlListener.HeadtrackingDataEvent += (string point) =>
            {
                AddConsoleRow("HT:" + point);
            };

            openSoundControlListener.Connect(_configuration.Hostname, _configuration.Port);
        }

        private void AddConsoleRow(string text)
        {
            TextConsole.Text = text + Environment.NewLine + TextConsole.Text;
        }
    }
}
