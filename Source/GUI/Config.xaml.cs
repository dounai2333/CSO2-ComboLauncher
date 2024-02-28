using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CSO2_ComboLauncher
{
    public partial class Config : Window
    {
        public static Config Instance { get; private set; }

        public string Username = "";
        public string Password = "";
        public bool NoAutoLogin = false;
        public string GameLanguage = "schinese";
        public string CustomArgs = "";
        public bool EnableConsole = false;
        public string Server = "Shanghai";
        public bool DisableSomeCheck = false;

        public Config()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            InitializeComponent();
            mainGrid.Margin = new Thickness(0);
            LStr.LocalifyControl(mainGrid.Children);

            // check OpenVpnServer() on Downloader.cs also!
            ServerInfo.Items.Add("Shanghai");

            if (File.Exists(Static.Config))
            {
                Misc.DecryptFile(Static.Config, Static.Config);

                IniParser ini = new IniParser(Static.Config);
                string _name = ini.GetSetting("Game", "Name");
                string _password = ini.GetSetting("Game", "Password");
                string _noautologin = ini.GetSetting("Game", "DisableAutoLogin");
                string _language = ini.GetSetting("Game", "Language");
                string _customargs = ini.GetSetting("Game", "CustomArgs");
                string _enableconsole = ini.GetSetting("Game", "EnableConsole");
                string _server = ini.GetSetting("Launcher", "Server");
                string _disablesomecheck = ini.GetSetting("Launcher", "DisableSomeCheck");

                Username = string.IsNullOrEmpty(_name) ? Username : ((_name.Length > userName.MaxLength) ? _name.Substring(0, userName.MaxLength) : _name);
                Password = string.IsNullOrEmpty(_password) ? Password : ((_password.Length > passWord.MaxLength) ? _password.Substring(0, passWord.MaxLength) : _password);
                NoAutoLogin = (_noautologin != "True" && _noautologin != "False") ? NoAutoLogin : (_noautologin.ToString() != NoAutoLogin.ToString());
                GameLanguage = string.IsNullOrEmpty(_language) ? GameLanguage : _language;
                CustomArgs = string.IsNullOrEmpty(_customargs) ? CustomArgs : _customargs;
                EnableConsole = (_enableconsole != "True" && _enableconsole != "False") ? EnableConsole : (_enableconsole.ToString() != EnableConsole.ToString());
                Server = _server == "Shanghai" ? _server : Server;
                DisableSomeCheck = (_disablesomecheck != "True" && _disablesomecheck != "False") ? DisableSomeCheck : _disablesomecheck.ToString() != DisableSomeCheck.ToString();
            }
            else
            {
                MessageBoxResult box = MessageBox.Show(LStr.Get("_user_agreement"), Static.CWindow, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (box == MessageBoxResult.No)
                    Environment.Exit(0);
            }

            userName.Text = Username;
            passWord.Password = Password;
            noautoLogin.IsChecked = NoAutoLogin;
            disableSomeCheck.IsChecked = DisableSomeCheck;
            customArgs.Text = CustomArgs;
            enableConsole.IsChecked = EnableConsole;

            ServerInfo.SelectedItem = Server;

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

            SaveConfig();
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
            File.WriteAllText(Static.Config, string.Empty);

            IniParser ini = new IniParser(Static.Config);
            ini.AddSetting("Game", "Name", Username);
            ini.AddSetting("Game", "Password", Password);
            ini.AddSetting("Game", "DisableAutoLogin", NoAutoLogin.ToString());
            ini.AddSetting("Game", "Language", GameLanguage);
            ini.AddSetting("Game", "CustomArgs", CustomArgs);
            ini.AddSetting("Game", "EnableConsole", EnableConsole.ToString());
            ini.AddSetting("Launcher", "Server", Server);
            ini.AddSetting("Launcher", "DisableSomeCheck", DisableSomeCheck.ToString());
            ini.SaveSettings();

            Misc.EncryptFile(Static.Config, Static.Config);
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
            Server = ServerInfo.SelectedItem.ToString();

            if (Static.started)
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