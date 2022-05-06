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


using StartNAV.Model;

namespace StartNAV.Dialog
{
    /// <summary>
    /// Interaktionslogik für GetObject.xaml
    /// </summary>
    public partial class GetObject : Window
    {
        private NavObject retGet= new NavObject();
        private List<NavObject> retList = new List<NavObject>();
  
        public GetObject(IniHandler handler)
        {
            if (handler is null) return;

            InitializeComponent();
            lv_items.SetItems(handler.GetObjectNames());

        }
        /// <summary>
        /// Show Dialog für Selektion eines Objekts
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog2()
        {
            b_add2Fav.Visibility = Visibility.Hidden;
            lv_items.SetSelectionmode(SelectionMode.Single);
            
            return this.ShowDialog();
        }
        /// <summary>
        /// Show Dialog für Multi Selektion
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog3()
        {
            b_get.Visibility = Visibility.Hidden;
            Grid.SetColumn(b_add2Fav, 0);
            lv_items.SetSelectionmode(SelectionMode.Extended);
            return this.ShowDialog();
        }

        #region Actions
        private void B_get_Click(object sender, RoutedEventArgs e)
        {
            List<NavObject> items = lv_items.GetSelectItems();

            if (items.Count() == 1)
            {
                retGet.ID = items[0].ID;
                retGet.Name = items[0].Name;
                retGet.Typ = items[0].Typ;
            }
            Close();
        }

        private void B_add2Fav_Click(object sender, RoutedEventArgs e)
        {
            foreach (NavObject temp in lv_items.GetSelectItems())
                retList.Add((NavObject)temp.Clone());

            Close();
        }
        #endregion

        public List<NavObject> getSelectionList()
        {
            return retList;
        }

        public NavObject getSelectedItem()
        {
            return retGet;
        }
    }
}
