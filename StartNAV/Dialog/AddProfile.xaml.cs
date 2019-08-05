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
        readonly IniHandler ini;
        public AddProfile(string file, string file2)
        {
            InitializeComponent();
            ini = new IniHandler(file, file2);
        }

        private void Addprofile_Click(object sender, RoutedEventArgs e)
        {
            ini.AddProfile(tb_profile.Text);
            Close();
        }
    }
}
