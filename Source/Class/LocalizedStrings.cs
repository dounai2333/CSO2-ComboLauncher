using System.Threading;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CSO2_ComboLauncher
{
    static class LStr
    {
        public static string localLang = "english";

        private static Dictionary<KeyValuePair<string, string>, string> lStrings = new Dictionary<KeyValuePair<string, string>, string>();

        public static void InitLocalizedStrings()
        {
            localLang = GetSystemLang();

            // App strings
            if (true)
            {
                AddKeyWords("english", "_wrong_folder", "Please place this launcher to the cso2 root folder!\nroot folder = having 'Bin', 'custom' and 'Data' exists.");
                AddKeyWords("schinese", "_wrong_folder", "请将启动器放置于cso2根目录下!\n同时拥有Bin、custom和Data三个文件夹的就是'根目录'.");
                AddKeyWords("tchinese", "_wrong_folder", "請將啟動器放置於cso2根目錄下!\n放有Bin、custom和Data的資料夾就是'根目錄'.");

                AddKeyWords("english", "_tier0_broken", "Your tier0.dll file is broken!\nYou have to fix it before start this program.");
                AddKeyWords("schinese", "_tier0_broken", "你的tier0.dll文件已损坏!\n你必须先修复这个问题才能启动此程序.");
                AddKeyWords("tchinese", "_tier0_broken", "你的tier0.dll文件已被破壞!\n你必須先修復這個問題才能啟動程式.");

                AddKeyWords("english", "_net_framework_lower_version", "Your .NET Framework version is lower than 4.7.2!\nTo avoid compatibility issues, you must install .NET Framework 4.7.2 or newer version.\n\nClick 'Yes' to open official website to download the .NET Framework 4.7.2.\nClick 'No' to ignore this and keep try run this program.");
                AddKeyWords("schinese", "_net_framework_lower_version", "你的.NET Framework版本低于4.7.2!\n为避免兼容性问题, 你必须安装.NET Framework 4.7.2或更新的版本.\n\n点击'是'打开官网以下载.NET Framework 4.7.2.\n点击'否'忽略并继续尝试运行此程序.");
                AddKeyWords("tchinese", "_net_framework_lower_version", "你的.NET Framework版本低於4.7.2!\n為了避免兼容性問題, 你必須安裝.NET Framework 4.7.2或更新的版本.\n\n點擊'是'打開官方網站以下載.NET Framework 4.7.2.\n點擊'否'忽略並繼續嘗試運行此程序.");

                AddKeyWords("english", "_no_multi_start", "Another launcher is already running, this program will not start!");
                AddKeyWords("schinese", "_no_multi_start", "另一个启动器已在运行中, 此程序将不会运行!");
                AddKeyWords("tchinese", "_no_multi_start", "另一個啟動器已經在運行著, 這個程序將不會啟動!");

                AddKeyWords("english", "_no_network_connection", "You need a working network connection to start this program.");
                AddKeyWords("schinese", "_no_network_connection", "你需要一个正常的网络才能启动此程序.");
                AddKeyWords("tchinese", "_no_network_connection", "你需要一個正常的網路才能啟動這個程式.");

                AddKeyWords("english", "_error_msg", "OH NO! The software encountered a fatal error!");
                AddKeyWords("schinese", "_error_msg", "哦不!程序发生了一个致命错误!");
                AddKeyWords("tchinese", "_error_msg", "哦天哪!程序發生了一個致命的錯誤!");

                AddKeyWords("english", "_error_time", "Error time: ");
                AddKeyWords("schinese", "_error_time", "错误时间: ");
                AddKeyWords("tchinese", "_error_time", "出錯時間: ");

                AddKeyWords("english", "_error_code", "Error code: ");
                AddKeyWords("schinese", "_error_code", "错误代码: ");
                AddKeyWords("tchinese", "_error_code", "錯誤代碼: ");

                AddKeyWords("english", "_error_info", "Error information:");
                AddKeyWords("schinese", "_error_info", "错误信息:");
                AddKeyWords("tchinese", "_error_info", "錯誤訊息:");

                AddKeyWords("english", "_error_method", "Error method:");
                AddKeyWords("schinese", "_error_method", "错误方法:");
                AddKeyWords("tchinese", "_error_method", "錯誤方法:");

                AddKeyWords("english", "_error_hint", "Some related files have been placed in the errorminidumps folder.");
                AddKeyWords("schinese", "_error_hint", "如果您需要帮助,请记得截图此页面!\n此外,在errorminidumps文件夹内生成了一些错误的相关资料!");
                AddKeyWords("tchinese", "_error_hint", "程序保留了一些訊息生成在了errorminidumps資料夾內,請記得查看!");
            }

            // Config strings
            if (true)
            {
                // GUI start
                AddKeyWords("english", "_username", "Nickname:");
                AddKeyWords("schinese", "_username", "游戏名称:");
                AddKeyWords("tchinese", "_username", "遊戲名稱:");

                AddKeyWords("english", "_password", "Password:");
                AddKeyWords("schinese", "_password", "登录密码:");
                AddKeyWords("tchinese", "_password", "登入密碼:");

                AddKeyWords("english", "_language", "Language:");
                AddKeyWords("schinese", "_language", "游戏语言:");
                AddKeyWords("tchinese", "_language", "遊戲語言:");

                AddKeyWords("english", "_save_hint", "Ps: Click 'X' To Save");
                AddKeyWords("schinese", "_save_hint", "注: 点击上方的'X'保存");
                AddKeyWords("tchinese", "_save_hint", "註: 點擊上方的'X'保存");

                AddKeyWords("english", "_custom_args", "Custom Startup Arguments");
                AddKeyWords("schinese", "_custom_args", "自定义启动项");
                AddKeyWords("tchinese", "_custom_args", "自訂義啟動參數");

                AddKeyWords("english", "_launcher_settings", "Launcher Settings");
                AddKeyWords("schinese", "_launcher_settings", "启动器设置");
                AddKeyWords("tchinese", "_launcher_settings", "啟動器設定");

                AddKeyWords("english", "_disable_unnecessary_check", "No Unnecessary Checks");
                AddKeyWords("schinese", "_disable_unnecessary_check", "关闭不必要的检查");
                AddKeyWords("tchinese", "_disable_unnecessary_check", "關閉不是必須的檢查");

                AddKeyWords("english", "_game_settings", "Game Settings");
                AddKeyWords("schinese", "_game_settings", "游戏设置");
                AddKeyWords("tchinese", "_game_settings", "遊戲設定");

                AddKeyWords("english", "_no_auto_login", "Disable Auto Login");
                AddKeyWords("schinese", "_no_auto_login", "关闭自动登录");
                AddKeyWords("tchinese", "_no_auto_login", "關閉自動登入");

                AddKeyWords("english", "_enable_console", "Enable Console");
                AddKeyWords("schinese", "_enable_console", "启用控制台");
                AddKeyWords("tchinese", "_enable_console", "開啟遊戲控制台");
                // GUI end

                AddKeyWords("english", "_user_agreement", "Before you use this program, you must allow this program to do couple things in your PC:\n- Install OpenVPN (for virtual LAN)\n- Install TAP-Windows (network adapter that OpenVPN needed)\n- Scan Dhcp service status (enable it if necessary)\n- Modify firewall exception list (add game client and program itself)\n\nOpenVPN may read your system information to generate a unique key to connect to the server.\nWhen you have connected to the server you will be exposed to a virtual LAN network which everybody in the same network can access your PC.\nWe highly recommended enabling your firewall or keep anti-virus open to avoid being affected by malicious activities in the network!\n\nIf you do not agree with any of them, you can click 'No' to exit program, your PC will keep untouched.\nClicking 'Yes' means that you agree to all the above content and assume possible potential risks.\nyou should be responsible for the security of your own PC!");
                AddKeyWords("schinese", "_user_agreement", "在你使用此程序之前, 你必须允许程序在你的电脑上做一些事情:\n- 安装OpenVPN (建立虚拟局域网)\n- 安装TAP-Windows (OpenVPN所需要的网络适配器)\n- 扫描Dhcp服务状态 (如有需要则启用)\n- 修改防火墙例外列表 (添加游戏客户端和程序自身)\n\nOpenVPN可能会收集你电脑的信息并生成唯一标识密钥用于连接服务器.\n当你连接服务器后, 你将暴露在一个虚拟局域网中, 任何人都可以通过此网络访问你的电脑.\n我们强烈推荐你启用防火墙或保持杀软开启以避免受到网络中恶意活动的影响!\n\n如果你不同意其中任何一条, 你可以点击'否'退出程序, 你的电脑将保持原状.\n点击'是'即代表你同意以上所有内容并愿意承担可能存在的风险.\n你应对自己的电脑安全负责!");
                AddKeyWords("tchinese", "_user_agreement", "在你使用此程式之前, 你必須允許程式在你的電腦上做一些事情:\n- 安裝OpenVPN (建立虛擬局域網)\n- 安裝TAP-Windows (OpenVPN所需要的網路適配器)\n- 掃描Dhcp服務狀態 (如有需要則啟用)\n- 修改防火墻例外列表 (添加遊戲客戶端和程式自身)\n\nOpenVPN可能會收集你電腦的信息並生成唯一標識密鑰用來連接伺服器.\n當你連接伺服器後, 你將暴露在一個虛擬局域網中, 任何人都可以通過該網路訪問你的電腦.\n我們強烈推薦你啟用防火墻或讓殺毒軟體一直開著以避免受到網路中惡意活動的影響!\n\n如果你不同意其中任何一條, 你可以點擊'否'退出程式, 你的電腦將保持原狀.\n點擊'是'就代表你同意以上所有內容並願意承擔可能存在的風險.\n你應該對自己的電腦安全負責!");

                AddKeyWords("english", "_lang_file_not_exist", "The file for the currently selected language could not be found, please change to other language, otherwise you will hit errors when launching the game!\n\nIf you want to use the language of your choice, please download the files by yourself.");
                AddKeyWords("schinese", "_lang_file_not_exist", "无法找到当前选择的语言的文件, 请修改语言, 否则启动游戏将会导致报错!\n\n如果你希望使用这个语言,请自行寻找相关的语言包!");
                AddKeyWords("tchinese", "_lang_file_not_exist", "無法找到當前選擇的語言的文件, 請修改語言, 強行開始遊戲將會導致出現錯誤!\n\n如果你希望使用這個語言,請自己尋找相關的語言文件!");

                AddKeyWords("english", "_enable_console_hint", "Console is only for debugging!\nYou should not enable it if you don't know what it is.\n\nDon't report any bug after you used console, it's 'normal'.\nIf you cannot close console window:\n- Click 'X' of the console window\n- execute 'toggleconsole' command\n\nConfirm to enable?");
                AddKeyWords("schinese", "_enable_console_hint", "控制台仅供调试使用!\n如果你不知道它是什么, 那就别勾选.\n\n使用控制台后, 不要汇报任何BUG, 都是'正常现象'.\n如果你无法关闭控制台窗口:\n- 点击控制台窗口的'X'\n- 执行'toggleconsole'命令\n\n是否确定勾选?");
                AddKeyWords("tchinese", "_enable_console_hint", "控制台僅為調試時使用!\n如果你不知道它是什麼, 那就不要勾選.\n\n使用控制台後, 不要匯報任何BUG, 這都是'正常現象'.\n-如果你無法關閉控制台窗口:\n- 點擊控制台窗口的'X'\n- 執行'toggleconsole'命令\n\n是否確定勾選?");
            }

            // QQGroup strings
            if (true)
            {
                AddKeyWords("english", "_join_qqqun", "Join Our QQ Group");
                AddKeyWords("schinese", "_join_qqqun", "加入我们的QQ群");
                AddKeyWords("tchinese", "_join_qqqun", "加入我們的QQ群組");

                AddKeyWords("english", "_copy", "Copy");
                AddKeyWords("schinese", "_copy", "复制");
                AddKeyWords("tchinese", "_copy", "複製");

                AddKeyWords("english", "_copied", "Copied!");
                AddKeyWords("schinese", "_copied", "已复制!");
                AddKeyWords("tchinese", "_copied", "已複製!");

                AddKeyWords("english", "_oneclick", "Join Now");
                AddKeyWords("schinese", "_oneclick", "一键加群");
                AddKeyWords("tchinese", "_oneclick", "一鍵加入");

                AddKeyWords("english", "_group_1", "Our group 1");
                AddKeyWords("schinese", "_group_1", "我们的1群");
                AddKeyWords("tchinese", "_group_1", "我們的1群");

                AddKeyWords("english", "_group_2", "Our group 2");
                AddKeyWords("schinese", "_group_2", "我们的2群");
                AddKeyWords("tchinese", "_group_2", "我們的2群");

                AddKeyWords("english", "_group_3", "Our group 3");
                AddKeyWords("schinese", "_group_3", "我们的3群");
                AddKeyWords("tchinese", "_group_3", "我們的3群");
            }

            // MainWindow strings
            if (true)
            {
                // GUI start
                AddKeyWords("english", "_log", "Log");
                AddKeyWords("schinese", "_log", "日志");
                AddKeyWords("tchinese", "_log", "日誌");

                AddKeyWords("english", "_start", "Start Game");
                AddKeyWords("schinese", "_start", "开始游戏");
                AddKeyWords("tchinese", "_start", "開始遊戲");

                AddKeyWords("english", "_config", "Settings");
                AddKeyWords("schinese", "_config", "设置");
                AddKeyWords("tchinese", "_config", "設定");

                AddKeyWords("english", "_reconnect", "Reconnect Server");
                AddKeyWords("schinese", "_reconnect", "重新连接服务器");
                AddKeyWords("tchinese", "_reconnect", "重新連接伺服器");

                AddKeyWords("english", "_content_repair", "Auto Repair");
                AddKeyWords("schinese", "_content_repair", "解决疑难杂症");
                AddKeyWords("tchinese", "_content_repair", "自動修復");

                AddKeyWords("english", "_verify_file", "Verify Game Files");
                AddKeyWords("schinese", "_verify_file", "检查游戏完整性");
                AddKeyWords("tchinese", "_verify_file", "檢查遊戲文件");

                AddKeyWords("english", "_update_check", "Check Update");
                AddKeyWords("schinese", "_update_check", "检查更新");
                AddKeyWords("tchinese", "_update_check", "檢查更新");

                AddKeyWords("english", "_version_info", "Launcher: V{0} | Game: V{1}");
                AddKeyWords("schinese", "_version_info", "启动器版本: V{0} | 游戏: V{1}");
                AddKeyWords("tchinese", "_version_info", "啟動器版本: V{0} | 遊戲: V{1}");
                // GUI end

                AddKeyWords("english", "_copyright", "Copyright of cso2 (Counter-Strike Online 2):\nCopyright © 2008(2013) NEXON Korea Corporation & Valve Corporation. All Rights Reserved.\nValve, Counter-Strike and Counter-Strike Online are trademarks, registered trademarks or applied trademarks of Valve Corporation.\n\nThis program is only been used for create virtual local area network (LAN) to help playing the game, will not and never modify any contents of cso2.");
                AddKeyWords("schinese", "_copyright", "cso2(反恐精英Online2)的版权:\n版权所有© 2008(2013) NEXON Korea Corporation & Valve Corporation. 保留所有权利.\nValve, Counter-Strike和Counter-Strike Online是Valve Corporation的商标, 注册商标或申请商标.\n\n此程序仅用于创建虚拟局域网(LAN)以帮助游玩游戏, 不会也永远不会修改cso2的任何内容.");
                AddKeyWords("tchinese", "_copyright", "cso2(絕對武力Online2)的版權:\n版權所有© 2008(2013) NEXON Korea Corporation & Valve Corporation. 保留所有權利.\nValve, Counter-Strike和Counter-Strike Online是Valve Corporation的商標, 註冊商標或申請商標.\n\n本程式僅負責創建虛擬局域網(LAN)以幫助遊玩遊戲, 不會也永遠不會修改cso2的任何內容.");

                AddKeyWords("english", "_author_and_helper", "CSO2 Combo Launcher:\n- Made by GEEKiDoS\n- Secondary development by leang97 and dounai2333\n- Sakura Frp for main server hosting");
                AddKeyWords("schinese", "_author_and_helper", "CSO2 Combo Launcher:\n- GEEKiDoS编写\n- leang97及dounai2333二次开发\n- 由Sakura Frp提供主服务器");
                AddKeyWords("tchinese", "_author_and_helper", "CSO2 Combo Launcher:\n- GEEKiDoS製作\n- leang97和dounai2333二次開發\n- 由Sakura Frp提供主服务器");

                AddKeyWords("english", "_third_party_library", "Third-Party Library(s):\n- Toqe.Downloader as Multi-Threaded Downloader\n- Chsword.JDynamic as JSON Parser");
                AddKeyWords("schinese", "_third_party_library", "第三方库:\n- Toqe.Downloader(多线程下载器)\n- Chsword.JDynamic(JSON解析器)");
                AddKeyWords("tchinese", "_third_party_library", "第三方庫:\n- Toqe.Downloader(多線程下載器)\n- Chsword.JDynamic(JSON解析器)");

                AddKeyWords("english", "_self_checking_gamefiledll", "Checking if game dll files is missing...");
                AddKeyWords("schinese", "_self_checking_gamefiledll", "正在检查游戏dll文件是否缺失...");
                AddKeyWords("tchinese", "_self_checking_gamefiledll", "正在校驗遊戲dll文件是否少了...");

                AddKeyWords("english", "_self_checking_gamefiledll_failed", "Detected these dll file(s) is missing:\n\"{0}\"\n\nIt's better to re-install your game or you may see bugs in the game or cannot start!\n\nIgnore this warning and continue?");
                AddKeyWords("schinese", "_self_checking_gamefiledll_failed", "检测到这些dll文件已缺失:\n\"{0}\"\n\n最好还是重装你的游戏, 不然可能会出现BUG或无法启动!\n\n忽略此警告, 并继续?");
                AddKeyWords("tchinese", "_self_checking_gamefiledll_failed", "發現這些dll文件少了:\n\"{0}\"\n\n最好還是重裝你的遊戲你的遊戲, 不然可能會出現BUG或無法啟動!\n\n忽略此警告, 並繼續?");

                AddKeyWords("english", "_self_checking_gamefilepkg", "Checking if game pkg files is missing...");
                AddKeyWords("schinese", "_self_checking_gamefilepkg", "正在检查游戏pkg文件是否缺失...");
                AddKeyWords("tchinese", "_self_checking_gamefilepkg", "正在校驗遊戲pkg文件是否少了...");

                AddKeyWords("english", "_self_checking_gamefilepkg_failed", "Detected pkg file(s) missing!\nYou only got {0} pkg files, but normally you should having {1} files.\n\nIt's better to re-install your game or you may see bugs in the game or cannot start!\n\nIgnore this warning and continue?");
                AddKeyWords("schinese", "_self_checking_gamefilepkg_failed", "检测到pkg文件缺失!\n你只有{0}个pkg文件, 而正常情况下你应该有{1}个.\n\n最好还是重装你的游戏, 不然可能会出现BUG或无法启动!\n\n忽略此警告, 并继续?");
                AddKeyWords("tchinese", "_self_checking_gamefilepkg_failed", "發現pkg文件少了!\n你只有{0}個pkg文件, 而正常情況下你應該有{1}個.\n\n最好還是重裝你的遊戲你的遊戲, 不然可能會出現BUG或無法啟動!\n\n忽略此警告, 並繼續?");

                AddKeyWords("english", "_self_checking_openvpnfile", "Checking if OpenVPN files is missing...");
                AddKeyWords("schinese", "_self_checking_openvpnfile", "正在检查OpenVPN文件是否缺失...");
                AddKeyWords("tchinese", "_self_checking_openvpnfile", "正在校驗OpenVPN文件是否少了...");

                AddKeyWords("english", "_self_checking_openvpnfile_failed", "OpenVPN file download failed!\nYour network connection may be having trouble, or server is current offline.\nIf you are keeping got this error, please report to a administrator.\n\nProgram will now exit.");
                AddKeyWords("schinese", "_self_checking_openvpnfile_failed", "OpenVPN文件下载失败!\n你的网络可能出现了问题, 或服务器当前处于离线状态.\n如果你一直遇见此错误, 请向管理员报告.\n\n程序将自动退出.");
                AddKeyWords("tchinese", "_self_checking_openvpnfile_failed", "OpenVPN文件下載失敗!\n你的網路可能出現了問題, 或者服務器當前為離線狀態.\n如果你一直遇見這個錯誤, 請向管理員報告.\n\n程式將會自動退出.");

                AddKeyWords("english", "_self_checking_tapwindows", "Checking if TAP-Windows adapter is installed...");
                AddKeyWords("schinese", "_self_checking_tapwindows", "正在检查TAP-Windows适配器是否已安装...");
                AddKeyWords("tchinese", "_self_checking_tapwindows", "正在校驗TAP-Windows適配器是否已安裝...");

                AddKeyWords("english", "_self_checking_tapwindows_failed", "TAP-Windows adapter download / install failed!\nIf you reject the installation request, please approve the installation.\nIf you are keeping got this error, please report to a administrator.\n\nProgram will now exit.");
                AddKeyWords("schinese", "_self_checking_tapwindows_failed", "TAP-Windows适配器下载或安装失败!\n如果你拒绝了安装请求, 请进行同意.\n如果你一直遇见此错误, 请向管理员报告.\n\n程序将自动退出.");
                AddKeyWords("tchinese", "_self_checking_tapwindows_failed", "TAP-Windows適配器下載或安裝失敗!\n如果你拒絕了安裝請求, 請同意它.\n如果你一直遇見這個錯誤, 請向管理員報告.\n\n程式將會自動退出.");

                AddKeyWords("english", "_self_checking_dhcpclient", "Checking if \"DHCP Client\" service is running...");
                AddKeyWords("schinese", "_self_checking_dhcpclient", "正在检查\"DHCP Client\"服务是否已运行...");
                AddKeyWords("tchinese", "_self_checking_dhcpclient", "正在校驗\"DHCP Client\"服務是否已運行...");

                AddKeyWords("english", "_self_checking_dhcpclient_failed", "\"DHCP Client\" service start failed!\nIf you are in the netcafe, then maybe you don't have enough permission to start it.\nIf you are using your personal PC, you should go check why it can't be started.\n\nProgram will now exit.");
                AddKeyWords("schinese", "_self_checking_dhcpclient_failed", "\"DHCP Client\"服务启动失败!\n如果你在网吧, 这可能意味着你没有足够的权限去启动它.\n如果你在使用你的个人电脑, 你应该去检查为什么它无法被启动.\n\n程序将自动退出.");
                AddKeyWords("tchinese", "_self_checking_dhcpclient_failed", "\"DHCP Client\"服務啟動失敗!\n如果你在網吧, 這可能意味著你沒有足夠的權限去啟動它.\n如果你在使用你的個人電腦, 你應該去檢查為什麼它無法被啟動.\n\n程式將會自動退出.");

                AddKeyWords("english", "_self_checking_firewall_addexception", "Adding program to the firewall exception list...");
                AddKeyWords("schinese", "_self_checking_firewall_addexception", "正在将程序添加到防火墙例外列表...");
                AddKeyWords("tchinese", "_self_checking_firewall_addexception", "正在把程式添加到防火墻例外列表...");

                AddKeyWords("english", "_self_checking_httplistener", "Checking if http service can be running...");
                AddKeyWords("schinese", "_self_checking_httplistener", "正在检查http服务是否可以运行...");
                AddKeyWords("tchinese", "_self_checking_httplistener", "正在校驗http服務是否可以運行...");

                AddKeyWords("english", "_self_checking_mainserverconnection", "Checking if main server is online...");
                AddKeyWords("schinese", "_self_checking_mainserverconnection", "正在检查主服务器是否在线...");
                AddKeyWords("tchinese", "_self_checking_mainserverconnection", "正在校驗主要伺服器是否在線...");

                AddKeyWords("english", "_self_checking_launcherupdate", "Checking if launcher are having update...");
                AddKeyWords("schinese", "_self_checking_launcherupdate", "正在检查启动器是否需要更新...");
                AddKeyWords("tchinese", "_self_checking_launcherupdate", "正在校驗啟動器是否需要更新...");

                AddKeyWords("english", "_self_checking_launcherupdate_needed", "We got a new version of launcher!\nNewer version: {0} , Update log:\n{1}\n\nWould you like to update now?");
                AddKeyWords("schinese", "_self_checking_launcherupdate_needed", "启动器有新的版本了!\n新版本: {0} , 更新日志:\n{1}\n\n你想现在更新吗?");
                AddKeyWords("tchinese", "_self_checking_launcherupdate_needed", "啟動器有新的版本了!\n新版本: {0} , 更新日誌:\n{1}\n\n你想現在更新嗎?");

                AddKeyWords("english", "_self_checking_launcherupdate_failed", "Launcher update failed!\nProgram will ignore the update for now so you can update it later.");
                AddKeyWords("schinese", "_self_checking_launcherupdate_failed", "启动器更新失败!\n程序将忽略此次更新以便你之后进行更新.");
                AddKeyWords("tchinese", "_self_checking_launcherupdate_failed", "啟動器更新失敗!\n程序將忽略這次更新讓你之後再進行更新.");

                AddKeyWords("english", "_self_checking_download_resource", "Downloading resource from {0} server...");
                AddKeyWords("schinese", "_self_checking_download_resource", "正在从{0}服务器下载资源...");
                AddKeyWords("tchinese", "_self_checking_download_resource", "正在從{0}伺服器下載資源...");

                AddKeyWords("english", "_openvpn_message", "Disclaimer: OpenVPN is only been used to make virtual LAN.");
                AddKeyWords("schinese", "_openvpn_message", "声明: OpenVPN仅用于创建虚拟局域网.");
                AddKeyWords("tchinese", "_openvpn_message", "聲明: OpenVPN僅用於創建虛擬局域網.");

                AddKeyWords("english", "_open_source_link", "CSO2 Combo Launcher is now open-source and free to view.\ngithub.com/dounai2333/CSO2-ComboLauncher");
                AddKeyWords("schinese", "_open_source_link", "CSO2 Combo Launcher现已开源并可免费阅览.\ngithub.com/dounai2333/CSO2-ComboLauncher");
                AddKeyWords("tchinese", "_open_source_link", "CSO2 Combo Launcher現在已開源並可以免費查看.\ngithub.com/dounai2333/CSO2-ComboLauncher");

                AddKeyWords("english", "_server_main", "main");
                AddKeyWords("schinese", "_server_main", "主");
                AddKeyWords("tchinese", "_server_main", "主要");

                AddKeyWords("english", "_server_backup", "backup");
                AddKeyWords("schinese", "_server_backup", "备用");
                AddKeyWords("tchinese", "_server_backup", "備用");

                AddKeyWords("english", "_download_server_info", "Getting information from {0} server...");
                AddKeyWords("schinese", "_download_server_info", "正在从{0}服务器获取信息...");
                AddKeyWords("tchinese", "_download_server_info", "正在從{0}伺服器獲取訊息...");

                AddKeyWords("english", "_download_server_info_failed", "Information download failed! May check your net connection?");
                AddKeyWords("schinese", "_download_server_info_failed", "信息下载失败! 也许你的网络出现了问题?");
                AddKeyWords("tchinese", "_download_server_info_failed", "訊息下載失敗! 也許你的網路出現了問題?");

                AddKeyWords("english", "_start_openvpn_and_connect", "OpenVPN started, trying to connect to the game server...");
                AddKeyWords("schinese", "_start_openvpn_and_connect", "OpenVPN已启动, 正在尝试连接到游戏服务器...");
                AddKeyWords("tchinese", "_start_openvpn_and_connect", "OpenVPN已啟動, 正在嘗試連接到遊戲伺服器...");

                AddKeyWords("english", "_connect_to_server_failed", "Tried connect to the server but failed!\nMaybe server is current offline, you can try again later.");
                AddKeyWords("schinese", "_connect_to_server_failed", "尝试连接服务器失败!\n服务器当前可能离线, 你可以稍后再试.");
                AddKeyWords("tchinese", "_connect_to_server_failed", "嘗試連接伺服器失敗!\n伺服器當前可能離線, 你可以稍後再試.");

                AddKeyWords("english", "_connect_to_server_failed_openvpnexited_fatalerror_notapwindows", "Tried to start OpenVPN but process reported \"No TAP-Windows adapter found\"!\n\nProgram will now exit.");
                AddKeyWords("schinese", "_connect_to_server_failed_openvpnexited_fatalerror_notapwindows", "尝试了启动OpenVPN但收到了\"未找到TAP-Windows网卡\"错误!\n\n程序将自动退出.");
                AddKeyWords("tchinese", "_connect_to_server_failed_openvpnexited_fatalerror_notapwindows", "嘗試了啟動OpenVPN但收到了\"未找到TAP-Windows網卡\"錯誤!\n\n程式將會自動退出.");

                AddKeyWords("english", "_connect_to_server_failed_openvpnexited_fatalerror_alltapwindowsinuse", "OpenVPN exited with fatal error!\nThere is no TAP-Windows adapter available.");
                AddKeyWords("schinese", "_connect_to_server_failed_openvpnexited_fatalerror_alltapwindowsinuse", "OpenVPN出现致命错误!\n现在没有空闲的TAP-Windows网卡.");
                AddKeyWords("tchinese", "_connect_to_server_failed_openvpnexited_fatalerror_alltapwindowsinuse", "OpenVPN出錯了!\n現在沒有空閒的TAP-Windows網卡.");

                AddKeyWords("english", "_connect_to_server_failed_openvpnexited_fatalerror", "OpenVPN exited with fatal error!\nYour system may having problem with OpenVPN.");
                AddKeyWords("schinese", "_connect_to_server_failed_openvpnexited_fatalerror", "OpenVPN出现致命错误!\n你的系统可能跟OpenVPN有些兼容性问题.");
                AddKeyWords("tchinese", "_connect_to_server_failed_openvpnexited_fatalerror", "OpenVPN出錯了!\n你的系統可能跟OpenVPN有一些兼容性問題.");

                AddKeyWords("english", "_connect_to_server_failed_openvpnexited", "OpenVPN exited too early!\nPlease check your anti-virus and see if it killed OpenVpn process.");
                AddKeyWords("schinese", "_connect_to_server_failed_openvpnexited", "OpenVPN过早退出!\n请检查你的杀毒软件是否进行了拦截.");
                AddKeyWords("tchinese", "_connect_to_server_failed_openvpnexited", "OpenVPN太早退出!\n請檢查你的殺毒軟體是否攔下了它.");

                AddKeyWords("english", "_connect_to_server_halfway", "Connect success, confirmation data...");
                AddKeyWords("schinese", "_connect_to_server_halfway", "连接成功, 正在确认数据...");
                AddKeyWords("tchinese", "_connect_to_server_halfway", "連接成功, 正在確認數據...");

                AddKeyWords("english", "_connect_to_server_failed_tcping_no_response", "Success connect to the server, but failed to receive game packet!\nGame server might be offline now.");
                AddKeyWords("schinese", "_connect_to_server_failed_tcping_no_response", "与服务器连接成功, 但无法收到游戏数据包回复!\n游戏服务器当前可能处于离线状态.");
                AddKeyWords("tchinese", "_connect_to_server_failed_tcping_no_response", "與伺服器連接成功, 但無法收到遊戲數據包回復!\n遊戲伺服器當前可能在脫機狀態.");

                AddKeyWords("english", "_connect_to_server_success", "Game server connected, {0}{1}you can start the game now!");
                AddKeyWords("schinese", "_connect_to_server_success", "游戏服务器已连接, {0}{1}你现在可以启动游戏了!");
                AddKeyWords("tchinese", "_connect_to_server_success", "遊戲伺服器已連接, {0}{1}你現在可以開始遊戲了!");

                AddKeyWords("english", "_server_delay", "delay: {0}ms, ");
                AddKeyWords("schinese", "_server_delay", "延迟: {0}ms, ");
                AddKeyWords("tchinese", "_server_delay", "延遲: {0}ms, ");

                AddKeyWords("english", "_player_count", "current {0} player(s), ");
                AddKeyWords("schinese", "_player_count", "当前有{0}名玩家, ");
                AddKeyWords("tchinese", "_player_count", "現在有{0}名玩家, ");

                AddKeyWords("english", "_start_connect_failed_hint", "You have failed to connect to the server, you can still start the game, but most likely you will not be available to login!\n\nContinue?");
                AddKeyWords("schinese", "_start_connect_failed_hint", "你连接服务器失败了, 你仍然可以启动游戏, 但极大可能你无法进行登录!\n\n确认并继续?");
                AddKeyWords("tchinese", "_start_connect_failed_hint", "你連接伺服器失敗了, 你仍然可以開始遊戲, 但很有可能你無法進行登錄!\n\n確認並繼續?");

                AddKeyWords("english", "_start_no_language_file", "The file for the currently selected language could not be found, please change to other language, otherwise you will hit errors when launching the game!\n\nIgnore this warning and continue?");
                AddKeyWords("schinese", "_start_no_language_file", "无法找到当前选择的语言的文件, 请修改语言, 否则启动游戏将会导致报错!\n\n忽略此警告, 并继续?");
                AddKeyWords("tchinese", "_start_no_language_file", "無法找到當前選擇的語言的文件, 請修改語言, 強行開始遊戲將會導致出現錯誤!\n\n忽略此警告, 並繼續?");

                AddKeyWords("english", "_start_name_blacklist", "Your nickname is not allowed, please change it before start the game.");
                AddKeyWords("schinese", "_start_name_blacklist", "你的游戏名称包含非法字符, 请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_name_blacklist", "你的遊戲名稱有被禁止的字符, 請修改後再開始遊戲.");

                AddKeyWords("english", "_start_codepage_wrong", "Your codepage is not \"936 Chinese (Simplified, China)\", you can only use the nickname with English only.\nPlease change it before start the game.");
                AddKeyWords("schinese", "_start_codepage_wrong", "你的代码页不是\"936 中文(简体, 中国)\", 因此你的游戏名称只能包含英文.\n请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_codepage_wrong", "你的代碼頁不是\"936 中文(簡體, 中國)\", 所以你的遊戲名稱只能有英文字符.\n請修改後再開始遊戲.");

                AddKeyWords("english", "_start_name_saved", "Some part of your nickname has been saved for special user, please change it before start the game.");
                AddKeyWords("schinese", "_start_name_saved", "你的游戏名称的一部分已保留给特定用户, 请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_name_saved", "你的遊戲名稱的一部分已經留給了部分用戶, 請修改後再開始遊戲.");

                AddKeyWords("english", "_start_password_wrongtext", "Your password contains symbol or other not allowed characters, password should be numbers and letters only.\nPlease change it before start the game.");
                AddKeyWords("schinese", "_start_password_wrongtext", "你的登录密码包含特殊符号或其他不被允许的字符, 密码应只有数字和字母.\n请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_password_wrongtext", "你的登入密碼有特殊符號或其他不被允許的字符, 密碼應該只有數字和字母.\n請修改後再開始遊戲.");

                AddKeyWords("english", "_start_no_name_password", "Cannot start the game without nickname and password, please change it before start the game.");
                AddKeyWords("schinese", "_start_no_name_password", "无法在没有游戏名称和密码的情况下启动游戏, 请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_no_name_password", "不能在沒有遊戲名稱和登入密碼的情況下開始遊戲, 請修改後再開始遊戲.");

                AddKeyWords("english", "_start_no_name", "Cannot start the game without nickname, please change it before start the game.");
                AddKeyWords("schinese", "_start_no_name", "无法在没有游戏名称的情况下启动游戏, 请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_no_name", "不能在沒有遊戲名稱的情況下開始遊戲, 請修改後再開始遊戲.");

                AddKeyWords("english", "_start_no_password", "Cannot start the game without password, please change it before start the game.");
                AddKeyWords("schinese", "_start_no_password", "无法在没有登录密码的情况下启动游戏, 请修改后再启动游戏.");
                AddKeyWords("tchinese", "_start_no_password", "不能在沒有登入密碼的情況下開始遊戲, 請修改後再開始遊戲.");

                AddKeyWords("english", "_start_name_convertfailed", "Your nickname has broken: {0}\nYou can't start game with a broken name!\nBut please don't panic, this error does not mean that it cannot be resolve, please follow the steps below:\n\nYou can not use the singular texts (example: 我爱所有人)\nYou can add \"丶\" or \"｜\"to solve this problem (example: 我爱所有人丶)\nDuel texts has been cut to singular texts (example: 我K爱大家、我 爱大家(space character))\nYou can delete them, or try to add more non-ASCII texts!\n\nWhen you solved problems, you can start the game normally!\nThe following sample text has no problem:\n世界上最帅气的人、脸皮挡子弹丶、xX灭神Xx、夜影o夜空、豆奶233");
                AddKeyWords("schinese", "_start_name_convertfailed", "你的游戏名称出现了损坏: {0}\n使用损坏的名称无法进入游戏!\n但请不要慌张, 出现此错误, 并不代表无法解决, 请跟着以下步骤排错:\n\n奇数的中文名是无法使用的(例如: 我爱所有人)\n你可以考虑添加一个\"丶\"或\"｜\"来解决此问题(例如: 我爱所有人丶)\n偶数的中文名被隔开变成了奇数(例如:我K爱大家、我 爱大家(有空格))\n你可以考虑删除导致中文被隔开的罪魁祸首, 或者想办法让奇数的字有个伴!\n\n解决以上问题后, 将可以正常进入游戏!\n以下示例文字是没有问题的, 可以进行参考:\n世界上最帅气的人、脸皮挡子弹丶、xX灭神Xx、夜影o夜空、豆奶233");
                AddKeyWords("tchinese", "_start_name_convertfailed", "你的遊戲名稱損壞了: {0}\n你不能用壞掉了的名稱開始遊戲!\n但是不要擔心, 出現這個問題, 並不代表不能解決, 請跟著以下步驟嘗試排除錯誤:\n\n你無法使用基數的名稱(如: 我愛所有人)\n你可以添加\"丶\"或\"｜\"解決這個問題(如: 我愛所有人丶)\n偶數的名稱被割開變成了基數(如:我K愛大家、我 愛大家(空格符號))\n你可以把那些字符刪除掉, 或者把中文字符改成偶數!\n\n解決這些問題之後, 你就可以正常開始遊戲了!\n下面的示例名稱是沒有問題的, 可以參考一下:\n世界上最帅气的人、脸皮挡子弹丶、xX灭神Xx、夜影o夜空、豆奶233");

                AddKeyWords("english", "_update_check_main_server_offline", "Main server is currently offline, cannot run the update check.");
                AddKeyWords("schinese", "_update_check_main_server_offline", "主服务器当前离线, 无法运行更新检查.");
                AddKeyWords("tchinese", "_update_check_main_server_offline", "主要伺服器當前離線, 無法運行更新檢查.");

                AddKeyWords("english", "_update_check_passed", "Update done, total {0} check(s), {1} check(s) passed.");
                AddKeyWords("schinese", "_update_check_passed", "更新完成, 总共{0}项检查, {1}项检查通过.");
                AddKeyWords("tchinese", "_update_check_passed", "更新完畢, 總共{0}個檢查, {1}個檢查通過.");

                AddKeyWords("english", "_file_check_cancel", "Cancel Verify");
                AddKeyWords("schinese", "_file_check_cancel", "取消检查");
                AddKeyWords("tchinese", "_file_check_cancel", "取消檢查");

                AddKeyWords("english", "_file_check_confirm", "Are you sure you want to verify game files?\nIt may take while, depends with your PC hardware.\n\nNote: this function will only check \"pkg\" files within \"Data\" folder.\n\nContinue?");
                AddKeyWords("schinese", "_file_check_confirm", "你是否确定想检查游戏完整性?\n这可能需要一段时间, 具体取决于你的电脑配置如何.\n\n注意: 此功能只会检查\"Data\"文件夹里的\"pkg\"文件.\n\n是否继续?");
                AddKeyWords("tchinese", "_file_check_confirm", "你是否確定想檢查遊戲文件?\n這可能需要一些時間, 取決于你的電腦配置.\n\n此功能只會檢查\"Data\"資料夾裡的\"pkg\"文件.\n\n是否繼續?");

                AddKeyWords("english", "_file_check_req_file_wrong", "The requirement file doesn't match what we expected. Verify failed.");
                AddKeyWords("schinese", "_file_check_req_file_wrong", "需求文件不符合预期. 检查失败.");
                AddKeyWords("tchinese", "_file_check_req_file_wrong", "需求的文件不符合預期. 檢查失敗.");

                AddKeyWords("english", "_file_check_progress", "Progress: {0} / {1} files.");
                AddKeyWords("schinese", "_file_check_progress", "进度: {0}/{1}个文件.");
                AddKeyWords("tchinese", "_file_check_progress", "進度: {0}/{1}個文件.");

                AddKeyWords("english", "_file_check_progress_file", "Verifying: {0} ({1})");
                AddKeyWords("schinese", "_file_check_progress_file", "检查中: {0} ({1})");
                AddKeyWords("tchinese", "_file_check_progress_file", "檢查中: {0} ({1})");

                AddKeyWords("english", "_file_check_done_check_messagebox", "File verify completed, check dialog box for info.");
                AddKeyWords("schinese", "_file_check_done_check_messagebox", "文件检查已完成, 去看一眼新对话框.");
                AddKeyWords("tchinese", "_file_check_done_check_messagebox", "文件檢查已完成, 去看看對話框.");

                AddKeyWords("english", "_file_check_file_message_text", "Verify done in {0} , here is the result:");
                AddKeyWords("schinese", "_file_check_file_message_text", "检查于{0}完成, 以下为结果:");
                AddKeyWords("tchinese", "_file_check_file_message_text", "檢查在{0}完成, 以下為結果:");

                AddKeyWords("english", "_file_check_file_missing_text", "Below file(s) is missing:");
                AddKeyWords("schinese", "_file_check_file_missing_text", "以下文件已缺失:");
                AddKeyWords("tchinese", "_file_check_file_missing_text", "這些文件不見了:");

                AddKeyWords("english", "_file_check_md5_notmatch_text", "Below file(s) md5 is not matches, probable broken:");
                AddKeyWords("schinese", "_file_check_md5_notmatch_text", "以下文件MD5值不匹配, 可能已损坏:");
                AddKeyWords("tchinese", "_file_check_md5_notmatch_text", "這些文件MD5值不對, 可能被破壞了:");

                AddKeyWords("english", "_file_check_filename", "Verify_Result_{0}.txt");
                AddKeyWords("schinese", "_file_check_filename", "检查结果_{0}.txt");
                AddKeyWords("tchinese", "_file_check_filename", "檢查結果_{0}.txt");

                AddKeyWords("english", "_file_check_file_error_detected", "File verify is completed, detected {0} file(s) doesn't matches.\nA result file has been generated \"{1}\", go check it for more info.");
                AddKeyWords("schinese", "_file_check_file_error_detected", "文件检查已完成, 检测到{0}个文件不匹配.\n已生成结果文件\"{1}\", 打开可以检查更多信息.");
                AddKeyWords("tchinese", "_file_check_file_error_detected", "文件檢查已完成, 檢測到{0}個文件不匹配.\n已生成結果文件\"{1}\", 打開可以檢查更多訊息.");

                AddKeyWords("english", "_file_check_file_all_good", "File verify is completed, all files is good and without error!\nkeep them good forever as much as you can!");
                AddKeyWords("schinese", "_file_check_file_all_good", "文件检查已完成, 所有文件均未出现错误!\n尽量保持它们的良好状态!");
                AddKeyWords("tchinese", "_file_check_file_all_good", "文件檢查已完成, 所有文件都沒有錯誤!\n盡你所能保持它們的良好狀態!");

                AddKeyWords("english", "_auto_repair_warn", "\"Auto Repair\" is not recommand to use if you didn't have any problem!\nThis function will reset most contents that needed to be use.\n\nAre you sure to continue?");
                AddKeyWords("schinese", "_auto_repair_warn", "如果你没有出现任何问题, 那么不建议使用\"解决疑难杂症\"!\n此功能将重置启动器和需要的内容.\n\n是否确定继续?");
                AddKeyWords("tchinese", "_auto_repair_warn", "如果你沒有出現任何問題, 是不建議使用\"自動修復\"的!\n此功能將重置啟動器和需要的內容.\n\n是否確定繼續?");

                AddKeyWords("english", "_auto_repair_running", "Resetting..");
                AddKeyWords("schinese", "_auto_repair_running", "重置中..");
                AddKeyWords("tchinese", "_auto_repair_running", "重置中..");
            }
        }

        private static string GetSystemLang()
        {
            switch (Thread.CurrentThread.CurrentCulture.Name)
            {
                case "zh-CN":
                case "zh-SG":
                case "zh-Hans":
                    return "schinese";
                case "zh-TW":
                case "zh-MO":
                case "zh-HK":
                case "zh-Hant":
                    return "tchinese";
                default:
                    return "english";
            }
        }

        public static void LocalifyControl(UIElementCollection uiControls)
        {
            foreach (UIElement element in uiControls)
            {
                if (element is TextBlock)
                {
                    var textBlock = element as TextBlock;
                    textBlock.Text = Get(textBlock.Text);
                }
                else if (element is CheckBox)
                {
                    var textBlock = element as CheckBox;
                    textBlock.Content = Get(textBlock.Content as string);
                }
                else if (element is Button)
                {
                    var button = element as Button;
                    button.Content = Get(button.Content as string);
                }
                else if (element is Grid)
                {
                    LocalifyControl((element as Grid).Children);
                }
                else if (element is GroupBox)
                {
                    var groupBox = element as GroupBox;
                    groupBox.Header = Get(groupBox.Header as string);
                    LocalifyControl(((element as GroupBox).Content as Grid).Children);
                }
            }
        }

        private static void AddKeyWords(string lang, string value, string content)
        {
            try
            {
                lStrings.Add(new KeyValuePair<string, string>(lang, value), content);
            }
            catch {}
        }

        public static string Get(string strCode, params object[] args)
        {
            try
            {
                return string.Format(lStrings[new KeyValuePair<string, string>(localLang, strCode)], args);
            }
            catch
            {
                return strCode;
            }
        }
    }
}