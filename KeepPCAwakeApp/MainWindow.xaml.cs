using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using System;

namespace KeepPCAwakeApp
{
    public sealed partial class MainWindow : Window
    {
        private readonly DispatcherQueueTimer _autoOffTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Give the layout some breathing room
            AppWindow.Resize(new SizeInt32(620, 300));

            // Keep the window from being resized too tiny
            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = 560;
                presenter.PreferredMinimumHeight = 280;
            }

            // Create the timer before hooking related events
            _autoOffTimer = DispatcherQueue.CreateTimer();
            _autoOffTimer.IsRepeating = false;
            _autoOffTimer.Tick += AutoOffTimer_Tick;

            // Hook events after the timer exists
            AutoOffCheckBox.Checked += AutoOffCheckBox_Changed;
            AutoOffCheckBox.Unchecked += AutoOffCheckBox_Changed;
            MinutesNumberBox.ValueChanged += MinutesNumberBox_ValueChanged;

            UpdateTimerInputState();
            UpdateStatusText();

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, WindowEventArgs args)
        {
            // Clean up timer and restore normal sleep behavior
            _autoOffTimer.Stop();
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }

        private void KeepAwakeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (KeepAwakeToggle.IsOn)
            {
                // Keep the PC and display awake
                NativeMethods.SetThreadExecutionState(
                    NativeMethods.ES_CONTINUOUS |
                    NativeMethods.ES_SYSTEM_REQUIRED |
                    NativeMethods.ES_DISPLAY_REQUIRED);
            }
            else
            {
                // Restore normal sleep behavior
                NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
            }

            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        private void AutoOffCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateTimerInputState();
            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        private void MinutesNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            NormalizeMinutesValue();
            RefreshAutoOffTimer();
            UpdateStatusText();
        }

        private void AutoOffTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            sender.Stop();
            KeepAwakeToggle.IsOn = false;
        }

        private void RefreshAutoOffTimer()
        {
            _autoOffTimer.Stop();

            if (!KeepAwakeToggle.IsOn || AutoOffCheckBox.IsChecked != true)
            {
                return;
            }

            int minutes = GetMinutes();
            _autoOffTimer.Interval = TimeSpan.FromMinutes(minutes);
            _autoOffTimer.Start();
        }

        private void UpdateTimerInputState()
        {
            MinutesNumberBox.IsEnabled = AutoOffCheckBox.IsChecked == true;
        }

        private void UpdateStatusText()
        {
            if (!KeepAwakeToggle.IsOn)
            {
                StatusText.Text = "Feeling sleepy. Might fall asleep";
                return;
            }

            if (AutoOffCheckBox.IsChecked == true)
            {
                int minutes = GetMinutes();
                StatusText.Text = $"Staying awake for {minutes} minute{(minutes == 1 ? "" : "s")}!";
            }
            else
            {
                StatusText.Text = "Staying awake until you turn it off";
            }
        }

        private int GetMinutes()
        {
            if (double.IsNaN(MinutesNumberBox.Value) || MinutesNumberBox.Value < 1)
            {
                return 1;
            }

            return (int)Math.Round(MinutesNumberBox.Value);
        }

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
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
    }
}