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
            if (threads == 1)
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

                string k = infoarray[0];
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
                    if (await QQMail.DownloadFile($"CSO2 Launcher V{latestversion}.exe", 2, code, sha1, k))
                    {
                        App.HideAllWindow();
                        if (Static.CurrentProcess.ProcessName.StartsWith("CSO2 Launcher V"))
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && del /q \"{Static.CurrentProcess.MainModule.FileName}\" && start \"\" \"CSO2 Launcher V{latestversion}.exe\"", false, true);
                        else
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && move /y \"CSO2 Launcher V{latestversion}.exe\" \"{Static.CurrentProcess.MainModule.FileName}\" && start \"\" \"{Static.CurrentProcess.MainModule.FileName}\"", false, true);

                        Environment.Exit(0);
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
            // GameUpdate() also do the job, I'm not sure this should be keeped, maybe delete?

            string gameclientinfo = Misc.Decrypt(await StringFromMainServer("updates/game/gameclient.txt"));
            string[] infoarray = Misc.SplitString(gameclientinfo);

            string k = infoarray[0];
            string code = infoarray[1];
            string sha1 = infoarray[2];

            if (File.Exists("Bin\\CounterStrikeOnline2.exe"))
                if (await Misc.GetHash("Bin\\CounterStrikeOnline2.exe", "sha1") == sha1)
                    return true;

            return await QQMail.DownloadFile("Bin\\CounterStrikeOnline2.exe", 2, code, sha1, k);
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

                    string k = infoarray[0];
                    string code = infoarray[1];
                    string sha1 = infoarray[2];
                    string path = $"{Path.GetTempPath()}update_{i}.zip";

                    if (await QQMail.DownloadFile(path, 8, code, sha1, k))
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
                {
                    Static.SetGameVersion(latestversion);
                    Main.Instance.Dispatcher.Invoke(new Action(delegate
                    {
                        Main.Instance.version.Content = LStr.Get("_version_info", Static.CVersion, latestversion);
                    }));
                    return true;
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
            string k = "cbc66d34d5378bc7f9b51f34323234644f83103430323464111600500803575c0d581c5753060249000155571d530d015514040606005002015a0203010a3f647b49545a66627a4a4e5041203133daf1344ec6cad90f1a3a9566044c2d80cb91";
            string code = "4914024d";
            string sha1 = "0101EE950077F7FEE93D2E5EA15F35781DB2FFF5";
            string path = Path.GetTempPath() + "OpenVPN.zip";

            if (await QQMail.DownloadFile(path, 2, code, sha1, k))
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

        /// <param name="install">if false, then uninstall TAP-Windows.</param>
        public static async Task<bool> TapWindows(bool install = true)
        {
            string k = "cfca5234dc3ddbc7fdb91c343b3864647689323439386464151a04050d01565208051f0c080f054904540003145a515c5518040c5d0e515d020d5650015d6b64647462196e510a005f42411a43511470b45a0d34e4daf81a4f6f78ad1acb155e3ebf0ec9";
            string code = "052498dd";
            string sha1 = "846F3F00DDE29C7E7F5A4A9923F3713A0E8A3CFD";
            string path = Path.GetTempPath() + "TAP-Windows.zip";

            if (!Environment.Is64BitOperatingSystem)
            {
                k = "9d9c016687618896afef4b66606437354ab0656662643735474c53565a5605015156485f51070718565700564f5d0301004e54030157550004025707075524353622354b350d59510d1416391a5c011b180a1572858453d624d739233a2fb351279364d161472acd";
                code = "bcefbd75";
                sha1 = "E7E064E346B45C45584B846445F001B703231DF8";
                path = Path.GetTempPath() + "TAP-Windows_x86.zip";
            }

            if (await QQMail.DownloadFile(path, 2, code, sha1, k))
            {
                if (await Zip.Extract(path, "Bin\\OpenVPN"))
                {
                    File.Delete(path);
                    if (await ProgramHelper.StartAndGetExitCode("Bin\\OpenVPN\\TAP-Windows\\tapinstall.exe", install ? "install \"Bin\\OpenVPN\\TAP-Windows\\tap0901.inf\" tap0901" : "remove tap0901", true) == 0)
                        return true;
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
                string k = "cb9b5b34d137dcc7f9e8163436326364754a383434326364114b0f0707055201075415040457064900070e571950560203490d56505352520100080d50017164430b4a506b500f05570f545d47464d1e5d142cdd64843c17c6e8d4dcfe96495bd8b86c4a477e6a";
                string code = "4d8442cd";
                string sha1 = "E950B65F73F28CECE8CAA42A3FECDC547E734C09";
                string path = Path.GetTempPath() + "word_blacklist.zip";

                if (await QQMail.DownloadFile(path, 2, code, sha1, k))
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

            return null;
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
                string k = "cf9c6b6581648b97fdef1865666134348324366564613434154c06060152075702551b5756070219045b075649590005544e055c50510c050457070757583834630b570b0309555d1e174e1170a1f6d765c534515fed3185e5859dd99b58cfe1b2";
                string code = "0c6eda44";
                string sha1 = "C0C2E355A602343B8C05B1D5E6ABBCFF39FBD582";

                string text = await QQMail.DownloadString(code, sha1, k);
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
            string k = "c8cf5462866687c7fabc1b62616338647906346263633864121f530754020c5054541801565459490351055a4e5b0b07041d540707000a50520857015b01286451595907000b5d075c4250134d195114239c4cb96270ad790acf5b1ac453e1162171127dfc";
            string code = "705bcc8d";
            string sha1 = "AC79DB0113951D3DFF6E78A730D9721641271F9F";
            string path = Path.GetTempPath() + "filecheckreq.zip";
            string unzippath = Path.GetTempPath() + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            if (await QQMail.DownloadFile(path, 2, code, sha1, k))
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