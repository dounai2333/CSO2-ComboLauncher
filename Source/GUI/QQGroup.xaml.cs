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

        private async void Copy(TextBox textb, Button func)
        {
            textb.IsEnabled = false;
            func.IsEnabled = false;
            func.Content = LStr.Get("_copied");

            Clipboard.SetText(textb.Text);

            await Misc.Sleep(1500);

            textb.IsEnabled = true;
            func.IsEnabled = true;
            func.Content = LStr.Get("_copy");
        }

        private void Start(string param)
        {
            Process.Start($"tencent://groupwpa/?subcmd=all&param={param}");
        }

        private void QQGroup_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void Copy1_Click(object sender, RoutedEventArgs e)
        {
            Copy(text1, copy1);
        }
        
        private void Join1_Click(object sender, RoutedEventArgs e)
        {
            Start("7B2267726F757055696E223A3739333539373634342C2274696D655374616D70223A313539383039393535357D");
        }

        private void Copy2_Click(object sender, RoutedEventArgs e)
        {
            Copy(text2, copy2);
        }

        private void Join2_Click(object sender, RoutedEventArgs e)
        {
            Start("7B2267726F757055696E223A3831303436353537352C2274696D655374616D70223A313539383136373734367D");
        }
        
        private void Copy3_Click(object sender, RoutedEventArgs e)
        {
            Copy(text3, copy3);
        }

        private void Join3_Click(object sender, RoutedEventArgs e)
        {
            Start("7B2267726F757055696E223A3635313037353432342C2274696D655374616D70223A313634333938373136307D");
        }
    }
}