using CUDC.Windows.InactivityMonitor.Monitors;
using CUDC.Windows.InactivityMonitor.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace InactivityPOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Window _shutDownWarningWindow;
        private InactivityCountdown _inactivityCountdown;
        public MainWindow()
        {
            InitializeComponent();

            var hm = new HookMonitor(false)
            {
                Interval = new TimeSpan(0,0,10).TotalMilliseconds,
                MonitorKeyboardEvents = true,
                MonitorMouseEvents = true,
                Enabled = true,
                DispatchThread = Application.Current.Dispatcher
            };

            hm.Elapsed += Hm_Elapsed;
            hm.Reactivated += Hm_Reactivated;
           
            overAllCountdown.SetAndStartTimer(new TimeSpan(5, 0, 0));
            overAllCountdown.OnTimerZero += OverAllCountdown_OnTimerZero;

            _inactivityCountdown = new InactivityCountdown();
            _inactivityCountdown.OnTimerZero += Ic_OnTimerZero;
        }

        private void OverAllCountdown_OnTimerZero(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Tmr_OnTimerZero(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Hm_Reactivated(object sender, EventArgs e)
        {
            Debug.WriteLine("REACTIVATED " + sender.ToString());
           
            _inactivityCountdown.StopTimer();
            _shutDownWarningWindow.Close();
        }

        private void Hm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine("ELAPSED");

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                this.BringIntoView();
            }

            TryShow();
            _inactivityCountdown.SetAndStartTimer(new TimeSpan(0, 0, 5));
        }

        private void TryShow()
        {
            try
            {
                _shutDownWarningWindow.Show();
            }
            catch
            {
                _shutDownWarningWindow = new Window()
                {
                    Background = Brushes.BlanchedAlmond,
                    Content = _inactivityCountdown,
                    ShowActivated = true,
                    Width = 500,
                    Height = 200,
                    Topmost = true,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                _shutDownWarningWindow.Show();
            }
        }

        private void Ic_OnTimerZero(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("I am a message box");
        }
    }
}
