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
using System.Windows.Threading;

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
            tx_seperator.Text = "|";
            tx_qualifer.Text = "";
            TextChanged(null,null);
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tx_input == null)
                return;

            string newtext = "";
            if (String.IsNullOrEmpty(tx_qualifer.Text))
                newtext = tx_input.Text.Replace(Environment.NewLine, tx_seperator.Text);
            else
            {
                newtext = tx_qualifer.Text;
                newtext += tx_input.Text.Replace(Environment.NewLine, tx_qualifer.Text + tx_seperator.Text + tx_qualifer.Text);
                newtext += tx_qualifer.Text;
            }

                if (newtext.Length > 0) { 
                while (newtext.Substring(newtext.Length-1, 1) == tx_seperator.Text)
                    newtext = newtext.Substring(0, newtext.Length - 1);
            }

            tx_output.Text = newtext;

            if (tx_input.LineCount > 1950)
                txWarning.Visibility = Visibility.Visible;
            else
                txWarning.Visibility = Visibility.Hidden;
        }

        private void RemoveDuplicates(object sender, RoutedEventArgs e)
        {

            if (tx_input.LineCount <= 1)
                return;

            var lines = new List<string>();
            for (int i = 0; i < tx_input.LineCount; i++)
            {
                lines.Add(tx_input.GetLineText(i).Replace("\r\n", ""));
            }

            lines.Sort();
            string prev = "";

            string temptxt = "";
            foreach (string temp in lines)
            {
                if (prev != temp)
                    temptxt += temp + Environment.NewLine;

                prev = temp;
            }
            tx_input.Text = temptxt;
        }
        private void cb_preset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cb_preset.SelectedItem;
            if (cbi.Content.ToString() == "NAV Filter")
            {
                tx_seperator.Text = "|";
                tx_qualifer.Text = "";
            }
            else if (cbi.Content.ToString() == "SQL in")
            {
                tx_seperator.Text = ",";
                tx_qualifer.Text = "'";
            }


        }
    }
}
