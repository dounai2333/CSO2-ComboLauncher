using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSO2_ComboLauncher
{
    static class Downloader
    {
        private static readonly Config Config = Config.Instance;
        private static Ftp Ftp = new Ftp(Static.service, Static.account, Static.password);

        public static async Task<string> StringFromMainServer(string file)
        {
            return await Ftp.DownloadString(file);
        }

        public static async Task<bool> FileFromMainServer(string path, string serverpath)
        {
            return await Ftp.DownloadFile(path, serverpath);
        }

        public static async Task<string> StringFromHttp(string link)
        {
            using (Web Web = new Web())
                return await Web.Client.DownloadStringTaskAsync(link);
        }

        public static async Task<bool> FileFromHttp(string link, string path, int threads, string checkhashtype = "", string hash = "")
        {
            if (threads <= 1)
            {
                using (Web Web = new Web())
                {
                    return await Web.DownloadFile(link, path, checkhashtype, hash);
                }
            }
            else
            {
                using (MultiThreadDownloader MTDownloader = new MultiThreadDownloader())
                {
                    try
                    {
                        return await MTDownloader.DownloadFile(link, path, threads, checkhashtype, hash);
                    }
                    catch
                    {
                        Download.ResetStatus();
                        return false;
                    }
                }
            }
        }

        public static async Task LauncherUpdate()
        {
            string latestversion = await StringFromMainServer("updates/launcher/latest.txt");

            if (latestversion != Static.CVersion)
            {
                string latestinfo = Misc.Decrypt(await StringFromMainServer($"updates/launcher/version/{latestversion}.txt"));
                string[] infoarray = Misc.SplitString(latestinfo);

                string key = infoarray[0];
                string code = infoarray[1];
                string sha1 = infoarray[2];

                int logcount = int.Parse(infoarray[3]);
                string logmessage = "";
                for (int i = 4; i < (logcount + 4); i++)
                {
                    if (i == (logcount + 4 - 1))
                        logmessage += "- " + infoarray[i];
                    else
                        logmessage += "- " + infoarray[i] + "\n";
                }

                MessageBoxResult box = MessageBox.Show(LStr.Get("_self_checking_launcherupdate_needed", latestversion, logmessage), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (box == MessageBoxResult.Yes)
                {
                    if (await QQMail.DownloadFile($"CSO2 Launcher V{latestversion}.exe", 2, code, sha1, key))
                    {
                        App.HideAllWindow();
                        if (Static.CurrentProcess.ProcessName.StartsWith("CSO2 Launcher V"))
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && del /q \"{Static.CurrentProcess.MainModule.FileName}\" && start \"\" \"CSO2 Launcher V{latestversion}.exe\"", false, true);
                        else
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && move /y \"CSO2 Launcher V{latestversion}.exe\" \"{Static.CurrentProcess.MainModule.FileName}\" && start \"\" \"{Static.CurrentProcess.MainModule.FileName}\"", false, true);

                        App.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show(LStr.Get("_self_checking_launcherupdate_failed"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        public static async Task<bool> GameClientUpdate()
        {
            string gameclientinfo = Misc.Decrypt(await StringFromMainServer("updates/game/gameclient.txt"));
            string[] infoarray = Misc.SplitString(gameclientinfo);

            string key = infoarray[0];
            string code = infoarray[1];
            string sha1 = infoarray[2];

            if (File.Exists("Bin\\CounterStrikeOnline2.exe"))
                if (await Misc.GetHash("Bin\\CounterStrikeOnline2.exe", "sha1") == sha1)
                    return true;

            return await QQMail.DownloadFile("Bin\\CounterStrikeOnline2.exe", 2, code, sha1, key);
        }

        public static async Task<bool> GameUpdate()
        {
            int currentversion = Static.GetGameVersion();
            int latestversion = int.Parse(await StringFromMainServer("updates/game/latest.txt"));

            if (latestversion > currentversion)
            {
                for (int i = currentversion + 1; i <= latestversion; i++)
                {
                    string latestinfo = Misc.Decrypt(await StringFromMainServer($"updates/game/version/{i}.txt"));
                    string[] infoarray = Misc.SplitString(latestinfo);

                    string key = infoarray[0];
                    string code = infoarray[1];
                    string sha1 = infoarray[2];
                    string path = $"{Path.GetTempPath()}update_{i}.zip";

                    if (await QQMail.DownloadFile(path, 8, code, sha1, key))
                    {
                        if (await Zip.Extract(path, ".\\"))
                        {
                            File.Delete(path);
                            currentversion++;

                            Static.SetGameVersion(currentversion);
                            Main.Instance.Dispatcher.Invoke(new Action(delegate
                            {
                                Main.Instance.version.Content = LStr.Get("_version_info", Static.CVersion, currentversion);
                            }));
                        }
                        else
                        {
                            File.Delete(path);
                            return false;
                        }
                    }
                    else
                    {
                        File.Delete(path);
                        return false;
                    }
                }

                if (currentversion == latestversion)
                    return true;
            }
            else if (latestversion < currentversion)
            {
                Static.SetGameVersion(latestversion);
                Main.Instance.Dispatcher.Invoke(new Action(delegate
                {
                    Main.Instance.version.Content = LStr.Get("_version_info", Static.CVersion, latestversion);
                }));
                return true;
            }

            return true;
        }

        public static async Task<bool> OpenVpn()
        {
            string key = "c7cb3d63d064d9c6f5b84f6337616665438e4063356166651d1b545a015250550e054c55075303480c01505b18005e545e195156065300000c06545000506d657744040d6331284b425d1177e329947cea3938deba5b7c5437e78a084e50b53c";
            string code = "84ac5afe";
            string sha1 = "D648F219D20D59BD8F3A1A310FD3EB6B7B31D359";
            string path = Path.GetTempPath() + "OpenVPN.zip";

            if (await QQMail.DownloadFile(path, 2, code, sha1, key))
            {
                if (await Zip.Extract(path, "Bin"))
                {
                    File.Delete(path);
                    return true;
                }
            }

            File.Delete(path);
            return false;
        }

        /// <param name="install">set to 'false' to uninstall TAP-Windows driver.</param>
        public static async Task<bool> TapWindows(bool install = true)
        {
            string key = "cc9e5036d0338c97feed1e363736333475dd303635363334164e510507070b5757001d57000f021907520801180e03040a4c5507045406010558515700023c346720601b625f5d505c1643184f5f4320cba3f2f05de7ff18df256d9c73673640f64e7891";
            string code = "3a065634";
            string sha1 = "F8C2C2C668D1CC2CEC445DAA46510574C52F48A7";
            string path = Path.GetTempPath() + "TAP-Windows.zip";

            if (!Environment.Is64BitOperatingSystem)
            {
                key = "ca9e5232d4608995f8ed1832336536361db2363231653636104e50040806030750001b0b5556021b015455041c070f00024c0703570054070c570250025c25366120661f660c58525a16456d495d00184f0846262273eef50c6d0f69647116d1100de92b02b35d77";
                code = "5a621e66";
                sha1 = "1316D8C3390C395B551420E7256CDF1933D66B41";
                path = path.Replace("TAP-Windows", "TAP-Windows_x86");
            }

            if (await QQMail.DownloadFile(path, 2, code, sha1, key))
            {
                if (await Zip.Extract(path, "Bin\\OpenVPN"))
                {
                    File.Delete(path);
                    if (await ProgramHelper.StartAndGetExitCode("Bin\\OpenVPN\\TAP-Windows\\tapinstall.exe", install ? "install \"Bin\\OpenVPN\\TAP-Windows\\tap0901.inf\" tap0901" : "remove tap0901", true) == 0)
                    {
                        if (install)
                            Static.newadapterinstalled = true; // there should be only 1 adapter installed per instance.
                        return true;
                    }
                }
            }

            File.Delete(path);
            return false;
        }

        public static async Task<List<string>> PromoImage()
        {
            try
            {
                int index = int.Parse(await StringFromMainServer("image/images.txt"));
                List<string> list = new List<string>();
                for (int i = 0; i < index; i++)
                {
                    string file = (i < 10) ? $"0{i}.jpg" : $"{i}.jpg";
                    await FileFromMainServer(Path.GetTempPath() + "_" + file, "image/" + file);
                    list.Add(file);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string[]> BlackList(bool backup)
        {
            if (!backup)
            {
                try
                {
                    string text = await StringFromMainServer("word_blacklist.txt");
                    return Misc.SplitString(Misc.Decrypt(text));
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                string key = "9ccf526180328797aebc1f6167373834221e316165373834461f085051035d0054511c515d535e19570953044855000c061d04505c55010d5201530004522a34145f43053a555455005b5d081643164e0a4025ba8b24dac90991af0bdf3bc2b84769339642fde8";
                string code = "c01ae784";
                string sha1 = "DBEE13E2FD6AA19E6ABA0CFA8C245902F727CAD0";
                string path = Path.GetTempPath() + "word_blacklist.zip";

                if (await QQMail.DownloadFile(path, 2, code, sha1, key))
                {
                    if (await Zip.Extract(path, Path.GetTempPath()))
                    {
                        Misc.DecryptFile(Path.GetTempPath() + "word_blacklist.txt", Path.GetTempPath() + "word_blacklist.txt");
                        string[] list = File.ReadAllLines(Path.GetTempPath() + "word_blacklist.txt");

                        File.Delete(path);
                        File.Delete(Path.GetTempPath() + "word_blacklist.txt");
                        return list;
                    }
                }

                File.Delete(path);
            }

            return Array.Empty<string>();
        }

        public static async Task<string> OpenVpnServer(bool backup)
        {
            switch (Config.Server)
            {
                case "Shanghai":
                    return await OpenVpnServer_Shanghai(backup);
                default:
                    return null;
            }
        }

        public static async Task<string> OpenVpnServer_Shanghai(bool backup)
        {
            string path = Path.GetTempPath() + Path.GetRandomFileName();

            if (!backup)
            {
                string text = await StringFromMainServer("server/Shanghai.txt");
                if (!string.IsNullOrEmpty(text))
                {
                    File.WriteAllText(path, Misc.Decrypt(text));
                    return path;
                }
            }
            else
            {
                string key = "c6c66f65d3358997f4b51c6534303634827e3265363036341c16505d05080f010d0a1f5c50550e190d5851531b520657581405500e565202585c565c52023a346a51530b5158575d174d4a1122a602436073a2b46edb9650e29718c7883fb5a830";
                string code = "992e6064";
                //string sha1 = "963477594A90D158EBA064DBAE2AA2BE0F839C09";

                string text = await QQMail.DownloadString(code, key);
                if (!string.IsNullOrEmpty(text))
                {
                    File.WriteAllText(path, Misc.Decrypt(text));
                    return path;
                }
            }

            return null;
        }

        public static async Task<string> FileCheckReq()
        {
            string key = "9ac80266863d8b92a8bb4d66613834312b0162666338343140185a50535a515206054e070209071c510606044e000001571a51570500565552010057015a2431035e0f03005051520e4506174d425d4171cc76397b2480ae65b32a9b64bc13c43b0731e71d";
            string code = "e7cfc841";
            string sha1 = "FB155F181CB49F008449FD078427F55E3052817E";
            string path = Path.GetTempPath() + "filecheckreq.zip";
            string unzippath = Path.GetTempPath() + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            if (await QQMail.DownloadFile(path, 2, code, sha1, key))
            {
                if (await Zip.Extract(path, unzippath))
                {
                    File.Delete(path);
                    return unzippath;
                }
            }

            File.Delete(path);
            return string.Empty;
        }
    }
}