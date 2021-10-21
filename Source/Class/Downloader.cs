using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
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
                        Process process = Process.GetCurrentProcess();
                        if (process.ProcessName.StartsWith("CSO2 Launcher V"))
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && del /q \"{process.ProcessName}.exe\" && start \"\" \"CSO2 Launcher V{latestversion}.exe\"", false, true);
                        else
                            ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && move /y \"CSO2 Launcher V{latestversion}.exe\" \"{process.ProcessName}.exe\" && start \"\" \"{process.ProcessName}.exe\"", false, true);

                        Environment.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show(LStr.Get("_self_checking_launcherupdate_failed", latestversion, logmessage), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        public static async Task<bool> GameClientUpdate()
        {
            string gameclientinfo = Misc.Decrypt(await StringFromMainServer("updates/game/gameclient.txt"));
            string[] infoarray = Misc.SplitString(gameclientinfo);

            string k = infoarray[0];
            string code = infoarray[1];
            string sha1 = infoarray[2];

            if (File.Exists("Bin\\CounterStrikeOnine2.exe"))
                if (await Misc.GetHash("Bin\\CounterStrikeOnine2.exe", "sha1") == sha1)
                    return true;

            return await QQMail.DownloadFile("Bin\\CounterStrikeOnine2.exe", 2, code, sha1, k);
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
            string[] k =
            {
                "21653338b621ecc884608c9b1564001856510a5c015751524c56525c0549060003521e5a0a05511a0454570e0006055456030b5b336f327811005d6e632a1c4d08153305",
                "24613032b426ef9981648f911763034955050803525b080749025306074e055e00021d530806504b51530600550504055756010a3168312914045e64612d1f1c0d11300f",
                "76356431b321edcdd330db921064011d04055252020102501b0c02500e490703520749535507071f005406020354070155035755366f337d46500a67662a1d485f45640c",
                "74326163b172bfcfd137dec01237531f0d0700070c00595419030407011a555505574c015600551d510253060656510252065007343c617f44570f3564794f4a5d42615e",
                "77383130e724bfc8d23d8e934461531852015306565105031a0f09535b4c5500535d1c085750021a53090853505353060f015502626a6178475d5f66322f4f4d5e48310d"
            };
            string[] code =
            {
                "ae383d27",
                "da021c1f",
                "65d16d32",
                "42ac47a0",
                "7810baa7"
            };
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
            string[] k =
            {
                "71666166d825c9cfd063dec54466041f0357555f50500e00185e5051034b025107514c04575e531d035e0550575f0408510259036269366474364c310b08525f42154f1c0b16360d",
                "266232308b269dc687678d93176550165a03530307065a5c4f5702515548565b04061f520304061455040a515554545806065006316a626d23321f67580b065615111c4a58156204",
                "2063366683769dca816689c51f35501a0207065009055750495400570e18565053021b075f545018560155535a565b06505a0e05393a626125331b31505b065a1310181c50456208",
                "20653739d82299ce8160889a4461541e535c005a5a5203014906050d074c520906541a5b5251531c515c0e5b5a540257055d550a626e666525351a6e0b0f025e131619430b11660c",
                "26633133df229ccf87668e904361511f005a07575d0307564f5005065d4c575253511c0a5c535b1d54000503510056075a005705656e636423331c640c0f075f15101f490c11630d"
            };
            string[] code =
            {
                "5fafbf60",
                "bb201eb9",
                "dc6f95b5",
                "de79baf1",
                "bc13eac0"
            };
            string sha1 = "846F3F00DDE29C7E7F5A4A9923F3713A0E8A3CFD";
            string path = Path.GetTempPath() + "TAP-Windows.zip";

            if (!Environment.Is64BitOperatingSystem)
            {
                k[0] = "2c356338e81a9c998130dc9b1636514952055b0a02025053490c505b001b575201054e010402014b5550000b52030507565406093025633225654e6f5958070913463c4008004d1c0d456305";
                k[1] = "71363966e81d9ecddc3386c51631531d0d0f0b00525400541406005f011c55010c5514040402001f090658530700030000075c073022616678661431595f055d4e45661e08074f485046395b";
                k[2] = "70333434e91a9bc7dd368b97173656170c0051070607015d15510752081b500b5c051956530401150d560c015052510c0b0a07013125646c79631963585800574f406b4c09004a4251433409";
                k[3] = "78653839e04fca9cd560879a1e63074c00500f0f010602021d570101004e015a085415580a55504e53510d095c52015006505b5d387035377135156e510d510c4716674100551b1959153804";
                k[4] = "7d643563ba4e9ac7d0618ac044625717045c075a0755565e185103515a4f51095105185a030601150c0602075656045e035405506271656c743418340b0c015742176a1b5a544b425c14355e";
                code[0] = "d5c806cf";
                code[1] = "969f01a2";
                code[2] = "834416d8";
                code[3] = "0e898c5c";
                code[4] = "5d5cbbe8";
                sha1 = "E7E064E346B45C45584B846445F001B703231DF8";
                path = Path.GetTempPath() + "TAP-Windows_x86.zip";
            }

            if (await QQMail.DownloadFile(path, 2, code, sha1, k))
            {
                if (await Zip.Extract(path, "Bin\\OpenVPN"))
                {
                    File.Delete(path);
                    if (await ProgramHelper.StartAndGetExitCode("Bin\\OpenVPN\\TAP-Windows\\tapinstall.exe", install ? "install \"Bin\\OpenVPN\\TAP-Windows\\tap0901.inf\" tap0901" : "remove tap0901", true) == 0)
                    {
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
                string[] k =
                {
                    "2165323888e09acf83608d9b1131571f5156010f065456004b55025d521c515350061f5a0257521d5307565906075054565c560b3723654709175667555d04530d095b4b431f1f5916650f",
                    "7665646688e7c8cfd460dbc51136051f02515203550f51071c515751541b0302525649070300001d07575d030502030403065455372437475e170039555a56535a090d1543184d59416559",
                    "253933308de59ccd873c8c931434511d52010256040100014f0c05050119570b560e1e090407541f535a02560707010203010507322663450d4b576f5058025109555a43461a195b12390e",
                    "22366361dce39ec78033dcc2453253175c0557005b04590d48025759501f550c00014e035706051557015b510553550b5c0501056320614f0a44073e015e005b0e5a0a12171c1b5115365e",
                    "24623865dee1c89a866787c64730054a00005e03515256574e065e57071d03525b53155c0406524855060c540405540056525c54612237120c105c3a035c5606080e5116151e4d0c136205"
                };
                string[] code =
                {
                    "fe2871e0",
                    "1edf7670",
                    "b93024c2",
                    "e6cac2a8",
                    "cb8ea07e"
                };
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
            switch (Config.CurrentServer.Name)
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
                string[] k =
                {
                    "cd996935d736ddc0ffea1a3530336263812134353233626317490000035701000a5119000603074e060356561f510751034b05540651070705550c5150516e63610e555b555b030a1c124c4126a72885e4c216df1a1056b1525a2dc810fcc91b03",
                    "cd996935d736ddc0ffea1a35303362638121343532336263174905570701065b055419050150564e060007021f0b5b00064b0450565057565004015407016e63610e555b555b030a1c124c4126a72885e4c216df1a1056b1525a2dc810fcc91b03",
                    "cd996935d736ddc0ffea1a35303362638121343532336263174904000a0a56560107190d5705514e065307011f0a0451504b560202565b5707550d040b516e63610e555b555b030a1c124c4126a72885e4c216df1a1056b1525a2dc810fcc91b03",
                    "cd996935d736ddc0ffea1a35303362638121343532336263174952060002015b5650195403055a4e060704571f0b555b534b0c0053015456505f510c01026e63610e555b555b030a1c124c4126a72885e4c216df1a1056b1525a2dc810fcc91b03",
                    "cd996935d736ddc0ffea1a35303362638121343532336263174950530a020005005519035105514e060052571f520153064b5050540352565457565303006e63610e555b555b030a1c124c4126a72885e4c216df1a1056b1525a2dc810fcc91b03"
                };
                string[] code =
                {
                    "2f4523bc",
                    "2f4523bc",
                    "2f4523bc",
                    "2f4523bc",
                    "2f4523bc"
                };
                string sha1 = "944AE6D6A422EA282334D2603C19FD22CFAB7831";

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
            string[] k =
            {
                "7d356262d4a8cd9ddd30ddc14061014d5e5055035255500615565755074c0703080d4f5a5502004f595006015455565a5a565a0066713304515907010e0450094a50134c1c08436205",
                "72343135d3f09d9ad2318e964739514a52000203030857041a0400575214575154501c0c565a54480e04070c530105510f075504612963035e585456095c000e4551401b1b5013650a",
                "70346263d0ad9fc8d031ddc0446453180501575451560003180c50005049550453014f010450001a0d5055535600545301565251627461515c5807000a01025c4751134d180d113708",
                "7535626180fac7cfd530ddc214330b1f09065b52540b5a531d500454571e0d0205024f030b000b1d0001565001565d025506045932233956595907025a565a5b4250134f485a49300d",
                "2636653382f0cccd8633da901639001d07545202080b07574e0057030014060355074852555b561f060f5d040701015005525552302932540a5a0050585c51591153141d4a5042325e"
            };
            string[] code =
            {
                "85bbfa3b",
                "7415a9ce",
                "54bcbda7",
                "05ba2390",
                "c6e30922"
            };
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