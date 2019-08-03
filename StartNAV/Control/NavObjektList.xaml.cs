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

using StartNAV.Model;

namespace StartNAV.Control
{
    /// <summary>
    /// Interaktionslogik für NavObjektList.xaml
    /// </summary>
    public partial class NavObjektList : UserControl
    {
        public NavObject retGet = new NavObject();


        public NavObjektList()
        {
            InitializeComponent();
        }

        public void SetItems(List<NavObject> items)
        {
            lv_items.ItemsSource = items;
            SetFilter();
        }

        public void SetFilter() {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv_items.ItemsSource);
            view.Filter = UserFilter;
        }

        public void SetSelectionmode(SelectionMode sm)
        {
            lv_items.SelectionMode = sm;
        }

        public List<NavObject> GetSelectItems()
        {
            List<NavObject> ret = new List<NavObject>();
            foreach (NavObject temp in lv_items.SelectedItems)
                ret.Add((NavObject)temp.Clone());

            return ret;
        }

        public List<NavObject> GetItems()
        {
            List<NavObject> ret = new List<NavObject>();
            foreach (NavObject temp in lv_items.Items)
                ret.Add((NavObject)temp.Clone());
                
            return ret;
        }

        public List<NavObject> GetItemSource()
        {
            return (List<NavObject>)lv_items.ItemsSource;
        }

        public void Clear()
        {
            lv_items.Items.Clear();
            SetFilter();
        }

        public void Add(NavObject o)
        {
            
            lv_items.Items.Add(o);
            SetFilter();
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
    }
}
