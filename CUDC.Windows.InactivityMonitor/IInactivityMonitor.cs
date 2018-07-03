using System;
using System.Timers;
using System.Windows.Threading;

namespace CUDC.Windows.InactivityMonitor
{
    public interface IInactivityMonitor : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when the period of time defined by <see cref="Interval"/>
        /// has passed without any user interaction
        /// </summary>
        event ElapsedEventHandler Elapsed;

        /// <summary>
        /// Occurs when the user continues to interact with the system after
        /// <see cref="Interval"/> has elapsed
        /// </summary>
        event EventHandler Reactivated;

        #endregion Events

        #region Properties

        /// <summary>
        /// Period of time without user interaction after which
        /// <see cref="Elapsed"/> is raised
        /// </summary>
        double Interval { get; set; }

        /// <summary>
        /// Specifies if the instances raises events
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Specifies if the instances monitors mouse events
        /// </summary>
        bool MonitorMouseEvents { get; set; }

        /// <summary>
        /// Specifies if the instances monitors keyboard events
        /// </summary>
        bool MonitorKeyboardEvents { get; set; }
        
        Dispatcher DispatchThread { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Resets the internal timer and status information
        /// </summary>
        void Reset();

        #endregion Methods
    }
}
