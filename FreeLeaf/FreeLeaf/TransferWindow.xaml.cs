using FreeLeaf.ViewModel;
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

namespace FreeLeaf
{
    public partial class TransferWindow : Window
    {
        private TransferViewModel model;

        public TransferWindow(DeviceItem item)
        {
            InitializeComponent();
            model = (TransferViewModel)DataContext;
            model.SelectedItem = item;
        }

        private void LocalDriveList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as DriveItem;
            if (item != null)
            {
                if (item.IsFolder)
                {
                    model.NavigateLocal(item.Path);
                }
            }
        }

        private void HyperlinkClearDestination_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ListLocal.SelectedItems)
            {
                (item as DriveItem).Destination = null;
            }
        }

        private void HyperlinkSetDestination_Click(object sender, RoutedEventArgs e)
        {
            Popip.IsOpen = true;
        }

        private void TreeView_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            var s = (sender as TreeView).SelectedItem as TreeViewItem;
            foreach (var item in ListLocal.SelectedItems)
            {
                (item as DriveItem).Destination = s.Header.ToString();
            }
        }
    }
}
