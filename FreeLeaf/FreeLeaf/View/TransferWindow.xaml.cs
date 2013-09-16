using FreeLeaf.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FreeLeaf.View
{
    public partial class TransferWindow : Window
    {
        private TransferViewModel model;

        public TransferWindow(DeviceItem item)
        {
            InitializeComponent();
            model = (TransferViewModel)this.DataContext;
        }

        private void ButtonFolderUp_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateLocalUp();
        }

        private void ButtonSetDestination_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dragData = new DataObject("LinkItems", ListLocal.SelectedItems);
            DragDrop.DoDragDrop(ListLocal, dragData, DragDropEffects.Link);
        }

        private void ButtonClearDestination_Click(object sender, RoutedEventArgs e)
        {
            foreach (LocalFileItem item in ListLocal.SelectedItems)
            {
                item.Destination = null;
            }
        }

        private void LocalList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as LocalFileItem;
            if (item != null)
            {
                if (item.IsFolder)
                {
                    model.NavigateLocal(item.Path);
                }
            }
        }

        private void RemoteListItem_Expanded(object sender, RoutedEventArgs e)
        {
            var element = e.OriginalSource as TreeViewItem;
            var item = element.Header as RemoteFileItem;
            model.PopulateRemoteFolder(item);
        }

        private void RemoteList_Drop(object sender, DragEventArgs e)
        {
            var dragItems = e.Data.GetData("LinkItems") as System.Collections.IList;
            var overData = (e.OriginalSource as FrameworkElement).DataContext as RemoteFileItem;
            foreach (LocalFileItem item in dragItems)
            {
                item.Destination = overData.Path;
            }
        }
    }
}
