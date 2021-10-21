using System;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CSO2_ComboLauncher
{
    public partial class ZipWorker : Window
    {
        public static ZipWorker Instance { get; private set; }

        private static Thread WriteThread { get; set; }

        public ZipWorker()
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
                        Instance.zip.Text += ".";
                        Instance.zip.Text = Instance.zip.Text.Replace(".....", "");
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

        public static void MainOutput(int FileProcessed, int TotalProgress)
        {
            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.zipwktext.Text = $"Progress:\n{FileProcessed} / {TotalProgress} Files";
                Instance.zipwkprogress.Value = Misc.GetPercentage(FileProcessed, TotalProgress);
            }));
        }

        public static void ResetStatus()
        {
            Instance.Dispatcher.Invoke(new Action(delegate
            {
                Instance.zip.Text = "Extracting";
                Instance.zipwkfile.Text = string.Empty;
                Instance.zipwktext.Text = string.Empty;
                Instance.zipwkprogress.Value = 0;
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