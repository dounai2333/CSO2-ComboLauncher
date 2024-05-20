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

        public static string CVersion = "6.5.8"; // 启动器版本 (记得也换程序集信息)
        public static string CWindow = "CSO2 Combo Launcher V" + CVersion;

        public static string gameserver = "10.8.0.1";

        public static Mutex instance = null;

        public static string Config = "cso2_launcher.ini";
        public static string Log = "cso2_launcher.log";

        public static FileStream logfile = null;

        public static bool started = false;

        public static bool newadapterinstalled = false;

        public static string[] blacklist = { };

        public static string netadapter = "TAP-Windows Adapter V9";
        public static string netadapterintername = "tap0901";

        public static Process CurrentProcess = Process.GetCurrentProcess();

        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        public static byte[] server = new byte[] { 0x63, 0x48, 0x2B, 0x58, 0x46, 0x55, 0x36, 0x77, 0x4D, 0x4B, 0x41, 0x6A, 0x34, 0x51, 0x62, 0x4C, 0x54, 0x55, 0x76, 0x66, 0x36, 0x6E, 0x56, 0x56, 0x54, 0x62, 0x43, 0x46, 0x36, 0x6E, 0x44, 0x79, 0x49, 0x61, 0x77, 0x39, 0x5A, 0x66, 0x69, 0x30, 0x54, 0x45, 0x35, 0x48, 0x66, 0x65, 0x33, 0x64, 0x78, 0x55, 0x4C, 0x71, 0x4B, 0x6B, 0x69, 0x30, 0x32, 0x2F, 0x72, 0x45, 0x70, 0x46, 0x50, 0x5A };

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
                return 200;
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