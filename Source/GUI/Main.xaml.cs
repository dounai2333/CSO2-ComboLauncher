﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Management;
using System.Diagnostics;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;

namespace CSO2_ComboLauncher
{
    public partial class Main : Window
    {
        public static Main Instance { get; private set; }

        public static Logger Log { get; private set; }

        private static HttpListenerHelper Http { get; set; }

        private Config Config { get; set; }

        public static bool connecterror = false;

        public Main()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            InitializeComponent();

            LStr.LocalifyControl(mainGrid.Children);

            Static.SetupIcon();
            Static.icon.MouseClick += Icon_MouseClick;

            titletext.Text = Static.CWindow;
            version.Content = LStr.Get("_version_info", Static.CVersion, Static.GetGameVersion());

            Config = new Config();
            Config = Config.Instance;
        }

        private async void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Log = new Logger { LogDisplay = logger };

            // check if any necessary game dll files is missing
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_gamefiledll") + Static.AuthorAndLibraryOutput());
            string[] dllfiles =
            {
                "BugTrapU.dll", "client.dll", "d3dx9_33.dll", "datacache.dll", "engine.dll", "FileSystem_Stdio.dll", "inputsystem.dll", "MaterialSystem.dll", "Mss32.dll", "mssmp3.asi",
                "mssvoice.asi", "nmcogame.dll", "NPS.dll", "scenefilecache.dll", "server.dll", "shaderapidx9.dll", "SoundEmitterSystem.dll", "stdshader_dx9.dll", "StudioRender.dll",
                "tier0.dll", "unicode.dll", "vaudio_miles.dll", "vaudio_speex.dll", "vgui2.dll", "vguimatsurface.dll", "video_services.dll", "vphysics.dll", "vstdlib.dll", "xinput1_3.dll"
            };
            string missedfiles = "";
            for (int i = 0; i < dllfiles.Count(); i++)
                if (!File.Exists("Bin\\" + dllfiles[i]))
                    missedfiles = string.IsNullOrEmpty(missedfiles) ? dllfiles[i] : missedfiles + " - " + dllfiles[i];

            if (!string.IsNullOrEmpty(missedfiles))
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_self_checking_gamefiledll_failed", missedfiles), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                {
                    App.Exit(1);
                }
            }

            await Misc.Sleep(250);

            // check if game pkg files count is match the expected number
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_gamefilepkg") + Static.AuthorAndLibraryOutput());

            int CorrectPkgCount = 2058;
            string[] PkgFiles = Directory.GetFiles("Data", "*.pkg");

            if (PkgFiles.Count() < CorrectPkgCount)
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_self_checking_gamefilepkg_failed", PkgFiles.Count(), CorrectPkgCount), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                {
                    App.Exit(1);
                }
            }

            await Misc.Sleep(250);

            // check if game pkg files size is exactly 0 byte
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_pkg_file_size") + Static.AuthorAndLibraryOutput());

            int LengthWrongPkgCount = 0;
            List<string> LengthWrongPkgList = new List<string>();

            foreach (string path in PkgFiles)
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists && fi.Length == 0)
                {
                    LengthWrongPkgCount++;
                    LengthWrongPkgList.Add(path);
                }
            }

            if (LengthWrongPkgCount != 0)
            {
                string list = "";
                foreach (string path in LengthWrongPkgList)
                    list += path + "  ";
                list = list.Trim();

                MessageBoxResult box = MessageBox.Show(LStr.Get("_self_checking_pkg_file_size_failed", LengthWrongPkgCount, list), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                {
                    App.Exit(1);
                }
            }

            await Misc.Sleep(250);

            // check program itself for updates (ignored if 'No Unnecessary Checks' is checked)
            if (!Config.DisableSomeCheck)
            {
                Log.Clear();
                Log.Write(LStr.Get("_self_checking_launcherupdate") + Static.AuthorAndLibraryOutput());
                await Downloader.LauncherUpdate();

                await Misc.Sleep(250);
            }

            // check and download OpenVPN if necessary
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_openvpnfile") + Static.AuthorAndLibraryOutput());
            if (!Directory.Exists("Bin\\OpenVPN") || Directory.GetFiles("Bin\\OpenVPN").Count() != 5)
            {
                if (Directory.Exists("Bin\\OpenVPN"))
                    Directory.Delete("Bin\\OpenVPN", true);

                if (!await Downloader.OpenVpn())
                {
                    App.HideAllWindow();
                    MessageBox.Show(LStr.Get("_self_checking_openvpnfile_failed"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Exit(1);
                }
            }

            await Misc.Sleep(250);

            // check and download TAP-Windows and install if necessary
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_tapwindows") + Static.AuthorAndLibraryOutput());
            if (!await Misc.ResetNetAdapter(Static.netadapter))
            {
                if (!await Downloader.TapWindows())
                {
                    App.HideAllWindow();
                    MessageBox.Show(LStr.Get("_self_checking_tapwindows_failed"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Exit(1);
                }
            }

            await Misc.Sleep(250);

            // check Dhcp service status and enable it if necessary
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_dhcpclient") + Static.AuthorAndLibraryOutput());
            await Task.Run(() =>
            {
                using (ServiceController service = new ServiceController("Dhcp"))
                {
                    try
                    {
                        if (service.StartType != ServiceStartMode.Automatic)
                            using (ManagementObject servicemo = new ManagementObject("Win32_Service.Name=\"Dhcp\""))
                                servicemo.InvokeMethod("ChangeStartMode", new object[] { "Automatic" });

                        if (service.Status == ServiceControllerStatus.Stopped)
                            service.Start();
                        else if (service.Status == ServiceControllerStatus.Paused)
                            service.Continue();

                        // if timed out, TimeoutException is throw.
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(7.5));
                    }
                    catch
                    {
                        App.HideAllWindow();
                        MessageBox.Show(LStr.Get("_self_checking_dhcpclient_failed"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                        App.Exit(1);
                    }
                }
            });

            await Misc.Sleep(250);

            // enable HttpListener for image showing and future API
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_httplistener") + Static.AuthorAndLibraryOutput());
            if (HttpListener.IsSupported)
            {
                Http = new HttpListenerHelper(27482);
                Http.Start();
                Http.AddResponse("test", "ok", false);

                /*
                Http.AddResponse("username", Config.Username, false);
                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        Misc.Sleep(1000, false);
                        Http.RefreshResponse("username", Config.NoAutoLogin ? "" : Config.Username, false);
                    }
                });
                */
            }

            await Misc.Sleep(250);

            // unpack promotional images for let game showing it and text blacklist for banning insulting or inappropriate username
            Log.Clear();
            Log.Write(LStr.Get("_self_checking_unpack_resource") + Static.AuthorAndLibraryOutput());

            List<string> files = await Downloader.PromoImage();
            if (files != null && files.Count() >= 1)
                for (int i = 0; i < files.Count(); i++)
                    Http.AddResponse(files[i], Path.GetTempPath() + "_" + files[i], true);

            Static.blacklist = await Downloader.BlackList();

            await Misc.Sleep(250);

            await StartOpenVpn();
            Static.started = true;
        }

        public async Task StartOpenVpn()
        {
            Log.Clear();

            Log.Write(LStr.Get("_openvpn_message"), "red");
            Log.Write(LStr.Get("_open_source_link"));

            // reset status
            connecterror = false;
            MainButtonStatus(false);
            await OpenVpn.Kill(true);
            await Misc.ResetNetAdapter(Static.netadapter);

            bool localfile = false;
            string path = "";
            foreach (string file in Directory.GetFiles(".", "*.ovpn"))
            {
                string content = File.ReadAllText(file);
                if (content.Contains("remote "))
                {
                    localfile = true;
                    path = file;
                    Log.Write(LStr.Get("_local_key_file", path.Replace(".\\", "")));
                    break;
                }
            }

            if (path == "")
            {
                Log.Write(LStr.Get("_download_server_info"));
                path = await Downloader.OpenVpnServer();
                if (string.IsNullOrEmpty(path))
                {
                    Log.Write(LStr.Get("_download_server_info_failed"), "red");
                    connecterror = true;
                    MainButtonStatus(true);
                    return;
                }
            }

            OpenVpn.Start("Bin\\OpenVPN\\openvpn.exe", path);
            Log.Write(LStr.Get("_start_openvpn_and_connect"));

            int count = 0;
            OpenVpn.OnConnected += () =>
            {
                Log.Write(LStr.Get("_connect_to_server_halfway"));
                if (!localfile)
                    File.Delete(path);
                count = 0;
            };

            while (!OpenVpn.Process.HasExited && !OpenVpn.IsProgressCompleted)
            {
                await Misc.Sleep(100);
                count++;
                if (count >= 150) // 15sec
                {
                    Log.Write(LStr.Get("_connect_to_server_failed"), "red");
                    await OpenVpn.Kill(true);
                    if (!localfile)
                        File.Delete(path);
                    connecterror = true;
                    MainButtonStatus(true);
                    return;
                }
            }

            if (OpenVpn.Process.HasExited)
            {
                if (OpenVpn.ExitedWithFatalError)
                {
                    if (OpenVpn.NoTapWindowsExist || OpenVpn.NoTapWindowsAvailable)
                    {
                        if (!Static.newadapterinstalled && await Downloader.TapWindows())
                        {
                            Reconnect_Click(null, null);
                            return;
                        }

                        Log.Write(LStr.Get("_connect_to_server_failed_openvpnexited_fatalerror_" + (OpenVpn.NoTapWindowsExist ? "notapwindows" : "alltapwindowsinuse")), "red");
                    }
                    else
                    {
                        Log.Write(LStr.Get("_connect_to_server_failed_openvpnexited_fatalerror"), "red");
                    }
                }
                else
                {
                    Log.Write(LStr.Get("_connect_to_server_failed_openvpnexited"), "red");
                }

                connecterror = true;
                MainButtonStatus(true);
                return;
            }

            MainButtonStatus(true);
            if (Config.DisableSomeCheck)
            {
                Log.Write(LStr.Get("_connect_to_server_success", "", ""));
            }
            else
            {
                PingReply ping = await new Ping().SendPingAsync(Static.gameserver, 2000);
                int pingdelay = (int)ping.RoundtripTime;
                if (ping.Status == IPStatus.Success)
                {
                    int tcpingdelay = await Misc.TCPing(Static.gameserver, 30001);
                    if (tcpingdelay == -1)
                    {
                        Log.Write(LStr.Get("_connect_to_server_failed_tcping_no_response"), "red");
                        //OpenVpn.Kill(true); // maybe false possibility
                        //connecterror = true;
                        return;
                    }
                    else if (pingdelay <= 1)
                    {
                        pingdelay = tcpingdelay;
                    }
                }

                int online = -1; // don't count current instance.
                await Task.Run(() =>
                {
                    using (Web Web = new Web())
                    {
                        try
                        {
                            // stop being curious, it got reason and necessary to hide some basic info (keep it as a secret even it's easy to decrypt)
                            string temp1 = Web.Client.DownloadString(Misc.Decrypt(Misc.UnicodeToString("\\u0059\\u0055\\u0068\\u0053\\u004d\\u0047\\u004e\\u0045\\u0062\\u0033\\u005a\\u004d\\u0065\\u006b\\u0056\\u0033\\u0054\\u0047\\u0070\\u006e\\u0064\\u0055\\u0031\\u0044\\u004e\\u0048\\u0068\\u004d\\u004d\\u0044\\u006c\\u0033\\u0057\\u006c\\u0063\\u0031\\u0056\\u0031\\u0056\\u0046\\u004e\\u0055\\u004a\\u0056\\u0052\\u0057\\u0074\\u0031\\u0059\\u0030\\u0064\\u006f\\u0064\\u0031\\u0041\\u007a\\u0054\\u006d\\u0078\\u005a\\u004d\\u0030\\u0070\\u0073\\u005a\\u0045\\u0051\\u0078\\u0061\\u0046\\u006c\\u0074\\u0054\\u006d\\u0074\\u004e\\u0056\\u0045\\u006c\\u0036\\u0054\\u006b\\u0052\\u0056\\u0065\\u0045\\u0031\\u006e\\u0050\\u0054\\u0030\\u003d")));
                            dynamic json = Json.Parse(temp1);

                            for (int i = 0; i < json.Length; i++)
                            {
                                dynamic sub = json[i];
                                List<string> array = Json.ReadArray(sub["list"]);
                                online += array.Count();
                            }
                        }
                        catch { }
                    }
                });

                Log.Write(LStr.Get("_connect_to_server_success", LStr.Get("_server_delay", pingdelay), (online == -1) ? "" : LStr.Get("_player_count", online)));
            }
        }

        private bool CantLogin()
        {
            string name = Config.Username;
            string pw = Config.Password;
            if (connecterror)
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_start_connect_failed_hint"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                    return true;
            }

            if (!Config.CheckLangExists(Config.GameLanguage))
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_start_no_language_file"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                    return true;
            }

            if (Encoding.Default.CodePage != 936) // 中文(简体, 中国)
            {
                if (Encoding.UTF8.GetByteCount(name) != name.Length)
                {
                    MessageBox.Show(LStr.Get("_start_codepage_wrong"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }
            }

            if (!Config.NoAutoLogin)
            {
                for (int i = 0; i < name.Length; i++)
                {
                    if (name[i] == 0x20 || name[i] == 0x200e || name[i] == 0x202e || name[i] == 0x2067 || name[i] == 0x3000 || name[i] == 0x3164
                        || name[i] == 0xe779 || name[i] == 0xe77a || name[i] == 0xe77c || name[i] == 0xe781 || name[i] == 0xe782 || name[i] == 0xe783 || name[i] == 0xe784 || name[i] == 0xe812)
                    {
                        MessageBox.Show(LStr.Get("_start_name_blacklist"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true;
                    }
                }

                if (Static.blacklist.Count() != 1)
                {
                    for (int i = 0; i < Static.blacklist.Count(); i++)
                    {
                        if (name.ToLower().Contains(Static.blacklist[i].ToLower()))
                        {
                            MessageBox.Show(LStr.Get("_start_name_blacklist"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return true;
                        }
                    }
                }

                if (!pw.All(char.IsLetterOrDigit))
                {
                    MessageBox.Show(LStr.Get("_start_password_wrongtext"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }

                bool noname = name.Length <= 0;
                bool nopw = pw.Length <= 0;
                if (noname || nopw)
                {
                    if (noname && nopw)
                    {
                        MessageBox.Show(LStr.Get("_start_no_name_password"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Error);
                        return true;
                    }
                    else if (noname)
                    {
                        MessageBox.Show(LStr.Get("_start_no_name"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(LStr.Get("_start_no_password"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true;
                    }
                }

                string tempname = Misc.ChangeTextEncoding(Misc.ChangeTextEncoding(name, "UTF-8", "GBK"), "GBK", "UTF-8");
                if (tempname != name)
                {
                    MessageBox.Show(LStr.Get("_start_name_convertfailed", tempname), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }
            return false;
        }

        private async void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            if (CantLogin())
                return;

            MainButtonStatus(false, false);

            if (!Config.DisableSomeCheck)
            {
                try
                {
                    await Downloader.GameClientUpdate();
                }
                catch { }
                try
                {
                    await Downloader.GameUpdate();
                }
                catch { }
            }

            string args = $"-masterip {Static.gameserver} ";
            args += $"-lang {Config.GameLanguage} ";
            if (!Config.NoAutoLogin && !connecterror)
                args += $"-username \"{Misc.ChangeTextEncoding(Config.Username, "UTF-8", "GBK")}\" -password \"{Config.Password}\" ";
            if (Config.EnableConsole)
                args += "-enableconsole ";
            args += Config.CustomArgs;

            Process process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "Bin\\CounterStrikeOnline2.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Bin")
                }
            };

            process.OutputDataReceived += (_, ee) =>
            {
                try
                {
                    if (ee.Data.Contains("Error 12002 has occurred."))
                    {
                        // when game exiting this problem almost happens 100%, so force to kill it and make launcher response normally.
                        process.Kill();
                    }
                }
                catch { }
            };

            process.Exited += (_, __) =>
            {
                MainButtonStatus(true);

                Dispatcher.Invoke(new Action(delegate
                {
                    Icon_MouseClick(null, null);
                }));

                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();

            MainButtonStatus(false, false, false);
            Min_Click(null, null);
        }

        private /*async*/ void Verify_Click(object sender, RoutedEventArgs e)
        {
            /*
            if ((string)verify.Content == LStr.Get("_file_check_cancel"))
            {
                verify.IsEnabled = false;
                verify.Content = LStr.Get("_verify_file");
                return;
            }
            else
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_file_check_confirm"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (box == MessageBoxResult.Yes)
                {
                    MainButtonStatus(false, false);
                    OpenVpn.Kill();

                    string wrongfiles = string.Empty;
                    string missingfiles = string.Empty;
                    bool shouldend = false;

                    await Task.Run(async () =>
                    {
                        Log.Clear();
                        Log.Write(LStr.Get("_file_check_downloading_needed_file"));

                        string[][] hasheslist =
                        {
                            Misc.SplitString(Misc.Decrypt(await Downloader.StringFromMainServer("verify_hashes_Bin.txt"))),
                            Misc.SplitString(Misc.Decrypt(await Downloader.StringFromMainServer("verify_hashes_custom.txt"))),
                            Misc.SplitString(Misc.Decrypt(await Downloader.StringFromMainServer("verify_hashes_Data.txt")))
                        };

                        Dispatcher.Invoke(new Action(delegate
                        {
                            verify.IsEnabled = true;
                            verify.Content = LStr.Get("_file_check_cancel");
                        }));

                        // todo: 多线程 Bin用1个线程 Data用2个线程 custom用4个线程
                        // 由当前线程负责每500毫秒输出检查过程 其他线程修改变量报告状态

                        for (int i = 0; i < hasheslist.Count(); i++)
                        {
                            int allfilescount = hasheslist[i].Count() - 1; // the last empty line should be ignored.
                            for (int j = 0; j < allfilescount; j++)
                            {
                                string content = "";
                                Dispatcher.Invoke(new Action(delegate { content = (string)verify.Content; }));
                                if (content == LStr.Get("_verify_file"))
                                {
                                    shouldend = true;
                                    break;
                                }

                                string[] fileinfo = hasheslist[i][j].Split(new string[] { " => " }, StringSplitOptions.None);
                                string file = fileinfo[0];
                                string md5 = fileinfo[1];
                                FileInfo fi = new FileInfo(file);

                                if (fi.Exists)
                                {
                                    Log.Clear();
                                    Log.Write(LStr.Get("_file_check_progress", j, allfilescount)
                                        + "\n" + LStr.Get("_file_check_progress_file", file, Misc.ConvertByteTo(fi.Length, "best", true)));

                                    if (await Misc.GetHash(file, "md5") != md5)
                                        wrongfiles += file + Environment.NewLine;

                                    await Misc.Sleep(1);
                                }
                                else
                                {
                                    missingfiles += file + Environment.NewLine;
                                }
                            }
                        }
                    });

                    if (!shouldend)
                    {
                        verify.IsEnabled = false;
                        verify.Content = LStr.Get("_verify_file");

                        Log.Clear();
                        Log.Write(LStr.Get("_file_check_done_check_messagebox"));

                        if (!string.IsNullOrEmpty(wrongfiles) || !string.IsNullOrEmpty(missingfiles))
                        {
                            string time = DateTime.Now.ToString("yyyy/MM/dd, HH:mm:ss");
                            string timeo = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");

                            string text = LStr.Get("_file_check_file_message_text", time) + Environment.NewLine + Environment.NewLine;
                            if (!string.IsNullOrEmpty(missingfiles))
                            {
                                text += LStr.Get("_file_check_file_missing_text") + Environment.NewLine + missingfiles;
                            }
                            if (!string.IsNullOrEmpty(wrongfiles))
                            {
                                if (!string.IsNullOrEmpty(missingfiles))
                                    text += Environment.NewLine;
                                text += LStr.Get("_file_check_hash_notmatch_text") + Environment.NewLine + wrongfiles;
                            }

                            string file = LStr.Get("_file_check_filename", timeo);
                            File.WriteAllText(file, text, Encoding.UTF8);
                            MessageBox.Show(LStr.Get("_file_check_file_error_detected", file), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show(LStr.Get("_file_check_file_all_good"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    Reconnect_Click(null, null);
                }
            }
            */
        }

        private async void Repair_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult box = MessageBox.Show(LStr.Get("_auto_repair_warn"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (box == MessageBoxResult.Yes)
            {
                MainButtonStatus(false, false);
                Log.Clear();

                _ = Task.Run(() =>
                {
                    Log.Write(LStr.Get("_auto_repair_running"), "", false);
                    while (true)
                    {
                        Misc.Sleep(500, false);
                        Log.Write(".", "", false);
                    }
                });

                OpenVpn.KillAllRelated();

                await Misc.ResetNetAdapter(Static.netadapter);
                await Downloader.TapWindows(false);

                if (Directory.Exists("Bin\\OpenVPN"))
                    Directory.Delete("Bin\\OpenVPN", true);

                try
                {
                    await Downloader.GameClientUpdate();
                }
                catch { }

                App.HideAllWindow();

                Config.DisableSomeCheck = false;
                Config.EnableConsole = false;
                Config.SaveConfig();

                Static.logfile.Dispose();
                File.Delete(Static.Log);

                ProgramHelper.CmdCommand($"timeout /nobreak /t 2 && start \"\" \"{Static.CurrentProcess.MainModule.FileName}\"", false, true);
                App.Exit(0);
            }
        }

        private async void LauncherUpdate_Click(object sender, RoutedEventArgs e)
        {
            MainButtonStatus(false, false);

            int totalchecks = 3;
            int passedcheck = 0;

            try
            {
                await Downloader.LauncherUpdate();
                passedcheck++;
            }
            catch { }
            try
            {
                if (await Downloader.GameClientUpdate())
                    passedcheck++;
            }
            catch { }
            try
            {
                if (await Downloader.GameUpdate())
                    passedcheck++;
            }
            catch { }

            Log.Write(LStr.Get("_update_check_passed", totalchecks, passedcheck));
            MainButtonStatus(true);
        }

        private async void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            await StartOpenVpn();
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Config.ShowDialog();
        }

        private void QQqun_Click(object sender, RoutedEventArgs e)
        {
            QQGroup.Instance.ShowDialog();
        }

        private void Copyright_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(LStr.Get("_copyright"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void MainButtonStatus(bool enable, bool cfgbutton = true, bool closebutton = true)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                btncfg.IsEnabled = cfgbutton;

                close.IsEnabled = closebutton;

                btngo.IsEnabled = enable;
                Config.ServerInfo.IsEnabled = enable;
                reconnect.IsEnabled = enable;
                launcherupdatecheck.IsEnabled = enable;
                verify.IsEnabled = enable;
                manualfix.IsEnabled = enable;
            }));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Icon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Visibility = Visibility.Visible;
            Static.icon.Visible = false;
        }

        private void Min_Click(object sender, MouseButtonEventArgs e)
        {
            if (close.IsEnabled)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                Visibility = Visibility.Hidden;
                Static.icon.Visible = true;
            }
        }

        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            App.Exit(0);
        }

        private void Main_Closing(object sender, CancelEventArgs e)
        {
            App.Exit(0);
        }

        public class Logger
        {
            public RichTextBox LogDisplay;

            public async void Write(object text, string color = "", bool newlineatend = true)
            {
                try
                {
                    if (string.IsNullOrEmpty(text.ToString()))
                        text = "(NULL)";
                }
                catch
                {
                    text = "(PARSING ERROR)";
                }

                await LogDisplay.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((Paragraph)LogDisplay.Document.Blocks.First()).Inlines.Add(new Run { Text = $"{text}{(newlineatend ? Environment.NewLine : "")}", Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(string.IsNullOrEmpty(color) ? "black" : color) });
                    LogDisplay.ScrollToEnd();
                }));
            }

            public void Write()
            {
                Log.Write(Environment.NewLine, "", false);
            }

            public void WriteToFile(object text)
            {
                using (StreamWriter sw = new StreamWriter(Static.logfile, Encoding.UTF8, 4096, true))
                {
                    sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"] {text}");
                }
            }

            public async void Clear()
            {
                await LogDisplay.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LogDisplay.SelectAll();
                    LogDisplay.Selection.Text = "⁧";
                }));
            }
        }
    }
}