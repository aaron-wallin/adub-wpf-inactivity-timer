using CUDC.Windows.InactivityMonitor.HookHelp;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CUDC.Windows.InactivityMonitor.Monitors
{
    /// <summary>
    /// Class for monitoring user inactivity by incorporating hooks
    /// </summary>
    public class HookMonitor : MonitorBase
	{
		#region Private Fields

		private bool _disposed = false;
		private bool _globalHooks = false;

		private int _keyboardHookHandle = 0;
		private int _mouseHookHandle = 0;

		private Win32HookProcHandler _keyboardHandler = null;
		private Win32HookProcHandler _mouseHandler = null;

        private int _currentThreadId;

		#endregion Private Fields

		#region Public Properties

		/// <summary>
		/// Specifies if the instances monitors mouse events
		/// </summary>
		public override bool MonitorMouseEvents
		{
			get
			{
				return base.MonitorMouseEvents;
			}
			set
			{
				if (_disposed)
					throw new ObjectDisposedException("Object has already been disposed");

				if (base.MonitorMouseEvents != value)
				{
					base.MonitorMouseEvents = value;
					if (value)
						RegisterMouseHook(_globalHooks);
					else
						UnRegisterMouseHook();
				}
			}
		}

		/// <summary>
		/// Specifies if the instances monitors keyboard events
		/// </summary>
		public override bool MonitorKeyboardEvents
		{
			get
			{
				return base.MonitorKeyboardEvents;
			}
			set
			{
				if (_disposed)
					throw new ObjectDisposedException("Object has already been disposed");
				
				if (base.MonitorKeyboardEvents != value)
				{
					base.MonitorKeyboardEvents = value;
					if (value)
						RegisterKeyboardHook(_globalHooks);
					else
						UnRegisterKeyboardHook();
				}
			}
		}

		#endregion Public Properties

		#region Constructors

		/// <summary>
		/// Creates a new instance of <see cref="HookMonitor"/>
		/// </summary>
		/// <param name="global">
		/// True if the system-wide activity will be monitored, otherwise only
		/// events in the current thread will be monitored
		/// </param>
		public HookMonitor(bool global) : base()
		{
            _currentThreadId = User32.GetCurrentThreadId();

            _globalHooks = global;
			if (MonitorKeyboardEvents)
				RegisterKeyboardHook(_globalHooks);
			if (MonitorMouseEvents)
				RegisterMouseHook(_globalHooks);
		}

		#endregion Constructors

		#region Deconstructor

		/// <summary>
		/// Deconstructor method for use by the garbage collector
		/// </summary>
		~HookMonitor()
		{
			Dispose(false);
		}

		#endregion Deconstructor

		#region Protected Methods

		/// <summary>
		/// Actual deconstructor in accordance with the dispose pattern
		/// </summary>
		/// <param name="disposing">
		/// True if managed and unmanaged resources will be freed
		/// (otherwise only unmanaged resources are handled)
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				UnRegisterKeyboardHook();
				UnRegisterMouseHook();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Private Methods

		private void ResetBase()
		{
			if (TimeElapsed && !ReactivatedRaised)
				OnReactivated(new EventArgs());
			base.Reset();
		}

		private int KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
				ResetBase();

            return User32.CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
		}

		private int MouseHook(int nCode, IntPtr wParam, IntPtr lParam)
		{
            if (nCode >= 0)
				ResetBase();
            
			return User32.CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
		}

		private void RegisterKeyboardHook(bool global)
		{
			if (_keyboardHookHandle == 0)
			{
				_keyboardHandler = new Win32HookProcHandler(KeyboardHook);
				if (global)
					_keyboardHookHandle = User32.SetWindowsHookEx(
						(int)Win32Hook.WH_KEYBOARD_LL, _keyboardHandler,
						Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
						(int)0);
				else
					_keyboardHookHandle = User32.SetWindowsHookEx(
						(int)Win32Hook.WH_KEYBOARD, _keyboardHandler,
						(IntPtr)0, _currentThreadId);
				if (_keyboardHookHandle == 0)
					base.MonitorKeyboardEvents = false;
			}
		}

		private void UnRegisterKeyboardHook()
		{
			if (_keyboardHookHandle != 0)
			{
				if (!User32.UnhookWindowsHookEx(_keyboardHookHandle))
					base.MonitorKeyboardEvents = true;
				else
				{
					_keyboardHookHandle = 0;
					_keyboardHandler = null;
				}
			}
		}

		private void RegisterMouseHook(bool global)
		{
			if (_mouseHookHandle == 0)
			{
				_mouseHandler = new Win32HookProcHandler(MouseHook);
				if (global)
					_mouseHookHandle = User32.SetWindowsHookEx(
						(int)Win32Hook.WH_MOUSE_LL, _mouseHandler,
						Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
						(int)0);
				else
					_mouseHookHandle = User32.SetWindowsHookEx(
						(int)Win32Hook.WH_MOUSE, _mouseHandler,
						(IntPtr)0, _currentThreadId);
				if (_mouseHookHandle == 0)
					base.MonitorMouseEvents = false;
			}
		}

		private void UnRegisterMouseHook()
		{
			if (_mouseHookHandle != 0)
			{
				if (!User32.UnhookWindowsHookEx(_mouseHookHandle))
					base.MonitorMouseEvents = true;
				else
				{
					_mouseHookHandle = 0;
					_mouseHandler = null;
				}
			}
		}

		#endregion Private Methods
	}
}
