using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using IniParser.Model;

namespace StartNAV.Dialog
{
    /// <summary>
    /// Interaktionslogik für Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        IniHandler ini;
        public Options(IniHandler Ini)
        {
            InitializeComponent();
            if (Ini == null)
                return;

            ini = Ini;
            if (ini.GetSetting("upd") == "true")
                opt_upd_yes.IsChecked = true;
            else
                opt_upd_no.IsChecked = true;

            if (!String.IsNullOrEmpty(ini.GetSetting("upd_user")))
                opt_upd_user.Text = ini.GetSetting("upd_user");

            if (!String.IsNullOrEmpty(ini.GetSetting("upd_user")))
                opt_upd_repository.Text = ini.GetSetting("upd_repository");


            if (ini.GetSetting("upd_beta") == "true")
                opt_upd_beta.IsChecked = true;
            else
                opt_upd_beta.IsChecked = false;

        }
        public void saveSettings (object sender, RoutedEventArgs e)
        {
            if (opt_upd_yes.IsChecked == true)
                ini.SetSettings("upd", "true");            
            else
                ini.SetSettings("upd", "false");

            ini.SetSettings("upd_user", opt_upd_user.Text);
            ini.SetSettings("upd_repository", opt_upd_repository.Text);

            if (opt_upd_beta.IsChecked == true)
                ini.SetSettings("upd_beta", "true");
            else
                ini.SetSettings("upd_beta", "false");

            ini.Save();
            Close();
        }
        public void abbort(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
