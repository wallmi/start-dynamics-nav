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

using System.Configuration;
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
            _instanz.Text = ConfigurationManager.AppSettings["DEFAULT_INSTANCE"];

        }

        public AddServer(IniHandler ini,string ServerName)
        {
            InitializeComponent();
            INI = ini;

            _servername.Text = ServerName;
            _servername.IsEnabled = false;

            string serverstring = INI.GetServerAdress(ServerName);

            _serveradress.Text = serverstring.Split(':')[0];
            _port.Text = serverstring.Split(':')[1].Split('/')[0];
            _instanz.Text = serverstring.Split('/')[1];

            Title = "Bearbeite Server";
            addServer.Content = "Server Aktualisieren";

        }


        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            INI.AddServer(_servername.Text, _serveradress.Text + ":" + _port.Text + "/" + _instanz.Text);
            Close();
        }
    }
}
