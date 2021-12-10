using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace CSO2_ComboLauncher
{
    static class Static
    {
        public static NotifyIcon icon = new NotifyIcon();

        public static string CVersion = "6.5.1.16"; // 启动器版本 (记得也换程序集信息)
        public static string CWindow = "CSO2 Combo Launcher V" + CVersion;

        public static string service = "cn-zz-bgp-5.natfrp.cloud:49971"; // hosted on Sakura Frp (www.natfrp.com)
        public static string account = "Ghost";
        public static string password = "made by dounai2333.";

        public static string gameserver = "10.8.0.1";

        public static string[] blacklist = {};

        public static string netadapter = "TAP-Windows Adapter V9";
        public static string netadapterintername = "tap0901";

        public static string AuthorAndHelper = "_author_and_helper";
        public static string ThirdPartyLibrary = "_third_party_library";

        public static void SetupIcon()
        {
            icon.Text = CWindow;
            icon.Visible = false;
            icon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public static string AuthorAndLibraryOutput()
        {
            return $"\n\n{LStr.Get(AuthorAndHelper)}\n{LStr.Get(ThirdPartyLibrary)}";
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
            for (int i=0;i<ver.Length;i++)
                file[versionarray[i]] = (byte)ver[i];
            File.WriteAllBytes("Bin\\tier0.dll", file);
        }
    }
}