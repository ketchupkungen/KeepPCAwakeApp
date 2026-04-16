using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace KeepPCAwakeApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Small app window size in pixels
            AppWindow.Resize(new SizeInt32(320, 180));

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, WindowEventArgs args)
        {
            // Reset sleep behavior when the app closes
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }

        private void KeepAwakeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (KeepAwakeToggle.IsOn)
            {
                // Keep system and display awake
                NativeMethods.SetThreadExecutionState(
                    NativeMethods.ES_CONTINUOUS |
                    NativeMethods.ES_SYSTEM_REQUIRED |
                    NativeMethods.ES_DISPLAY_REQUIRED);

                StatusText.Text = "Staying awake!";
            }
            else
            {
                // Clear awake flags and allow normal sleep again
                NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
                StatusText.Text = "Feeling sleepy. Might fall asleep";
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