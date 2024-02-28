using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSO2_ComboLauncher
{
    static class Downloader
    {
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
            string resourcepath = "CSO2_ComboLauncher.Resource.OpenVPN.zip";

            using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
            {
                if (stream == null)
                    return false;

                if (await Zip.Extract(stream, "Bin"))
                    return true;
            }

            return false;
        }

        /// <param name="install">set to 'false' to uninstall TAP-Windows driver.</param>
        public static async Task<bool> TapWindows(bool install = true)
        {
            string resourcepath = "CSO2_ComboLauncher.Resource.TAP-Windows.zip";
            if (!Environment.Is64BitOperatingSystem)
                resourcepath = resourcepath.Replace("TAP-Windows", "TAP-Windows_x86");

            using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
            {
                if (stream == null)
                    return false;

                if (await Zip.Extract(stream, "Bin\\OpenVPN"))
                {
                    if (await ProgramHelper.StartAndGetExitCode("Bin\\OpenVPN\\TAP-Windows\\tapinstall.exe", install ? "install \"Bin\\OpenVPN\\TAP-Windows\\tap0901.inf\" tap0901" : "remove tap0901", true) == 0)
                    {
                        if (install)
                            Static.newadapterinstalled = true; // there should be only 1 adapter installed per instance.
                        return true;
                    }
                }
            }

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
                    string file = i.ToString("00") + ".jpg";
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
                string resourcepath = "CSO2_ComboLauncher.Resource.world_blacklist.zip";
                string path = Path.GetTempPath() + "word_blacklist.txt";

                using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
                {
                    if (stream == null)
                        return Array.Empty<string>();

                    if (await Zip.Extract(stream, Path.GetTempPath()))
                    {
                        Misc.DecryptFile(path, path);
                        string[] list = File.ReadAllLines(path);

                        File.Delete(path);
                        return list;
                    }
                }
            }

            return Array.Empty<string>();
        }

        public static async Task<string> OpenVpnServer(bool backup)
        {
            switch (Config.Instance.Server)
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
                string resourcepath = "CSO2_ComboLauncher.Resource.Shanghai.zip";

                using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
                {
                    if (stream == null)
                        return null;

                    if (await Zip.Extract(stream, Path.GetTempPath()))
                    {
                        Misc.DecryptFile(Path.GetTempPath() + "Shanghai.txt", path);
                        File.Delete(Path.GetTempPath() + "Shanghai.txt");
                        return path;
                    }
                }
            }

            return null;
        }
    }
}