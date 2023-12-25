using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace CSO2_ComboLauncher
{
    static class Static
    {
        public static NotifyIcon icon = new NotifyIcon();

        public static string CVersion = "6.5.5"; // 启动器版本 (记得也换程序集信息)
        public static string CWindow = "CSO2 Combo Launcher V" + CVersion;

        public static string service = "frp-egg.top:49971"; // hosted on Sakura Frp (www.natfrp.com)
        public static string account = "Ghost";
        public static string password = "made by dounai2333.";

        public static bool mainserveronline = true;

        public static string gameserver = "10.8.0.1";

        public static Mutex instance = null;

        public static string Config = "cso2_launcher.ini";
        public static string Log = "cso2_launcher.log";

        public static FileStream logfile = null;

        public static bool started = false;

        public static bool newadapterinstalled = false;

        public static string[] blacklist = {};

        public static string netadapter = "TAP-Windows Adapter V9";
        public static string netadapterintername = "tap0901";

        public static Process CurrentProcess = Process.GetCurrentProcess();

        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        public static void SetupIcon()
        {
            icon.Text = CWindow;
            icon.Visible = false;
            icon.Icon = Icon.ExtractAssociatedIcon(CurrentProcess.MainModule.FileName);
        }

        public static bool CheckSingleMutex()
        {
            if (instance == null)
                instance = new Mutex(false, "CSO2_ComboLauncher");

            try
            {
                return instance.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                instance.ReleaseMutex();
                return instance.WaitOne(0, false);
            }
        }

        public static string AuthorAndLibraryOutput()
        {
            return $"\n\n{LStr.Get("_author_and_helper")}\n{LStr.Get("_third_party_library")}";
        }

        public static int[] versionarray = { 0x35528, 0x35528 + 2, 0x35528 + 4 };

        public static int GetGameVersion()
        {
            byte[] file = File.ReadAllBytes("Bin\\tier0.dll");
            string version = ((char)file[versionarray[0]]).ToString() + ((char)file[versionarray[1]]).ToString() + ((char)file[versionarray[2]]).ToString();
            if (int.TryParse(version, out int ver))
                return ver;
            else
                return 100;
        }

        public static void SetGameVersion(int version)
        {
            string ver = version.ToString();
            if (ver.Length != 3)
                throw new Exception("version must be 3 length!");

            byte[] file = File.ReadAllBytes("Bin\\tier0.dll");
            for (int i = 0; i < ver.Length; i++)
                file[versionarray[i]] = (byte)ver[i];
            File.WriteAllBytes("Bin\\tier0.dll", file);
        }
    }
}