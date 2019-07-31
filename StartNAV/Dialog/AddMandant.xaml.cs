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
    public partial class AddMandant : Window
    {
        readonly IniHandler ini;
        public AddMandant(string file)
        {
            InitializeComponent();
            ini = new IniHandler(file,"");
        }

        public void SetServer(List<String> server, string defaultServer){
            cb_servername.ItemsSource = server;
            cb_servername.Text = defaultServer;
        }

        private void Addmandant_Click(object sender, RoutedEventArgs e)
        {
            ini.AddMandant(cb_servername.Text, tb_mandant.Text);
            Close();
        }
    }
}
