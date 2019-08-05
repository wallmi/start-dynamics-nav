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
        public NavObject retGet= new NavObject();
        public List<NavObject> retList = new List<NavObject>();
  
        public GetObject(ObjectHandler handler)
        {
            InitializeComponent();

            List<NavObject> names= handler.GetObjectNames();

            lv_items.ItemsSource = names;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv_items.ItemsSource);
            view.Filter = UserFilter;
        }
        /// <summary>
        /// Show Dialog für Selektion eines Objekts
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog2()
        {
            b_add2Fav.Visibility = Visibility.Hidden;
            lv_items.SelectionMode = SelectionMode.Single;
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
            lv_items.SelectionMode = SelectionMode.Extended;
            return this.ShowDialog();
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(tb_search.Text))
                return true;
            else
                return ((item as NavObject).Name.IndexOf(tb_search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lv_items.ItemsSource).Refresh();

        }

        private void Lv_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (lv_items.SelectedItems.Count == 1)
            //    b_get.IsEnabled = true;
            //else
            //    b_get.IsEnabled = false;

        }

        private void B_get_Click(object sender, RoutedEventArgs e)
        {
            retGet.ID = (lv_items.SelectedItems[0] as NavObject).ID;
            retGet.Name = (lv_items.SelectedItems[0] as NavObject).Name;
            retGet.Typ = (lv_items.SelectedItems[0] as NavObject).Typ;
            Close();
        }

        private void B_add2Fav_Click(object sender, RoutedEventArgs e)
        {
            foreach (NavObject temp in lv_items.SelectedItems)
                retList.Add((NavObject)temp.Clone());

            Close();
        }
    }
}
