using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CUDC.Windows.InactivityMonitor.WPF
{
    /// <summary>
    /// Interaction logic for InactivityCountdown.xaml
    /// </summary>
    public partial class InactivityCountdown : UserControl
    {
        public event EventHandler OnTimerZero;

        private DispatcherTimer _countdownTimer;
        private TimeSpan _timeToBoom;
        private object _lockObj = new object();

        public InactivityCountdown()
        {
            InitializeComponent();

            _countdownTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1),
            };

            _countdownTimer.Tick += _countdownTimer_Tick;
        }

        public void SetAndStartTimer(TimeSpan remainingTime)
        {
            _timeToBoom = new TimeSpan(remainingTime.Ticks);
            SetText();
            _countdownTimer.Start();

            Debug.WriteLine("SetAndStartTimer " + _timeToBoom.ToString());
            Debug.WriteLine("SetAndStartTimer " + _countdownTimer.IsEnabled);
        }

        public void StopTimer()
        {
            if (_countdownTimer != null && _countdownTimer.IsEnabled)
            {
                _countdownTimer.Stop();                
            }
        }

        private void SetCountDownBoom(TimeSpan value)
        {
            lock (_lockObj)
            {
                _timeToBoom = value;
            }
        }

        private void _countdownTimer_Tick(object sender, EventArgs e)
        {
            SetCountDownBoom(_timeToBoom.Subtract(new TimeSpan(0,0,1)));

            Debug.WriteLine("TICK " + _timeToBoom.Seconds);

            if (_timeToBoom.TotalSeconds <= 0)
            {                
                OnTimerZero?.Invoke(this, new EventArgs());
            }

            SetText();
        }

        private void SetText()
        {
            txtBlk.Text = string.Format("{2}:{0}:{1}", _timeToBoom.Minutes.ToString("00"), _timeToBoom.Seconds.ToString("00"), _timeToBoom.Hours.ToString("00"));
        }
    }
}
