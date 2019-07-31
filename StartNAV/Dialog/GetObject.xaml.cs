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
        public NavObjects.NavObject retGet= new NavObjects.NavObject();
        public List<NavObjects.NavObject> retList = new List<NavObjects.NavObject>();

        public GetObject(ObjectHandler handler)
        {
            InitializeComponent();

            List<NavObjects.NavObject> names= handler.GetObjectNames();

            lv_items.ItemsSource = names;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv_items.ItemsSource);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(tb_search.Text))
                return true;
            else
                return ((item as NavObjects.NavObject).Name.IndexOf(tb_search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lv_items.ItemsSource).Refresh();

        }

        private void Lv_items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv_items.SelectedItems.Count == 1)
                b_get.IsEnabled = true;
            else
                b_get.IsEnabled = false;

        }

        private void B_get_Click(object sender, RoutedEventArgs e)
        {
            retGet.ID = (lv_items.SelectedItems[0] as NavObjects.NavObject).ID;
            retGet.Name = (lv_items.SelectedItems[0] as NavObjects.NavObject).Name;
            retGet.Typ = (lv_items.SelectedItems[0] as NavObjects.NavObject).Typ;
            Close();
        }

        private void B_add2Fav_Click(object sender, RoutedEventArgs e)
        {
            foreach (NavObjects.NavObject temp in lv_items.SelectedItems)
                retList.Add((NavObjects.NavObject)temp.Clone());

            Close();
        }
    }
}
