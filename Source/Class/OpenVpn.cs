using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CSO2_ComboLauncher
{
    static class OpenVpn
    {
        public static Process Process { get; private set; }

        public static bool IsRunning
        {
            get { return Process != null && !Process.HasExited; }
        }

        public static string InterFaceGuid
        {
            get { return IsRunning ? interfaceGuid : string.Empty; }
        }

        public static string CurrentIp
        {
            get { return IsRunning ? currentIp : string.Empty; }
        }

        public static bool IsConnected
        {
            get { return IsRunning && isConnected; }
        }

        public static bool IsProgressCompleted
        {
            get { return IsRunning && isProgressCompleted; }
        }

        public static bool ExitedWithFatalError
        {
            get { return Process.HasExited && exitedWithFatalError; }
        }

        public static bool NoTapWindowsAvailable
        {
            get { return ExitedWithFatalError && noTapWindowsAvailable; }
        }

        public static bool NoTapWindowsExist
        {
            get { return ExitedWithFatalError && noTapWindowsExist; }
        }

        private static bool exitedWithFatalError = false;

        private static bool noTapWindowsAvailable = false;

        private static bool noTapWindowsExist = false;

        private static string interfaceGuid = string.Empty;

        private static string currentIp = string.Empty;

        private static bool isConnected = false;

        private static bool isProgressCompleted = false;

        public delegate void ProgressCompleted();
        public static event ProgressCompleted OnProgressCompleted;

        public delegate void Disconnected();
        /// <summary>
        /// if output level ('verb') is lower than 2, then this will not be called.
        /// </summary>
        public static event Disconnected OnDisconnected;

        public delegate void Connected();
        public static event Connected OnConnected;

        private static void ResetValue()
        {
            OnProgressCompleted = (ProgressCompleted)Delegate.RemoveAll(OnProgressCompleted, OnProgressCompleted);
            OnDisconnected = (Disconnected)Delegate.RemoveAll(OnDisconnected, OnDisconnected);
            OnConnected = (Connected)Delegate.RemoveAll(OnConnected, OnConnected);

            ExitEvent.Reset();

            noTapWindowsAvailable = false;
            noTapWindowsExist = false;
            exitedWithFatalError = false;
            interfaceGuid = string.Empty;
            currentIp = string.Empty;
            isConnected = false;
            isProgressCompleted = false;
        }

        /// <summary>
        /// Start the OpenVPN container process
        /// </summary>
        /// <param name="openVpnExe">Exe file of openvpn</param>
        /// <param name="configFile">Config file path</param>
        public static void Start(string openVpnExe, string configFile)
        {
            if (IsRunning)
                return;

            ResetValue();
            Process?.Dispose();

            Process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo()
                {
                    FileName = openVpnExe,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"--config \"{configFile}\" --suppress-timestamps --service \"{ExitEvent.Event}\" 0"
                }
            };
            Process.OutputDataReceived += OnDataReceived;
            Process.ErrorDataReceived += OnDataReceived;
            Process.Exited += (s, e) =>
            {
                Main.Log.WriteToFile("OpenVPN: exited with exitcode " + Misc.DecimalToHex(Process.ExitCode));
            };

            Process.Start();
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
        }

        /// <summary>
        /// Gracefully stops the OpenVPN process, notifying the server of the disconnect
        /// </summary>
        public static bool Kill()
        {
            if (!IsRunning)
                return false;

            return ExitEvent.Toggle();
        }

        /// <summary>
        /// Gracefully stops the OpenVPN process, notifying the server of the disconnect
        /// </summary>
        /// <param name="wait">Wait for the process to exit</param>
        public static async Task<bool> Kill(bool wait = true)
        {
            if (!IsRunning)
                return false;

            if (wait)
            {
                if (ExitEvent.Toggle())
                {
                    await Task.Run(() => Process.WaitForExit());
                    return true;
                }
                return false;
            }
            else
            {
                return ExitEvent.Toggle();
            }
        }

        /// <summary>
        /// Forces the OpenVPN container process to close
        /// Using this means that OpenVPN will not stop gracefully, which will skip notifying the server of the disconnect
        /// The server will pick up the disconnect typically within 5 minutes of disconnects, via this method
        /// </summary>
        public static void ForceKill()
        {
            if (IsRunning)
                Process.Kill();
        }

        /// <summary>
        /// Kill all related openvpn process to prevent unexpected error
        /// </summary>
        public static bool KillAllRelated()
        {
            Process[] processes = Process.GetProcessesByName("openvpn");
            foreach (Process proc in processes)
                if (proc.MainModule.FileName.StartsWith(System.IO.Path.GetDirectoryName(Static.CurrentProcess.MainModule.FileName)))
                    proc.Kill();

            return ExitEvent.Reset();
        }

        private static void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;

            Main.Log.WriteToFile("OpenVPN: " + e.Data);
            CheckMessage(e.Data);
        }

        private static void OnConnect()
        {
            isConnected = true;
            OnConnected?.Invoke();
        }

        private static void OnDisconnect()
        {
            interfaceGuid = string.Empty;
            currentIp = string.Empty;
            isConnected = false;
            isProgressCompleted = false;
            OnDisconnected?.Invoke();
        }

        private static void OnProgressComplete()
        {
            isProgressCompleted = true;
            OnProgressCompleted?.Invoke();
        }

        private static void CheckMessage(string message)
        {
            if (message.Contains("Closing TUN/TAP interface"))
                OnDisconnect();
            else if (message.Contains("Initialization Sequence Completed"))
                OnProgressComplete();
            else if (message.Contains("Exiting due to fatal error"))
                exitedWithFatalError = true;
            else if (message.Contains("All TAP-Windows adapters on this system are currently in use"))
                noTapWindowsAvailable = true;
            else if (message.Contains("There are no TAP-Windows adapters on this system"))
                noTapWindowsExist = true;
            else if (message.Contains("Notified TAP-Windows driver"))
            {
                currentIp = new Regex(@"\b((([0-2]\d[0-5])|(\d{2})|(\d))\.){3}(([0-2]\d[0-5])|(\d{2})|(\d))\b").Match(message).Value;
                interfaceGuid = message.Substring(message.IndexOf("{", StringComparison.Ordinal), 39);
                OnConnect();
            }
        }

        /// <summary>
        /// Controls the threading exit event that we will tell the openvpn process to look for
        /// </summary>
        internal class ExitEvent
        {
            public const string Event = "CSO2_OpenVPN";

            public static bool Reset()
            {
                if (EventWaitHandle.TryOpenExisting(Event, out EventWaitHandle handle))
                    return handle.Reset();

                return false;
            }

            public static bool Toggle()
            {
                if (EventWaitHandle.TryOpenExisting(Event, out EventWaitHandle handle))
                    return handle.Set();

                return false;
            }
        }
    }
}