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
        readonly IniHandler INI;
        String oldmandant = "";
        public AddMandant(IniHandler ini)
        {
            InitializeComponent();
            INI = ini;
        }

        public AddMandant(IniHandler ini, string Mandant)
        {
            InitializeComponent();
            INI = ini;
            
            oldmandant = Mandant;               //Speichern des alten Mandanten
            tb_mandant.Text = Mandant;          
        }

        public void SetServer(List<String> server, string defaultServer){
            cb_servername.ItemsSource = server;
            cb_servername.Text = defaultServer;
            Title = "Ändere Mandant";
            addmandant.Content = "Ändern";
            if (String.IsNullOrEmpty(oldmandant))
                cb_servername.IsEnabled = false;
        }

        private void Addmandant_Click(object sender, RoutedEventArgs e)
        {
            if (oldmandant != "")
                INI.DelMandant(cb_servername.Text, oldmandant);     //Löschen des alten Mandanten

            INI.AddMandant(cb_servername.Text, tb_mandant.Text);
            Close();
        }
    }
}
