using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;

using Microsoft.Win32;

namespace CSO2_ComboLauncher
{
    public static class MiniDumpHelper
    {
        // Taken almost verbatim from http://blog.kalmbach-software.de/2008/12/13/writing-minidumps-in-c/
        [Flags]
        public enum Option : uint
        {
            // From dbghelp.h:
            Normal = 0x00000000,
            WithDataSegs = 0x00000001,
            WithFullMemory = 0x00000002,
            WithHandleData = 0x00000004,
            FilterMemory = 0x00000008,
            ScanMemory = 0x00000010,
            WithUnloadedModules = 0x00000020,
            WithIndirectlyReferencedMemory = 0x00000040,
            FilterModulePaths = 0x00000080,
            WithProcessThreadData = 0x00000100,
            WithPrivateReadWriteMemory = 0x00000200,
            WithoutOptionalData = 0x00000400,
            WithFullMemoryInfo = 0x00000800,
            WithThreadInfo = 0x00001000,
            WithCodeSegs = 0x00002000,
            WithoutAuxiliaryState = 0x00004000,
            WithFullAuxiliaryState = 0x00008000,
            WithPrivateWriteCopyMemory = 0x00010000,
            IgnoreInaccessibleMemory = 0x00020000,
            ValidTypeFlags = 0x0003ffff
        };

        public enum ExceptionInfo
        {
            None,
            Present
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // Pack=4 is important! So it works also for x64!

        public struct MiniDumpExceptionInformation
        {
            public uint ThreadId;

            public IntPtr ExceptionPointers;

            [MarshalAs(UnmanagedType.Bool)]
            public bool ClientPointers;
        }

        // Overload requiring MiniDumpExceptionInformation
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

        // Overload supporting MiniDumpExceptionInformation == NULL
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        static extern uint GetCurrentThreadId();

        public static bool Write(SafeHandle fileHandle, Option options, ExceptionInfo exceptionInfo)
        {
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr currentProcessHandle = currentProcess.Handle;
            uint currentProcessId = (uint)currentProcess.Id;
            MiniDumpExceptionInformation exp;
            exp.ThreadId = GetCurrentThreadId();
            exp.ClientPointers = false;
            exp.ExceptionPointers = IntPtr.Zero;
            if (exceptionInfo == ExceptionInfo.Present)
                exp.ExceptionPointers = Marshal.GetExceptionPointers();

            if (exp.ExceptionPointers == IntPtr.Zero)
                return MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            else
                return MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle, (uint)options, ref exp, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool Write(string dmpPath, Option dumpType)
        {
            using (FileStream stream = new FileStream(dmpPath, FileMode.Create))
                return Write(stream.SafeFileHandle, dumpType, ExceptionInfo.Present);
        }
    }

    public partial class App : Application
    {
        public App()
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LStr.InitLocalizedStrings();

            if (!File.Exists("Bin\\engine.dll") || !File.Exists("Bin\\tier0.dll") || !File.Exists("Data\\1b87c6b551e518d11114ee21b7645a47.pkg"))
            {
                MessageBox.Show(LStr.Get("_wrong_folder"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            if (new FileInfo("Bin\\tier0.dll").Length < Static.versionarray[Static.versionarray.Count() - 1])
            {
                MessageBox.Show(LStr.Get("_tier0_broken"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                if (releaseKey < 461808)
                {
                    Task.Run(() =>
                    {
                        MessageBoxResult box = MessageBox.Show(LStr.Get("_net_framework_lower_version"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (box == MessageBoxResult.Yes)
                        {
                            Process.Start("https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-offline-installer");
                            Environment.Exit(1);
                        }
                    }).Wait();
                }
            }

            bool network = false;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((ni.OperationalStatus != OperationalStatus.Up) || (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) || (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;
                if (ni.Speed < 10000000) // 10M
                    continue;
                if (ni.Name.ToLower().Contains("virtual") || ni.Description.ToLower().Contains("virtual"))
                    continue;
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;

                network = true;
                break;
            }
            if (!network && !NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show(LStr.Get("_no_network_connection"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }

            if (!Misc.IsFileAvailable(Path.GetTempPath() + "combolauncher.log"))
            {
                MessageBox.Show(LStr.Get("_no_multi_start"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            Task.Run(() =>
            {
                FileStream fs = new FileStream(Path.GetTempPath() + "combolauncher.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                lock (fs)
                {
                    fs.SetLength(0);
                    Process.GetCurrentProcess().WaitForExit();
                }
            });

            if (!Directory.Exists("errorminidumps"))
            {
                Directory.CreateDirectory("errorminidumps");
            }
            else
            {
                string[] files = Directory.GetFiles("errorminidumps");
                for (int i = 0; i < files.Count(); i++)
                    if (!Path.GetFileName(files[i]).StartsWith($"error_{DateTime.Now:yyyy-MM-dd}"))
                        File.Delete(files[i]);
            }

            // setup window, Main will be called after this function ends
            //new Config(); // bugged when cso2_launcher.ini is not exist, it been called in Main.xaml.cs
            new QQGroup();
            new Download();
            Download.StartLoop();
            new ZipWorker();
            ZipWorker.StartLoop();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public static void HideAllWindow()
        {
            Current.Dispatcher.Invoke(new Action(delegate
            {
                if (Download.Instance != null)
                    if (Download.Instance.IsVisible)
                        Download.Instance.Hide();
                if (ZipWorker.Instance != null)
                    if (ZipWorker.Instance.IsVisible)
                        ZipWorker.Instance.Hide();
                if (Config.Instance != null)
                    if (Config.Instance.IsVisible)
                        Config.Instance.Hide();
                if (CSO2_ComboLauncher.Main.Instance != null)
                    if (CSO2_ComboLauncher.Main.Instance.IsVisible)
                        CSO2_ComboLauncher.Main.Instance.Hide();
            }));
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(e.Name);
            if (assemblyName.Name.EndsWith(".resources"))
                return null;
            if (!assemblyName.Name.StartsWith("Toqe.Downloader.Business") && assemblyName.Name != "Chsword.JDynamic")
                return null;

            var path = "CSO2_ComboLauncher.Library." + assemblyName.Name + ".dll";
            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            HideAllWindow();

            Static.icon.Visible = false;
            Static.icon.Dispose();

            OpenVpn.Kill();

            string time = DateTime.Now.ToString("yyyy/MM/dd, HH:mm:ss");
            string timeo = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");

            if (!Directory.Exists("errorminidumps"))
                Directory.CreateDirectory("errorminidumps");
            MiniDumpHelper.Write($"errorminidumps\\error_{timeo}.mdmp", MiniDumpHelper.Option.WithFullMemoryInfo);
            File.WriteAllText($"errorminidumps\\error_{timeo}.txt",
                              LStr.Get("_error_time") + time + Environment.NewLine + Environment.NewLine
                              + ex.Message + Environment.NewLine + Environment.NewLine
                              + ((ex.InnerException != null) ? ex.InnerException.ToString() : string.Empty) + Environment.NewLine + Environment.NewLine
                              + ex.StackTrace, Encoding.UTF8);

            string methods = "";
            string[] messages = Misc.SplitString(ex.StackTrace);
            for (int i = 0; i < messages.Count(); i++)
            {
                if (messages[i].Contains("CSO2_ComboLauncher"))
                {
                    if (i > 0)
                        methods += messages[i - 1].Substring(5) + "\n";
                    methods += messages[i].Substring(5);
                    break;
                }
            }

            MessageBox.Show(LStr.Get("_error_msg") + "\n\n"
                            + LStr.Get("_error_time") + time + "\n"
                            + LStr.Get("_error_code") + Misc.DecimalToHex(ex.HResult) + "\n"
                            + LStr.Get("_error_info") + "\n" + ex.Message + "\n"
                            + LStr.Get("_error_method") + "\n" + methods
                            + "\n\n" + LStr.Get("_error_hint")
                            , Static.CWindow
                            , MessageBoxButton.OK
                            , MessageBoxImage.Error);
            Environment.Exit(ex.HResult);
        }
    }
}