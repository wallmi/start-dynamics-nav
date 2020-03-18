using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        //private NavObject retGet = new NavObject();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        private readonly string TYPE_ALL = "* (ALL)";

        public NavObjektList()
        {
            InitializeComponent();
            cb_type.Items.Add(TYPE_ALL);
            cb_type.SelectedItem = TYPE_ALL;
            foreach (String temp in NavObjects.GetObjectNames())
                cb_type.Items.Add(temp);

            cb_type.Items.Remove(ObjectType.None.ToString());
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

        public void setShowVersion (bool show)
        {
            if (!show)
            {
                _version.Visibility = Visibility.Hidden;
                gvc_version.Width = 0;
            }
        }

        private bool UserFilter(object item)
        {
            // 1) Wenn der Typ nicht zusammenstimmt --> FALSE
            // 2) Wenn kein Text eingegeben wurd --> TRUE
            // 3) Wenn der Suchtext in dem Text enthalten ist --> TRUE
            if (cb_type.SelectedItem.ToString() != (item as NavObject).Typ.ToString() && cb_type.SelectedItem.ToString() != TYPE_ALL)
                return false;
            else if (String.IsNullOrEmpty(tb_search.Text))
                return true;
            else
                return ((item as NavObject).Name.IndexOf(tb_search.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lv_items.ItemsSource).Refresh();
        }

        private void Cb_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv_items.ItemsSource == null)
                return;

            CollectionViewSource.GetDefaultView(lv_items.ItemsSource).Refresh();
        }


        #region Sort
        private void LV_items_Click_Header(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lv_items.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lv_items.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));

        }
    }

    public class SortAdorner : Adorner
    {
        private static readonly Geometry ascGeometry =
            Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static readonly Geometry descGeometry =
            Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
            : base(element)
        {
            Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (drawingContext == null)
                return;

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                (
                    AdornedElement.RenderSize.Width - 15,
                    (AdornedElement.RenderSize.Height - 5) / 2
                );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
        #endregion
    }
}
