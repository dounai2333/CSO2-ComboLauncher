using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CSO2_ComboLauncher
{
    /// <summary>
    /// Config.xaml 的交互逻辑
    /// </summary>
    public partial class Config : Window
    {
        public static Config Instance { get; private set; }

        private static string ConfigName = "cso2_launcher.ini";

        public string Username = "";
        public string Password = "";
        public bool NoAutoLogin = false;
        public string GameLanguage = "schinese";
        public string CustomArgs = "";
        public bool EnableConsole = false;
        public Server CurrentServer;
        public string Location = "Shanghai";
        public bool DisableSomeCheck = false;
        public string Secret = "";

        public Config()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            InitializeComponent();
            mainGrid.Margin = new Thickness(0);
            LStr.LocalifyControl(mainGrid.Children);

            ServerInfo.Items.Add(new Server("Shanghai", "47.100.199.171"));
            ServerInfo.SelectedItem = Server.Servers[Location];

            if (File.Exists(ConfigName))
            {
                Misc.DecryptFile(ConfigName, ConfigName);

                var ini = new IniParser(ConfigName);
                var _name = ini.GetSetting("Game", "Name");
                var _password = ini.GetSetting("Game", "Password");
                var _noautologin = ini.GetSetting("Game", "DisableAutoLogin");
                var _language = ini.GetSetting("Game", "Language");
                var _customargs = ini.GetSetting("Game", "CustomArgs");
                var _enableconsole = ini.GetSetting("Game", "EnableConsole");
                var _server = ini.GetSetting("Launcher", "Server");
                var _disablesomecheck = ini.GetSetting("Launcher", "DisableSomeCheck");
                var _secret = ini.GetSetting("Launcher", "Secret");

                Username = string.IsNullOrEmpty(_name) ? Username : ((_name.Length > userName.MaxLength) ? _name.Substring(0, userName.MaxLength) : _name);
                Password = string.IsNullOrEmpty(_password) ? Password : ((_password.Length > passWord.MaxLength) ? _password.Substring(0, passWord.MaxLength) : _password);
                NoAutoLogin = (_noautologin != "True" && _noautologin != "False") ? NoAutoLogin : (_noautologin.ToString() != NoAutoLogin.ToString());
                GameLanguage = string.IsNullOrEmpty(_language) ? GameLanguage : _language;
                CustomArgs = string.IsNullOrEmpty(_customargs) ? CustomArgs : _customargs;
                EnableConsole = (_enableconsole != "True" && _enableconsole != "False") ? EnableConsole : (_enableconsole.ToString() != EnableConsole.ToString());
                Location = _server == "Shanghai" /* || _server == "Server" */ ? _server : Location;
                DisableSomeCheck = (_disablesomecheck != "True" && _disablesomecheck != "False") ? DisableSomeCheck : _disablesomecheck.ToString() != DisableSomeCheck.ToString();
                Secret = string.IsNullOrEmpty(_secret) ? Secret : _secret;

                SaveConfig();
            }
            else
            {
                MessageBox.Show(LStr.Get("_no_ini_found"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            userName.Text = Username;
            passWord.Password = Password;
            noautoLogin.IsChecked = NoAutoLogin;
            disableSomeCheck.IsChecked = DisableSomeCheck;
            customArgs.Text = CustomArgs;
            enableConsole.IsChecked = EnableConsole;

            languageList.Items.Add("简体中文 - SChinese");
            languageList.Items.Add("繁體中文 - TChinese");
            languageList.Items.Add("English");
            languageList.Items.Add("日本語 - Japanese");
            languageList.Items.Add("한국어 - Korean");

            switch (GameLanguage)
            {
                case "schinese":
                    languageList.SelectedItem = "简体中文 - SChinese";
                    break;
                case "taiwan":
                    languageList.SelectedItem = "繁體中文 - TChinese";
                    break;
                case "english":
                    languageList.SelectedItem = "English";
                    break;
                case "japan":
                    languageList.SelectedItem = "日本語 - Japanese";
                    break;
                case "koreana":
                    languageList.SelectedItem = "한국어 - Korean";
                    break;
            }

            if (NoAutoLogin)
            {
                userName.IsEnabled = false;
                passWord.IsEnabled = false;
            }

            if (!File.Exists(ConfigName))
                ShowDialog();
        }

        public bool CheckLangExists(string langfile)
        {
            if (langfile == "koreana")
                return true;

            if (File.Exists($"Data\\cstrike\\resource\\cso2_{langfile}.txt") || File.Exists($"custom\\resource\\cso2_{langfile}.txt"))
                return true;

            return false;
        }

        public void SaveConfig()
        {
            Misc.DecryptFile(ConfigName, ConfigName);

            File.WriteAllText(ConfigName, string.Empty);

            IniParser ini = new IniParser(ConfigName);
            ini.AddSetting("Game", "Name", Username);
            ini.AddSetting("Game", "Password", Password);
            ini.AddSetting("Game", "DisableAutoLogin", NoAutoLogin.ToString());
            ini.AddSetting("Game", "Language", GameLanguage);
            ini.AddSetting("Game", "CustomArgs", CustomArgs);
            ini.AddSetting("Game", "EnableConsole", EnableConsole.ToString());
            ini.AddSetting("Launcher", "Server", Location);
            ini.AddSetting("Launcher", "DisableSomeCheck", DisableSomeCheck.ToString());
            ini.AddSetting("Launcher", "Secret", Secret);
            ini.SaveSettings();

            Misc.EncryptFile(ConfigName, ConfigName);
        }

        private void Config_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            SaveConfig();
            Hide();
        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Username = userName.Text;
        }
        
        private void PassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = passWord.Password;
        }

        private void LanguageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (languageList.SelectedItem)
            {
                case "简体中文 - SChinese":
                    GameLanguage = "schinese";
                    break;
                case "繁體中文 - TChinese":
                    GameLanguage = "taiwan";
                    break;
                case "English":
                    GameLanguage = "english";
                    break;
                case "日本語 - Japanese":
                    GameLanguage = "japan";
                    break;
                case "한국어 - Korean":
                    GameLanguage = "koreana";
                    break;
            }

            if (!CheckLangExists(GameLanguage))
                MessageBox.Show(LStr.Get("_lang_file_not_exist"), Static.CWindow, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async void Server_Changed(object sender, SelectionChangedEventArgs e)
        {
            CurrentServer = (Server)ServerInfo.SelectedItem;
            Location = CurrentServer.Name;

            if (Main.Instance != null && Main.Instance.started)
            {
                ServerInfo.IsEnabled = false;
                await Main.Instance.StartOpenVpn();
            }
        }

        private void CustomArgs_TextChanged(object sender, TextChangedEventArgs e)
        {
            CustomArgs = customArgs.Text;
        }

        private void DisableSomeCheck_Click(object sender, RoutedEventArgs e)
        {
            DisableSomeCheck = (bool)disableSomeCheck.IsChecked;
        }

        private void NoAutoLogin_Click(object sender, RoutedEventArgs e)
        {
            NoAutoLogin = (bool)noautoLogin.IsChecked;

            if (NoAutoLogin)
            {
                userName.IsEnabled = false;
                passWord.IsEnabled = false;
            }
            else
            {
                userName.IsEnabled = true;
                passWord.IsEnabled = true;
            }
        }

        private void EnableConsole_Click(object sender, RoutedEventArgs e)
        {
            EnableConsole = (bool)enableConsole.IsChecked;

            if (EnableConsole)
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_enable_console_hint"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (box == MessageBoxResult.No)
                {
                    EnableConsole = false;
                    enableConsole.IsChecked = false;
                }
            }
        }
    }
}
