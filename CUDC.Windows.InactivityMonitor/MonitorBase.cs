using System;
using System.Threading;
using System.Timers;
using System.Windows.Threading;

namespace CUDC.Windows.InactivityMonitor
{
    public abstract class MonitorBase : IInactivityMonitor
    {
        #region Private Fields

        private bool disposed = false;
        private bool enabled = false;

        private bool monitorMouse = true;
        private bool monitorKeyboard = true;

        private bool timeElapsed = false;
        private bool reactivated = false;

        private System.Timers.Timer monitorTimer = null;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="MonitorBase"/> which
        /// includes a <see cref="System.Timers.Timer"/> object
        /// </summary>
        protected MonitorBase()
        {
            monitorTimer = new System.Timers.Timer()
            {   
                AutoReset = false
            };

            monitorTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        #endregion Constructors

        #region Dispose Pattern

        /// <summary>
        /// Unregisters all event handlers and disposes internal objects
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Actual deconstructor in accordance with the dispose pattern
        /// </summary>
        /// <param name="disposing">
        /// True if managed and unmanaged resources will be freed
        /// (otherwise only unmanaged resources are handled)
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing)
                {
                    Delegate[] delegateBuffer = null;

                    monitorTimer.Elapsed -= new ElapsedEventHandler(TimerElapsed);
                    monitorTimer.Dispose();

                    delegateBuffer = Elapsed.GetInvocationList();
                    foreach (ElapsedEventHandler item in delegateBuffer)
                        Elapsed -= item;
                    Elapsed = null;

                    delegateBuffer = Reactivated.GetInvocationList();
                    foreach (EventHandler item in delegateBuffer)
                        Reactivated -= item;
                    Reactivated = null;
                }
            }
        }

        /// <summary>
        /// Deconstructor method for use by the garbage collector
        /// </summary>
        ~MonitorBase()
        {
            Dispose(false);
        }

        #endregion Dispose Pattern

        #region Public Events

        /// <summary>
        /// Occurs when the period of time defined by <see cref="Interval"/>
        /// has passed without any user interaction
        /// </summary>
        public event ElapsedEventHandler Elapsed;

        /// <summary>
        /// Occurs when the user continues to interact with the system after
        /// <see cref="Interval"/> has elapsed
        /// </summary>
        public event EventHandler Reactivated;

        #endregion Public Events

        #region Protected Properties

        /// <summary>
        /// True after <see cref="Elapsed"/> has been raised until
        /// <see cref="Reset"/> is called
        /// </summary>
        protected bool TimeElapsed
        {
            get
            {
                return timeElapsed;
            }
        }

        /// <summary>
        /// True after <see cref="Reactivated"/> has been raised until
        /// <see cref="Reset"/> is called
        /// </summary>
        protected bool ReactivatedRaised
        {
            get
            {
                return reactivated;
            }
        }

        #endregion Protected Properties

        #region Public Properties

        /// <summary>
        /// Period of time without user interaction after which
        /// <see cref="Elapsed"/> is raised
        /// </summary>
        public virtual double Interval
        {
            get
            {
                return monitorTimer.Interval;
            }
            set
            {
                monitorTimer.Interval = value;
            }
        }

        /// <summary>
        /// Specifies if the instances raises events
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                monitorTimer.Enabled = enabled = value;
            }
        }

        /// <summary>
        /// Specifies if the instances monitors mouse events
        /// </summary>
        public virtual bool MonitorMouseEvents
        {
            get
            {
                return monitorMouse;
            }
            set
            {
                monitorMouse = value;
            }
        }

        /// <summary>
        /// Specifies if the instances monitors keyboard events
        /// </summary>
        public virtual bool MonitorKeyboardEvents
        {
            get
            {
                return monitorKeyboard;
            }
            set
            {
                monitorKeyboard = value;
            }
        }

        public virtual Dispatcher DispatchThread { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Resets the internal timer and status information
        /// </summary>
        public virtual void Reset()
        {
            if (disposed)
                throw new ObjectDisposedException("Object has already been disposed");

            if (enabled)
            {
                monitorTimer.Interval = monitorTimer.Interval;
                timeElapsed = false;
                reactivated = false;
            }
        }

        #endregion Public Methods

        #region Proteced Methods

        /// <summary>
        /// Method to raise the <see cref="Elapsed"/> event
        /// (performs consistency checks before raising <see cref="Elapsed"/>)
        /// </summary>
        /// <param name="e">
        /// <see cref="ElapsedEventArgs"/> object provided by the internal timer object
        /// </param>
        protected void OnElapsed(ElapsedEventArgs e)
        {
            timeElapsed = true;
            if (Elapsed != null && enabled && (monitorKeyboard || monitorMouse))
                Elapsed(this, e);
        }

        /// <summary>
        /// Method to raise the <see cref="Reactivated"/> event (performs
        /// consistency checks before raising <see cref="Reactivated"/>)
        /// </summary>
        /// <param name="e">
        /// <see cref="EventArgs"/> object
        /// </param>
        protected void OnReactivated(EventArgs e)
        {
            reactivated = true;
            if (Reactivated != null && enabled && (monitorKeyboard || monitorMouse))
                Reactivated(this, e);
        }

        #endregion Proteced Methods

        #region Private Methods

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            MonitorMouseEvents = false; /*not ideal, but quirks with wpf textbox made this necessary*/
           // MonitorKeyboardEvents = false;

            if (DispatchThread != null)
            {
                DispatchThread.BeginInvoke(new Action(() =>
                {
                    OnElapsed(e);                    
                }));
            }
            else
            {
                OnElapsed(e);
            }

            Thread.Sleep(500); /*not ideal, but quirks with wpf textbox made this necessary*/
            MonitorMouseEvents = true; /*not ideal, but quirks with wpf textbox made this necessary*/
            //MonitorKeyboardEvents = true;
        }

        #endregion Private Methods
    }
}
