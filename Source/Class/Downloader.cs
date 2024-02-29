using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSO2_ComboLauncher
{
    static class Downloader
    {
        public static async Task<string> StringFromHttp(string link)
        {
            using (Web Web = new Web())
            {
                return await Web.Client.DownloadStringTaskAsync(link);
            }
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
            string latestinfo = Misc.Decrypt(await StringFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + "update/launcher/launcher.txt"), true);
            string[] infoarray = Misc.SplitString(latestinfo);

            string latestversion = infoarray[0];
            if (latestversion != Static.CVersion)
            {
                string md5 = infoarray[1];

                int logcount = int.Parse(infoarray[2]);
                string logmessage = "";
                for (int i = 3; i < (logcount + 3); i++)
                    logmessage += "- " + infoarray[i] + "\n";

                MessageBoxResult box = MessageBox.Show(LStr.Get("_self_checking_launcherupdate_needed", latestversion, logmessage), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (box == MessageBoxResult.Yes)
                {
                    if (await FileFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + $"update/launcher/CSO2 Launcher V{latestversion}.exe", $"CSO2 Launcher V{latestversion}.exe", 2, "md5", md5))
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
            string gameclientinfo = Misc.Decrypt(await StringFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + "update/gameclient.txt"), true);
            string[] infoarray = Misc.SplitString(gameclientinfo);

            string filemd5hash = infoarray[0];
            string subdomain = infoarray[1];
            string code = infoarray[2];
            string md5 = infoarray[3];
            string path = Path.GetTempPath() + "CounterStrikeOnline2.zip";

            if (File.Exists("Bin\\CounterStrikeOnline2.exe"))
                if (await Misc.GetHash("Bin\\CounterStrikeOnline2.exe", "md5") == filemd5hash)
                    return true;

            if (await Lanzou.DownloadFile(path, 2, subdomain, code, "md5", md5))
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

        public static async Task<bool> GameUpdate()
        {
            int currentversion = Static.GetGameVersion();
            int latestversion = int.Parse(Misc.Decrypt(await StringFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + "update/game/latest.txt"), true));
            if (latestversion > currentversion)
            {
                for (int i = currentversion + 1; i <= latestversion; i++)
                {
                    string latestinfo = Misc.Decrypt(await StringFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + $"update/game/version/{i}.txt"), true);
                    string[] infoarray = Misc.SplitString(latestinfo);

                    string from = infoarray[0];

                    string keyorsubdomain = infoarray[1];
                    string code = infoarray[2];
                    string md5 = infoarray[3];
                    string path = $"{Path.GetTempPath()}update_{i}.zip";

                    bool result = false;
                    switch (from.ToLower())
                    {
                        case "qqmail":
                            result = await QQMail.DownloadFile(path, 8, code, keyorsubdomain, "md5", md5);
                            break;
                        case "lanzou":
                            result = await Lanzou.DownloadFile(path, 4, keyorsubdomain, code, "md5", md5);
                            break;
                    }

                    if (result)
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
                string resourcepath = "CSO2_ComboLauncher.Resource.PromoImage.zip";

                using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
                {
                    if (stream == null)
                        return null;

                    if (await Zip.Extract(stream, Path.GetTempPath()))
                    {
                        int index = int.Parse(File.ReadAllText(Path.GetTempPath() + "images.txt"));
                        File.Delete(Path.GetTempPath() + "images.txt");

                        List<string> list = new List<string>();
                        for (int i = 0; i < index; i++)
                        {
                            string file = i.ToString("00") + ".jpg";
                            File.Copy(Path.GetTempPath() + file, Path.GetTempPath() + "_" + file, true);
                            File.Delete(Path.GetTempPath() + file);
                            list.Add(file);
                        }
                        return list;
                    }
                }
            }
            catch { }

            return null;
        }

        public static async Task<string[]> BlackList()
        {
            string resourcepath = "CSO2_ComboLauncher.Resource.word_blacklist.zip";
            string path = Path.GetTempPath() + "word_blacklist.txt";

            using (Stream stream = Static.ExecutingAssembly.GetManifestResourceStream(resourcepath))
            {
                if (stream == null)
                    return Array.Empty<string>();

                if (await Zip.Extract(stream, Path.GetTempPath()))
                {
                    Misc.DecryptFile(path, path, true);
                    string[] list = File.ReadAllLines(path);

                    File.Delete(path);
                    return list;
                }
            }

            return Array.Empty<string>();
        }

        public static async Task<string> OpenVpnServer()
        {
            string path = Path.GetTempPath() + Path.GetRandomFileName();

            string server = await StringFromHttp(Misc.Decrypt(Encoding.UTF8.GetString(Static.server), true) + $"server/{Config.Instance.Server}.txt");
            File.WriteAllText(path, Misc.Decrypt(server, true));
            return path;
        }
    }
}