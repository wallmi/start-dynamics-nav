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
    public partial class AddProfile : Window
    {
        IniHandler INI;
        public AddProfile(IniHandler ini)
        {
            InitializeComponent();
            INI = ini;
        }

        private void Addprofile_Click(object sender, RoutedEventArgs e)
        {
            INI.AddProfile(tb_profile.Text);
            Close();
        }
    }
}
