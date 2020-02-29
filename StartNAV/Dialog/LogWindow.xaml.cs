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
    /// Interaktionslogik für Log.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow(List<Model.LogEntry> entries)
        {
            InitializeComponent();
            lv_logentries.ItemsSource = entries;
        }
    }
}
