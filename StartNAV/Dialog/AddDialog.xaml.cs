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
        public string input
        {
            get { return input; }
            set { input = value; }
        }
        public AddDialog(string name)
        {
            InitializeComponent();

            tb_Name.Text = name;
            Title = "Füge " + name +" hinzu";
            
        }

        private void Addprofile_Click(object sender, RoutedEventArgs e)
        {
            input = tb_profile.Text;
            Close();
        }
    }
}
