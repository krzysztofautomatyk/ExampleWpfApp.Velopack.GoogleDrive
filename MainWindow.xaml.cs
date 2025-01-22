using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Velopack;
using VelopackExtension.GoogleDrive;

namespace ExampleWpfApp.Velopack.GoogleDrive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string CurrentVersion { get; } = GetAppVersion();

        private UpdateManager _um;
        private UpdateInfo _update;

        public MainWindow()
        {
            InitializeComponent();


            //TODO: Replace these values with your own
            string folderId = "11111111111111122222222222222222233333333333333333333"; // Your folder ID
            string apiKey = "4444444444444555555555555555666666666666666666"; // Your API key

            var updateSource = new GoogleDriveUpdateSource(folderId, apiKey, "WpfUpdateExample");

            _um = new UpdateManager(updateSource, logger: Program.Log);

            this.Title = $"Moja Aplikacja ver. {Program.AppVersion}";

            TextLog.Text = Program.Log.ToString();
            Program.Log.LogUpdated += LogUpdated;
            UpdateStatus();
        }


        private async void BtnCheckUpdateClick(object sender, RoutedEventArgs e)
        {
            Working();
            try
            {
                // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
                _update = await _um.CheckForUpdatesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Program.Log.LogError(ex, "Error checking for updates");
            }
            UpdateStatus();
        }

        private async void BtnDownloadUpdateClick(object sender, RoutedEventArgs e)
        {
            Working();
            try
            {
                // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
                await _um.DownloadUpdatesAsync(_update, Progress).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Program.Log.LogError(ex, "Error downloading updates");
            }
            UpdateStatus();
        }

        private void BtnRestartApplyClick(object sender, RoutedEventArgs e)
        {
            _um.ApplyUpdatesAndRestart(_update);
        }

        private void LogUpdated(object sender, LogUpdatedEventArgs e)
        {
            // logs can be sent from other threads
            this.Dispatcher.InvokeAsync(() => {
                TextLog.Text = e.Text;
                ScrollLog.ScrollToEnd();
            });
        }

        private void Progress(int percent)
        {
            // progress can be sent from other threads
            this.Dispatcher.InvokeAsync(() => {
                TextStatus.Text = $"Downloading ({percent}%)...";
            });
        }

        private void Working()
        {
            Program.Log.LogInformation("");
            BtnCheckUpdate.IsEnabled = false;
            BtnDownloadUpdate.IsEnabled = false;
            BtnRestartApply.IsEnabled = false;
            TextStatus.Text = "Working...";
        }

        private void UpdateStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
            sb.AppendLine($"This app version: {(_um.IsInstalled ? _um.CurrentVersion : "(n/a - not installed)")}");

            if (_update != null)
            {
                sb.AppendLine($"Update available: {_update.TargetFullRelease.Version}");
                BtnDownloadUpdate.IsEnabled = true;
            }
            else
            {
                BtnDownloadUpdate.IsEnabled = false;
            }

            if (_um.UpdatePendingRestart != null)
            {
                sb.AppendLine("Update ready, pending restart to install");
                BtnRestartApply.IsEnabled = true;
            }
            else
            {
                BtnRestartApply.IsEnabled = false;
            }

            TextStatus.Text = sb.ToString();
            BtnCheckUpdate.IsEnabled = true;
        }

        private static string GetAppVersion()
        {
            // Get version from AssemblyInformationalVersion
            var version = Assembly.GetExecutingAssembly()
                                  .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                  .InformationalVersion;

            // If missing, use the basic version
            if (string.IsNullOrEmpty(version))
            {
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            }

            return version;
        }
    }
}