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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NAVFilterConvert
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void tx_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newtext = tx_input.Text.Replace(Environment.NewLine, "|");

            if (newtext.Length > 0) { 
                while (newtext.Substring(newtext.Length-1, 1) == "|")
                    newtext = newtext.Substring(0, newtext.Length - 1);
            }

            tx_output.Text = newtext;
        }
    }
}
