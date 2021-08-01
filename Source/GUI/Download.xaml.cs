using System;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CSO2_ComboLauncher
{
    public partial class Download : Window
    {
        public static Download Instance { get; private set; }

        private static Thread WriteThread { get; set; }

        public Download()
        {
            if (Instance == null)
                Instance = this;
            else
                return;
            InitializeComponent();
        }

        public static void StartLoop()
        {
            if (WriteThread != null)
                return;

            WriteThread = new Thread(() =>
            {
                while (true)
                {
                    Misc.Sleep(250, false);
                    if (!Instance.IsVisible)
                        continue;

                    Instance.Dispatcher.Invoke(new Action(delegate
                    {
                        Instance.dw.Text += ".";
                        Instance.dw.Text = Instance.dw.Text.Replace(".....", "");
                    }));
                }
            });
            WriteThread.Start();
        }

        public static void StopLoop()
        {
            if (WriteThread == null)
                return;

            WriteThread.Abort();
            WriteThread = null;
        }

        public static void MainOutput(long BytesReceived, long TotalBytesToReceive, long CurrentSpeed, int ProgressPercentage)
        {
            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.dwldtext.Text = $"{Misc.ConvertByteTo(BytesReceived, "best", true)} / {Misc.ConvertByteTo(TotalBytesToReceive, "best", true)}\n{Misc.ConvertByteTo(CurrentSpeed, "best", true)}/s";
                Instance.dwldprogress.Value = ProgressPercentage;
            }));
        }

        public static void ResetStatus()
        {
            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.dw.Text = "Downloading";
                Instance.dwldfile.Text = string.Empty;
                Instance.dwldtext.Text = string.Empty;
                Instance.dwldprogress.Value = 0;
                Instance.Hide();
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
