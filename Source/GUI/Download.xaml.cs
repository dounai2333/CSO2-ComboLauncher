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

        public delegate void RetryListener();
        public static event RetryListener OnRetry;

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

        public static void MainOutput(long BytesReceived, long TotalBytesToReceive, string CurrentState, int ProgressPercentage)
        {
            if (long.TryParse(CurrentState, out long CurrentSpeed))
                CurrentState = Misc.ConvertByteTo(CurrentSpeed, "best", true) + "/s";

            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.dwldtext.Text = $"{Misc.ConvertByteTo(BytesReceived, "best", true)} / {Misc.ConvertByteTo(TotalBytesToReceive, "best", true)}\n{CurrentState}";
                Instance.dwldprogress.Value = ProgressPercentage;

                if (ProgressPercentage >= 35 && CurrentState != "Paused")
                    Instance.retry.IsEnabled = true;
            }));
        }

        public static void ResetStatus()
        {
            OnRetry = (RetryListener)Delegate.RemoveAll(OnRetry, OnRetry);
            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.dw.Text = "Downloading";
                Instance.dwldfile.Text = string.Empty;
                Instance.dwldtext.Text = string.Empty;
                Instance.dwldprogress.Value = 0;
                Instance.retry.IsEnabled = false;
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

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            // does nothing if download request is from Web.cs.
            OnRetry?.Invoke();
        }
    }
}