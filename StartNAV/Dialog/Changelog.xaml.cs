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

using Octokit;

namespace StartNAV.Dialog
{
    /// <summary>
    /// Interaktionslogik für Changelog.xaml
    /// </summary>
    public partial class Changelog : Window
    {

        public Changelog()
        {
            InitializeComponent();

        }
        public Changelog(string[,] Log)
        {
            InitializeComponent();
            SetContent(Log);
        }
        public void SetContent(string[,] Log)
        {
            for (int i = 0; i < (Log.Length / 2); i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = Log[i, 0];
                tb.FontSize = 20;
                tb.FontWeight = FontWeights.Bold;
                sp_Log.Children.Add(tb);
                TextBlock tb2 = new TextBlock();
                if (String.IsNullOrEmpty(Log[i, 1]))
                    tb2.Text = "Keine Informationen verfügbar";
                else
                    tb2.Text = Log[i, 1];
                tb2.Text += "\n\r";
                tb2.FontSize = 12;
                sp_Log.Children.Add(tb2);
            }
        }

    }
}
