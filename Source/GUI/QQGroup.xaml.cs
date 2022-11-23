using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CSO2_ComboLauncher
{
    public partial class QQGroup : Window
    {
        public static QQGroup Instance = null;

        public QQGroup()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            InitializeComponent();
            mainGrid.Margin = new Thickness(0);
            LStr.LocalifyControl(mainGrid.Children);
        }

        private void Start(string qqgroup)
        {
            Process.Start($"tencent://groupwpa/?subcmd=all&param={Misc.StringToHex($"{{\"groupUin\":{qqgroup}}}")}");
        }

        private void QQGroup_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button button)
            {
                foreach (UIElement element in mainGrid.Children)
                {
                    if (element is TextBox textBox)
                    {
                        if (textBox.Name.StartsWith(button.Name))
                        {
                            Start(textBox.Text);
                            break;
                        }
                    }
                }
            }
        }
    }
}