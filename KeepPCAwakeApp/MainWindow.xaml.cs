using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;

namespace KeepPCAwakeApp
{
    // Main window for the KeepPCAwake application. Handles UI interactions and requests
    // to the operating system to prevent sleep while the user requests it.

    public sealed partial class MainWindow : Window
    {
        // Dispatcher timer used to automatically turn off the "keep awake" mode after a set number of minutes
        private readonly DispatcherQueueTimer _autoOffTimer;

        // Constructor: initialize UI, sizing and event handlers
        public MainWindow()
        {
            InitializeComponent(); // Wire up XAML-defined controls

            // Set a comfortable default window size so controls have room
            AppWindow.Resize(new SizeInt32(620, 300));

            // Prevent the window from being resized smaller than usable
            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = 560;
                presenter.PreferredMinimumHeight = 280;
            }

            // Create the one-shot timer used for the optional auto-off feature
            _autoOffTimer = DispatcherQueue.CreateTimer();
            _autoOffTimer.IsRepeating = false; // run only once per start
            _autoOffTimer.Tick += AutoOffTimer_Tick; // timer callback

            // Hook UI events after the timer exists so handlers can reference it safely
            AutoOffCheckBox.Checked += AutoOffCheckBox_Changed;
            AutoOffCheckBox.Unchecked += AutoOffCheckBox_Changed;
            MinutesNumberBox.ValueChanged += MinutesNumberBox_ValueChanged;

            // Sync UI state with current control values
            UpdateTimerInputState();
            UpdateStatusText();

            // Ensure cleanup when the window is closed
            this.Closed += MainWindow_Closed;
        }

        // Window closed: stop timer and restore normal sleep behavior
        private void MainWindow_Closed(object? sender, WindowEventArgs args)
        {
            _autoOffTimer.Stop();
            // Clear any execution state so the system can sleep normally
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }

        // Main toggle handler: enable or disable the keep-awake request
        private void KeepAwakeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (KeepAwakeToggle.IsOn)
            {
                // Request that the system and display stay awake until we clear the flag
                NativeMethods.SetThreadExecutionState(
                    NativeMethods.ES_CONTINUOUS |
                    NativeMethods.ES_SYSTEM_REQUIRED |
                    NativeMethods.ES_DISPLAY_REQUIRED);
            }
            else
            {
                // Clear the persistent keep-awake request so sleep can occur normally
                NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
            }

            // Restart/stop the auto-off timer based on new state, and update UI
            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        // Runs when the auto-off checkbox is toggled: enable/disable minute input and update timer
        private void AutoOffCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateTimerInputState();
            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        // Called when the NumberBox value changes: normalize value and refresh timer/status
        private void MinutesNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            NormalizeMinutesValue();
            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        // Fired when the auto-off timer interval elapses
        private void AutoOffTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            sender.Stop(); // timer is one-shot
            KeepAwakeToggle.IsOn = false; // turn off keep-awake when time runs out
        }

        // Start or stop the auto-off timer depending on toggle and checkbox state
        private void RefreshAutoOffTimer()
        {
            _autoOffTimer.Stop();

            // Only start the timer if we're keeping the PC awake and auto-off is enabled
            if (!KeepAwakeToggle.IsOn || AutoOffCheckBox.IsChecked != true)
            {
                return;
            }

            int minutes = GetMinutes();
            _autoOffTimer.Interval = TimeSpan.FromMinutes(minutes);
            _autoOffTimer.Start();
        }

        // Enable/disable the minutes input control based on whether auto-off is active
        private void UpdateTimerInputState()
        {
            MinutesNumberBox.IsEnabled = AutoOffCheckBox.IsChecked == true;
        }

        // Update the StatusText to reflect the current keep-awake and timer state
        private void UpdateStatusText()
        {
            if (!KeepAwakeToggle.IsOn)
            {
                StatusText.Text = "Feeling sleepy. Might fall asleep"; // idle state
                return;
            }

            if (AutoOffCheckBox.IsChecked == true)
            {
                int minutes = GetMinutes();
                StatusText.Text = $"Staying awake for {minutes} minute{(minutes == 1 ? "" : "s")}!"; // showing chosen duration
            }
            else
            {
                StatusText.Text = "Staying awake until you turn it off"; // persistent until user toggles off
            }
        }

        // Read the minutes value safely from the NumberBox and ensure a minimum of 1
        private int GetMinutes()
        {
            if (double.IsNaN(MinutesNumberBox.Value) || MinutesNumberBox.Value < 1)
            {
                return 1; // minimum one minute
            }

            return (int)Math.Round(MinutesNumberBox.Value);
        }

        // Normalize the NumberBox value to avoid NaN or values below the minimum
        private void NormalizeMinutesValue()
        {
            if (double.IsNaN(MinutesNumberBox.Value) || MinutesNumberBox.Value < 1)
            {
                MinutesNumberBox.Value = 1;
            }
        }
    }

    internal static class NativeMethods
    {
        // P/Invoke constants and helper for calling SetThreadExecutionState
        // ES_CONTINUOUS marks the request as persistent until cleared.
        // ES_SYSTEM_REQUIRED prevents system sleep; ES_DISPLAY_REQUIRED prevents display sleep.
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
    }
}