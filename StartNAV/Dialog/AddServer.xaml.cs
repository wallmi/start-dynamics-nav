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

namespace StartNAV.Dialog
{
    /// <summary>
    /// Interaktionslogik für Add.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        IniHandler INI;
        public AddServer(IniHandler ini)
        {
            InitializeComponent();
            INI = ini;
            if (ini.GetSetting("DefaultService") != null)
                _instanz.Text = ini.GetSetting("DefaultService");
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            INI.AddServer(_servername.Text, _serveradress.Text + ":" + _port.Text + "/" + _instanz.Text);
            Close();
        }
    }
}
