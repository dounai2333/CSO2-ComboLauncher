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

        public static bool NoFreeTapWindows
        {
            get { return ExitedWithFatalError && noFreeTapWindows; }
        }

        public static bool NoAnyTapWindows
        {
            get { return ExitedWithFatalError && noAnyTapWindows; }
        }

        private static bool exitedWithFatalError = false;

        private static bool noFreeTapWindows = false;

        private static bool noAnyTapWindows = false;

        private static string interfaceGuid = "";

        private static string currentIp = "";

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
        public static event Disconnected OnConnected;

        private static void ResetValue()
        {
            noFreeTapWindows = false;
            noAnyTapWindows = false;
            exitedWithFatalError = false;
            interfaceGuid = "";
            currentIp = "";
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
            ExitEvent.Reset();

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
            Process.Exited += OnExit;
            Process.OutputDataReceived += OnOutputDataReceived;
            Process.ErrorDataReceived += OnErrorDataReceived;

            Process.Start();
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
        }

        /// <summary>
        /// Gracefully stops the OpenVPN process, notifying the server of the disconnect
        /// </summary>
        public static void Kill()
        {
            if (!IsRunning)
                return;
            ExitEvent.Toggle();
        }

        /// <summary>
        /// Gracefully stops the OpenVPN process, notifying the server of the disconnect
        /// </summary>
        /// <param name="wait">Wait for the process to exit</param>
        public static async Task Kill(bool wait = true)
        {
            if (!IsRunning)
                return;
            ExitEvent.Toggle();
            if (wait)
                await Task.Run(() => Process.WaitForExit());
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
        public static void KillAllRelated()
        {
            ExitEvent.Toggle();
        }

        private static void OnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            OpenVpnInterpreter.Message(dataReceivedEventArgs.Data);
        }

        private static void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            OpenVpnInterpreter.Message(dataReceivedEventArgs.Data);
        }

        private static void OnConnect()
        {
            isConnected = true;
            OnConnected?.Invoke();
        }

        private static void OnDisconnect()
        {
            interfaceGuid = "";
            currentIp = "";
            isConnected = false;
            isProgressCompleted = false;
            OnDisconnected?.Invoke();
        }

        private static void OnProgressComplete()
        {
            isProgressCompleted = true;
            OnProgressCompleted?.Invoke();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            interfaceGuid = "";
            currentIp = "";
            isConnected = false;
            isProgressCompleted = false;
            ExitEvent.Reset();
        }

        /// <summary>
        /// Controls the threading exit event that we will tell the openvpn process to look for
        /// </summary>
        internal class ExitEvent
        {
            public const string Event = "CSO2_OpenVPN";

            public static void Reset()
            {
                if (EventWaitHandle.TryOpenExisting(Event, out EventWaitHandle handle))
                    handle?.Reset();
            }

            public static void Toggle()
            {
                if (EventWaitHandle.TryOpenExisting(Event, out EventWaitHandle handle))
                    handle?.Set();
            }
        }

        internal class OpenVpnInterpreter
        {
            private delegate void MsgPass(string msg);

            public static void Message(string msg)
            {
                foreach (var action in OVpnActions.Where(action => msg != null).Where(action => msg.Contains(action.Key)))
                {
                    action.Value(msg);
                }
            }

            /// <summary>
            /// When messages from the OpenVPN process come in, they are run through this to process the data for anything that actionable
            /// </summary>
            private static readonly Dictionary<string, MsgPass> OVpnActions = new Dictionary<string, MsgPass>()
            {
                {
                    "Closing TUN/TAP interface", msg =>
                    {
                        OnDisconnect();
                    }
                },
                {
                    "Initialization Sequence Completed", msg =>
                    {
                        OnProgressComplete();
                    }
                },
                {
                    "Exiting due to fatal error", msg =>
                    {
                        exitedWithFatalError = true;
                    }
                },
                {
                    "All TAP-Windows adapters on this system are currently in use", msg =>
                    {
                        noFreeTapWindows = true;
                    }
                },
                {
                    "There are no TAP-Windows adapters on this system", msg =>
                    {
                        noAnyTapWindows = true;
                    }
                },
                {
                    "Notified TAP-Windows driver", msg =>
                    {
                        currentIp = new Regex(@"((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}").Match(msg).Value;
                        interfaceGuid = msg.Substring(msg.IndexOf("{", StringComparison.Ordinal), 39);
                        OnConnect();
                    }
                }
            };
        }

    }
}
