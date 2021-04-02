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
    /// Interaktionslogik für AddProfile.xaml
    /// </summary>
    public partial class AddDialog : Window
    {
        string _input = "";
        public AddDialog(string name)
        {
            InitializeComponent();

            tb_Name.Text = name;
            Title = "Füge " + name +" hinzu";
            
        }

        public AddDialog(string name, string editname)
        {
            InitializeComponent();

            tb_Name.Text = name;
            Title = "Ändere " + name;
            tb_profile.Text = editname;
            addprofile.Content = "Edit";

        }

        public string INPUT { 
            get { return _input; } 
            set { _input = value; }
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            _input = tb_profile.Text;
            Close();
        }
    }
}
